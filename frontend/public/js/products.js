const grid = document.getElementById('productGrid');
const statusMsg = document.getElementById('statusMsg');
const searchInput = document.getElementById('searchInput');

function renderProducts(products) {
  grid.innerHTML = '';
  if (!products.length) {
    grid.innerHTML = '<p>No products found.</p>';
    return;
  }
  products.forEach(p => {
    const card = document.createElement('div');
    card.className = 'product-card';
    card.innerHTML = `
      <h3>${escapeHtml(p.name)}</h3>
      <p>${escapeHtml(p.description || '')}</p>
      <p class="price">$${p.price.toFixed(2)}</p>
      <p class="stock">Stock: ${p.stock}</p>
      <button ${!isLoggedIn() ? 'disabled title="Login to add to cart"' : ''}
              onclick="addToCart(${p.id})">Add to cart</button>
    `;
    grid.appendChild(card);
  });
}

async function loadProducts() {
  const res = await fetch(`${API_BASE}/products`);
  const data = await res.json();
  renderProducts(data);
}

async function searchProducts() {
  const query = searchInput.value.trim();
  if (!query) return loadProducts();

  const res = await fetch(`${API_BASE}/products/search?query=${encodeURIComponent(query)}`);
  if (res.status === 404) {
    renderProducts([]);
    return;
  }
  const data = await res.json();
  renderProducts(data);
}

async function addToCart(productId) {
  const res = await apiFetch('/cart', {
    method: 'POST',
    body: JSON.stringify({ productId, quantity: 1 })
  });
  if (res && res.ok) {
    statusMsg.textContent = 'Added to cart!';
    statusMsg.classList.remove('hidden');
    setTimeout(() => statusMsg.classList.add('hidden'), 1500);
  }
}

document.getElementById('searchBtn').addEventListener('click', searchProducts);
document.getElementById('clearSearchBtn').addEventListener('click', () => {
  searchInput.value = '';
  loadProducts();
});
searchInput.addEventListener('keypress', (e) => {
  if (e.key === 'Enter') searchProducts();
});

loadProducts();
