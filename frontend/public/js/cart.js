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

const paymentSection = document.getElementById('paymentSection');
const paymentSummary = document.getElementById('paymentSummary');
const payBtn = document.getElementById('payBtn');
const payMsg = document.getElementById('payMsg');

let stripe = null;
let elements = null;
let currentOrder = null;

function showPayMsg(text) {
  payMsg.textContent = text;
  payMsg.classList.remove('hidden');
}

function redirectToOrders() {
  setTimeout(() => (window.location.href = '/orders'), 1800);
}

// Mounts Stripe's card form for the order that checkout just created.
function showPaymentForm(order) {
  const stripeReady = typeof Stripe !== 'undefined'
    && !STRIPE_PUBLISHABLE_KEY.includes('REPLACE_ME')
    && order.clientSecret;

  if (!stripeReady) {
    statusMsg.textContent = `Order #${order.id} created, but card payments are not configured yet. Redirecting to orders...`;
    statusMsg.classList.remove('hidden');
    redirectToOrders();
    return;
  }

  currentOrder = order;

  cartContent.classList.add('hidden');
  checkoutBtn.classList.add('hidden');
  statusMsg.classList.add('hidden');
  paymentSummary.textContent = `Order #${order.id} — Total: $${order.total.toFixed(2)}`;
  paymentSection.classList.remove('hidden');

  // Mount only after the container is visible — Stripe's Payment Element needs
  // real layout dimensions, so mounting it while #paymentSection is display:none
  // leaves it half-initialized and confirmPayment() later fails with
  // "elements should have a mounted Payment Element".
  stripe = Stripe(STRIPE_PUBLISHABLE_KEY);
  elements = stripe.elements({ clientSecret: order.clientSecret });
  elements.create('payment').mount('#payment-element');
}

checkoutBtn.addEventListener('click', async () => {
  checkoutBtn.disabled = true;

  try {
    const res = await apiFetch('/order/checkout', { method: 'POST' });
    if (!res) return;

    if (!res.ok) {
      const err = await res.json().catch(() => ({}));
      statusMsg.textContent = err.error || 'Checkout failed.';
      statusMsg.classList.remove('hidden');
      return;
    }

    showPaymentForm(await res.json());
  } finally {
    checkoutBtn.disabled = false;
  }
});

payBtn.addEventListener('click', async () => {
  payBtn.disabled = true;
  showPayMsg('Processing payment...');

  const { error, paymentIntent } = await stripe.confirmPayment({
    elements,
    redirect: 'if_required'
  });

  if (error) {
    showPayMsg(error.message);
    payBtn.disabled = false;
    return;
  }

  if (!paymentIntent || paymentIntent.status !== 'succeeded') {
    showPayMsg(`Payment is not complete yet (status: ${paymentIntent ? paymentIntent.status : 'unknown'}). Please try again.`);
    payBtn.disabled = false;
    return;
  }

  // Stripe says it worked in the browser — ask the API to verify with Stripe and
  // flip the order to Confirmed.
  let confirmed = false;
  try {
    const confirmRes = await apiFetch(`/order/${currentOrder.id}/confirm-payment`, { method: 'POST' });
    if (confirmRes && confirmRes.ok) {
      const data = await confirmRes.json();
      confirmed = data.status === 'Confirmed';
    }
  } catch (err) {
    console.error('confirm-payment failed:', err);
  }

  showPayMsg(confirmed
    ? `Payment confirmed! Order #${currentOrder.id} is confirmed. Redirecting to orders...`
    : 'Payment received. If your order still shows Pending, press "Confirm Payment" on the orders page. Redirecting...');
  redirectToOrders();
});

loadCart();
