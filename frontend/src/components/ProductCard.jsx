import React, { useState } from 'react';
import { Plus, Check, Eye } from 'lucide-react';
import { useCart } from '../context/CartContext';
import { useToast } from './Toast';

export function ProductCard({ product, onSelect }) {
  const { addToCart } = useCart();
  const { showSuccess } = useToast();
  const [added, setAdded] = useState(false);

  const handleAddToCart = (e) => {
    e.stopPropagation();
    if (product.stockQuantity <= 0) return;

    addToCart(product, 1);
    setAdded(true);
    showSuccess(`Added "${product.name}" to cart`);
    setTimeout(() => setAdded(false), 1200);
  };

  const isOutOfStock = product.stockQuantity <= 0;

  return (
    <div
      className="glass-card"
      onClick={() => onSelect(product)}
      style={{
        display: 'flex',
        flexDirection: 'column',
        borderRadius: 'var(--radius-md)',
        overflow: 'hidden',
        cursor: 'pointer',
        position: 'relative',
        transition: 'transform 0.25s ease, box-shadow 0.25s ease, border-color 0.25s ease',
      }}
      onMouseEnter={(e) => {
        e.currentTarget.style.transform = 'translateY(-6px)';
        e.currentTarget.style.boxShadow = '0 20px 30px -10px rgba(0, 0, 0, 0.7)';
        e.currentTarget.style.borderColor = 'rgba(99, 102, 241, 0.4)';
      }}
      onMouseLeave={(e) => {
        e.currentTarget.style.transform = 'translateY(0)';
        e.currentTarget.style.boxShadow = 'var(--shadow-md)';
        e.currentTarget.style.borderColor = 'var(--border-subtle)';
      }}
    >
      {/* Product Image */}
      <div
        style={{
          position: 'relative',
          width: '100%',
          paddingTop: '75%', // 4:3 Aspect ratio
          background: '#0f172a',
          overflow: 'hidden',
        }}
      >
        <img
          src={product.imageUrl || 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=800'}
          alt={product.name}
          style={{
            position: 'absolute',
            top: 0,
            left: 0,
            width: '100%',
            height: '100%',
            objectFit: 'cover',
            transition: 'transform 0.4s ease',
          }}
          onMouseEnter={(e) => (e.currentTarget.style.transform = 'scale(1.05)')}
          onMouseLeave={(e) => (e.currentTarget.style.transform = 'scale(1)')}
          loading="lazy"
        />

        {/* Category & Stock Badges */}
        <div
          style={{
            position: 'absolute',
            top: 12,
            left: 12,
            display: 'flex',
            gap: 6,
            zIndex: 2,
          }}
        >
          <span className="badge badge-primary" style={{ backdropFilter: 'blur(8px)' }}>
            {product.categoryName || 'General'}
          </span>
        </div>

        <div
          style={{
            position: 'absolute',
            top: 12,
            right: 12,
            zIndex: 2,
          }}
        >
          {isOutOfStock ? (
            <span className="badge badge-danger">Out of Stock</span>
          ) : product.stockQuantity < 10 ? (
            <span className="badge badge-warning">Low: {product.stockQuantity} left</span>
          ) : (
            <span className="badge badge-success">In Stock</span>
          )}
        </div>
      </div>

      {/* Content */}
      <div
        style={{
          padding: '16px 18px',
          display: 'flex',
          flexDirection: 'column',
          flex: 1,
          justifyContent: 'space-between',
        }}
      >
        <div>
          <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)', fontFamily: 'monospace' }}>
            SKU: {product.sku}
          </span>
          <h3
            style={{
              fontSize: '1.05rem',
              fontWeight: 600,
              marginTop: 4,
              marginBottom: 6,
              color: 'var(--text-primary)',
              lineHeight: 1.35,
              display: '-webkit-box',
              WebkitLineClamp: 2,
              WebkitBoxOrient: 'vertical',
              overflow: 'hidden',
            }}
          >
            {product.name}
          </h3>
          <p
            style={{
              fontSize: '0.85rem',
              color: 'var(--text-secondary)',
              display: '-webkit-box',
              WebkitLineClamp: 2,
              WebkitBoxOrient: 'vertical',
              overflow: 'hidden',
              marginBottom: 14,
            }}
          >
            {product.description}
          </p>
        </div>

        {/* Price & Action */}
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            borderTop: '1px solid var(--border-subtle)',
            paddingTop: 12,
            marginTop: 'auto',
          }}
        >
          <div>
            <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Price</span>
            <p
              className="brand-font"
              style={{
                fontSize: '1.25rem',
                fontWeight: 700,
                color: 'var(--text-primary)',
              }}
            >
              ${Number(product.price).toFixed(2)}
            </p>
          </div>

          <button
            className={`btn btn-sm ${added ? 'btn-secondary' : 'btn-primary'}`}
            onClick={handleAddToCart}
            disabled={isOutOfStock}
            style={{
              opacity: isOutOfStock ? 0.5 : 1,
              cursor: isOutOfStock ? 'not-allowed' : 'pointer',
              background: added ? '#10b981' : undefined,
              borderColor: added ? '#10b981' : undefined,
            }}
          >
            {added ? (
              <>
                <Check size={16} />
                <span>Added</span>
              </>
            ) : (
              <>
                <Plus size={16} />
                <span>Add</span>
              </>
            )}
          </button>
        </div>
      </div>
    </div>
  );
}
