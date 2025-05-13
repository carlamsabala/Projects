import os
import json

# Path to your dataset folder
dataset_folder = "Delphi2C#"
output_jsonl = "paired_data.jsonl"

# Try multiple encodings to read files
def read_file_safely(path):
    for encoding in ["utf-8", "cp1252", "latin-1"]:
        try:
            with open(path, "r", encoding=encoding, errors="replace") as f:
                return f.read().strip()
        except Exception:
            continue
    print(f"⚠️ Could not read file: {path}")
    return ""

# Collect all base filenames
base_names = set(os.path.splitext(f)[0] for f in os.listdir(dataset_folder))

# Match .pas and .cs pairs
pairs = [
    (f"{base}.pas", f"{base}.cs")
    for base in base_names
    if f"{base}.pas" in os.listdir(dataset_folder) and f"{base}.cs" in os.listdir(dataset_folder)
]

# Convert to JSONL
converted = 0
with open(output_jsonl, "w", encoding="utf-8") as out:
    for pas_file, cs_file in pairs:
        pas_path = os.path.join(dataset_folder, pas_file)
        cs_path = os.path.join(dataset_folder, cs_file)

        delphi_code = read_file_safely(pas_path)
        csharp_code = read_file_safely(cs_path)

        if delphi_code and csharp_code:
            json.dump({"input": delphi_code, "output": csharp_code}, out)
            out.write("\n")
            converted += 1

print(f"converted {converted} pairs into {output_jsonl}")
