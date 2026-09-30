import React, { useState } from 'react';
import { X, ShoppingBag, Check, Shield, Truck, RotateCcw } from 'lucide-react';
import { useCart } from '../context/CartContext';
import { useToast } from './Toast';

export function ProductDetailModal({ product, onClose }) {
  const { addToCart } = useCart();
  const { showSuccess } = useToast();
  const [quantity, setQuantity] = useState(1);
  const [added, setAdded] = useState(false);

  if (!product) return null;

  const handleAddToCart = () => {
    if (product.stockQuantity <= 0) return;
    addToCart(product, quantity);
    setAdded(true);
    showSuccess(`Added ${quantity}x "${product.name}" to cart`);
    setTimeout(() => {
      setAdded(false);
      onClose();
    }, 1000);
  };

  const isOutOfStock = product.stockQuantity <= 0;

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div
        className="modal-content"
        onClick={(e) => e.stopPropagation()}
        style={{ maxWidth: 800, padding: 0, overflow: 'hidden' }}
      >
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))' }}>
          {/* Image preview */}
          <div style={{ background: '#090d16', position: 'relative', minHeight: 350 }}>
            <img
              src={product.imageUrl || 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=800'}
              alt={product.name}
              style={{ width: '100%', height: '100%', objectFit: 'cover' }}
            />
            <div style={{ position: 'absolute', top: 16, left: 16 }}>
              <span className="badge badge-primary">{product.categoryName}</span>
            </div>
          </div>

          {/* Details */}
          <div style={{ padding: '28px 24px', display: 'flex', flexDirection: 'column' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
              <div>
                <span style={{ fontSize: '0.8rem', color: 'var(--text-muted)', fontFamily: 'monospace' }}>
                  SKU: {product.sku}
                </span>
                <h2 style={{ fontSize: '1.4rem', marginTop: 4, marginBottom: 8 }}>{product.name}</h2>
              </div>
              <button className="btn-icon" onClick={onClose}>
                <X size={20} />
              </button>
            </div>

            <div style={{ display: 'flex', alignItems: 'baseline', gap: 12, margin: '12px 0 16px 0' }}>
              <span className="brand-font" style={{ fontSize: '1.8rem', fontWeight: 800, color: 'var(--primary)' }}>
                ${Number(product.price).toFixed(2)}
              </span>
              <span
                className={`badge ${
                  isOutOfStock ? 'badge-danger' : product.stockQuantity < 10 ? 'badge-warning' : 'badge-success'
                }`}
              >
                {isOutOfStock ? 'Out of Stock' : `${product.stockQuantity} in stock`}
              </span>
            </div>

            <p style={{ color: 'var(--text-secondary)', fontSize: '0.925rem', lineHeight: 1.6, marginBottom: 20 }}>
              {product.description}
            </p>

            {/* Perks */}
            <div
              style={{
                display: 'grid',
                gridTemplateColumns: '1fr 1fr',
                gap: 10,
                padding: '12px 0',
                borderTop: '1px solid var(--border-subtle)',
                borderBottom: '1px solid var(--border-subtle)',
                marginBottom: 20,
              }}
            >
              <div style={{ display: 'flex', alignItems: 'center', gap: 8, fontSize: '0.8rem', color: 'var(--text-muted)' }}>
                <Truck size={16} color="#06b6d4" />
                <span>Fast Global Shipping</span>
              </div>
              <div style={{ display: 'flex', alignItems: 'center', gap: 8, fontSize: '0.8rem', color: 'var(--text-muted)' }}>
                <Shield size={16} color="#10b981" />
                <span>2 Year Warranty</span>
              </div>
            </div>

            {/* Quantity and Add to Cart */}
            <div style={{ marginTop: 'auto' }}>
              <div style={{ display: 'flex', gap: 12, alignItems: 'center' }}>
                <div
                  style={{
                    display: 'flex',
                    alignItems: 'center',
                    background: 'var(--bg-input)',
                    borderRadius: 'var(--radius-sm)',
                    border: '1px solid var(--border-subtle)',
                  }}
                >
                  <button
                    className="btn-icon"
                    onClick={() => setQuantity((q) => Math.max(1, q - 1))}
                    disabled={quantity <= 1 || isOutOfStock}
                    style={{ border: 'none', padding: '10px 14px' }}
                  >
                    -
                  </button>
                  <span style={{ padding: '0 12px', fontWeight: 600, minWidth: 36, textAlign: 'center' }}>
                    {quantity}
                  </span>
                  <button
                    className="btn-icon"
                    onClick={() => setQuantity((q) => Math.min(product.stockQuantity, q + 1))}
                    disabled={quantity >= product.stockQuantity || isOutOfStock}
                    style={{ border: 'none', padding: '10px 14px' }}
                  >
                    +
                  </button>
                </div>

                <button
                  className={`btn ${added ? 'btn-secondary' : 'btn-primary'}`}
                  onClick={handleAddToCart}
                  disabled={isOutOfStock}
                  style={{
                    flex: 1,
                    padding: '12px 20px',
                    background: added ? '#10b981' : undefined,
                  }}
                >
                  {added ? (
                    <>
                      <Check size={18} />
                      <span>Added to Cart!</span>
                    </>
                  ) : (
                    <>
                      <ShoppingBag size={18} />
                      <span>{isOutOfStock ? 'Sold Out' : 'Add to Cart'}</span>
                    </>
                  )}
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
