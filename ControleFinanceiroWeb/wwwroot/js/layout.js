//#region Navigation and Sidebar

/**
 * Sidebar Toggle for Mobile layout.
 */
function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    const backdrop = document.getElementById('sidebarBackdrop');
    
    sidebar.classList.toggle('active');
    backdrop.classList.toggle('show');
}

//#endregion


//#region Period Filter

/**
 * Global Filter Period Helper to reload window with query dates.
 */
function filterPeriod() {
    const sdInput = document.getElementById('startDate');
    const edInput = document.getElementById('endDate');

    if (sdInput && edInput) {
        const sd = sdInput.value;
        const ed = edInput.value;
        const url = new URL(window.location.href);

        url.searchParams.set('startDate', sd);
        url.searchParams.set('endDate', ed);

        window.location.href = url.pathname + url.search;
    }
}

//#endregion


//#region Statement Types Accounts

/**
 * Statement Types Open/Configure Modal Helper.
 * @param {number} id - Account statement type ID.
 * @param {string} name - Account statement type name.
 */
function openStatementTypeModal(id = 0, name = '') {
    const modalEl = document.getElementById('modalNewStatementType');

    if (!modalEl) return;
    
    const titleEl = modalEl.querySelector('.modal-title');
    const inputName = document.getElementById('inputStatementTypeName');
    const inputId = document.getElementById('inputStatementTypeId');
    const submitBtn = modalEl.querySelector('button[type="submit"]');

    if (id === 0) {
        titleEl.innerHTML = '<i class="bi bi-plus-circle text-success"></i> Cadastrar Novo Tipo de Extrato';

        if (inputName) inputName.value = '';
        if (inputId) inputId.value = '0';
        if (submitBtn) submitBtn.innerHTML = '<i class="bi bi-check-lg me-1"></i> Criar Extrato';
    } else {
        titleEl.innerHTML = '<i class="bi bi-pencil-square text-success"></i> Editar Conta / Extrato';

        if (inputName) inputName.value = name;
        if (inputId) inputId.value = id;
        if (submitBtn) submitBtn.innerHTML = '<i class="bi bi-check-lg me-1"></i> Salvar Alterações';
    }

    let modal = bootstrap.Modal.getInstance(modalEl);
    
    if (!modal) 
        modal = new bootstrap.Modal(modalEl);

    modal.show();
}

/**
 * Global Custom Statement Deletion Helper.
 * @param {Event} event - Mouse click event.
 * @param {number} id - Statement type ID to delete.
 * @param {string} name - Name of the statement type account.
 */
async function deleteCustomStatementType(event, id, name) {
    event.preventDefault();
    event.stopPropagation();
    
    const confirmed = await messageBox(`Deseja realmente excluir a conta/extrato "${name}"?`, true, "Excluir Extrato");

    if (confirmed) {
        const result = await BaseFetch.delete(`/StatementTypes/Delete/${id}`);

        if (result.success) {
            showToast(result.message || "Extrato excluído com sucesso.");
            
            const urlParams = new URLSearchParams(window.location.search);
            const currentId = urlParams.get('statementTypeId');

            if (currentId === id.toString()) {
                setTimeout(() => {
                    window.location.href = '/Summary/Index';
                }, 1000);
            } else {
                setTimeout(() => window.location.reload(), 1000);
            }
        } else {
            await messageBox(result.message || "Não foi possível excluir o extrato.", false, "Erro");
        }
    }
}

//#endregion


//#region Notification Toast

/**
 * Global Toast Notification Helper.
 * @param {string} msg - Message body context.
 * @param {string} type - Notification status class type ('success' or 'warning').
 */
function showToast(msg, type = 'success') {
    const toastEl = document.getElementById('actionToast');
    const toastMsg = document.getElementById('toastMessage');
    const icon = type === 'success' ? 'bi-check-circle-fill text-success' : 'bi-exclamation-triangle-fill text-warning';
    
    if (toastEl && toastMsg) {
        toastMsg.innerHTML = `<i class="bi ${icon} fs-5"></i> ${msg}`;

        const toast = new bootstrap.Toast(toastEl);

        toast.show();
    }
}

//#endregion


//#region Event Listeners

document.addEventListener('DOMContentLoaded', function() {
    const sidebarLinks = document.querySelectorAll('.sidebar-link');

    sidebarLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            const href = this.getAttribute('href');

            if (!href || href.startsWith('#') || href.startsWith('javascript:')) return;
            
            const sdInput = document.getElementById('startDate');
            const edInput = document.getElementById('endDate');
    
            if (sdInput && edInput) {
                e.preventDefault();

                const sd = sdInput.value;
                const ed = edInput.value;
                
                const url = new URL(href, window.location.origin);

                url.searchParams.set('startDate', sd);
                url.searchParams.set('endDate', ed);
                
                window.location.href = url.pathname + url.search;
            }
        });
    });

    const formNewStatementType = document.getElementById('formNewStatementType');

    if (formNewStatementType) {
        formNewStatementType.addEventListener('submit', async function(e) {
            e.preventDefault();

            const inputName = document.getElementById('inputStatementTypeName');
            const inputId = document.getElementById('inputStatementTypeId');
            const name = inputName ? inputName.value.trim() : '';
            const id = inputId ? parseInt(inputId.value || '0', 10) : 0;
            
            const modalEl = document.getElementById('modalNewStatementType');
            const modal = bootstrap.Modal.getInstance(modalEl);

            if (modal) modal.hide();

            const result = await BaseFetch.post('/StatementTypes/Save', { id: id, name: name });

            if (result.success) {
                showToast(result.message || "Ação realizada com sucesso!");

                if (inputName) inputName.value = '';
                if (inputId) inputId.value = '0';
                
                const urlParams = new URLSearchParams(window.location.search);
                const currentId = urlParams.get('statementTypeId');

                if (id > 0 && currentId === id.toString()) {
                    setTimeout(() => {
                        window.location.href = `/Transactions/Index?statementType=${encodeURIComponent(name)}&statementTypeId=${id}`;
                    }, 1000);
                } else {
                    setTimeout(() => window.location.reload(), 1000);
                }
            } else {
                await messageBox(result.message || "Não foi possível realizar a ação.", false, "Erro");

                if (modal) modal.show();
            }
        });
    }
});

//#endregion
/**
 * Binds the delete buttons rendered by the sidebar view component.
 * The identifier and name travel in data attributes, which Razor encodes,
 * instead of being interpolated into an inline onclick handler.
 */
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.delete-statement-btn').forEach(button => {
        button.addEventListener('click', event => {
            const id = parseInt(button.dataset.statementId, 10);
            const name = button.dataset.statementName || '';

            deleteCustomStatementType(event, id, name);
        });
    });
});
