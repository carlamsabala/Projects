import json
import faiss
import openai
import numpy as np
from tqdm import tqdm
import os
from dotenv import load_dotenv

load_dotenv()
openai.api_key = os.getenv("OPENAI_API_KEY")
 
# Files
DATA_PATH = "paired_data.jsonl"
INDEX_PATH = "faiss_index.index"
METADATA_PATH = "indexed_metadata.jsonl"

EMBED_MODEL = "text-embedding-3-small"
EMBED_DIM = 1536
MAX_CHARS = 24000  # Limit input size to avoid token overflow

# Load dataset
examples = []
with open(DATA_PATH, "r", encoding="utf-8") as f:
    for line in f:
        item = json.loads(line)
        if item["input"] and item["output"]:
            # Truncate Delphi code to ~8k tokens worth of chars
            item["input"] = item["input"][:MAX_CHARS]
            examples.append(item)

# Embed Delphi (input) code
def get_embedding(text):
    response = openai.embeddings.create(
        model=EMBED_MODEL,
        input=text,
    )
    return response.data[0].embedding

print(f"Embedding {len(examples)} Delphi snippets (with truncation)...")
vectors = []
metadata = []

for ex in tqdm(examples):
    try:
        emb = get_embedding(ex["input"])
        vectors.append(np.array(emb, dtype=np.float32))
        metadata.append(ex)
    except Exception as e:
        print(f"Skipped one example due to: {e}")

# Create FAISS index
index = faiss.IndexFlatL2(EMBED_DIM)
index.add(np.array(vectors))

# Save index
faiss.write_index(index, INDEX_PATH)
print(f"Saved FAISS index to {INDEX_PATH}")

# Save metadata for lookup
with open(METADATA_PATH, "w", encoding="utf-8") as f:
    for item in metadata:
        json.dump(item, f)
        f.write("\n")
print(f"Saved metadata to {METADATA_PATH}")
