import React, { useState, useEffect } from 'react';
import { X, Package, Clock, MapPin, DollarSign, ChevronRight, AlertCircle } from 'lucide-react';
import { api } from '../services/api';
import { useToast } from './Toast';

export function OrdersModal({ isOpen, onClose }) {
  const { showError } = useToast();
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (isOpen) {
      loadOrders();
    }
  }, [isOpen]);

  const loadOrders = async () => {
    setLoading(true);
    try {
      const res = await api.getMyOrders();
      if (res.success && res.data) {
        setOrders(res.data);
      }
    } catch (err) {
      showError(err.message || 'Failed to fetch your orders.');
    } finally {
      setLoading(false);
    }
  };

  if (!isOpen) return null;

  const getStatusBadge = (status) => {
    const s = String(status).toLowerCase();
    if (s === 'delivered') return <span className="badge badge-success">Delivered</span>;
    if (s === 'shipped') return <span className="badge badge-primary">Shipped</span>;
    if (s === 'processing') return <span className="badge badge-warning">Processing</span>;
    if (s === 'cancelled') return <span className="badge badge-danger">Cancelled</span>;
    return <span className="badge badge-warning">Pending</span>;
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div
        className="modal-content"
        onClick={(e) => e.stopPropagation()}
        style={{ maxWidth: 700, padding: 28 }}
      >
        {/* Header */}
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <Package size={22} color="var(--primary)" />
            <h2 style={{ fontSize: '1.3rem' }}>My Order History</h2>
          </div>
          <button className="btn-icon" onClick={onClose}>
            <X size={18} />
          </button>
        </div>

        {/* Content */}
        {loading ? (
          <div style={{ padding: 40, textAlign: 'center', color: 'var(--text-muted)' }}>
            Loading your order records...
          </div>
        ) : orders.length === 0 ? (
          <div
            style={{
              padding: 40,
              textAlign: 'center',
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
              color: 'var(--text-muted)',
            }}
          >
            <Package size={48} style={{ opacity: 0.3, marginBottom: 16 }} />
            <h3 style={{ color: 'var(--text-primary)', marginBottom: 6 }}>No Orders Yet</h3>
            <p style={{ fontSize: '0.9rem' }}>When you complete a purchase, your orders will show up here.</p>
          </div>
        ) : (
          <div style={{ display: 'flex', flexDirection: 'column', gap: 16, maxHeight: '65vh', overflowY: 'auto' }}>
            {orders.map((order) => (
              <div
                key={order.id}
                className="glass-card"
                style={{
                  padding: 18,
                  border: '1px solid var(--border-subtle)',
                  borderRadius: 'var(--radius-sm)',
                }}
              >
                <div
                  style={{
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'flex-start',
                    marginBottom: 12,
                    paddingBottom: 10,
                    borderBottom: '1px solid var(--border-subtle)',
                  }}
                >
                  <div>
                    <span style={{ fontSize: '0.8rem', fontFamily: 'monospace', color: 'var(--text-muted)' }}>
                      Order #{order.id.slice(0, 8)}
                    </span>
                    <div style={{ display: 'flex', alignItems: 'center', gap: 6, marginTop: 2, fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
                      <Clock size={13} />
                      <span>{new Date(order.createdAtUtc).toLocaleDateString()} at {new Date(order.createdAtUtc).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</span>
                    </div>
                  </div>
                  {getStatusBadge(order.statusText || order.status)}
                </div>

                {/* Items */}
                <div style={{ display: 'flex', flexDirection: 'column', gap: 10, marginBottom: 12 }}>
                  {order.items.map((item) => (
                    <div key={item.id} style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                      <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                        <img
                          src={item.productImageUrl || 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=800'}
                          alt={item.productName}
                          style={{ width: 36, height: 36, borderRadius: 6, objectFit: 'cover' }}
                        />
                        <div>
                          <p style={{ fontSize: '0.85rem', fontWeight: 500 }}>{item.productName}</p>
                          <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>
                            Qty: {item.quantity} × ${Number(item.unitPrice).toFixed(2)}
                          </span>
                        </div>
                      </div>
                      <span style={{ fontWeight: 600, fontSize: '0.9rem' }}>
                        ${Number(item.totalPrice).toFixed(2)}
                      </span>
                    </div>
                  ))}
                </div>

                {/* Footer details */}
                <div
                  style={{
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                    paddingTop: 10,
                    borderTop: '1px solid rgba(255, 255, 255, 0.05)',
                    fontSize: '0.85rem',
                  }}
                >
                  <div style={{ display: 'flex', alignItems: 'center', gap: 6, color: 'var(--text-muted)' }}>
                    <MapPin size={14} />
                    <span style={{ maxWidth: 300, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                      {order.shippingAddress}
                    </span>
                  </div>
                  <div style={{ display: 'flex', alignItems: 'baseline', gap: 6 }}>
                    <span style={{ color: 'var(--text-muted)' }}>Total:</span>
                    <span className="brand-font" style={{ fontSize: '1.1rem', fontWeight: 700, color: 'var(--primary)' }}>
                      ${Number(order.totalAmount).toFixed(2)}
                    </span>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
