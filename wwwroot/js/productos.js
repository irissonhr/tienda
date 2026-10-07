async function cargarProductos() {
    const grid = document.getElementById('productosGrid');
    if (!grid) return;

    try {
        const productos = await apiFetch('/productos');
        grid.innerHTML = productos.map(p => `
            <div class="producto-card">
                <div class="producto-img">${p.emoji}</div>
                <div class="producto-info">
                    <h3>${p.nombre}</h3>
                    <p>${p.descripcion}</p>
                    <div class="producto-precio">$${Number(p.precio).toFixed(2)}</div>
                    <button class="btn-agregar" onclick="agregarAlCarrito(${p.id}, '${p.nombre}', ${p.precio}, '${p.emoji}')">
                        🛒 Agregar
                    </button>
                </div>
            </div>
        `).join('');
    } catch (ex) {
        grid.innerHTML = `<p style="color:red">Error: ${ex.message}</p>`;
    }
}

document.addEventListener('DOMContentLoaded', cargarProductos);
