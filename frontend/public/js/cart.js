const cartContent = document.getElementById('cartContent');
const checkoutBtn = document.getElementById('checkoutBtn');
const statusMsg = document.getElementById('statusMsg');

if (!isLoggedIn()) {
  window.location.href = '/login';
}

async function loadCart() {
  const res = await apiFetch('/cart');
  if (!res) return;
  const cart = await res.json();

  if (!cart.items || !cart.items.length) {
    cartContent.innerHTML = '<p>Your cart is empty.</p>';
    checkoutBtn.classList.add('hidden');
    return;
  }

  // Cart items only carry productId/quantity — fetch product details for display
  const rows = await Promise.all(cart.items.map(async (item) => {
    const pRes = await fetch(`${API_BASE}/products/${item.productId}`);
    const product = pRes.ok ? await pRes.json() : { name: 'Unknown product', price: 0 };
    return { ...item, product };
  }));

  let total = 0;
  cartContent.innerHTML = `
    <table class="cart-table">
      <thead><tr><th>Product</th><th>Price</th><th>Qty</th><th>Subtotal</th><th></th></tr></thead>
      <tbody>
        ${rows.map(r => {
          const subtotal = r.product.price * r.quantity;
          total += subtotal;
          return `
            <tr>
              <td>${escapeHtml(r.product.name)}</td>
              <td>$${r.product.price.toFixed(2)}</td>
              <td>${r.quantity}</td>
              <td>$${subtotal.toFixed(2)}</td>
              <td><button onclick="removeItem(${r.productId})">Remove</button></td>
            </tr>`;
        }).join('')}
      </tbody>
    </table>
    <h3>Total: $${total.toFixed(2)}</h3>
  `;
  checkoutBtn.classList.remove('hidden');
}

async function removeItem(productId) {
  const res = await apiFetch(`/cart/${productId}`, { method: 'DELETE' });
  if (res && res.ok) loadCart();
}

checkoutBtn.addEventListener('click', async () => {
  const res = await apiFetch('/order/checkout', { method: 'POST' });
  if (!res) return;

  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    statusMsg.textContent = err.error || 'Checkout failed.';
    statusMsg.classList.remove('hidden');
    return;
  }

  const order = await res.json();
  statusMsg.textContent = `Order #${order.id} created! Total: $${order.total.toFixed(2)}. Redirecting to orders...`;
  statusMsg.classList.remove('hidden');
  setTimeout(() => (window.location.href = '/orders'), 1800);
});

loadCart();
