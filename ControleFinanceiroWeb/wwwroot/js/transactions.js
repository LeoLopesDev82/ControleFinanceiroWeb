//#region Variables

const statementTypeId = parseInt(document.getElementById('transactionsContainer')?.dataset?.statementTypeId || '0', 10);

let cachedCategories = null;

//#endregion

//#region Listeners
/**
 * Setup event listeners for transaction buttons and Excel copy-paste imports.
 */
document.addEventListener('DOMContentLoaded', () => {
    const btnSave = document.getElementById('btnSaveTransaction');

    if (btnSave) {
        btnSave.addEventListener('click', saveTransaction);
    }

    const pasteArea = document.getElementById('pasteArea');
    const hiddenPasteInput = document.getElementById('hiddenPasteInput');
    
    if (pasteArea && hiddenPasteInput) {
        pasteArea.addEventListener('click', () => {
            hiddenPasteInput.focus();
        });
        
        pasteArea.addEventListener('keydown', (e) => {
            if (e.key === 'Enter' || e.key === ' ') {
                hiddenPasteInput.focus();

                e.preventDefault();
            }
        });

        hiddenPasteInput.addEventListener('paste', handlePasteEvent);
    }
});

//#endregion

//#region Functions

/**
 * Filters the transactions table based on search input.
 * @param {HTMLInputElement} input - The search input element.
 */
function filterTransactionsTable(input) {
    const query = input.value.toLowerCase();
    const rows = document.querySelectorAll('#tableTransactions tbody tr');

    rows.forEach(row => {
        const text = row.innerText.toLowerCase();

        row.style.display = text.includes(query) ? '' : 'none';
    });
}

/**
 * Sets the transaction modal title depending on whether it's an insertion or an update.
 * @param {HTMLElement} titleEl - The title element.
 * @param {number} id - The transaction ID.
 */
function setTransactionModalTitle(titleEl, id) {
    if (!titleEl) return;

    if (id === 0) {
        titleEl.innerHTML = '<i class="bi bi-plus-circle text-success"></i> Incluir Movimentação';
    } else {
        titleEl.innerHTML = '<i class="bi bi-pencil-square text-success"></i> Editar Movimentação';
    }
}

/**
 * Renders a loading spinner inside the modal body.
 * @param {HTMLElement} bodyEl - The modal body element.
 */
function showTransactionModalSpinner(bodyEl) {
    if (!bodyEl) return;

    bodyEl.innerHTML = `
        <div class="text-center py-4">
            <div class="spinner-border text-success" role="status">
                <span class="visually-hidden">Carregando...</span>
            </div>
        </div>`;
}

/**
 * Initializes and triggers the display of the Bootstrap modal.
 * @param {HTMLElement} modalEl - The modal container element.
 */
function triggerTransactionModal(modalEl) {
    if (!modalEl) return;

    let modal = bootstrap.Modal.getInstance(modalEl);

    if (!modal) {
        modal = new bootstrap.Modal(modalEl);
    }

    modal.show();
}

/**
 * Fetches the transaction form HTML from the server, updates the modal body, and binds validation.
 * @param {HTMLElement} bodyEl - The modal body element.
 * @param {number} id - The transaction ID.
 * @returns {Promise<void>}
 */
async function loadAndBindTransactionForm(bodyEl, id) {
    if (!bodyEl) return;

    const result = await BaseFetch.get(`/Transactions/GetForm?id=${id}&statementTypeId=${statementTypeId}`);

    if (result.success) {
        bodyEl.innerHTML = result.data;
        
        if (window.jQuery && window.jQuery.validator && window.jQuery.validator.unobtrusive) {
            window.jQuery.validator.unobtrusive.parse("#formTransaction");
        }
    } else {
        bodyEl.innerHTML = `<div class="alert alert-danger mb-0">${escapeHtml(result.message) || 'Erro ao carregar o formulário.'}</div>`;
    }
}

/**
 * Opens the dynamic transaction modal.
 * @param {number} id - The ID of the transaction to edit, or 0 to create a new one.
 * @returns {Promise<void>}
 */
async function openTransactionModal(id = 0) {
    const modalEl = document.getElementById('modalTransaction');
    const titleEl = document.getElementById('modalTransactionTitle');
    const bodyEl = document.getElementById('modalTransactionBody');

    setTransactionModalTitle(titleEl, id);
    showTransactionModalSpinner(bodyEl);
    triggerTransactionModal(modalEl);

    await loadAndBindTransactionForm(bodyEl, id);
}

