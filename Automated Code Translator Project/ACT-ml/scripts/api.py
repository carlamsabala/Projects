import os
import json
import openai
import faiss
import numpy as np
import time
from flask import Flask, request, jsonify
from flask_cors import CORS
from dotenv import load_dotenv

# Load .env
load_dotenv()
openai.api_key = os.getenv("OPENAI_API_KEY")

# Load FAISS index + metadata
INDEX_PATH = "faiss_index.index"
METADATA_PATH = "indexed_metadata.jsonl"
EMBED_MODEL = "text-embedding-3-small"
EMBED_DIM = 1536
TOP_K = 3

index = faiss.read_index(INDEX_PATH)

# Load metadata
metadata = []
with open(METADATA_PATH, "r", encoding="utf-8") as f:
    for line in f:
        metadata.append(json.loads(line))

# Embed query Delphi code
def get_embedding(text):
    response = openai.embeddings.create(
        model=EMBED_MODEL,
        input=text,
    )
    return np.array(response.data[0].embedding, dtype=np.float32)

# Build GPT prompt with top-k few-shot examples
def build_prompt(examples, user_input):
    prompt = "Translate Delphi Pascal to C# using the examples below.\n\n"
    for ex in examples:
        prompt += f"Delphi:\n{ex['input']}\n\nC#:\n{ex['output']}\n\n---\n"
    prompt += f"Delphi:\n{user_input}\n\nC#:\n"
    return prompt

# Flask app
app = Flask(__name__)
CORS(app)

@app.route("/translate", methods=["POST"])
def translate_code():
    try:
        start_time = time.time()

        data = request.get_json()
        delphi_code = data.get("delphi_code", "").strip()
        if not delphi_code:
            return jsonify({"error": "No Delphi code provided"}), 400

        # Embed user input
        query_emb = get_embedding(delphi_code).reshape(1, -1)

        # Search FAISS for top-k similar Delphi examples
        distances, indices = index.search(query_emb, TOP_K)
        top_examples = [metadata[i] for i in indices[0]]

        # Build prompt
        prompt = build_prompt(top_examples, delphi_code)

        # Query GPT-4o-mini
        response = openai.chat.completions.create(
            model="gpt-4o-mini",
            messages=[
                {"role": "system", "content": "You are an expert in Delphi Pascal and C# programming. Translate code from Delphi to C#. Only output the C# translated code."},
                {"role": "user", "content": prompt}
            ],
            max_tokens=2048,
            temperature=0.4
        )

        translated = response.choices[0].message.content.strip()
        translated = translated.replace("```csharp", "").replace("```", "").strip()

        elapsed = time.time() - start_time  # ⏱️ End timer
        print(f"⏱️ Translation completed in {elapsed:.2f} seconds")
        
        return jsonify({"translated_csharp": translated})

    except Exception as e:
        return jsonify({"error": str(e)}), 500

# Run server
if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5000, debug=True)
