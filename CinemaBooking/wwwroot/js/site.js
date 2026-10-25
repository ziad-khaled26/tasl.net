document.querySelectorAll('[data-print]').forEach(button => button.addEventListener('click', () => window.print()));
const bookingForm = document.querySelector('[data-booking-form]');
if (bookingForm) {
    const quantity = bookingForm.querySelector('[name="Quantity"]');
    const total = bookingForm.querySelector('[data-booking-total]');
    const price = Number(bookingForm.dataset.price);
    const update = () => {
        const value = Number(quantity.value);
        total.textContent = Number.isInteger(value) && value >= 1 && value <= 10 ? `${(value * price).toLocaleString('en-GB', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} EGP` : 'Choose 1-10 tickets';
    };
    quantity.addEventListener('input', update);
    update();
}
const currentPath = window.location.pathname.toLowerCase().replace(/\/$/, '');
document.querySelectorAll('nav a.nav-link').forEach(link => {
    const path = new URL(link.href).pathname.toLowerCase().replace(/\/$/, '');
    if (path === currentPath || path && path !== '/admin' && currentPath.startsWith(path + '/') || path === '/admin' && currentPath.startsWith('/admin/dashboard')) {
        link.classList.add('active');
        link.setAttribute('aria-current', 'page');
    }
});
