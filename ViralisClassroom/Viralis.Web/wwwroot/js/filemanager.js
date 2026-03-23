console.log('filemanager.js loaded');

const fileManagers = {}; // store managers by inputId

function createFileManager(inputId, chipsId) {
    const input = document.getElementById(inputId);
    const chipsContainer = document.getElementById(chipsId);
    let files = [];
    let isSyncing = false;

    if (!input || !chipsContainer) return;

    function formatSize(bytes) {
        if (bytes < 1024) return bytes + ' B';
        if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
        return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
    }

    function renderChips() {
        chipsContainer.innerHTML = '';
        files.forEach((file, index) => {
            const ext = file.name.split('.').pop().toLowerCase();
            const icon = ['pdf'].includes(ext) ? '📄'
                : ['jpg', 'jpeg', 'png', 'gif', 'webp'].includes(ext) ? '🖼️'
                    : ['zip', 'rar', '7z'].includes(ext) ? '🗜️'
                        : ['doc', 'docx'].includes(ext) ? '📝'
                            : ['xls', 'xlsx'].includes(ext) ? '📊'
                                : '📎';

            const chip = document.createElement('div');
            chip.className = 'file-chip';
            chip.innerHTML = `
                <span class="file-chip-name">
                    <span class="file-chip-icon">${icon}</span>
                    <span class="file-chip-name-text" title="${file.name}">${file.name}</span>
                    <span class="file-chip-size">${formatSize(file.size)}</span>
                </span>
                <button type="button" class="file-chip-remove"
                        onclick="removeFile('${inputId}', ${index})"
                        title="Remove file">✕</button>
            `;
            chipsContainer.appendChild(chip);
        });
    }

    function addFiles(newFiles) {
        Array.from(newFiles).forEach(newFile => {
            if (!files.find(f => f.name === newFile.name && f.size === newFile.size)) {
                files.push(newFile);
            }
        });
        renderChips();
    }

    input.addEventListener('change', function () {
        if (isSyncing) return;
        addFiles(input.files);
    });

    const zone = input.closest('.file-upload-zone');
    if (zone) {
        zone.addEventListener('dragover', e => {
            e.preventDefault();
            zone.classList.add('dragover');
        });
        zone.addEventListener('dragleave', () => zone.classList.remove('dragover'));
        zone.addEventListener('drop', e => {
            e.preventDefault();
            zone.classList.remove('dragover');
            addFiles(e.dataTransfer.files);
        });
    }

    window[`removeFile_${inputId}`] = function (index) {
        files.splice(index, 1);
        renderChips();
    };

    // Before the form submits natively, sync the managed files array back to
    // input.files so the browser sends ALL accumulated files, not just the last pick.
    const parentForm = input.closest('form');
    if (parentForm) {
        parentForm.addEventListener('submit', function () {
            const dt = new DataTransfer();
            files.forEach(f => dt.items.add(f));
            input.files = dt.files;
        });
    }

    // Store manager so forms can access the files array
    fileManagers[inputId] = { getFiles: () => files };
}

window.removeFile = function (inputId, index) {
    if (window[`removeFile_${inputId}`]) {
        window[`removeFile_${inputId}`](index);
    }
};

function toggleEdit() {
    const form = document.getElementById('editForm');
    if (form) form.classList.toggle('open');
}

// Submit a form using fetch + FormData, appending files from our managed arrays
function submitFormWithFiles(formId, fileInputIds) {
    const form = document.getElementById(formId);
    if (!form) return;

    form.addEventListener('submit', async function (e) {
        e.preventDefault();

        const formData = new FormData(form);

        // Remove any files the browser added from the input (unreliable)
        // and append from our managed arrays instead
        fileInputIds.forEach(inputId => {
            formData.delete('Files'); // remove browser-managed files
            const manager = fileManagers[inputId];
            if (manager) {
                manager.getFiles().forEach(file => {
                    formData.append('Files', file);
                });
            }
        });

        // Submit via fetch
        const response = await fetch(form.action, {
            method: 'POST',
            body: formData,
            headers: {
                'RequestVerificationToken': document.querySelector(
                    'input[name="__RequestVerificationToken"]')?.value || ''
            }
        });

        // Follow the redirect
        if (response.redirected) {
            window.location.href = response.url;
        } else {
            window.location.reload();
        }
    });
}