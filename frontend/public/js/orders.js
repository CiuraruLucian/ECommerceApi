const ordersContent = document.getElementById('ordersContent');

if (!isLoggedIn()) {
  window.location.href = '/login';
}

async function loadOrders() {
  const res = await apiFetch('/order');
  if (!res) return;

  if (res.status === 404 || !res.ok) {
    ordersContent.innerHTML = '<p>No orders yet.</p>';
    return;
  }

  const orders = await res.json();
  if (!orders.length) {
    ordersContent.innerHTML = '<p>No orders yet.</p>';
    return;
  }

  ordersContent.innerHTML = orders.map(o => `
    <div class="order-card">
      <div class="order-header">
        <strong>Order #${o.id}</strong>
        <span class="status status-${o.status.toLowerCase()}">${escapeHtml(o.status)}</span>
      </div>
      <ul>
        ${o.items.map(i => `<li>${escapeHtml(i.productName)} × ${i.quantity} — $${i.unitPrice.toFixed(2)} each</li>`).join('')}
      </ul>
      <p class="order-total">Total: $${o.total.toFixed(2)}</p>
      ${o.status !== 'Paid' ? `<button onclick="confirmPayment(${o.id})">Confirm Payment</button>` : ''}
    </div>
  `).join('');
}

async function confirmPayment(orderId) {
  const res = await apiFetch(`/order/${orderId}/confirm-payment`, { method: 'POST' });
  if (res && res.ok) loadOrders();
}

loadOrders();
