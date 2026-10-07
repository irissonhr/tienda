function agregarAlCarrito(id, nombre, precio, emoji) {
    let carrito = JSON.parse(localStorage.getItem('carrito') || '[]');
    const existente = carrito.find(i => i.id === id);

    if (existente) {
        existente.cantidad++;
    } else {
        carrito.push({ id, nombre, precio, emoji, cantidad: 1 });
    }

    localStorage.setItem('carrito', JSON.stringify(carrito));
    actualizarContador();
    mostrarNotificacion(`✅ ${nombre} agregado`);
}

function actualizarContador() {
    const carrito = JSON.parse(localStorage.getItem('carrito') || '[]');
    const total = carrito.reduce((s, i) => s + i.cantidad, 0);
    document.querySelectorAll('#cartCount').forEach(e => e.textContent = total);
}

function mostrarNotificacion(msg) {
    const n = document.createElement('div');
    n.textContent = msg;
    n.style.cssText = `position:fixed;bottom:30px;right:30px;background:#27ae60;color:#fff;
        padding:15px 25px;border-radius:8px;box-shadow:0 5px 20px rgba(0,0,0,.3);
        z-index:9999;font-weight:600;`;
    document.body.appendChild(n);
    setTimeout(() => n.remove(), 2500);
}

document.addEventListener('DOMContentLoaded', actualizarContador);
