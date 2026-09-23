const express = require('express');
const fs = require('fs');
const path = require('path');

// Locally, settings come from frontend/.env (gitignored — copy .env.example).
// On Azure there is no .env file; they come from the App Service's
// Environment variables (App settings) instead.
const envFile = path.join(__dirname, '.env');
if (fs.existsSync(envFile)) {
  process.loadEnvFile(envFile);
}

const REQUIRED_ENV = ['API_BASE_URL', 'STRIPE_PUBLISHABLE_KEY'];
const missingEnv = REQUIRED_ENV.filter((name) => !process.env[name]);
if (missingEnv.length > 0) {
  throw new Error(
    `Missing environment variable(s): ${missingEnv.join(', ')}. ` +
    'Locally, add them to frontend/.env (see .env.example); on Azure, add them under the App Service\'s Environment variables.');
}

const app = express();
const PORT = process.env.PORT || 3000;

app.set('view engine', 'ejs');
app.set('views', path.join(__dirname, 'views'));

// Hands the browser the settings it needs as window.APP_CONFIG, read by
// public/js/config.js. Only put values here that are safe to be public.
app.get('/js/env.js', (req, res) => {
  const config = {
    apiBase: process.env.API_BASE_URL,
    stripePublishableKey: process.env.STRIPE_PUBLISHABLE_KEY,
  };
  res.type('application/javascript');
  res.set('Cache-Control', 'no-store');
  res.send(`window.APP_CONFIG = ${JSON.stringify(config)};`);
});

app.use(express.static(path.join(__dirname, 'public')));

// All pages are simple shells — the actual data comes from the ASP.NET API
// via fetch() calls in the client-side JS files (public/js/*.js).

app.get('/', (req, res) => res.redirect('/products'));

app.get('/login', (req, res) => res.render('login', { title: 'Login' }));
app.get('/register', (req, res) => res.render('register', { title: 'Register' }));
app.get('/products', (req, res) => res.render('products', { title: 'Products' }));
app.get('/cart', (req, res) => res.render('cart', { title: 'Your Cart' }));
app.get('/orders', (req, res) => res.render('orders', { title: 'Your Orders' }));
app.get('/admin', (req, res) => res.render('admin', { title: 'Admin Panel' }));

app.listen(PORT, () => {
  console.log(`Frontend running at http://localhost:${PORT}`);
});