/**
 * Submits the transaction form using BaseFetch with FormData.
 * @returns {Promise<void>}
 */
async function saveTransaction() {
    const form = document.getElementById('formTransaction');

    if (!form || !form.reportValidity()) return;

    const modalEl = document.getElementById('modalTransaction');
    const modal = bootstrap.Modal.getInstance(modalEl);

    if (modal) modal.hide();

    const result = await BaseFetch.post('/Transactions/Save', new FormData(form));

    if (result.success) {
        showToast(result.message || "Movimentação salva com sucesso!");
        setTimeout(() => window.location.reload(), 1000);
    } else {
        await messageBox(result.message || "Não foi possível salvar a movimentação.", false, "Erro");

        if (modal) modal.show();
    }
}

/**
 * Triggers modal edit mode.
 * @param {HTMLElement} btn - The clicked button element.
 * @param {number} id - The transaction ID to edit.
 */
function editTransactionRow(btn, id) {
    openTransactionModal(id);
}

/**
 * Deletes a transaction row after user confirmation.
 * @param {HTMLElement} btn - The clicked button element.
 * @param {number} id - The transaction ID to delete.
 * @returns {Promise<void>}
 */
async function deleteTransactionRow(btn, id) {
    const confirmed = await messageBox("Você tem certeza que deseja excluir esse registro?", true, "Excluir Registro");

    if (confirmed) {
        const result = await BaseFetch.delete(`/Transactions/Delete/${id}`);

        if (result.success) {
            showToast(result.message || "Registro excluído com sucesso.");
            setTimeout(() => window.location.reload(), 1000);
        } else {
            await messageBox(result.message || "Não foi possível excluir o registro.", false, "Erro");
        }
    }
}

/**
 * Triggers the automatic category identification for the current statement and period.
 * @returns {Promise<void>}
 */
async function identifyStatementItems() {
    const sdInput = document.getElementById('startDate');
    const edInput = document.getElementById('endDate');

    if (!sdInput || !edInput) return;

    const startDate = sdInput.value;
    const endDate = edInput.value;

    const confirmed = await messageBox("Deseja executar a identificação automática de categorias para os lançamentos pendentes deste período?", true, "Identificar Categorias");

    if (!confirmed) return;

    showToast("Analisando lançamentos...");

    const formData = new FormData();

    formData.append('statementTypeId', statementTypeId);
    formData.append('startDate', startDate);
    formData.append('endDate', endDate);

    const result = await BaseFetch.post('/Transactions/Identify', formData);

    if (result.success) {
        await messageBox(result.message || "Identificação concluída!", false, "Sucesso");

        window.location.reload();
    } else {
        await messageBox(result.message || "Ocorreu um erro ao processar a identificação.", false, "Erro");
    }
}

/**
 * Formats a yyyy-MM-dd date string to dd/MM/yyyy.
 * @param {string} dateStr - The raw date string.
 * @returns {string} The formatted date string.
 */
function formatDateString(dateStr) {
    if (!dateStr) return '';

    const parts = dateStr.split('-');

    if (parts.length === 3) {
        return `${parts[2]}/${parts[1]}/${parts[0]}`;
    }

    return dateStr;
}

/**
 * Formats a decimal number into local Brazilian Currency.
 * @param {number|string} val - The numeric value.
 * @returns {string} The formatted currency string.
 */
