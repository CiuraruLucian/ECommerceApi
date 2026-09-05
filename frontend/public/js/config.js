// Change this if your API runs on a different port/host
const API_BASE = 'https://localhost:7224/api';

function getToken() {
  return localStorage.getItem('jwt_token');
}

function setToken(token) {
  localStorage.setItem('jwt_token', token);
}

function clearToken() {
  localStorage.removeItem('jwt_token');
}

function isLoggedIn() {
  return !!getToken();
}

// Decodes the JWT payload (no verification — just for reading claims client-side)
function getTokenPayload() {
  const token = getToken();
  if (!token) return null;
  try {
    const payload = token.split('.')[1];
    return JSON.parse(atob(payload.replace(/-/g, '+').replace(/_/g, '/')));
  } catch {
    return null;
  }
}

function getRoles() {
  const payload = getTokenPayload();
  if (!payload) return [];
  const roleClaim = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
    || payload['role'];
  if (!roleClaim) return [];
  return Array.isArray(roleClaim) ? roleClaim : [roleClaim];
}

function isAdmin() {
  return getRoles().includes('Admin');
}

// Wrapper around fetch that automatically attaches the Bearer token
async function apiFetch(path, options = {}) {
  const headers = options.headers ? { ...options.headers } : {};
  if (options.body && !headers['Content-Type']) headers['Content-Type'] = 'application/json';
  const token = getToken();
  if (token) headers['Authorization'] = `Bearer ${token}`;

  let res;
  try {
    res = await fetch(`${API_BASE}${path}`, { ...options, headers });
  } catch (error) {
    console.error('API request failed:', error);
    throw new Error(`Could not connect to the API at ${API_BASE}. Make sure the ASP.NET API is running and CORS is enabled.`);
  }

  if (res.status === 401) {
    clearToken();
    window.location.href = '/login';
    return null;
  }

  return res;
}

function logout() {
  clearToken();
  window.location.href = '/login';
}

// Escapes untrusted values before inserting them into innerHTML templates.
// Always wrap any product/user/order field that came from the API in this
// before interpolating it into an HTML string.
function escapeHtml(str) {
  if (str === null || str === undefined) return '';
  return String(str)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#039;');
}
