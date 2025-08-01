const modal = document.getElementById('modal-statement-details');
const title = document.getElementById('modal-statement-details-title');
const hiddenId = document.getElementById('modal-statement-details-id');
const btnSave = document.getElementById('modal-statement-details-btn-save');
const movementDateInput = document.getElementById('statement-details-movement-date');
const dueDateInput = document.getElementById('statement-details-due-date');
const historyInput = document.getElementById('statement-details-history');
const categorySelect = document.getElementById('statement-details-category');
const valueInput = document.getElementById('statement-details-value');

btnSave?.addEventListener('click', saveStatementDetail);

function openModalStatementDetails(row) {
    if (!row) {
        title.textContent = 'Adicionar linha ao Extrato';

        hiddenId.value = '';
        movementDateInput.value = '';
        dueDateInput.value = '';
        historyInput.value = '';
        categorySelect.value = '';
        valueInput.value = '';
    } else {
        title.textContent = 'Editar linha do Extrato';

        hiddenId.value = row.getAttribute('data-statement-id') || '';

        const cells = row.querySelectorAll('div');
       
        movementDateInput.value = cells[0]?.textContent.trim().split('/').reverse().join('-') || '';      
        dueDateInput.value = cells[1]?.textContent.trim().split('/').reverse().join('-') || '';
        historyInput.value = cells[2]?.textContent.trim() || '';  
        valueInput.value = getValueImput(cells);
        
        setValueOnCategorySelect(cells);
    }

    modal.style.display = 'flex';
}

function getValueImput(cells) {
    const rawAmount = cells[4]?.textContent.replace(/[^\d,-]/g, '').replace(',', '.').trim() || '';

    return parseFloat(rawAmount) || '';
}

function setValueOnCategorySelect(cells) {
    const categoryText = cells[3]?.textContent.trim();

    let matched = false;

    for (const option of categorySelect.options) {
        if (option.text.trim().toLowerCase() === categoryText.toLowerCase()) {
            categorySelect.value = option.value;

            matched = true;

            break;
        }
    }

    if (!matched) categorySelect.value = '';    
}

async function saveStatementDetail() {
    const idValue = hiddenId.value?.trim();
    const transactionDateValue = movementDateInput.value?.trim();
    const dueDateValue = dueDateInput.value?.trim();
    const amountValue = valueInput.value?.trim();
    const descriptionValue = historyInput.value?.trim();
    const entryIdValue = categorySelect.value?.trim();
    const statementTypeIdValue = document.getElementById('hidden-id')?.value?.trim();

    const dto = {
        id: idValue && !isNaN(parseInt(idValue)) ? parseInt(idValue) : null,
        transactionDate: transactionDateValue && !isNaN(Date.parse(transactionDateValue)) ? transactionDateValue : null,
        dueDate: dueDateValue && !isNaN(Date.parse(dueDateValue)) ? dueDateValue : null,
        amount: amountValue && !isNaN(parseFloat(amountValue)) ? parseFloat(amountValue) : null,
        description: descriptionValue || null,
        entryId: entryIdValue && !isNaN(parseInt(entryIdValue)) ? parseInt(entryIdValue) : null,
        statementTypeId: statementTypeIdValue && !isNaN(parseInt(statementTypeIdValue)) ? parseInt(statementTypeIdValue) : null
    };

    const result = await postFromBody('/Statements/save', dto);

    if (result.success) {
        location.reload();
    }
}