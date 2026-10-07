const productoForm = document.getElementById('productoForm');
const productosAdminList = document.getElementById('productosAdminList');
const productoError = document.getElementById('productoError');
const cancelarEdicionBtn = document.getElementById('cancelarEdicion');

async function cargarProductosAdmin() {
    try {
        const productos = await apiFetch('/productos');
        if (!productosAdminList) return;

        productosAdminList.innerHTML = productos.map(producto => `
            <div class="producto-card">
                <div class="producto-img">${producto.emoji || '📦'}</div>
                <div class="producto-info">
                    <h3>${producto.nombre}</h3>
                    <p>${producto.descripcion}</p>
                    <div class="producto-precio">$${Number(producto.precio).toFixed(2)}</div>
                    <div style="display:flex; gap:10px;">
                        <button class="btn-agregar" onclick="editarProducto(${producto.id})">Editar</button>
                        <button class="btn-logout" onclick="eliminarProducto(${producto.id})">Eliminar</button>
                    </div>
                </div>
            </div>
        `).join('');
    } catch (error) {
        if (productoError) {
            productoError.textContent = error.message;
        }
    }
}

function resetFormularioProducto() {
    document.getElementById('productoId').value = '';
    document.getElementById('productoNombre').value = '';
    document.getElementById('productoDescripcion').value = '';
    document.getElementById('productoPrecio').value = '';
    document.getElementById('productoEmoji').value = '📦';
    cancelarEdicionBtn.style.display = 'none';
    productoForm.querySelector('button[type="submit"]').textContent = 'Guardar producto';
}

async function editarProducto(id) {
    try {
        const producto = await apiFetch(`/productos/${id}`);
        document.getElementById('productoId').value = producto.id;
        document.getElementById('productoNombre').value = producto.nombre;
        document.getElementById('productoDescripcion').value = producto.descripcion;
        document.getElementById('productoPrecio').value = producto.precio;
        document.getElementById('productoEmoji').value = producto.emoji || '📦';
        cancelarEdicionBtn.style.display = 'inline-block';
        productoForm.querySelector('button[type="submit"]').textContent = 'Actualizar producto';
        window.scrollTo({ top: 0, behavior: 'smooth' });
    } catch (error) {
        if (productoError) {
            productoError.textContent = error.message;
        }
    }
}

async function eliminarProducto(id) {
    if (!confirm('¿Deseas eliminar este producto?')) return;

    try {
        await apiFetch(`/productos/${id}`, { method: 'DELETE' });
        await cargarProductosAdmin();
    } catch (error) {
        if (productoError) {
            productoError.textContent = error.message;
        }
    }
}

if (productoForm) {
    productoForm.addEventListener('submit', async (event) => {
        event.preventDefault();

        const id = document.getElementById('productoId').value;
        const payload = {
            nombre: document.getElementById('productoNombre').value.trim(),
            descripcion: document.getElementById('productoDescripcion').value.trim(),
            precio: Number(document.getElementById('productoPrecio').value),
            emoji: document.getElementById('productoEmoji').value.trim() || '📦'
        };

        if (!payload.nombre || !payload.descripcion || Number.isNaN(payload.precio) || payload.precio < 0) {
            productoError.textContent = 'Completa los campos correctamente.';
            return;
        }

        try {
            if (id) {
                await apiFetch(`/productos/${id}`, {
                    method: 'PUT',
                    body: JSON.stringify(payload)
                });
            } else {
                await apiFetch('/productos', {
                    method: 'POST',
                    body: JSON.stringify(payload)
                });
            }

            resetFormularioProducto();
            productoError.textContent = '';
            await cargarProductosAdmin();
        } catch (error) {
            productoError.textContent = error.message;
        }
    });
}

if (cancelarEdicionBtn) {
    cancelarEdicionBtn.addEventListener('click', resetFormularioProducto);
}

document.addEventListener('DOMContentLoaded', () => {
    const usuario = JSON.parse(localStorage.getItem('usuario') || 'null');
    if (!usuario || !usuario.esAdmin) {
        window.location.href = 'login.html';
        return;
    }

    cargarProductosAdmin();
});
