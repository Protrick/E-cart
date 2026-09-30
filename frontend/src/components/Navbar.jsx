import React, { useState, useEffect } from 'react';
import { 
  ShoppingBag, 
  Search, 
  User as UserIcon, 
  LogOut, 
  ShieldCheck, 
  Package, 
  SlidersHorizontal,
  Server,
  ChevronDown
} from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import { useCart } from '../context/CartContext';
import { api } from '../services/api';

export function Navbar({ 
  searchQuery, 
  setSearchQuery, 
  selectedCategory, 
  setSelectedCategory, 
  categories,
  onOpenAuth,
  onOpenOrders,
  onOpenAdmin 
}) {
  const { user, isAdmin, logout } = useAuth();
  const { totalItems, setIsCartOpen } = useCart();
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const [backendHealth, setBackendHealth] = useState(null);

  useEffect(() => {
    async function checkStatus() {
      try {
        const res = await api.checkHealth();
        if (res.success) {
          setBackendHealth('online');
        } else {
          setBackendHealth('offline');
        }
      } catch {
        setBackendHealth('offline');
      }
    }

    checkStatus();
    const interval = setInterval(checkStatus, 30000);
    return () => clearInterval(interval);
  }, []);

  return (
    <header
      style={{
        position: 'sticky',
        top: 0,
        zIndex: 100,
        background: 'rgba(9, 13, 22, 0.85)',
        backdropFilter: 'blur(20px)',
        borderBottom: '1px solid var(--border-subtle)',
      }}
    >
      <div
        style={{
          maxWidth: 1300,
          margin: '0 auto',
          padding: '14px 20px',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          gap: 20,
        }}
      >
        {/* Brand */}
        <div style={{ display: 'flex', alignItems: 'center', gap: 16 }}>
          <div
            style={{
              display: 'flex',
              alignItems: 'center',
              gap: 10,
              cursor: 'pointer',
            }}
            onClick={() => {
              setSelectedCategory(null);
              setSearchQuery('');
            }}
          >
            <div
              style={{
                width: 40,
                height: 40,
                borderRadius: '12px',
                background: 'linear-gradient(135deg, #6366f1, #06b6d4)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                boxShadow: '0 4px 15px rgba(99, 102, 241, 0.4)',
              }}
            >
              <ShoppingBag size={22} color="white" />
            </div>
            <div>
              <span
                className="brand-font"
                style={{
                  fontSize: '1.4rem',
                  fontWeight: 800,
                  background: 'linear-gradient(to right, #f8fafc, #94a3b8)',
                  WebkitBackgroundClip: 'text',
                  WebkitTextFillColor: 'transparent',
                }}
              >
                E-CART
              </span>
              <div style={{ display: 'flex', alignItems: 'center', gap: 6, marginTop: -2 }}>
                <span
                  style={{
                    width: 6,
                    height: 6,
                    borderRadius: '50%',
                    background: backendHealth === 'online' ? '#10b981' : '#ef4444',
                    boxShadow: backendHealth === 'online' ? '0 0 8px #10b981' : '0 0 8px #ef4444',
                  }}
                />
                <span style={{ fontSize: '0.7rem', color: 'var(--text-muted)' }}>
                  .NET 10 + NeonDB
                </span>
              </div>
            </div>
          </div>
        </div>

        {/* Search Bar */}
        <div
          style={{
            flex: 1,
            maxWidth: 500,
            position: 'relative',
            display: 'flex',
            alignItems: 'center',
          }}
        >
          <Search
            size={18}
            style={{
              position: 'absolute',
              left: 14,
              color: 'var(--text-muted)',
              pointerEvents: 'none',
            }}
          />
          <input
            type="text"
            className="input"
            placeholder="Search products by title, SKU, or specs..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            style={{
              paddingLeft: 42,
              borderRadius: 'var(--radius-full)',
              background: 'rgba(30, 41, 59, 0.6)',
            }}
          />
        </div>

        {/* Actions */}
        <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
          {/* Admin Panel Button */}
          {isAdmin && (
            <button
              className="btn btn-secondary btn-sm"
              onClick={onOpenAdmin}
              style={{
                borderColor: 'rgba(99, 102, 241, 0.4)',
                background: 'rgba(99, 102, 241, 0.1)',
                color: '#818cf8',
              }}
            >
              <ShieldCheck size={16} />
              <span>Admin Panel</span>
            </button>
          )}

          {/* Cart Button */}
          <button
            className="btn btn-secondary"
            onClick={() => setIsCartOpen(true)}
            style={{
              position: 'relative',
              padding: '10px 16px',
            }}
          >
            <ShoppingBag size={18} />
            <span style={{ display: 'none' }}>Cart</span>
            {totalItems > 0 && (
              <span
                style={{
                  position: 'absolute',
                  top: -6,
                  right: -6,
                  background: 'linear-gradient(135deg, #6366f1, #ec4899)',
                  color: 'white',
                  borderRadius: '10px',
                  padding: '2px 7px',
                  fontSize: '0.75rem',
                  fontWeight: 700,
                  boxShadow: '0 2px 10px rgba(236, 72, 153, 0.4)',
                }}
              >
                {totalItems}
              </span>
            )}
          </button>

          {/* Auth Button or User Menu */}
          {user ? (
            <div style={{ position: 'relative' }}>
              <button
                className="btn btn-secondary"
                onClick={() => setIsMenuOpen(!isMenuOpen)}
                style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: 8,
                  padding: '8px 14px',
                }}
              >
                <div
                  style={{
                    width: 26,
                    height: 26,
                    borderRadius: '50%',
                    background: isAdmin ? '#8b5cf6' : '#06b6d4',
                    color: 'white',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    fontSize: '0.8rem',
                    fontWeight: 700,
                  }}
                >
                  {user.firstName ? user.firstName[0].toUpperCase() : 'U'}
                </div>
                <span style={{ fontSize: '0.9rem' }}>{user.firstName || user.username}</span>
                <ChevronDown size={14} />
              </button>

              {isMenuOpen && (
                <div
                  className="glass-card"
                  style={{
                    position: 'absolute',
                    top: '110%',
                    right: 0,
                    width: 220,
                    padding: 8,
                    zIndex: 100,
                    boxShadow: 'var(--shadow-lg)',
                    animation: 'fadeIn 0.15s ease-out',
                  }}
                >
                  <div style={{ padding: '8px 12px', borderBottom: '1px solid var(--border-subtle)' }}>
                    <p style={{ fontWeight: 600, fontSize: '0.9rem' }}>
                      {user.firstName} {user.lastName}
                    </p>
                    <p style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>{user.email}</p>
                    <span
                      className={`badge ${isAdmin ? 'badge-primary' : 'badge-success'}`}
                      style={{ marginTop: 6, fontSize: '0.65rem' }}
                    >
                      {user.role}
                    </span>
                  </div>

                  <div style={{ display: 'flex', flexDirection: 'column', gap: 4, marginTop: 6 }}>
                    <button
                      className="btn"
                      onClick={() => {
                        setIsMenuOpen(false);
                        onOpenOrders();
                      }}
                      style={{
                        justifyContent: 'flex-start',
                        background: 'transparent',
                        padding: '8px 12px',
                        fontSize: '0.85rem',
                        color: 'var(--text-secondary)',
                      }}
                    >
                      <Package size={16} />
                      <span>My Orders</span>
                    </button>

                    {isAdmin && (
                      <button
                        className="btn"
                        onClick={() => {
                          setIsMenuOpen(false);
                          onOpenAdmin();
                        }}
                        style={{
                          justifyContent: 'flex-start',
                          background: 'transparent',
                          padding: '8px 12px',
                          fontSize: '0.85rem',
                          color: '#818cf8',
                        }}
                      >
                        <ShieldCheck size={16} />
                        <span>Admin Dashboard</span>
                      </button>
                    )}

                    <button
                      className="btn btn-danger btn-sm"
                      onClick={() => {
                        setIsMenuOpen(false);
                        logout();
                      }}
                      style={{
                        justifyContent: 'flex-start',
                        marginTop: 4,
                        padding: '8px 12px',
                      }}
                    >
                      <LogOut size={16} />
                      <span>Sign Out</span>
                    </button>
                  </div>
                </div>
              )}
            </div>
          ) : (
            <button className="btn btn-primary" onClick={onOpenAuth}>
              <UserIcon size={16} />
              <span>Sign In</span>
            </button>
          )}
        </div>
      </div>

      {/* Category Pills Bar */}
      <div
        style={{
          borderTop: '1px solid var(--border-subtle)',
          padding: '10px 20px',
          background: 'rgba(15, 23, 42, 0.4)',
        }}
      >
        <div
          style={{
            maxWidth: 1300,
            margin: '0 auto',
            display: 'flex',
            alignItems: 'center',
            gap: 10,
            overflowX: 'auto',
            paddingBottom: 2,
          }}
        >
          <button
            className={`btn btn-sm ${!selectedCategory ? 'btn-primary' : 'btn-secondary'}`}
            onClick={() => setSelectedCategory(null)}
            style={{ borderRadius: 'var(--radius-full)' }}
          >
            All Products
          </button>
          {categories.map((cat) => (
            <button
              key={cat.id}
              className={`btn btn-sm ${selectedCategory === cat.id ? 'btn-primary' : 'btn-secondary'}`}
              onClick={() => setSelectedCategory(cat.id)}
              style={{ borderRadius: 'var(--radius-full)' }}
            >
              {cat.name} ({cat.productCount})
            </button>
          ))}
        </div>
      </div>
    </header>
  );
}
