
document.addEventListener('DOMContentLoaded', async () => {
    const response = await fetch("http://localhost:3000/get-translations");
    const data = await response.json();
    // Global click to close the dropdown
    
    if (data.success) {
        const translations = data.translations;
        const docList = document.getElementById('docList');
        docList.innerHTML = ''; // Clear placeholder

        translations.forEach(doc => {
            // Create a clickable list item
            const listItem = document.createElement('div');
            listItem.className = 'listItem';

            // beginning of menu toggle

            // Create a dropdown container for the three-dot menu
            const dropdownContainer = document.createElement('div');
            dropdownContainer.className = 'dropdown-container';

            // Create the three dots menu button
            const menuButton = document.createElement('button');
            menuButton.className = 'dropdown-button';
            const menuIcon = document.createElement('img');
            menuIcon.src = '../assets/Images/tripledots.png'; 
            menuIcon.alt = 'Menu';
            menuIcon.style.width = '5px'; 
            menuIcon.style.height = '22px';
            menuButton.appendChild(menuIcon);

            // Create the dropdown menu (hidden by default)
            const dropdownMenu = document.createElement('div');
            dropdownMenu.className = 'dropdown-menu';
            dropdownMenu.style.display = 'none'; 

            // Create Delete option (as an <a> instead of a <button>)
            const deleteOption = document.createElement('a');
            deleteOption.className = 'dropdown-option';
            deleteOption.textContent = 'Delete';
            deleteOption.href = '#';

            // Create Edit Title option
            const editOption = document.createElement('a');
            editOption.className = 'dropdown-option';
            editOption.textContent = 'Edit Title';
            editOption.href = '#';

            // Create Download .cs option
            const downloadOption = document.createElement('a');
            downloadOption.className = 'dropdown-option';
            downloadOption.textContent = 'Download';
            downloadOption.href = '#';

            // Append options to the dropdown menu
            dropdownMenu.appendChild(deleteOption);
            dropdownMenu.appendChild(editOption);
            dropdownMenu.appendChild(downloadOption);

            // Append menu button and dropdown menu to container
            dropdownContainer.appendChild(menuButton);
            dropdownContainer.appendChild(dropdownMenu);

            // Show a title if available, or fallback to a snippet/timestamp
            const title = doc.documentTitle || `Translation: ${doc._id}`;
            const date = doc.timestamp || new Date().toISOString();

            // Format the date and time
            const formattedDateTime = new Date(date).toLocaleString('en-US', {
                year: 'numeric',
                month: 'short',
                day: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });

            listItem.innerHTML = `
                <div class="doc-file">
                    <img src="../assets/Images/document.png" alt="Document" class="document-image">
                    <div class="doc-title">${title}</div>
                </div>
                <pre class="code-preview grid-only">${(doc.delphiCode || '').slice(0, 75)}...</pre>
                <div class="doc-date">${formattedDateTime}</div>
            `;

            listItem.setAttribute('data-delphi', (doc.delphiCode || '').toLowerCase());

            // Append the dropdown container to the list item
            listItem.appendChild(dropdownContainer);

            // Toggle dropdown visibility on menu button click
            menuButton.addEventListener('click', (event) => {
            event.stopPropagation();
            dropdownMenu.style.display = 
                dropdownMenu.style.display === 'none' ? 'block' : 'none';
            });

            // ----- EVENT LISTENERS ----- //

            // Delete
            deleteOption.addEventListener('click', async (event) => {
            event.preventDefault(); // prevent '#' navigation
            event.stopPropagation();
            if (confirm("Are you sure you want to delete this translation?")) {
                try {
                const deleteResponse = await fetch(
                    `http://localhost:3000/delete-translation/${doc._id}`, {
                    method: 'DELETE'
                    }
                );
                const result = await deleteResponse.json();
                if (result.success) {
                    listItem.remove();
                } else {
                    alert("Error deleting translation: " + result.error);
                }
                } catch (error) {
                console.error("Error deleting translation:", error);
                alert("An error occurred while deleting the translation.");
                }
            }
            dropdownMenu.style.display = 'none';
            });

            // Edit Title
            editOption.addEventListener('click', async (event) => {
            event.preventDefault();
            event.stopPropagation();
            const newTitle = prompt("Enter new title", doc.documentTitle);
            if (newTitle && newTitle !== doc.documentTitle) {
                try {
                const updateResponse = await fetch(
                    `http://localhost:3000/update-translation/${doc._id}`, {
                    method: 'PUT',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ documentTitle: newTitle })
                    }
                );
                if (!updateResponse.ok) {
                    console.error('HTTP Error:', updateResponse.status, updateResponse.statusText);
                    alert("HTTP error updating title: " + updateResponse.status);
                    return;
                }
                const result = await updateResponse.json();
                if (result.success) {
                    listItem.querySelector('.doc-title').textContent = newTitle;
                    doc.documentTitle = newTitle;
                } else {
                    alert("Error updating title: " + result.error);
                }
                } catch (error) {
                console.error("Error updating title:", error);
                alert("An error occurred while updating the title.");
                }
            }
            dropdownMenu.style.display = 'none';
            });

            // Download
            downloadOption.addEventListener('click', (event) => {
            event.preventDefault();
            event.stopPropagation();
            // Assume doc.translation holds the translation content.
            const content = doc.translation || "No translation content available.";
            const filename = doc.documentTitle 
                ? `${doc.documentTitle}.cs` 
                : `translation-${doc._id}.cs`;
            const blob = new Blob([content], { type: 'text/plain' });
            const link = document.createElement('a');
            link.href = URL.createObjectURL(blob);
            link.download = filename;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            dropdownMenu.style.display = 'none';
            });


            // end of menu toggle

            listItem.addEventListener('click', () => {
                // Save data to localStorage
                localStorage.setItem('delphiCode', doc.delphiCode || '');
                localStorage.setItem('csharpCode', doc.csharpCode || '');
                
            
                // Redirect to the translation page
                window.location.href = 'translation.html';
                
            });
            
            docList.appendChild(listItem);
            });

        document.addEventListener('click', () => {
            document.querySelectorAll('.dropdown-menu').forEach(menu => {
                menu.style.display = 'none';
            });
        });

        // Add search filtering functionality
        searchInput.addEventListener('input', function() {
            const query = this.value.toLowerCase();
            const items = document.querySelectorAll('.listItem');
        
            items.forEach(item => {
                const titleElement = item.querySelector('.doc-title');
                const delphiCode = item.getAttribute('data-delphi') || '';
        
                const titleText = titleElement?.textContent.toLowerCase() || '';
        
                const matchesTitle = titleText.includes(query);
                const matchesDelphi = delphiCode.includes(query);
        
                item.style.display = (matchesTitle || matchesDelphi) ? '' : 'none';
            });
        });
    } else {
        console.error("Failed to load translations:", data.error);
    }

    // View toggle logic
    const listViewBtn = document.getElementById('listViewBtn');
    const gridViewBtn = document.getElementById('gridViewBtn');
    const docList = document.getElementById('docList');
    listViewBtn.addEventListener('click', () => {
        docList.classList.remove('gridView');
        docList.classList.add('listView');
        document.querySelector('.table-header').style.display = 'flex';
    });
    gridViewBtn.addEventListener('click', () => {
        docList.classList.remove('listView');
        docList.classList.add('gridView');
        document.querySelector('.table-header').style.display = 'none';
    });
});
