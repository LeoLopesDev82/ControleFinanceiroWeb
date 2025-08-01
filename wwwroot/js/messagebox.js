window.showMessageBox = function (message, yesNo) {
    return new Promise((resolve) => {
        const modal = document.getElementById('customMessageBox');
        const messageContent = document.getElementById('cfMessageContent');
        const btnYes = document.getElementById('cfBtnYes');
        const btnNo = document.getElementById('cfBtnNo');
        const btnOk = document.getElementById('cfBtnOk');

        messageContent.innerHTML = message.replace(/\n/g, '<br>');

        btnYes.style.display = yesNo ? 'inline-block' : 'none';
        btnNo.style.display = yesNo ? 'inline-block' : 'none';
        btnOk.style.display = yesNo ? 'none' : 'inline-block';

        modal.style.display = 'flex';

        const cleanup = () => {
            btnYes.removeEventListener('click', yesHandler);
            btnNo.removeEventListener('click', noHandler);
            btnOk.removeEventListener('click', okHandler);

            modal.style.display = 'none';
        };

        const yesHandler = () => {
            cleanup();
            resolve(true);
        };

        const noHandler = () => {
            cleanup();
            resolve(false);
        };

        const okHandler = () => {
            cleanup();
            resolve(true);
        };

        btnYes.addEventListener('click', yesHandler);
        btnNo.addEventListener('click', noHandler);
        btnOk.addEventListener('click', okHandler);
    });
};
