// Sample cart data - in a real app, this would come from a database or API
let cartItems = [
    {
        id: 1,
        name: "Margherita Pizza",
        description: "Fresh tomatoes, mozzarella, and basil",
        price: 18.99,
        quantity: 2,
        image: "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='80' height='80' viewBox='0 0 80 80'%3E%3Ccircle cx='40' cy='40' r='35' fill='%23f39c12'/%3E%3Ccircle cx='40' cy='40' r='30' fill='%23e74c3c'/%3E%3Ccircle cx='25' cy='30' r='4' fill='%23fff'/%3E%3Ccircle cx='55' cy='35' r='4' fill='%23fff'/%3E%3Ccircle cx='35' cy='55' r='4' fill='%23fff'/%3E%3Ccircle cx='50' cy='50' r='4' fill='%23fff'/%3E%3C/svg%3E"
    },
    {
        id: 2,
        name: "Caesar Salad",
        description: "Crisp romaine lettuce with parmesan and croutons",
        price: 12.50,
        quantity: 1,
        image: "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='80' height='80' viewBox='0 0 80 80'%3E%3Ccircle cx='40' cy='40' r='35' fill='%2327ae60'/%3E%3Cpath d='M20 35 Q40 20 60 35 Q50 50 40 45 Q30 50 20 35' fill='%2352c41a'/%3E%3Crect x='35' y='25' width='10' height='8' fill='%23f1c40f'/%3E%3Crect x='25' y='45' width='8' height='6' fill='%23f39c12'/%3E%3Crect x='47' y='40' width='6' height='8' fill='%23f39c12'/%3E%3C/svg%3E"
    },
    {
        id: 3,
        name: "Chocolate Brownie",
        description: "Rich chocolate brownie with vanilla ice cream",
        price: 8.75,
        quantity: 1,
        image: "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='80' height='80' viewBox='0 0 80 80'%3E%3Crect x='15' y='25' width='50' height='30' rx='5' fill='%238b4513'/%3E%3Crect x='20' y='30' width='40' height='20' rx='3' fill='%23654321'/%3E%3Ccircle cx='30' cy='35' r='2' fill='%23fff'/%3E%3Ccircle cx='45' cy='40' r='2' fill='%23fff'/%3E%3Ccircle cx='35' cy='45' r='2' fill='%23fff'/%3E%3C/svg%3E"
    }
];

// Define all functions first
function updateCartBadge() {
    const totalItems = cartItems.reduce((sum, item) => sum + item.quantity, 0);
    const cartBadge = document.getElementById('cartBadge');
    if (cartBadge) {
        cartBadge.textContent = totalItems;

        if (totalItems === 0) {
            cartBadge.style.display = 'none';
        } else {
            cartBadge.style.display = 'flex';
        }
    }
}

function toggleCartView() {
    alert('Navigate to cart view - In a real app, this would show/hide the cart or navigate to cart page');
}

function toggleUserDropdown() {
    const dropdown = document.getElementById('userDropdown');
    const avatar = document.querySelector('.user-avatar');

    if (dropdown && avatar) {
        dropdown.classList.toggle('show');
        avatar.classList.toggle('active');
    }
}

function updateQuantity(itemId, change) {
    const item = cartItems.find(item => item.id === itemId);
    if (item) {
        item.quantity += change;
        if (item.quantity <= 0) {
            removeItem(itemId);
        } else {
            renderCart();
        }
    }
}

function removeItem(itemId) {
    cartItems = cartItems.filter(item => item.id !== itemId);
    renderCart();
}

