document.addEventListener('DOMContentLoaded', () => {
    const buttons = document.querySelectorAll('.header-btn');
    const path = window.location.pathname.toLowerCase();
    const refreshBtn = document.getElementById('btnRefresh');
    const btnEditCategoryStatementType = document.getElementById('btnEdit');
    const btnDeleteCategoryStatementType = document.getElementById('btnDelete');
    const btnAddCategoryStatementType = document.getElementById('btn-add-statement-type');    
    const transactionGridBody = document.getElementById('dgv-transactions-body');
    const modalStatementTypeId = document.getElementById('modal-statement-type-id')
    const btnAddStatementDetail = document.getElementById('btn-add-row');
    const btnEditStatementDetail = document.getElementById('btn-edit-row');
    const btnDeleteStatementDetail = document.getElementById('btn-delete-row');
    const btnUploadExcel = document.getElementById('btn-excel');
    const btnTagCategories = document.getElementById('btn-tag-categories');
    const modalStatementDetails = document.getElementById('modal-statement-details');
    const btnCloseStatementDetails = document.getElementById('modal-statement-details-close');  
    const tabTitle = document.getElementById('titleLabel').innerText;

    const getDateParam = (inputId) => {
        const input = document.getElementById(inputId);

        return input && input.value ? input.value : null;
    };

    buttons.forEach(button => {
        const controller = button.getAttribute('data-controller')?.toLowerCase();
        const action = button.getAttribute('data-action')?.toLowerCase();
        const id = button.getAttribute('data-id');

        let expectedPath = `/${controller}/${action}`;

        if (id !== null) {
            expectedPath += `/${id}`;
        }

        if (path === expectedPath) {
            button.classList.add('nav-selected');
        }

        button.addEventListener('click', () => {
            const startDate = getDateParam('datePickerStartDate');
            const endDate = getDateParam('datePickerEndDate');

            let url = `/${controller}/${action}`;

            if (id !== null) {
                url += `/${id}`;
            }

            const queryParams = [];

            if (startDate) queryParams.push(`startDate=${startDate}`);
            if (endDate) queryParams.push(`endDate=${endDate}`);

            if (queryParams.length > 0) {
                url += `?${queryParams.join('&')}`;
            }

            window.location.href = url;
        });
    });
        
    refreshBtn.addEventListener('click', async () => {
        const selectedTab = document.querySelector('.header-btn.nav-selected');

        if (selectedTab) {
            selectedTab.click();
        }
    });

    btnAddCategoryStatementType.addEventListener('click', async () => {
        const resposta = await showMessageBox("Deseja criar um novo tipo de extrato?", true);

        if (resposta) {
            openModalStatementType("Criar Tipo de Extrato", "", "");            
        }
    });

    btnEditCategoryStatementType?.addEventListener('click', async () => {   
        const id = getStatementTypeId();
        const resposta = await showMessageBox("Deseja editar o nome do tipo de extrato atual?", true);

        if (resposta) {
            openModalStatementType("Editar Tipo de Extrato", tabTitle, id);
        }
    });

    btnDeleteCategoryStatementType?.addEventListener('click', async () => {
        const id = getStatementTypeId();
        const resposta = await showMessageBox("Deseja apagar o tipo de extrato atual?", true);

        if (!resposta) return;

        const result = await deleteFromUrl(`/StatementTypes/Delete/${id}`);

        if (result.isSuccess()) {
            window.location.href = replaceIdInUrl(0);
        }
    });

    btnAddStatementDetail?.addEventListener('click', async () => {
        openModalStatementDetails(null);
    });

    btnEditStatementDetail?.addEventListener('click', async () => {
        const row = await getGridViewRow();

        if (row) openModalStatementDetails(row);
    });

    btnDeleteStatementDetail?.addEventListener('click', async () => {
        const row = await getGridViewRow();

        if (!row) return;

        const id = row.getAttribute('data-statement-id');

        if (!id || isNaN(parseInt(id))) return;

        const confirm = await showMessageBox('Tem certeza que deseja excluir o registro selecionado?', true);

        if (!confirm) return;

        const result = await deleteFromUrl(`/Statements/Delete/${id}`);

        if (result.success) {
            location.reload();
        }
    });

    transactionGridBody?.addEventListener('dblclick', (event) => {
        const row = event.target.closest('.grid-row');

        if (row) {
            document.querySelectorAll('.grid-row.transactions-grid-selected')
                .forEach(r => r.classList.remove('transactions-grid-selected'));

            row.classList.add('transactions-grid-selected');

            btnEditStatementDetail?.click();
        }
    });

    btnCloseStatementDetails?.addEventListener('click', () => {
        modalStatementDetails.style.display = 'none';
    });

    btnUploadExcel?.addEventListener('click', async () => {
        const message =
            "A planilha deve ter o seguinte formato:\n\n" +
            "- Coluna A: Data da Transação\n" +
            "- Coluna B: Data de Vencimento\n" +
            "- Coluna C: Descrição (até 255 caracteres)\n" +
            "- Coluna D: Valor (decimal)\n\n" +
            "⚠ A primeira linha (cabeçalho) será ignorada automaticamente.\n" +
            "📄 Uma planilha modelo está disponível na solução do projeto para referência.\n\n" +
            "Certifique-se de que todas as células estão preenchidas corretamente antes de importar.";

        await showMessageBox(message, false);

        const fileInput = document.getElementById('excelFileInput');

        fileInput.value = '';

        fileInput.click();

        fileInput.onchange = async () => {
            const file = fileInput.files[0];
            const statementTypeId = document.getElementById('hidden-id')?.value?.trim();

            if (!file || !statementTypeId) {
                await showMessageBox("Arquivo ou ID do tipo de lançamento inválido.", false);

                return;
            }

            const result = await uploadExcelStatement(file, statementTypeId);

            if (result.success) {
                location.reload();
            }
        };
    });

    btnTagCategories?.addEventListener('click', async () => {
        const resposta = await showMessageBox(
            "Deseja reidentificar as categorias dos lançamentos que ainda não foram reconhecidos automaticamente no período selecionado?",
            true
        );

        if (!resposta) return;

        const startDateInput = document.getElementById('datePickerStartDate');
        const endDateInput = document.getElementById('datePickerEndDate');

        const startDate = startDateInput?.value || null;
        const endDate = endDateInput?.value || null;

        const dto = {
            startDate: startDate ? new Date(startDate).toISOString() : null,
            endDate: endDate ? new Date(endDate).toISOString() : null
        };

        const result = await postFromBody('/Statements/reidentify', dto);

        if (result.success) {
            location.reload();
        }
    });
                        
    if (transactionGridBody) {
        transactionGridBody.addEventListener('click', (event) => {
            const clickedRow = event.target.closest('.grid-row');
            const allRows = transactionGridBody.querySelectorAll('.grid-row');

            if (!clickedRow) return;            

            allRows.forEach(row => row.classList.remove('transactions-grid-selected'));

            clickedRow.classList.add('transactions-grid-selected');
        });
    }   

    function openModalStatementType(titleContent, inputValue, id) {
        const modal = document.getElementById('modal-statement-type');
        const title = document.getElementById('modal-statement-type-title');
        const input = document.getElementById('modal-statement-type-input');
        const closeBtn = document.getElementById('modal-statement-type-close');

        title.textContent = titleContent;

        input.value = inputValue;
        modalStatementTypeId.value = id;

        modal.style.display = 'flex';

        closeBtn.onclick = () => {
            modal.style.display = 'none';
        };
    }

    function getStatementTypeId() {
        return document.getElementById('hidden-id')?.value ?? "";
    }

    async function getGridViewRow() {
        const selectedRow = document.querySelector('.grid-row.transactions-grid-selected');

        if (!selectedRow) {
            await showMessageBox('Selecione um registro válido!', false);
            
            return null;
        }

        return selectedRow;
    }    
});

