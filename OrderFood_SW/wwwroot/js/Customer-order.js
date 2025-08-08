// wwwroot/js/Customer-order.js
const OrderApp = (function () {
    // config
    const DESKTOP_MIN = 992;

    // state
    let isDesktop = window.innerWidth >= DESKTOP_MIN;
    let state = {
        // desktop: cartHidden true => panel đóng/ẩn on desktop
        cartHidden: false,
        // mobile: mobileVisible true => panel hiện lên
        mobileVisible: false
    };

    // cached DOM (dynamic - call when needed)
    function cacheDom() {
        return {
            doc: document,
            body: document.body,
            cartPanel: document.querySelector('.order-panel'),
            toggleBtn: document.getElementById('toggleOrderPanelBtn'),
            mobileCartBtn: document.getElementById('mobileCartBtn'),
            categoryContainer: document.querySelector('.categories'),
            messagesBox: document.getElementById('cart-messages'),
            overlay: document.getElementById('cart-overlay')
        };
    }

    // persist desktop hide state
    function loadState() {
        try {
            if (isDesktop) {
                state.cartHidden = localStorage.getItem('pcCartHidden') === '1';
            } else {
                // mobile: never persist to localStorage - always start hidden
                state.cartHidden = false;
                state.mobileVisible = false;
            }
        } catch (e) {
            state.cartHidden = false;
            state.mobileVisible = false;
        }
    }
    function saveState() {
        try {
            if (isDesktop) {
                localStorage.setItem('pcCartHidden', state.cartHidden ? '1' : '0');
            }
        } catch (e) { /* ignore */ }
    }

    // create overlay element for mobile
    function ensureOverlay() {
        let ov = document.getElementById('cart-overlay');
        if (!ov) {
            ov = document.createElement('div');
            ov.id = 'cart-overlay';
            ov.style.cssText = 'position:fixed; inset:0; background:rgba(0,0,0,0.3); z-index:1995; display:none;';
            document.body.appendChild(ov);
        }
        return ov;
    }

    // ensure there's only one .order-panel active and attach to body for fixed positioning
    function normalizePanels() {
        const panels = Array.from(document.querySelectorAll('.order-panel'));
        if (panels.length === 0) return null;

        // Prefer the one already attached to body (so we keep a stable element), otherwise first
        let keep = panels.find(p => p.parentNode === document.body) || panels[0];

        // Remove others
        panels.forEach(p => {
            if (p !== keep) p.remove();
        });

        // Ensure keep is direct child of body (avoid issues when ancestors have transform)
        if (keep.parentNode !== document.body) {
            keep.setAttribute('data-cart-moved', '1');
            document.body.appendChild(keep);
        }

        // Ensure it has data-cart-container attribute (used by refresh)
        keep.setAttribute('data-cart-container', '');

        return keep;
    }

    // UI apply
    function applyCartState(cached) {
        const panel = cached.cartPanel || document.querySelector('.order-panel');
        if (!panel) return;

        // normalize duplicates & attachment
        normalizePanels();

        if (isDesktop) {
            // Desktop: show/hide via .order-panel-hidden
            panel.classList.toggle('order-panel-hidden', state.cartHidden);
            // update icon (if present)
            const toggle = cached.toggleBtn || document.getElementById('toggleOrderPanelBtn');
            if (toggle && toggle.querySelector('i')) {
                const icon = toggle.querySelector('i');
                icon.classList.toggle('fa-eye-slash', !state.cartHidden);
                icon.classList.toggle('fa-eye', state.cartHidden);
            }
            // remove overlay & restore body scroll
            const ov = ensureOverlay();
            ov.style.display = 'none';
            document.body.style.overflow = '';
        } else {
            // Mobile: show via class .order-panel-visible and use overlay to close
            const ov = ensureOverlay();
            if (state.mobileVisible) {
                panel.classList.add('order-panel-visible');
                ov.style.display = 'block';
                // prevent background scroll
                document.body.style.overflow = 'hidden';
            } else {
                panel.classList.remove('order-panel-visible');
                ov.style.display = 'none';
                document.body.style.overflow = '';
            }
        }

        // remove temporary preload class (compat)
        document.documentElement.classList.remove('pc-cart-hidden');
    }

    // Messages
    function ensureMessagesBox() {
        let box = document.getElementById('cart-messages');
        if (!box) {
            box = document.createElement('div');
            box.id = 'cart-messages';
            box.style.cssText = 'position: fixed; top: 20px; right: 20px; z-index: 3000;';
            document.body.appendChild(box);
        }
        return box;
    }
    function showMessage(text, type = 'success') {
        const box = ensureMessagesBox();
        const el = document.createElement('div');
        el.textContent = text;
        el.style.cssText = `
            padding: 10px 14px;
            margin-bottom: 8px;
            border-radius: 6px;
            box-shadow: 0 2px 6px rgba(0,0,0,0.12);
            background-color: ${type === 'success' ? '#d4edda' : '#f8d7da'};
            color: ${type === 'success' ? '#155724' : '#721c24'};
            border: 1px solid ${type === 'success' ? '#c3e6cb' : '#f5c6cb'};
            min-width: 200px;
        `;
        box.appendChild(el);
        setTimeout(() => el.remove(), 3000);
    }

    // AJAX helpers (ensure proper content-type for urlencoded bodies)
    function fetchText(url) {
        return fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } })
            .then(r => {
                if (!r.ok) throw new Error('Network error');
                return r.text();
            });
    }
    function postJson(url, body) {
        // if body is FormData, let fetch handle headers; if string, set urlencoded content-type
        const opts = {
            method: 'POST',
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        };
        if (body instanceof FormData) {
            opts.body = body;
        } else if (body === null || body === undefined) {
            opts.body = null;
        } else {
            // string urlencoded
            opts.headers['Content-Type'] = 'application/x-www-form-urlencoded; charset=UTF-8';
            opts.body = body;
        }
        return fetch(url, opts).then(r => r.json());
    }

    // Core actions
    function removeFromCart(dishId) {
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
        const body = 'id=' + encodeURIComponent(dishId) + '&__RequestVerificationToken=' + encodeURIComponent(token);

        postJson('/CustomerOrder/RemoveFromCart', body)
            .then(data => {
                if (data && data.success) {
                    refreshCartPartial(); // update UI
                    showMessage('Item removed from cart', 'success');
                } else {
                    showMessage(data?.message || 'Failed to remove item', 'error');
                }
            })
            .catch(err => {
                console.error(err);
                showMessage('Failed to remove item', 'error');
            });
    }

    function removeAllCart() {
        if (!confirm('Are you sure you want to delete all dishes?')) return;
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
        const body = '__RequestVerificationToken=' + encodeURIComponent(token);
        postJson('/CustomerOrder/RemoveAllCart', body)
            .then(data => {
                if (data && data.success) {
                    refreshCartPartial();
                    showMessage('Cart cleared successfully', 'success');
                } else {
                    showMessage(data?.message || 'Failed to clear cart', 'error');
                }
            })
            .catch(err => {
                console.error(err);
                showMessage('Failed to clear cart', 'error');
            });
    }

    // Add-to-cart handled via form submit in module
    function handleAddToCart(form) {
        const btn = form.querySelector('.add-btn');
        const originalText = btn ? btn.textContent : null;
        if (btn) { btn.textContent = 'Adding...'; btn.disabled = true; btn.style.opacity = '0.7'; }

        const formData = new FormData(form);
        fetch(form.action, {
            method: 'POST',
            body: formData,
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        })
            .then(r => r.json())
            .then(data => {
                if (data && data.success) {
                    showMessage('Item added to cart!', 'success');
                    refreshCartPartial();
                    const qty = form.querySelector('input[name="Quantity"]');
                    if (qty) qty.value = 1;
                } else {
                    showMessage(data?.message || 'Failed to add to cart', 'error');
                }
            })
            .catch(err => {
                console.error(err);
                showMessage('Failed to add to cart', 'error');
            })
            .finally(() => {
                if (btn) { setTimeout(() => { btn.textContent = originalText; btn.disabled = false; btn.style.opacity = '1'; }, 300); }
            });
    }

    // Refresh cart partial - fetch and replace container; preserve state
    function refreshCartPartial() {
        const cached = cacheDom();
        const wasHidden = state.cartHidden;
        const wasMobileVisible = state.mobileVisible;
        return fetchText('/CustomerOrder/GetCart')
            .then(html => {
                const container = document.querySelector('[data-cart-container]');
                if (!container) {
                    // If container missing, try to parse and append to body
                    const tmp = document.createElement('div');
                    tmp.innerHTML = html;
                    const newEl = tmp.firstElementChild;
                    if (newEl) {
                        document.body.appendChild(newEl);
                    }
                    bindAll();
                    state.cartHidden = wasHidden;
                    state.mobileVisible = wasMobileVisible;
                    applyCartState(cacheDom());
                    return;
                }
                // Replace outerHTML safely
                const parent = container.parentNode;
                const tmp = document.createElement('div');
                tmp.innerHTML = html;
                const newEl = tmp.firstElementChild;
                parent.replaceChild(newEl, container);

                // Rebind after replacement
                bindAll(); // re-cache inside
                state.cartHidden = wasHidden; // restore flag
                state.mobileVisible = wasMobileVisible;
                applyCartState(cacheDom());
            })
            .catch(err => {
                console.error('Error updating cart:', err);
            });
    }

    // Category selection: use event delegation
    function onCategoryClick(e) {
        const card = e.target.closest('.category-card');
        if (!card) return;
        const categoryId = card.getAttribute('data-category-id') || '0';
        // update active class locally for visual immediate feedback
        document.querySelectorAll('.category-card').forEach(c => c.classList.remove('active'));
        card.classList.add('active');

        const searchBox = document.getElementById('search-box');
        const keyword = searchBox ? searchBox.value.trim() : '';

        const url = new URL(window.location.origin + '/CustomerOrder/CreateOrder');
        if (categoryId !== '0') url.searchParams.append('categoryId', categoryId);
        if (keyword) url.searchParams.append('searchKeyword', keyword);

        // Navigate — full page load (server returns new product grid + cart partial)
        window.location.href = url.toString();
    }

    // Bind events (idempotent: use dataset flags to avoid duplicates)
    function bindAll() {
        const cached = cacheDom();

        // normalize panels and overlay
        normalizePanels();
        ensureOverlay();

        // desktop toggle - replace node to clear old listeners for toggle only
        const toggle = document.getElementById('toggleOrderPanelBtn');
        if (toggle) {
            if (!toggle.dataset.ooBound) {
                toggle.addEventListener('click', function (ev) {
                    ev.preventDefault();
                    state.cartHidden = !state.cartHidden;
                    applyCartState(cacheDom());
                    saveState();
                });
                toggle.dataset.ooBound = '1';
            }
        }

        // mobile cart toggle
        if (cached.mobileCartBtn) {
            if (!cached.mobileCartBtn.dataset.ooBound) {
                cached.mobileCartBtn.addEventListener('click', function (ev) {
                    ev.preventDefault();
                    state.mobileVisible = !state.mobileVisible;
                    applyCartState(cacheDom());
                });
                cached.mobileCartBtn.dataset.ooBound = '1';
            }
        }

        // overlay click closes mobile panel
        const ov = ensureOverlay();
        if (!ov.dataset.ooBound) {
            ov.addEventListener('click', function () {
                state.mobileVisible = false;
                applyCartState(cacheDom());
            });
            ov.dataset.ooBound = '1';
        }

        // ESC key to close mobile panel
        if (!document.body.dataset.escBound) {
            document.addEventListener('keydown', function (ev) {
                if (ev.key === 'Escape' && state.mobileVisible) {
                    state.mobileVisible = false;
                    applyCartState(cacheDom());
                }
            });
            document.body.dataset.escBound = '1';
        }

        // Remove item buttons
        document.querySelectorAll('.remove-cart-item').forEach(btn => {
            // remove previous handlers (use dataset marker)
            if (!btn.dataset.ooBound) {
                btn.addEventListener('click', function (ev) {
                    ev.preventDefault();
                    const dishId = this.getAttribute('data-dish-id');
                    removeFromCart(dishId);
                });
                btn.dataset.ooBound = '1';
            }
        });

        // Clear cart button
        document.querySelectorAll('.clear-cart-btn').forEach(btn => {
            if (!btn.dataset.ooBound) {
                btn.addEventListener('click', function (ev) {
                    ev.preventDefault();
                    removeAllCart();
                });
                btn.dataset.ooBound = '1';
            }
        });

        // Add-to-cart forms
        document.querySelectorAll('.add-to-cart-form').forEach(form => {
            if (!form.dataset.ooBound) {
                form.addEventListener('submit', function (ev) {
                    ev.preventDefault();
                    handleAddToCart(form);
                });
                form.dataset.ooBound = '1';
            }
        });

        // Category click delegation
        const catContainer = document.querySelector('.categories');
        if (catContainer && !catContainer.dataset.ooBound) {
            catContainer.addEventListener('click', onCategoryClick);
            catContainer.dataset.ooBound = '1';
        }

        // ensure cart state applied after binds
        applyCartState(cacheDom());
    }

    // Public init and refresh
    function init() {
        isDesktop = window.innerWidth >= DESKTOP_MIN;
        loadState();
        bindAll();
        // ensure listeners re-bind if window resizes across mobile/desktop threshold
        window.addEventListener('resize', onResize);
    }

    function onResize() {
        const wasDesktop = isDesktop;
        isDesktop = window.innerWidth >= DESKTOP_MIN;
        if (wasDesktop !== isDesktop) {
            // reload saved state when crossing threshold
            loadState();
            // hide mobile panel on resize to desktop to avoid inconsistency
            state.mobileVisible = false;
            applyCartState(cacheDom());
        }
    }

    // Expose API
    return {
        init,
        refresh: refreshCartPartial,
        removeFromCart,
        removeAllCart
    };

})();

// Auto-init when DOM content loaded
document.addEventListener('DOMContentLoaded', function () {
    try { OrderApp.init(); } catch (e) { console.error(e); }
});
