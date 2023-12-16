document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('form-id');
    const submitButton = document.getElementById('submit-id');

    form.addEventListener('input', function () {
        const isValid = Array.from(form.elements).every(element => element.checkValidity());

        if (isValid) {
            submitButton.classList.add('active');
            submitButton.removeAttribute('disabled');
        } else {
            submitButton.classList.remove('active');
            submitButton.setAttribute('disabled', 'true');
        }
    });
});