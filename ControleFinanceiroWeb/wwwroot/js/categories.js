//#region Listeners

document.addEventListener('DOMContentLoaded', () => {
    const modalCategory = document.getElementById('modalCategoryManagement');

    if (modalCategory) {
        modalCategory.addEventListener('hidden.bs.modal', () => {
            refreshCategoryDropdown();

            window.removeEventListener('keydown', handleCategoryArrowScroll);
        });

        modalCategory.addEventListener('shown.bs.modal', () => {
            window.addEventListener('keydown', handleCategoryArrowScroll);
        });
    }

    const btnSaveCategory = document.getElementById('btnSaveCategory');

    btnSaveCategory?.addEventListener('click', saveCategory);
});

//#endregion

//#region Functions
/**
 * Keyboard listener to scroll the categories grid using ArrowUp/ArrowDown keys.
 * @param {KeyboardEvent} e - The keyboard event object.
 */
function handleCategoryArrowScroll(e) {
    const container = document.getElementById('categoriesTableContainer');

    if (!container) return;

    const scrollStep = 40;

    if (e.key === 'ArrowUp') {
        container.scrollTop -= scrollStep;

        e.preventDefault();
    } else if (e.key === 'ArrowDown') {
        container.scrollTop += scrollStep;

        e.preventDefault();
    }
}

/**
 * Opens the Category Management modal and loads the listing grid.
 * @returns {Promise<void>}
 */
async function openCategoryManagementModal() {
    const modalEl = document.getElementById('modalCategoryManagement');

    if (!modalEl) return;

    let modal = bootstrap.Modal.getInstance(modalEl);

    if (!modal) {
        modal = new bootstrap.Modal(modalEl);
    }

    modal.show();

    await showCategoryListMode();
}

/**
 * Loads the categories grid view inside the modal.
 * @returns {Promise<void>}
 */
async function showCategoryListMode() {
    const modalTitle = document.getElementById('modalCategoryManagementTitle');
    const bodyEl = document.getElementById('modalCategoryManagementBody');
    const listFooter = document.getElementById('categoryModalListFooter');
    const formFooter = document.getElementById('categoryModalFormFooter');

    if (modalTitle)
        modalTitle.innerHTML = '<i class="bi bi-tags text-success"></i> Gerenciar Categorias';

    if (listFooter) {
        listFooter.classList.remove('d-none');
        listFooter.classList.add('d-flex');
    }
    if (formFooter) {
        formFooter.classList.remove('d-flex');
        formFooter.classList.add('d-none');
    }

    if (bodyEl) {
        bodyEl.innerHTML = `
            <div class="text-center py-4">
                <div class="spinner-border text-success" role="status">
                    <span class="visually-hidden">Carregando...</span>
                </div>
            </div>`;
    }

    const result = await BaseFetch.get('/Categories/GetList');

    if (result.success) {
        bodyEl.innerHTML = result.data;
    } else {
        bodyEl.innerHTML = `<div class="alert alert-danger mb-0">${escapeHtml(result.message) || 'Erro ao carregar a listagem de categorias.'}</div>`;
    }
}

/**
 * Loads the Category insert/edit form inside the modal.
 * @param {number} id - The ID of the category to edit, or 0 to create a new one.
 * @returns {Promise<void>}
 */
async function openCategoryFormModal(id = 0) {
    const modalTitle = document.getElementById('modalCategoryManagementTitle');
    const bodyEl = document.getElementById('modalCategoryManagementBody');
    const listFooter = document.getElementById('categoryModalListFooter');
    const formFooter = document.getElementById('categoryModalFormFooter');

    if (id === 0) {
        if (modalTitle)
            modalTitle.innerHTML = '<i class="bi bi-plus-circle text-success"></i> Incluir Categoria';
    } else {
        if (modalTitle)
            modalTitle.innerHTML = '<i class="bi bi-pencil-square text-success"></i> Editar Categoria';
    }

    if (listFooter) {
        listFooter.classList.remove('d-flex');
        listFooter.classList.add('d-none');
    }

    if (formFooter) {
        formFooter.classList.remove('d-none');
        formFooter.classList.add('d-flex');
    }

    if (bodyEl) {
        bodyEl.innerHTML = `
            <div class="text-center py-4">
                <div class="spinner-border text-success" role="status">
                    <span class="visually-hidden">Carregando...</span>
                </div>
            </div>`;
    }

    const result = await BaseFetch.get(`/Categories/GetForm/${id}`);

    if (result.success) {
        bodyEl.innerHTML = result.data;
        
        if (window.jQuery && window.jQuery.validator && window.jQuery.validator.unobtrusive) {
            window.jQuery.validator.unobtrusive.parse("#formCategory");
        }
    } else {
        bodyEl.innerHTML = `<div class="alert alert-danger mb-0">${escapeHtml(result.message) || 'Erro ao carregar o formulário.'}</div>`;
    }
}

/**
 * Submits the category form using BaseFetch with FormData.
 * @returns {Promise<void>}
 */
async function saveCategory() {
    const form = document.getElementById('formCategory');

    if (!form || !form.reportValidity()) return;

    const result = await BaseFetch.post('/Categories/Save', new FormData(form));

    if (result.success) {
        showToast(result.message || "Categoria salva com sucesso!");

        await showCategoryListMode();
    } else {
        await messageBox(result.message || "Não foi possível salvar a categoria.", false, "Erro");
    }
}

/**
 * Deletes a category after user confirmation and reloads list.
 * @param {HTMLElement} btn - The clicked button element.
 * @param {number} id - The category ID to delete.
 * @returns {Promise<void>}
 */
async function deleteCategoryRow(btn, id) {
    try {
        const confirmed = await messageBox("Você tem certeza que deseja excluir essa categoria?", true, "Excluir Categoria");

        if (confirmed) {
            const result = await BaseFetch.delete(`/Categories/Delete/${id}`);

            if (result.success) {
                showToast(result.message || "Categoria excluída com sucesso.");

                await showCategoryListMode();
            } else {
                await messageBox(result.message || "Não foi possível excluir a categoria.", false, "Erro");
            }
        }
    } catch {
        
    }
}

/**
 * Filters the categories grid table in real-time.
 * @param {HTMLInputElement} input - The filter input element.
 */
function filterCategoriesTable(input) {
    const query = input.value.toLowerCase();
    const rows = document.querySelectorAll('#tableCategories tbody tr');

    rows.forEach(row => {
        const text = row.innerText.toLowerCase();

        row.style.display = text.includes(query) ? '' : 'none';
    });
}

/**
 * Refreshes the Category select options inside the transaction form.
 * @returns {Promise<void>}
 */
async function refreshCategoryDropdown() {
    const selectEl = document.getElementById('CategoryId');

    if (!selectEl) return;

    const currentValue = selectEl.value;
    const result = await BaseFetch.get('/Categories/GetCategoriesJson');

    if (result && Array.isArray(result)) {
        selectEl.innerHTML = '<option value="">-- Selecione a Categoria --</option>';

        result.forEach(cat => {
            const typeLabel = cat.entryType === 'F' ? 'Fixo' : 'Variável';
            const option = document.createElement('option');

            option.value = cat.id;
            option.textContent = `${cat.description} [${typeLabel}]`;

            selectEl.appendChild(option);
        });

        selectEl.value = currentValue;
    }
}
//#endregion