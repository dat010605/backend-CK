const API_URL = 'http://localhost:5115/api'; 
// Toast
function toast(msg, type = 'success') {
  Toastify({
    text: msg,
    duration: 3000,
    backgroundColor: type === 'error' ? '#e74c3c' : '#27ae60',
    close: true
  }).showToast();
}

// Kiểm tra đăng nhập
function checkAuth(requiredRole = null) {
  const token = localStorage.getItem('token');
  const role = localStorage.getItem('role');
  if (!token) {
    window.location.href = 'login.html';
    return false;
  }
  if (requiredRole && role !== requiredRole) {
    toast('Bạn không có quyền truy cập!', 'error');
    setTimeout(() => window.location.href = 'products.html', 1000);
    return false;
  }
  return { token, role };
}

// Đăng xuất
function logout() {
  localStorage.clear();
  window.location.href = 'login.html';
}

// Cập nhật số lượng giỏ hàng
function updateCartCount() {
  const cart = JSON.parse(localStorage.getItem('cart') || '[]');
  const total = cart.reduce((sum, item) => sum + item.quantity, 0);
  const el = document.getElementById('cart-count');
  if (el) el.textContent = total;
}
updateCartCount();

// Thêm vào giỏ
function addToCart(id, name, price) {
  let cart = JSON.parse(localStorage.getItem('cart') || '[]');
  const exist = cart.find(x => x.id === id);
  if (exist) exist.quantity++;
  else cart.push({ id, name, price, quantity: 1 });
  localStorage.setItem('cart', JSON.stringify(cart));
  updateCartCount();
  toast('Đã thêm vào giỏ hàng!');
}

// Xóa khỏi giỏ
function removeFromCart(id) {
  let cart = JSON.parse(localStorage.getItem('cart') || '[]');
  cart = cart.filter(x => x.id !== id);
  localStorage.setItem('cart', JSON.stringify(cart));
  loadCart(); // sẽ được gọi từ cart.html
  updateCartCount();
}