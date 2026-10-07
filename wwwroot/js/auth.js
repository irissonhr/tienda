const registroForm = document.getElementById('registroForm');
if (registroForm) {
    registroForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const err = document.getElementById('regError');

        try {
            await apiFetch('/auth/registro', {
                method: 'POST',
                body: JSON.stringify({
                    nombre: document.getElementById('regNombre').value,
                    email: document.getElementById('regEmail').value,
                    password: document.getElementById('regPassword').value
                })
            });

            err.style.color = '#27ae60';
            err.textContent = '✅ Registro exitoso. Redirigiendo...';
            setTimeout(() => window.location.href = 'login.html', 1200);
        } catch (ex) {
            err.style.color = '#e74c3c';
            err.textContent = '❌ ' + ex.message;
        }
    });
}

const loginForm = document.getElementById('loginForm');
if (loginForm) {
    loginForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const err = document.getElementById('loginError');

        try {
            const data = await apiFetch('/auth/login', {
                method: 'POST',
                body: JSON.stringify({
                    email: document.getElementById('loginEmail').value,
                    password: document.getElementById('loginPassword').value
                })
            });

            localStorage.setItem('token', data.token);
            localStorage.setItem('usuario', JSON.stringify({
                nombre: data.nombre,
                email: data.email,
                rol: data.rol,
                esAdmin: data.esAdmin
            }));
            window.location.href = 'index.html';
        } catch (ex) {
            err.style.color = '#e74c3c';
            err.textContent = '❌ ' + ex.message;
        }
    });
}

const pagina = window.location.pathname.split('/').pop();
const token = localStorage.getItem('token');
let usuarioActual = null;
try {
    usuarioActual = JSON.parse(localStorage.getItem('usuario') || 'null');
} catch (error) {
    localStorage.removeItem('usuario');
}

if (pagina === 'index.html' || pagina === '' || pagina === 'carrito.html') {
    if (!token) {
        window.location.replace('login.html');
    }
} else if (pagina === 'admin.html') {
    if (!token || !usuarioActual || !(usuarioActual.esAdmin || usuarioActual.rol === 'Admin')) {
        window.location.href = 'login.html';
    }
}

document.addEventListener('DOMContentLoaded', () => {
    const usuario = usuarioActual;
    const userInfo = document.getElementById('userInfo');
    const logoutBtn = document.getElementById('logoutBtn');
    const adminLink = document.getElementById('adminLink');
    const loginLink = document.getElementById('loginLink');
    const registerLink = document.getElementById('registerLink');

    if (userInfo && usuario) userInfo.textContent = `👤 ${usuario.nombre}`;
    if (logoutBtn) logoutBtn.style.display = usuario ? 'inline-block' : 'none';
    if (loginLink) loginLink.style.display = usuario ? 'none' : 'inline';
    if (registerLink) registerLink.style.display = usuario ? 'none' : 'inline';
    if (adminLink && usuario && (usuario.esAdmin || usuario.rol === 'Admin')) {
        adminLink.style.display = 'inline';
    }
    if (logoutBtn) {
        logoutBtn.addEventListener('click', () => {
            localStorage.removeItem('token');
            localStorage.removeItem('usuario');
            window.location.href = 'login.html';
        });
    }
});
