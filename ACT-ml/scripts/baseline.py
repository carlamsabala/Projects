import torch
import argparse
from transformers import BitsAndBytesConfig, AutoModelForCausalLM, AutoTokenizer

print(torch.__version__)
print("CUDA Available:", torch.cuda.is_available())
print("CUDA Device Count:", torch.cuda.device_count())
print("CUDA Device Name:", torch.cuda.get_device_name(0) if torch.cuda.is_available() else "No GPU detected")

# DeepSeek-Coder or GPT-based model
MODEL_NAME = "deepseek-ai/DeepSeek-Coder-V2-Lite-Instruct"  

# Enable 8-bit quantization to reduce memory usage
quantization_config = BitsAndBytesConfig(load_in_8bit=True, llm_int8_enable_fp32_cpu_offload=True)

tokenizer = AutoTokenizer.from_pretrained(MODEL_NAME)
model = AutoModelForCausalLM.from_pretrained(
    MODEL_NAME, 
    trust_remote_code=True, 
    quantization_config=quantization_config, 
    device_map="auto",
    offload_folder="offload",
)

# Move model to GPU if available
device = "cuda" if torch.cuda.is_available() else "cpu"

# Function to read Delphi source code from a .pas file
def read_delphi_file(file_path):
    try:
        with open(file_path, "r", encoding="utf-8") as file:
            delphi_code = file.read()
        return delphi_code.strip()
    except FileNotFoundError:
        print(f"Error: File '{file_path}' not found.")
        return None

# Function to translate Delphi code to C#
def translate_delphi_to_csharp(delphi_code):
    if not delphi_code:
        return "No Delphi code to translate."

    # Improved GPT-style prompting
    prompt = f"""### Instruction: Translate the following Delphi Pascal code into equivalent C# code. 

    ### Delphi Code:
    {delphi_code}

    ### C# Equivalent:"""

    # Tokenize input properly
    inputs = tokenizer(prompt, return_tensors="pt", padding=True, truncation=True, max_length=2048).to(device)

    # Generate translation
    with torch.no_grad():
        output_ids = model.generate(inputs.input_ids, max_length=2048, temperature=0.3, top_p=0.9)

    translated_code = tokenizer.decode(output_ids[0], skip_special_tokens=True)

    return translated_code.strip()

# Command-line interface
def main():
    parser = argparse.ArgumentParser(description="Translate Delphi (.pas) files to C# using GPT-based models.")
    parser.add_argument("input_file", help="Path to the Delphi (.pas) file to translate.")
    parser.add_argument("-o", "--output", help="Output file for the translated C# code", default="output/csharp.cs")

    args = parser.parse_args()

    delphi_code = read_delphi_file(args.input_file)
    if delphi_code:
        translated_csharp = translate_delphi_to_csharp(delphi_code)

        with open(args.output, "w", encoding="utf-8") as file:
            file.write(translated_csharp)

        print(f"Translation completed! C# code saved to {args.output}")
    else:
        print("No valid Delphi code found in the file.")

if __name__ == "__main__":
    main()