function addSampleItems() {
    cartItems = [
        {
            id: 1,
            name: "Margherita Pizza",
            description: "Fresh tomatoes, mozzarella, and basil",
            price: 18.99,
            quantity: 2,
            image: "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='80' height='80' viewBox='0 0 80 80'%3E%3Ccircle cx='40' cy='40' r='35' fill='%23f39c12'/%3E%3Ccircle cx='40' cy='40' r='30' fill='%23e74c3c'/%3E%3Ccircle cx='25' cy='30' r='4' fill='%23fff'/%3E%3Ccircle cx='55' cy='35' r='4' fill='%23fff'/%3E%3Ccircle cx='35' cy='55' r='4' fill='%23fff'/%3E%3Ccircle cx='50' cy='50' r='4' fill='%23fff'/%3E%3C/svg%3E"
        },
        {
            id: 2,
            name: "Caesar Salad",
            description: "Crisp romaine lettuce with parmesan and croutons",
            price: 12.50,
            quantity: 1,
            image: "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='80' height='80' viewBox='0 0 80 80'%3E%3Ccircle cx='40' cy='40' r='35' fill='%2327ae60'/%3E%3Cpath d='M20 35 Q40 20 60 35 Q50 50 40 45 Q30 50 20 35' fill='%2352c41a'/%3E%3Crect x='35' y='25' width='10' height='8' fill='%23f1c40f'/%3E%3Crect x='25' y='45' width='8' height='6' fill='%23f39c12'/%3E%3Crect x='47' y='40' width='6' height='8' fill='%23f39c12'/%3E%3C/svg%3E"
        },
        {
            id: 3,
            name: "Chocolate Brownie",
            description: "Rich chocolate brownie with vanilla ice cream",
            price: 8.75,
            quantity: 1,
            image: "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='80' height='80' viewBox='0 0 80 80'%3E%3Crect x='15' y='25' width='50' height='30' rx='5' fill='%238b4513'/%3E%3Crect x='20' y='30' width='40' height='20' rx='3' fill='%23654321'/%3E%3Ccircle cx='30' cy='35' r='2' fill='%23fff'/%3E%3Ccircle cx='45' cy='40' r='2' fill='%23fff'/%3E%3Ccircle cx='35' cy='45' r='2' fill='%23fff'/%3E%3C/svg%3E"
        }
    ];
    renderCart();
}

function checkout() {
    alert('Proceeding to checkout! In a real application, this would redirect to the payment page.');
}

function renderCart() {
    const cartContent = document.getElementById('cartContent');

    // Update cart badge
    updateCartBadge();

    if (cartItems.length === 0) {
        cartContent.innerHTML = `
                    <div class="empty-cart">
                        <i class="fas fa-shopping-cart"></i>
                        <h2>Your cart is empty</h2>
                        <p>Add some delicious items to get started!</p>
                        <button class="add-items-btn" onclick="addSampleItems()">
                            <i class="fas fa-plus"></i> Add Sample Items
                        </button>
                    </div>
                `;
        return;
    }

    let cartHTML = '';
    let subtotal = 0;

    cartItems.forEach(item => {
        const itemTotal = item.price * item.quantity;
        subtotal += itemTotal;

        cartHTML += `
                    <div class="cart-item">
                        <img src="${item.image}" alt="${item.name}" class="item-image">
                        <div class="item-details">
                            <div class="item-name">${item.name}</div>
                            <div class="item-description">${item.description}</div>
                            <div class="item-price">$${item.price.toFixed(2)}</div>
                        </div>
                        <div class="quantity-controls">
                            <button class="quantity-btn" onclick="updateQuantity(${item.id}, -1)">
                                <i class="fas fa-minus"></i>
                            </button>
                            <div class="quantity-display">${item.quantity}</div>
                            <button class="quantity-btn" onclick="updateQuantity(${item.id}, 1)">
                                <i class="fas fa-plus"></i>
                            </button>
                        </div>
                        <button class="remove-btn" onclick="removeItem(${item.id})">
                            <i class="fas fa-trash"></i> Remove
                        </button>
                    </div>
                `;
    });

    const tax = subtotal * 0.08; // 8% tax
    const deliveryFee = subtotal > 25 ? 0 : 3.99;
    const total = subtotal + tax + deliveryFee;

    cartHTML += `
                <div class="cart-summary">
                    <div class="summary-row">
                        <span>Subtotal:</span>
                        <span>$${subtotal.toFixed(2)}</span>
                    </div>
                    <div class="summary-row">
                        <span>Tax (8%):</span>
                        <span>$${tax.toFixed(2)}</span>
                    </div>
                    <div class="summary-row">
                        <span>Delivery Fee:</span>
                        <span>${deliveryFee === 0 ? 'FREE' : '$' + deliveryFee.toFixed(2)}</span>
                    </div>
                    <div class="summary-row total-row">
                        <span>Total:</span>
                        <span>$${total.toFixed(2)}</span>
                    </div>
                    <button class="checkout-btn" onclick="checkout()">
                        <i class="fas fa-credit-card"></i> Proceed to Checkout
                    </button>
                </div>
            `;

    cartContent.innerHTML = cartHTML;
}

// Close dropdown when clicking outside
document.addEventListener('click', function (event) {
    const userMenu = document.querySelector('.user-menu');
    const dropdown = document.getElementById('userDropdown');
    const avatar = document.querySelector('.user-avatar');

    if (userMenu && dropdown && avatar && !userMenu.contains(event.target)) {
        dropdown.classList.remove('show');
        avatar.classList.remove('active');
    }
});

// Initialize the cart when DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
    renderCart();
});

// Also initialize immediately in case DOMContentLoaded already fired
renderCart();