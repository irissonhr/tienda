const API_URL = `${window.location.origin}/api`;

async function apiFetch(endpoint, options = {}) {
    const token = localStorage.getItem("token");
    const headers = {
        "Content-Type": "application/json",
        ...(options.headers || {})
    };

    if (token) {
        headers["Authorization"] = `Bearer ${token}`;
    }

    const res = await fetch(`${API_URL}${endpoint}`, { ...options, headers });
    const text = await res.text();
    const data = text ? JSON.parse(text) : {};

    if (res.status === 401 && token) {
        localStorage.removeItem("token");
        localStorage.removeItem("usuario");

        if (window.location.pathname.split('/').pop() !== "login.html") {
            window.location.replace("login.html");
        }
    }

    if (!res.ok) throw new Error(data.mensaje || "Error en la petición");
    return data;
}