function formatCurrency(val) {
    const num = parseFloat(val);

    if (isNaN(num)) return 'R$ 0,00';

    return num.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

/**
 * Fetches and caches the category options list.
 * @returns {Promise<Array>} A promise resolving to the category array.
 */
async function getCategoriesList() {
    if (cachedCategories) return cachedCategories;

    const result = await BaseFetch.get('/Categories/GetCategoriesJson');

    if (result && Array.isArray(result)) {
        cachedCategories = result;
    } else {
        cachedCategories = [];
    }

    return cachedCategories;
}

/**
 * Opens the Excel copy-paste import modal.
 */
function openImportModal() {
    const modalEl = document.getElementById('modalImportTransactions');

    if (!modalEl) return;

    document.getElementById('importPreviewContainer').classList.add('d-none');
    document.getElementById('importErrorMessage').classList.add('d-none');
    document.getElementById('btnConfirmImport').classList.add('d-none');
    document.querySelector('#tableImportPreview tbody').innerHTML = '';

    let modal = bootstrap.Modal.getInstance(modalEl);

    if (!modal) {
        modal = new bootstrap.Modal(modalEl);
    }

    modal.show();

    setTimeout(() => {
        const hiddenPasteInput = document.getElementById('hiddenPasteInput');

        if (hiddenPasteInput) hiddenPasteInput.focus();
    }, 500);
}

/**
 * Prepares the UI container states and displays the loading spinner in the import table.
 * @param {HTMLElement} tbody - The table body element.
 */
function prepareImportPreviewUI(tbody) {
    if (!tbody) return;

    tbody.innerHTML = `
        <tr>
            <td colspan="7" class="text-center py-4">
                <div class="spinner-border text-success" role="status">
                    <span class="visually-hidden">Carregando...</span>
                </div>
            </td>
        </tr>`;

    document.getElementById('importPreviewContainer').classList.remove('d-none');
    document.getElementById('importErrorMessage').classList.add('d-none');
    document.getElementById('btnConfirmImport').classList.add('d-none');
}

/**
 * Sends the raw clipboard text to the server to generate preview data.
 * @param {string} rawText - The raw clipboard text.
 * @returns {Promise<object>} The server preview response.
 */
async function fetchImportPreviewData(rawText) {
    const formData = new FormData();

    formData.append('rawText', rawText);

    return await BaseFetch.post('/Transactions/PreviewImport', formData);
}

/**
 * Builds the category dropdown HTML element for an import preview row.
 * @param {number|null} selectedId - The preselected category ID.
 * @param {number} rowIndex - The index of the preview row.
 * @param {Array} categories - The list of cached categories.
 * @returns {string} The HTML select element.
 */
function buildImportCategorySelect(selectedId, rowIndex, categories) {
    let selectHtml = `<select class="form-select form-select-sm import-cat-select" data-row-idx="${rowIndex}" style="max-width: 220px;">`;

    selectHtml += '<option value="">-- Selecione a Categoria --</option>';

    categories.forEach(cat => {
        const prefix = cat.entryType === 'F' ? 'Fixo' : 'Variável';
        const selected = cat.id === selectedId ? 'selected' : '';

        selectHtml += `<option value="${cat.id}" ${selected}>${escapeHtml(cat.description)} [${prefix}]</option>`;
    });

    selectHtml += '</select>';

    return selectHtml;
}

/**
 * Creates and configures a table row for an import item.
 * @param {object} item - The import item data.
 * @param {Array} categories - The list of categories.
 * @returns {HTMLTableRowElement} The configured table row element.
 */
function createImportPreviewRow(item, categories) {
    const tr = document.createElement('tr');

    tr.className = item.isValid ? 'align-middle' : 'align-middle table-danger-subtle';

    const catSelectHtml = buildImportCategorySelect(item.categoryId, item.rowIndex, categories);

    const errorTitle = escapeHtml(item.errorMessage);

    const dateCell = item.isValid && item.parsedDate
        ? formatDateString(item.parsedDate)
        : `<span class="text-danger fw-semibold" title="${errorTitle}">${escapeHtml(item.rawDate) || '[Vazio]'}</span>`;

    const dueDateCell = item.isValid && item.parsedDueDate
        ? formatDateString(item.parsedDueDate)
        : `<span class="text-danger fw-semibold" title="${errorTitle}">${escapeHtml(item.rawDueDate) || '[Vazio]'}</span>`;

    const descCell = item.description
        ? `<span>${escapeHtml(item.description)}</span>`
        : `<span class="text-danger fw-semibold">[Vazio]</span>`;

    const amountFormatted = item.parsedAmount !== null && item.parsedAmount !== undefined
        ? formatCurrency(item.parsedAmount)
        : `<span class="text-danger fw-semibold" title="${errorTitle}">${escapeHtml(item.rawAmount) || '[Vazio]'}</span>`;

    const statusCell = item.isValid
        ? '<i class="bi bi-check-circle-fill text-success fs-5"></i>'
        : `<i class="bi bi-exclamation-triangle-fill text-danger fs-5" title="${errorTitle}"></i>`;

    tr.innerHTML = `
        <td>${item.rowIndex}</td>
        <td>${dateCell}</td>
        <td>${dueDateCell}</td>
        <td class="text-truncate" style="max-width: 250px;" title="${escapeHtml(item.description)}">${descCell}</td>
        <td>${catSelectHtml}</td>
        <td class="text-end fw-bold">${amountFormatted}</td>
        <td class="text-center">${statusCell}</td>
    `;

    tr.dataset.transactionDate = item.parsedDate || '';
    tr.dataset.dueDate = item.parsedDueDate || '';
    tr.dataset.description = item.description || '';
    tr.dataset.amount = item.parsedAmount !== null ? item.parsedAmount : '';
    tr.dataset.isValid = item.isValid;

    return tr;
}

/**
 * Updates the import confirmation footer controls based on the presence of validation errors.
 * @param {boolean} hasErrors - Whether validation errors are present.
 */
function updateImportFooterControls(hasErrors) {
    if (hasErrors) {
        document.getElementById('importErrorMessage').classList.remove('d-none');
        document.getElementById('btnConfirmImport').classList.add('d-none');
    } else {
        document.getElementById('importErrorMessage').classList.add('d-none');
        document.getElementById('btnConfirmImport').classList.remove('d-none');
    }
}

/**
 * Intercepts the clipboard paste event and renders the preview grid.
 * @param {ClipboardEvent} e - The clipboard event.
 * @returns {Promise<void>}
 */
async function handlePasteEvent(e) {
    e.preventDefault();

    const rawText = (e.clipboardData || window.clipboardData).getData('text');

    if (!rawText || rawText.trim() === '') return;

    const tbody = document.querySelector('#tableImportPreview tbody');

    prepareImportPreviewUI(tbody);

    const result = await fetchImportPreviewData(rawText);

    if (result.success && result.items) {
        const categories = await getCategoriesList();

        tbody.innerHTML = '';

        let hasErrors = false;

        const items = result.items;

        document.getElementById('importItemCount').textContent = items.length;

        if (items.length === 0) {
            tbody.innerHTML = '<tr><td colspan="7" class="text-center text-muted py-4">Nenhum registro de lançamento encontrado.</td></tr>';

            return;
        }

        items.forEach(item => {
            const tr = createImportPreviewRow(item, categories);

            if (!item.isValid) {
                hasErrors = true;
            }

            tbody.appendChild(tr);
        });

        updateImportFooterControls(hasErrors);
    } else {
        tbody.innerHTML = `<tr><td colspan="7" class="text-center text-danger py-4">${escapeHtml(result.message) || 'Erro ao carregar prévia dos dados.'}</td></tr>`;
    }
}

/**
 * Reads all rows from the import preview table and gathers valid items.
 * @returns {{ items: Array, allValid: boolean }} Gathered items and overall validation state.
 */
function parseImportPreviewRows() {
    const rows = document.querySelectorAll('#tableImportPreview tbody tr');
    const items = [];

    let allValid = true;

    rows.forEach(row => {
        const isValid = row.dataset.isValid === 'true';

        if (!isValid) {
            allValid = false;

            return;
        }

        const catSelect = row.querySelector('.import-cat-select');
        const categoryId = catSelect ? parseInt(catSelect.value || '0', 10) : null;

        items.push({
            transactionDate: row.dataset.transactionDate,
            dueDate: row.dataset.dueDate,
            description: row.dataset.description,
            amount: row.dataset.amount,
            categoryId: categoryId > 0 ? categoryId : null
        });
    });

    return { items, allValid };
}

/**
 * Closes the import transactions modal.
 */
function hideImportModal() {
    const modalEl = document.getElementById('modalImportTransactions');
    const modal = bootstrap.Modal.getInstance(modalEl);

    if (modal) {
        modal.hide();
    }
}

/**
 * Confirms with the user and posts the bulk payload to the backend.
 * @param {Array} items - The list of transaction items to save.
 * @returns {Promise<object>} The server response.
 */
async function submitImportedItems(items) {
    const confirmed = await messageBox(`Deseja realmente salvar estes ${items.length} lançamentos no extrato atual?`, true, "Confirmar Importação");

    if (!confirmed) {
        return { success: false, cancelled: true };
    }

    showToast("Gravando lançamentos...");

    return await BaseFetch.post(`/Transactions/SaveImport?statementTypeId=${statementTypeId}`, items);
}

/**
 * Gathers the preview rows and sends the bulk payload to the backend.
 * @returns {Promise<void>}
 */
async function saveImportedItems() {
    const { items, allValid } = parseImportPreviewRows();

    if (!allValid || items.length === 0) {
        await messageBox("Existem linhas com erro na prévia de importação. Corrija-as no Excel e cole novamente.", false, "Erro");

        return;
    }

    const result = await submitImportedItems(items);

    if (result.cancelled) return;

    if (result.success) {
        hideImportModal();

        await messageBox(result.message || "Importação concluída com sucesso!", false, "Sucesso");

        window.location.reload();
    } else {
        await messageBox(result.message || "Não foi possível concluir o salvamento da importação.", false, "Erro");
    }
}

//#endregion