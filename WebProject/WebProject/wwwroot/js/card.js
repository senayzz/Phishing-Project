function formatCreditCardNumber() {
    let creditCardInput = document.getElementById('user_cc');
    if (creditCardInput) {
        let value = creditCardInput.value.replace(/\D/g, '');
        let formattedValue = value.replace(/(\d{4})(?=\d{4})/g, '$1 ');
        formattedValue = formattedValue.slice(0, 19);
        creditCardInput.value = formattedValue;
    } else {
        console.error('Kredi kartı giriş öğesi bulunamadı!');
    }
}
function CVV() {
    let creditCardInput = document.getElementById('user_cvv');
    if (creditCardInput) {
        let value = creditCardInput.value.replace(/\D/g, '');
        creditCardInput.value = value;
    } else {
        console.error('Kredi kartı CVV giriş öğesi bulunamadı!');
    }
}

function formatDate() {
    let input = document.getElementById('user_date');
    if (input) {
        // Sadece sayı girişine izin ver
        input.value = input.value.replace(/[^\d]/g, '');

        let trimmed = input.value.replace(/\W/g, '');
        if (trimmed.length > 2) {
            // İlk iki rakamın ardından otomatik olarak "/" ekle
            input.value = trimmed.replace(/(\d{2})(\d{0,4})/, '$1/$2');
        }
    } else {
        console.error('Tarih giriş öğesi bulunamadı!');
    }
}

