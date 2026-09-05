if (!isLoggedIn() || !isAdmin()) {
  window.location.href = '/products';
}

const addProductForm = document.getElementById('addProductForm');
const productMsg = document.getElementById('productMsg');
const adminProductList = document.getElementById('adminProductList');
const userList = document.getElementById('userList');
const assignRoleForm = document.getElementById('assignRoleForm');
const roleMsg = document.getElementById('roleMsg');

addProductForm.addEventListener('submit', async (e) => {
  e.preventDefault();
  const body = {
    name: document.getElementById('name').value,
    description: document.getElementById('description').value,
    price: parseFloat(document.getElementById('price').value),
    stock: parseInt(document.getElementById('stock').value, 10)
  };

  const res = await apiFetch('/products', { method: 'POST', body: JSON.stringify(body) });
  if (!res) return;

  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    productMsg.textContent = err.error || 'Failed to add product.';
  } else {
    productMsg.textContent = 'Product added!';
    addProductForm.reset();
    loadAdminProducts();
  }
  productMsg.classList.remove('hidden');
});

async function loadAdminProducts() {
  const res = await fetch(`${API_BASE}/products`);
  const products = await res.json();
  adminProductList.innerHTML = `
    <table class="cart-table">
      <thead><tr><th>Name</th><th>Price</th><th>Stock</th><th></th></tr></thead>
      <tbody>
        ${products.map(p => `
          <tr>
            <td>${escapeHtml(p.name)}</td>
            <td>$${p.price.toFixed(2)}</td>
            <td>${p.stock}</td>
            <td><button onclick="deleteProduct(${p.id})">Delete</button></td>
          </tr>
        `).join('')}
      </tbody>
    </table>
  `;
}

async function deleteProduct(id) {
  const res = await apiFetch(`/products/${id}`, { method: 'DELETE' });
  if (res && res.ok) loadAdminProducts();
}

async function loadUsers() {
  const res = await apiFetch('/user');
  if (!res || !res.ok) return;
  const users = await res.json();
  userList.innerHTML = `
    <table class="cart-table">
      <thead><tr><th>Id</th><th>Username</th><th>Email</th></tr></thead>
      <tbody>
        ${users.map(u => `<tr><td>${escapeHtml(u.id)}</td><td>${escapeHtml(u.userName)}</td><td>${escapeHtml(u.email)}</td></tr>`).join('')}
      </tbody>
    </table>
  `;
}

assignRoleForm.addEventListener('submit', async (e) => {
  e.preventDefault();
  const body = {
    userId: document.getElementById('userId').value,
    roleName: document.getElementById('roleName').value
  };
  const res = await apiFetch('/role/assign', { method: 'POST', body: JSON.stringify(body) });
  if (!res) return;

  const data = await res.json().catch(() => ({}));
  roleMsg.textContent = res.ok ? 'Role assigned!' : (data.error || 'Failed to assign role.');
  roleMsg.classList.remove('hidden');
});

loadAdminProducts();
loadUsers();
