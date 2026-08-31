document.addEventListener('DOMContentLoaded', () => {
    const inputs = document.querySelectorAll('.pin-input');

    // The fields are plain text fields masked through -webkit-text-security,
    // so that the browser does not treat them as a password and offer to save
    // the PIN. Where that property is unsupported the text would show in the
    // clear, so those browsers get a password field back.
    const maskedByCss = CSS.supports('-webkit-text-security', 'disc');

    inputs.forEach(input => {
        if (!maskedByCss) {
            input.type = 'password';
        }

        input.addEventListener('input', () => {
            input.value = input.value.replace(/\D/g, '').slice(0, 6);
        });
    });
});
