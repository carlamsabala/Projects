document.addEventListener('DOMContentLoaded', () => {
    const uploadContainers = [
      document.getElementById('content'),
      document.querySelector('.file-upload-container')
    ];
    const dropMessage = document.getElementById('dropMessage');
    const fileInput = document.getElementById('fileInput');
    const browseBtn = document.getElementById('browseFile');
  
    // Open file picker when Browse button is clicked
    browseBtn.addEventListener('click', () => {
      fileInput.click();
    });
  
    // Handle dropped files
    function handleFile(file) {
      if (file && file.name.endsWith(".dpr")) {
        const reader = new FileReader();
        reader.onload = (e) => {
          const delphiCode = e.target.result;
          if (typeof delphiEditor !== 'undefined') {
            delphiEditor.setValue(delphiCode);
          }
        };
        reader.readAsText(file);
      } else {
        alert("Please upload a valid .dpr file.");
      }
    }
  
    // Set drag events on all containers
    uploadContainers.forEach(container => {
      if (!container) return;
  
      ['dragenter', 'dragover'].forEach(eventName => {
        container.addEventListener(eventName, (e) => {
          e.preventDefault();
          e.stopPropagation();
          container.classList.add('drag-over');
        });
      });
  
      ['dragleave', 'drop'].forEach(eventName => {
        container.addEventListener(eventName, (e) => {
          e.preventDefault();
          e.stopPropagation();
          container.classList.remove('drag-over');
        });
      });
  
      container.addEventListener('drop', (e) => {
        const files = e.dataTransfer.files;
        if (files.length > 0) {
          handleFile(files[0]);
        }
      });
    });
  
    // Handle Browse file input
    fileInput.addEventListener('change', (e) => {
      const files = e.target.files;
      if (files.length > 0) {
        handleFile(files[0]);
      }
    });
  });
  