document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.pin-input').forEach(input => {
        input.addEventListener('input', () => {
            input.value = input.value.replace(/\D/g, '').slice(0, 6);
        });
    });
});
