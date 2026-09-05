const express = require('express');
const path = require('path');

const app = express();
const PORT = process.env.PORT || 3000;

app.set('view engine', 'ejs');
app.set('views', path.join(__dirname, 'views'));
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
