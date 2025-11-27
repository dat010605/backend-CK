const API_URL = 'http://localhost:5115/api'; 

// Toast thông báo
function toast(msg, type = 'success') {
  Toastify({
    text: msg,
    duration: 3000,
    backgroundColor: type === 'error' ? '#e74c3c' : '#27ae60',
    close: true
  }).showToast();
}

// Kiểm tra đăng nhập & Quyền
function checkAuth(requiredRole = null) {
  const token = localStorage.getItem('token');
  const role = localStorage.getItem('role');

  // 1. Chưa đăng nhập -> Về Login
  if (!token) {
    window.location.href = 'login.html';
    return false;
  }

  // 2. Kiểm tra Role (So sánh không phân biệt hoa thường)
  if (requiredRole) {
    // Chuyển cả 2 về chữ thường để so sánh
    const currentRole = role ? role.toLowerCase() : '';
    const reqRole = requiredRole.toLowerCase();

    if (currentRole !== reqRole) {
      toast('Bạn không có quyền Admin!', 'error');
      setTimeout(() => window.location.href = 'products.html', 1000);
      return false;
    }
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

// Gọi hàm này để đảm bảo nó chạy khi script load
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
  // Nếu đang ở trang cart thì load lại
  if (typeof loadCart === 'function') loadCart(); 
  updateCartCount();
}