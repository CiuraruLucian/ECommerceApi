const loginForm = document.getElementById('loginForm');
const registerForm = document.getElementById('registerForm');

if (loginForm) {
  loginForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    const errorMsg = document.getElementById('errorMsg');
    errorMsg.classList.add('hidden');

    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;
    const res = await fetch(`${API_BASE}/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password })
    });

    if(res.status === 429) {
      errorMsg.textContent = 'Too many login attempts. Please try again later.';
      errorMsg.classList.remove('hidden');
      return;
    }

    if (!res.ok) {
      errorMsg.textContent = 'Invalid username or password.';
      errorMsg.classList.remove('hidden');
      return;
    }

    const data = await res.json();
    setToken(data.token);
    window.location.href = '/products';
  });
}

if (registerForm) {
  registerForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    const errorMsg = document.getElementById('errorMsg');
    const successMsg = document.getElementById('successMsg');
    errorMsg.classList.add('hidden');
    successMsg.classList.add('hidden');

    const email = document.getElementById('email').value;
    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;

    const res = await fetch(`${API_BASE}/auth/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, username, password })
    });
    
    if(res.status === 429) {
      errorMsg.textContent = 'Too many login attempts. Please try again later.';
      errorMsg.classList.remove('hidden');
      return;
    }

    if (!res.ok) {
      const err = await res.json().catch(() => ({}));
      errorMsg.textContent = err.error || 'Registration failed. Check your details.';
      errorMsg.classList.remove('hidden');
      return;
    }

    successMsg.textContent = 'Account created! Redirecting to login...';
    successMsg.classList.remove('hidden');
    setTimeout(() => (window.location.href = '/login'), 1200);
  });
}
