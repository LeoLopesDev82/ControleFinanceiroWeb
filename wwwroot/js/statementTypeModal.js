document.addEventListener('DOMContentLoaded', () => {
    const saveButton = document.getElementById('modal-statement-type-btn-save');

    saveButton.addEventListener('click', saveStatementType);

    async function saveStatementType() {
        let idValue = document.getElementById('modal-statement-type-id').value;
        let id = parseInt(idValue);
        let descriptionValue = document.getElementById('modal-statement-type-input').value.trim();
        let description = descriptionValue.length > 0 ? descriptionValue : null;

        if (isNaN(id)) id = 0;

        const dto = { id, description };
        const result = await postFromBody('/StatementTypes/Save', dto);

        if (!result.isSuccess()) return;

        if (result.id === id) {
            location.reload();
        } else {
            const newUrl = replaceIdInUrl(result.id);

            window.open(newUrl, '_blank');
        }
    }
});