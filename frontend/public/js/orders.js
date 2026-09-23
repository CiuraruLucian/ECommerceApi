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

  ordersContent.innerHTML = orders.map(o => {
    // Orders confirmed before the rename are stored as "Paid" — show them as Confirmed too.
    const isConfirmed = o.status === 'Confirmed' || o.status === 'Paid';
    const statusLabel = isConfirmed ? 'Confirmed' : o.status;

    return `
    <div class="order-card">
      <div class="order-header">
        <strong>Order #${o.id}</strong>
        <span class="status status-${statusLabel.toLowerCase()}">${escapeHtml(statusLabel)}</span>
      </div>
      <ul>
        ${o.items.map(i => `<li>${escapeHtml(i.productName)} × ${i.quantity} — $${i.unitPrice.toFixed(2)} each</li>`).join('')}
      </ul>
      <p class="order-total">Total: $${o.total.toFixed(2)}</p>
      ${!isConfirmed ? `<button onclick="confirmPayment(${o.id})">Confirm Payment</button>` : ''}
      <p id="orderMsg-${o.id}" class="hidden"></p>
    </div>
  `;
  }).join('');
}

async function confirmPayment(orderId) {
  const msg = document.getElementById(`orderMsg-${orderId}`);
  const showMsg = (text) => {
    msg.textContent = text;
    msg.classList.remove('hidden');
  };

  const res = await apiFetch(`/order/${orderId}/confirm-payment`, { method: 'POST' });
  if (!res) return;

  const data = await res.json().catch(() => ({}));

  if (!res.ok) {
    showMsg(data.error || 'Could not check the payment. Please try again.');
    return;
  }

  if (data.status === 'Confirmed') {
    loadOrders();
  } else {
    showMsg(`Payment not completed yet (Stripe status: ${data.stripeStatus}).`);
  }
}

loadOrders();
