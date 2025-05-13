import torch
from transformers import T5Tokenizer, T5ForConditionalGeneration
from peft import get_peft_model, LoraConfig, TaskType
from datasets import load_dataset

# Load the tokenizer and model (Salesforce's CodeT5)
MODEL_NAME = "Salesforce/codet5-small"
tokenizer = T5Tokenizer.from_pretrained(MODEL_NAME)
model = T5ForConditionalGeneration.from_pretrained(MODEL_NAME)

# Apply LoRA for lightweight fine-tuning
lora_config = LoraConfig(
    task_type=TaskType.SEQ_2_SEQ_LM,
    r=16,  # LoRA rank
    lora_alpha=32,
    lora_dropout=0.1
)
model = get_peft_model(model, lora_config)

# Load dataset (Need to replace this later with an actual dataset)
# We are assuming our future dataset has `delphi_code` and `csharp_code` columns
dataset = load_dataset("json", data_files={"train": "dataset.json"})

# Tokenize dataset
def tokenize_function(example):
    input_text = "translate Delphi to C#: " + example["delphi_code"]
    target_text = example["csharp_code"]
    return tokenizer(input_text, padding="max_length", truncation=True, max_length=512), \
           tokenizer(target_text, padding="max_length", truncation=True, max_length=512)

tokenized_datasets = dataset["train"].map(tokenize_function)

# Training setup
from transformers import TrainingArguments, Trainer

training_args = TrainingArguments(
    output_dir="./results",
    evaluation_strategy="epoch",
    learning_rate=2e-5,
    per_device_train_batch_size=8,
    per_device_eval_batch_size=8,
    num_train_epochs=3,
    weight_decay=0.01,
    save_total_limit=2,
    save_strategy="epoch",
    logging_dir="./logs"
)

trainer = Trainer(
    model=model,
    args=training_args,
    train_dataset=tokenized_datasets
)

# Train the model
trainer.train()

# Save the fine-tuned model
model.save_pretrained("delphi-to-csharp-model")
tokenizer.save_pretrained("delphi-to-csharp-model")

######################################################

# Test function (need to test later when model more finalized)
def translate_code(delphi_code):
    inputs = tokenizer("translate Delphi to C#: " + delphi_code, return_tensors="pt", padding=True, truncation=True)
    output_ids = model.generate(**inputs)
    translated_code = tokenizer.decode(output_ids[0], skip_special_tokens=True)
    return translated_code

# Test example
test_code = "procedure Greet; begin WriteLn('Hello, World!'); end;"
print(translate_code(test_code))
