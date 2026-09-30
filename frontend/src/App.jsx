import React, { useState, useEffect } from 'react';
import { Navbar } from './components/Navbar';
import { ProductCard } from './components/ProductCard';
import { ProductDetailModal } from './components/ProductDetailModal';
import { CartDrawer } from './components/CartDrawer';
import { AuthModal } from './components/AuthModal';
import { OrdersModal } from './components/OrdersModal';
import { AdminPanel } from './components/AdminPanel';
import { ToastProvider, useToast } from './components/Toast';
import { AuthProvider, useAuth } from './context/AuthContext';
import { CartProvider } from './context/CartContext';
import { api } from './services/api';
import { 
  Sparkles, 
  ArrowUpDown, 
  Filter, 
  ShieldCheck, 
  Layers, 
  Database, 
  Zap,
  CheckCircle,
  PackageX
} from 'lucide-react';

function Storefront() {
  const { user, isAdmin } = useAuth();
  const { showError, showInfo } = useToast();

  // State
  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(true);

  // Filters & Search
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedCategory, setSelectedCategory] = useState(null);
  const [sortBy, setSortBy] = useState('createdAt');
  const [isDescending, setIsDescending] = useState(true);
  const [inStockOnly, setInStockOnly] = useState(false);

  // Modals
  const [selectedProduct, setSelectedProduct] = useState(null);
  const [isAuthOpen, setIsAuthOpen] = useState(false);
  const [isOrdersOpen, setIsOrdersOpen] = useState(false);
  const [isAdminOpen, setIsAdminOpen] = useState(false);

  // Load categories
  useEffect(() => {
    async function fetchCategories() {
      try {
        const res = await api.getCategories();
        if (res.success && res.data) {
          setCategories(res.data);
        }
      } catch (err) {
        console.error('Failed to load categories', err);
      }
    }
    fetchCategories();
  }, []);

  // Load products when filters change
  const fetchProducts = async () => {
    setLoading(true);
    try {
      const res = await api.getProducts({
        search: searchQuery,
        categoryId: selectedCategory,
        sortBy,
        isDescending,
        inStockOnly,
        pageSize: 24,
      });

      if (res.success && res.data) {
        setProducts(res.data.items);
      }
    } catch (err) {
      showError(err.message || 'Failed to fetch products from backend.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    const timer = setTimeout(() => {
      fetchProducts();
    }, 250); // Debounce search

    return () => clearTimeout(timer);
  }, [searchQuery, selectedCategory, sortBy, isDescending, inStockOnly]);

  const handleDataChanged = () => {
    fetchProducts();
    api.getCategories().then((res) => {
      if (res.success && res.data) setCategories(res.data);
    });
  };

  return (
    <div className="app-container">
      <div className="ambient-glow" />

      {/* Navigation */}
      <Navbar
        searchQuery={searchQuery}
        setSearchQuery={setSearchQuery}
        selectedCategory={selectedCategory}
        setSelectedCategory={setSelectedCategory}
        categories={categories}
        onOpenAuth={() => setIsAuthOpen(true)}
        onOpenOrders={() => {
          if (!user) {
            setIsAuthOpen(true);
          } else {
            setIsOrdersOpen(true);
          }
        }}
        onOpenAdmin={() => setIsAdminOpen(true)}
      />

      {/* Main Container */}
      <main style={{ maxWidth: 1300, margin: '0 auto', padding: '32px 20px', width: '100%' }}>
        {/* Hero Section */}
        <section
          className="glass-card"
          style={{
            padding: '40px 36px',
            marginBottom: 36,
            borderRadius: 'var(--radius-lg)',
            background: 'linear-gradient(135deg, rgba(30, 41, 59, 0.7) 0%, rgba(15, 23, 42, 0.9) 100%)',
            border: '1px solid rgba(255, 255, 255, 0.1)',
            position: 'relative',
            overflow: 'hidden',
          }}
        >
          <div style={{ maxWidth: 650, position: 'relative', zIndex: 1 }}>
            <div style={{ display: 'inline-flex', alignItems: 'center', gap: 8, marginBottom: 12 }}>
              <span className="badge badge-primary" style={{ padding: '6px 12px' }}>
                <Sparkles size={13} />
                <span>Modern Clean Architecture & Neon PostgreSQL</span>
              </span>
            </div>
            <h1 style={{ fontSize: '2.5rem', lineHeight: 1.15, marginBottom: 14 }}>
              Next-Generation Commerce Engine
            </h1>
            <p style={{ color: 'var(--text-secondary)', fontSize: '1.05rem', lineHeight: 1.6, marginBottom: 24 }}>
              Powered by a high-throughput C# .NET 10 Web API backend, Neon PostgreSQL relational database, JWT claims security, and centralized rate limiting.
            </p>

            <div style={{ display: 'flex', flexWrap: 'wrap', gap: 12 }}>
              <div
                style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: 8,
                  fontSize: '0.85rem',
                  padding: '8px 14px',
                  borderRadius: 'var(--radius-sm)',
                  background: 'rgba(255, 255, 255, 0.05)',
                  border: '1px solid var(--border-subtle)',
                }}
              >
                <Database size={15} color="#06b6d4" />
                <span>NeonDB PostgreSQL</span>
              </div>
              <div
                style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: 8,
                  fontSize: '0.85rem',
                  padding: '8px 14px',
                  borderRadius: 'var(--radius-sm)',
                  background: 'rgba(255, 255, 255, 0.05)',
                  border: '1px solid var(--border-subtle)',
                }}
              >
                <ShieldCheck size={15} color="#10b981" />
                <span>JWT Role-Based Auth</span>
              </div>
              <div
                style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: 8,
                  fontSize: '0.85rem',
                  padding: '8px 14px',
                  borderRadius: 'var(--radius-sm)',
                  background: 'rgba(255, 255, 255, 0.05)',
                  border: '1px solid var(--border-subtle)',
                }}
              >
                <Zap size={15} color="#f59e0b" />
                <span>Rate Limiting & Serilog</span>
              </div>
            </div>
          </div>
        </section>

        {/* Filter Controls Bar */}
        <section
          style={{
            display: 'flex',
            flexWrap: 'wrap',
            alignItems: 'center',
            justifyContent: 'space-between',
            gap: 16,
            marginBottom: 24,
            padding: '12px 18px',
            background: 'rgba(15, 23, 42, 0.6)',
            borderRadius: 'var(--radius-sm)',
            border: '1px solid var(--border-subtle)',
          }}
        >
          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <Filter size={16} color="var(--text-muted)" />
            <span style={{ fontSize: '0.9rem', fontWeight: 600 }}>
              {products.length} Products Found
            </span>
          </div>

          <div style={{ display: 'flex', flexWrap: 'wrap', alignItems: 'center', gap: 14 }}>
            {/* In stock toggle */}
            <label style={{ display: 'flex', alignItems: 'center', gap: 8, cursor: 'pointer', fontSize: '0.85rem' }}>
              <input
                type="checkbox"
                checked={inStockOnly}
                onChange={(e) => setInStockOnly(e.target.checked)}
                style={{ accentColor: 'var(--primary)', cursor: 'pointer' }}
              />
              <span style={{ color: 'var(--text-secondary)' }}>In Stock Only</span>
            </label>

            {/* Sorting */}
            <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
              <span style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>Sort by:</span>
              <select
                className="select"
                value={`${sortBy}-${isDescending}`}
                onChange={(e) => {
                  const [field, desc] = e.target.value.split('-');
                  setSortBy(field);
                  setIsDescending(desc === 'true');
                }}
                style={{ width: 'auto', padding: '6px 12px', fontSize: '0.85rem' }}
              >
                <option value="createdAt-true">Newest First</option>
                <option value="price-false">Price: Low to High</option>
                <option value="price-true">Price: High to Low</option>
                <option value="name-false">Product Name (A-Z)</option>
                <option value="stock-true">Highest Stock</option>
              </select>
            </div>
          </div>
        </section>

        {/* Product Grid */}
        {loading ? (
          <div
            style={{
              padding: 80,
              textAlign: 'center',
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
              justifyContent: 'center',
            }}
          >
            <div
              style={{
                width: 40,
                height: 40,
                border: '3px solid rgba(99, 102, 241, 0.2)',
                borderTopColor: 'var(--primary)',
                borderRadius: '50%',
                animation: 'spin 1s linear infinite',
                marginBottom: 16,
              }}
            />
            <p style={{ color: 'var(--text-muted)' }}>Querying backend API...</p>
          </div>
        ) : products.length === 0 ? (
          <div
            className="glass-card"
            style={{
              padding: 60,
              textAlign: 'center',
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
              margin: '20px 0',
            }}
          >
            <PackageX size={54} color="var(--text-muted)" style={{ marginBottom: 16 }} />
            <h3 style={{ fontSize: '1.3rem', marginBottom: 8 }}>No Products Found</h3>
            <p style={{ color: 'var(--text-secondary)', maxWidth: 450, marginBottom: 20 }}>
              No products match your current search and filter criteria. Try clearing filters or resetting the category.
            </p>
            <button
              className="btn btn-secondary"
              onClick={() => {
                setSearchQuery('');
                setSelectedCategory(null);
                setInStockOnly(false);
              }}
            >
              Reset All Filters
            </button>
          </div>
        ) : (
          <div
            style={{
              display: 'grid',
              gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))',
              gap: 24,
            }}
          >
            {products.map((product) => (
              <ProductCard
                key={product.id}
                product={product}
                onSelect={(prod) => setSelectedProduct(prod)}
              />
            ))}
          </div>
        )}
      </main>

      {/* Modals & Overlays */}
      <ProductDetailModal
        product={selectedProduct}
        onClose={() => setSelectedProduct(null)}
      />

      <CartDrawer
        onOpenAuth={() => setIsAuthOpen(true)}
        onOrderPlaced={fetchProducts}
      />

      <AuthModal
        isOpen={isAuthOpen}
        onClose={() => setIsAuthOpen(false)}
      />

      <OrdersModal
        isOpen={isOrdersOpen}
        onClose={() => setIsOrdersOpen(false)}
      />

      <AdminPanel
        isOpen={isAdminOpen}
        onClose={() => setIsAdminOpen(false)}
        categories={categories}
        onDataChanged={handleDataChanged}
      />

      {/* Footer */}
      <footer
        style={{
          marginTop: 'auto',
          borderTop: '1px solid var(--border-subtle)',
          padding: '40px 20px 30px',
          background: 'rgba(9, 13, 22, 0.95)',
          color: 'var(--text-muted)',
          fontSize: '0.85rem',
        }}
      >
        <div
          style={{
            maxWidth: 1300,
            margin: '0 auto',
            display: 'flex',
            flexWrap: 'wrap',
            justifyContent: 'space-between',
            alignItems: 'center',
            gap: 20,
          }}
        >
          <div>
            <span className="brand-font" style={{ color: 'var(--text-primary)', fontSize: '1.1rem', fontWeight: 700 }}>
              E-CART ENTERPRISE
            </span>
            <p style={{ marginTop: 4 }}>
              C# .NET Web API • Neon PostgreSQL • Clean Architecture • React
            </p>
          </div>

          <div style={{ display: 'flex', gap: 20 }}>
            <span>Swagger OpenAPI: <a href="http://localhost:5000" target="_blank" rel="noreferrer" style={{ color: 'var(--primary)', textDecoration: 'none' }}>http://localhost:5000</a></span>
            <span>Health: <a href="http://localhost:5000/api/health" target="_blank" rel="noreferrer" style={{ color: '#10b981', textDecoration: 'none' }}>/api/health</a></span>
          </div>
        </div>
      </footer>
    </div>
  );
}

export default function App() {
  return (
    <ToastProvider>
      <AuthProvider>
        <CartProvider>
          <Storefront />
        </CartProvider>
      </AuthProvider>
    </ToastProvider>
  );
}
