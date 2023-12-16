function formatCreditCardNumber() {
    let creditCardInput = document.getElementById('creditCard');
    let value = creditCardInput.value.replace(/\D/g, '');
    let formattedValue = value.replace(/(\d{4})(?=\d{4})/g, '$1 ');
    formattedValue = formattedValue.slice(0, 19);
    creditCardInput.value = formattedValue;
}
function CVV() {
    let creditCardInput = document.getElementById('cvv');
    let value = creditCardInput.value.replace(/\D/g, '');
    creditCardInput.value = value;
}

function formatDate() {
    let input = document.getElementById('expirationDate');
    if (!input) {
        console.error('Input element bulunamadı!');
        return;
    }

    // Sadece sayı girişine izin ver
    input.value = input.value.replace(/[^\d]/g, '');

    let trimmed = input.value.replace(/\W/g, '');
    if (trimmed.length > 2) {
        // İlk iki rakamın ardından otomatik olarak "/" ekle
        input.value = trimmed.replace(/(\d{2})(\d{0,4})/, '$1/$2');
    }
}
