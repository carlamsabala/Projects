let docTitle = "untitled"
function showSpinner() {
    document.getElementById("loading-spinner").classList.remove("hidden");
}

function hideSpinner() {
    document.getElementById("loading-spinner").classList.add("hidden");
}



async function translateDelphiToCSharp() {
    const delphiCode = delphiEditor.getValue();

    console.log("Input Delphi Code:\n", delphiCode);
    try {
        const response = await axios.post("http://localhost:5001/translate", {
            delphi_code: delphiCode
        });

        // Log the raw response for debugging
        console.log("Raw API Response:", response.data);

        const translatedCSharp = response.data.translated_csharp;
        console.log("Output C# Code:\n", translatedCSharp);
        return translatedCSharp;
    } catch (error) {
        console.error("Error during translation:", error.response ? error.response.data : error.message);
        return null;
    }
}

async function translateCode() {
    const delphiCode = delphiEditor.getValue();
    const trimmedCode = delphiCode.trim();

    if (trimmedCode === '') {
        alert('Error: Please enter Delphi code to translate.');
        return;
    }

    // Retrieve the title from the new input field
    const userTitleInput = document.getElementById('translationTitle');
    const userTitle = userTitleInput ? userTitleInput.value.trim() : '';

    // Use the user-provided title or default to the first line of the Delphi code
    const firstLine = trimmedCode.split('\n')[0].trim();
    const docTitle = userTitle || firstLine || "Untitled";

     // 1) Check if we've already translated this exact snippet
    try {
        const listResp = await axios.get("http://localhost:3000/get-translations");
        if (listResp.data.success) {
            const match = listResp.data.translations.find(doc =>
                (doc.delphiCode || '').trim() === trimmedCode
            );
            if (match) {
                // Notify user and load cached translation
                alert("This Delphi code was already translated before. Loading the existing C# translation.");
                csharpEditor.setValue(match.csharpCode);
                console.log("Loaded cached translation for:", docTitle);
                return; // skip the API call
            }
        }
    } catch (e) {
        console.error("Could not fetch existing translations:", e);
        // (If this fails, just fall through and re-translate)
    }

    showSpinner();

    try {
        const startTime = performance.now(); // ⏱️ Start timer

        const csharpCode = await translateDelphiToCSharp(trimmedCode);
        const endTime = performance.now(); // ⏱️ End timer
        const elapsed = ((endTime - startTime) / 1000).toFixed(2);
        console.log(`⏱️ Translation took ${elapsed} seconds`);
        if (csharpCode) {
            csharpEditor.setValue(csharpCode);

            try {

                const saveResponse = await axios.post("http://localhost:3000/save-translation", {
                    delphiCode: trimmedCode,
                    csharpCode,
                    documentTitle: docTitle
                });

                console.log("Translation saved with ID:", saveResponse.data.id);
                console.log("Title: ", saveResponse.data.documentTitle);
            } catch (saveError) {
                console.error("Error saving translation:", saveError.response ? saveError.response.data : saveError.message);
            }
        } else {
            alert("Translation failed. Please try again.");
        }
    } finally {
        hideSpinner();
    }
}

function copyToClipboard() { 
    const csharpCode = csharpEditor.getValue();
    navigator.clipboard.writeText(csharpCode).then(() => {
        document.getElementById("copy-text").textContent = "Copied!";
        setTimeout(() => {
        document.getElementById("copy-text").textContent = "Copy";
        }, 2000);
    });
}

function downloadCSharp() {
    const csharpCode = csharpEditor.getValue();
    const filename = docTitle.replace(/\s+/g, '_') + ".cs"; // Clean title for filename
    const blob = new Blob([csharpCode], { type: "text/plain" });
    const link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }

function logout() {
    const logoutUrl = "http://localhost:9000/application/o/act-app/end-session/?next=http://localhost:3000";
    window.location.href = logoutUrl;
}