//#region MessageBox

/**
 * Global Custom MessageBox Helper
 * @param {string} message - Message body content.
 * @param {boolean} isYesNo - Flag to render Yes/No buttons instead of standard OK button.
 * @param {string} title - The header title text.
 * @returns {Promise<boolean>} Resolves to true if OK/Yes clicked, false if Cancel/No/Closed.
 */
function messageBox(message, isYesNo = false, title = "Aviso") {
    return new Promise((resolve) => {
        const modalEl = document.getElementById('customMessageBoxModal');
        const titleEl = document.getElementById('customMessageBoxTitle');
        const messageEl = document.getElementById('customMessageBoxMessage');
        const yesBtn = document.getElementById('customMessageBoxYesBtn');
        const noBtn = document.getElementById('customMessageBoxNoBtn');
        const okBtn = document.getElementById('customMessageBoxOkBtn');
        const closeBtn = document.getElementById('customMessageBoxCloseBtn');

        titleEl.textContent = title;
        messageEl.textContent = message;

        let result = false; 

        const modal = new bootstrap.Modal(modalEl, {
            backdrop: 'static',
            keyboard: false
        });

        if (isYesNo) {
            yesBtn.classList.remove('d-none');
            noBtn.classList.remove('d-none');
            okBtn.classList.add('d-none');
        } else {
            yesBtn.classList.add('d-none');
            noBtn.classList.add('d-none');
            okBtn.classList.remove('d-none');
        }

        const onYes = () => {
            result = true;
            modal.hide();
        };

        const onNo = () => {
            result = false;
            modal.hide();
        };

        const onOk = () => {
            result = true;
            modal.hide();
        };

        const newYesBtn = yesBtn.cloneNode(true);
        const newNoBtn = noBtn.cloneNode(true);
        const newOkBtn = okBtn.cloneNode(true);
        const newCloseBtn = closeBtn.cloneNode(true);

        yesBtn.parentNode.replaceChild(newYesBtn, yesBtn);
        noBtn.parentNode.replaceChild(newNoBtn, noBtn);
        okBtn.parentNode.replaceChild(newOkBtn, okBtn);
        closeBtn.parentNode.replaceChild(newCloseBtn, closeBtn);

        newYesBtn.addEventListener('click', onYes);
        newNoBtn.addEventListener('click', onNo);
        newOkBtn.addEventListener('click', onOk);
        newCloseBtn.addEventListener('click', () => {
            result = false;
            modal.hide();
        });

        const onHidden = () => {
            modalEl.removeEventListener('hidden.bs.modal', onHidden);

            resolve(result);
        };

        modalEl.addEventListener('hidden.bs.modal', onHidden);

        modal.show();
    });
}

//#endregion