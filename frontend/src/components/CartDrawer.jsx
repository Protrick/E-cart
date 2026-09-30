import React, { useState } from 'react';
import { X, Trash2, ShoppingBag, ArrowRight, CheckCircle2, ShieldAlert } from 'lucide-react';
import { useCart } from '../context/CartContext';
import { useAuth } from '../context/AuthContext';
import { useToast } from './Toast';
import { api } from '../services/api';

export function CartDrawer({ onOpenAuth, onOrderPlaced }) {
  const { items, updateQuantity, removeFromCart, clearCart, totalPrice, totalItems, isCartOpen, setIsCartOpen } = useCart();
  const { user } = useAuth();
  const { showSuccess, showError } = useToast();

  const [shippingAddress, setShippingAddress] = useState('123 Innovation Drive, Silicon Valley, CA 94025');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [completedOrder, setCompletedOrder] = useState(null);

  if (!isCartOpen) return null;

  const handleCheckout = async () => {
    if (!user) {
      showError('Please sign in to place an order.');
      onOpenAuth();
      return;
    }

    if (items.length === 0) {
      showError('Your cart is empty.');
      return;
    }

    if (!shippingAddress.trim() || shippingAddress.trim().length < 5) {
      showError('Please enter a valid shipping address (at least 5 characters).');
      return;
    }

    setIsSubmitting(true);
    try {
      const orderPayload = {
        shippingAddress: shippingAddress.trim(),
        items: items.map((i) => ({
          productId: i.product.id,
          quantity: i.quantity,
        })),
      };

      const res = await api.createOrder(orderPayload);
      if (res.success && res.data) {
        showSuccess('Order placed successfully!');
        setCompletedOrder(res.data);
        clearCart();
        if (onOrderPlaced) onOrderPlaced();
      }
    } catch (err) {
      showError(err.message || 'Failed to place order.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div
      className="modal-overlay"
      onClick={() => {
        setIsCartOpen(false);
        setCompletedOrder(null);
      }}
      style={{
        justifyContent: 'flex-end',
        padding: 0,
        animation: 'fadeIn 0.2s ease-out',
      }}
    >
      <div
        className="glass-card"
        onClick={(e) => e.stopPropagation()}
        style={{
          width: '100%',
          maxWidth: 480,
          height: '100vh',
          borderRadius: '24px 0 0 24px',
          display: 'flex',
          flexDirection: 'column',
          background: '#0d1322',
          borderLeft: '1px solid rgba(255, 255, 255, 0.12)',
          boxShadow: '-20px 0 50px rgba(0, 0, 0, 0.8)',
          animation: 'slideIn 0.3s cubic-bezier(0.16, 1, 0.3, 1)',
        }}
      >
        {/* Header */}
        <div
          style={{
            padding: '20px 24px',
            borderBottom: '1px solid var(--border-subtle)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
          }}
        >
          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <ShoppingBag size={20} color="var(--primary)" />
            <h2 style={{ fontSize: '1.2rem' }}>Shopping Cart ({totalItems})</h2>
          </div>
          <button
            className="btn-icon"
            onClick={() => {
              setIsCartOpen(false);
              setCompletedOrder(null);
            }}
          >
            <X size={20} />
          </button>
        </div>

        {/* Content */}
        {completedOrder ? (
          <div
            style={{
              padding: 32,
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
              justifyContent: 'center',
              flex: 1,
              textAlign: 'center',
            }}
          >
            <div
              style={{
                width: 64,
                height: 64,
                borderRadius: '50%',
                background: 'rgba(16, 185, 129, 0.15)',
                color: '#10b981',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                marginBottom: 16,
              }}
            >
              <CheckCircle2 size={36} />
            </div>
            <h3 style={{ fontSize: '1.4rem', marginBottom: 8 }}>Order Confirmed!</h3>
            <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem', marginBottom: 16 }}>
              Thank you for your purchase. Your order #{completedOrder.id.slice(0, 8)} has been placed successfully.
            </p>
            <div
              className="glass-card"
              style={{
                padding: 16,
                width: '100%',
                marginBottom: 24,
                textAlign: 'left',
                fontSize: '0.85rem',
              }}
            >
              <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 6 }}>
                <span style={{ color: 'var(--text-muted)' }}>Status:</span>
                <span className="badge badge-warning">{completedOrder.statusText || 'Pending'}</span>
              </div>
              <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 6 }}>
                <span style={{ color: 'var(--text-muted)' }}>Total Amount:</span>
                <span style={{ fontWeight: 700 }}>${Number(completedOrder.totalAmount).toFixed(2)}</span>
              </div>
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <span style={{ color: 'var(--text-muted)' }}>Shipping To:</span>
                <span style={{ maxWidth: 200, textAlign: 'right' }}>{completedOrder.shippingAddress}</span>
              </div>
            </div>
            <button
              className="btn btn-primary"
              onClick={() => {
                setCompletedOrder(null);
                setIsCartOpen(false);
              }}
              style={{ width: '100%' }}
            >
              Continue Shopping
            </button>
          </div>
        ) : items.length === 0 ? (
          <div
            style={{
              padding: 40,
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
              justifyContent: 'center',
              flex: 1,
              color: 'var(--text-muted)',
              textAlign: 'center',
            }}
          >
            <ShoppingBag size={48} style={{ opacity: 0.3, marginBottom: 16 }} />
            <h3 style={{ color: 'var(--text-primary)', marginBottom: 6 }}>Your Cart is Empty</h3>
            <p style={{ fontSize: '0.9rem' }}>Browse our curated collection and add items to your cart.</p>
          </div>
        ) : (
          <>
            {/* Items List */}
            <div
              style={{
                padding: '16px 24px',
                flex: 1,
                overflowY: 'auto',
                display: 'flex',
                flexDirection: 'column',
                gap: 16,
              }}
            >
              {items.map(({ product, quantity }) => (
                <div
                  key={product.id}
                  style={{
                    display: 'flex',
                    gap: 14,
                    padding: 12,
                    background: 'rgba(255, 255, 255, 0.03)',
                    borderRadius: 'var(--radius-sm)',
                    border: '1px solid var(--border-subtle)',
                  }}
                >
                  <img
                    src={product.imageUrl || 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=800'}
                    alt={product.name}
                    style={{
                      width: 70,
                      height: 70,
                      borderRadius: 'var(--radius-sm)',
                      objectFit: 'cover',
                      background: '#1e293b',
                    }}
                  />
                  <div style={{ flex: 1, display: 'flex', flexDirection: 'column', justifyContent: 'space-between' }}>
                    <div>
                      <h4
                        style={{
                          fontSize: '0.9rem',
                          fontWeight: 600,
                          lineHeight: 1.3,
                          marginBottom: 4,
                        }}
                      >
                        {product.name}
                      </h4>
                      <p className="brand-font" style={{ fontSize: '0.95rem', color: 'var(--primary)', fontWeight: 700 }}>
                        ${Number(product.price).toFixed(2)}
                      </p>
                    </div>

                    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginTop: 8 }}>
                      <div
                        style={{
                          display: 'flex',
                          alignItems: 'center',
                          background: 'var(--bg-input)',
                          borderRadius: 6,
                          border: '1px solid var(--border-subtle)',
                        }}
                      >
                        <button
                          className="btn-icon"
                          onClick={() => updateQuantity(product.id, quantity - 1)}
                          style={{ border: 'none', padding: '4px 8px', fontSize: '0.8rem' }}
                        >
                          -
                        </button>
                        <span style={{ padding: '0 8px', fontSize: '0.85rem', fontWeight: 600 }}>{quantity}</span>
                        <button
                          className="btn-icon"
                          onClick={() => updateQuantity(product.id, quantity + 1)}
                          disabled={quantity >= product.stockQuantity}
                          style={{ border: 'none', padding: '4px 8px', fontSize: '0.8rem' }}
                        >
                          +
                        </button>
                      </div>

                      <button
                        className="btn-icon"
                        onClick={() => removeFromCart(product.id)}
                        style={{ color: 'var(--danger)', border: 'none' }}
                      >
                        <Trash2 size={16} />
                      </button>
                    </div>
                  </div>
                </div>
              ))}
            </div>

            {/* Footer Checkout */}
            <div
              style={{
                padding: '20px 24px',
                borderTop: '1px solid var(--border-subtle)',
                background: 'rgba(9, 13, 22, 0.95)',
              }}
            >
              {/* Shipping address input */}
              <div className="form-group" style={{ marginBottom: 14 }}>
                <label className="form-label" style={{ fontSize: '0.8rem' }}>
                  Shipping Destination
                </label>
                <input
                  type="text"
                  className="input"
                  style={{ fontSize: '0.85rem', padding: '8px 12px' }}
                  value={shippingAddress}
                  onChange={(e) => setShippingAddress(e.target.value)}
                  placeholder="Enter full shipping address..."
                />
              </div>

              {/* Subtotal */}
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'baseline', marginBottom: 16 }}>
                <span style={{ color: 'var(--text-secondary)' }}>Total Amount</span>
                <span className="brand-font" style={{ fontSize: '1.5rem', fontWeight: 800, color: 'var(--text-primary)' }}>
                  ${totalPrice.toFixed(2)}
                </span>
              </div>

              <button
                className="btn btn-primary"
                onClick={handleCheckout}
                disabled={isSubmitting}
                style={{ width: '100%', padding: '12px 20px' }}
              >
                {isSubmitting ? (
                  <span>Processing Order...</span>
                ) : (
                  <>
                    <span>Proceed to Checkout</span>
                    <ArrowRight size={18} />
                  </>
                )}
              </button>

              {!user && (
                <p style={{ textAlign: 'center', fontSize: '0.75rem', color: 'var(--text-muted)', marginTop: 10 }}>
                  You will be prompted to sign in before finalizing.
                </p>
              )}
            </div>
          </>
        )}
      </div>
    </div>
  );
}
