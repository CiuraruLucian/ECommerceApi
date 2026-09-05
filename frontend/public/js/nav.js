document.addEventListener('DOMContentLoaded', () => {
  const authArea = document.getElementById('authArea');
  const adminLink = document.getElementById('adminLink');

  if (isLoggedIn()) {
    const payload = getTokenPayload();
    const name = payload?.name
      || payload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name']
      || 'Account';
    authArea.innerHTML = `<span class="username">${escapeHtml(name)}</span> <button onclick="logout()">Logout</button>`;
    if (isAdmin() && adminLink) adminLink.classList.remove('hidden');
  } else {
    authArea.innerHTML = `<a href="/login">Login</a> <a href="/register">Register</a>`;
  }
});