document.addEventListener('keydown', (event) => {
    const transactionGridBody = document.getElementById('dgv-transactions-body');

    if (!transactionGridBody) return;

    const rows = Array.from(transactionGridBody.querySelectorAll('.grid-row'));
    const selectedIndex = rows.findIndex(row => row.classList.contains('transactions-grid-selected'));

    if (selectedIndex === -1) return;

    let newIndex = selectedIndex;

    if (event.key === 'ArrowUp') {
        newIndex = Math.max(0, selectedIndex - 1);
    } else if (event.key === 'ArrowDown') {
        newIndex = Math.min(rows.length - 1, selectedIndex + 1);
    } else {
        return;
    }

    event.preventDefault();

    rows[selectedIndex].classList.remove('transactions-grid-selected');
    rows[newIndex].classList.add('transactions-grid-selected');

    rows[newIndex].scrollIntoView({ block: 'nearest', behavior: 'smooth' });
});

function replaceIdInUrl(newId) {
    const url = new URL(window.location.href);

    let segments = url.pathname.split('/').filter(s => s.length > 0);
    let index = -1;

    for (let i = segments.length - 1; i >= 0; i--) {
        if (!isNaN(parseInt(segments[i], 10))) {
            index = i;

            break;
        }
    }

    if (index !== -1) {
        segments[index] = String(newId);
    } else {
        segments.push(String(newId));
    }

    url.pathname = '/' + segments.join('/');

    return url.toString();
}