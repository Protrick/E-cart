import React, { useState, useEffect } from 'react';
import { 
  X, 
  Plus, 
  Edit, 
  Trash2, 
  Package, 
  FolderTree, 
  ShoppingBag, 
  Users, 
  Save, 
  AlertTriangle,
  RefreshCw
} from 'lucide-react';
import { api } from '../services/api';
import { useToast } from './Toast';

export function AdminPanel({ isOpen, onClose, categories, onDataChanged }) {
  const { showSuccess, showError } = useToast();
  const [activeTab, setActiveTab] = useState('products'); // 'products', 'categories', 'orders', 'users'

  // Data states
  const [products, setProducts] = useState([]);
  const [orders, setOrders] = useState([]);
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(false);

  // Modals inside Admin
  const [editingProduct, setEditingProduct] = useState(null); // null, 'new', or product object
  const [editingCategory, setEditingCategory] = useState(null); // null or 'new'

  // Product Form state
  const [productForm, setProductForm] = useState({
    name: '',
    description: '',
    sku: '',
    price: '',
    stockQuantity: '',
    imageUrl: '',
    categoryId: '',
    isActive: true,
  });

  // Category Form state
  const [categoryForm, setCategoryForm] = useState({
    name: '',
    description: '',
    slug: '',
    imageUrl: '',
  });

  useEffect(() => {
    if (isOpen) {
      loadTabData(activeTab);
    }
  }, [isOpen, activeTab]);

  const loadTabData = async (tab) => {
    setLoading(true);
    try {
      if (tab === 'products') {
        const res = await api.getProducts({ pageSize: 50 });
        if (res.success && res.data) {
          setProducts(res.data.items);
        }
      } else if (tab === 'orders') {
        const res = await api.getAllOrders();
        if (res.success && res.data) {
          setOrders(res.data);
        }
      } else if (tab === 'users') {
        const res = await api.getUsers();
        if (res.success && res.data) {
          setUsers(res.data);
        }
      }
    } catch (err) {
      showError(err.message || 'Failed to load admin data.');
    } finally {
      setLoading(false);
    }
  };

  if (!isOpen) return null;

  // PRODUCT ACTIONS
  const handleOpenNewProduct = () => {
    setProductForm({
      name: '',
      description: '',
      sku: `SKU-${Math.floor(1000 + Math.random() * 9000)}`,
      price: '',
      stockQuantity: '25',
      imageUrl: 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=800',
      categoryId: categories.length > 0 ? categories[0].id : '',
      isActive: true,
    });
    setEditingProduct('new');
  };

  const handleEditProduct = (prod) => {
    setProductForm({
      name: prod.name,
      description: prod.description,
      sku: prod.sku,
      price: String(prod.price),
      stockQuantity: String(prod.stockQuantity),
      imageUrl: prod.imageUrl || '',
      categoryId: prod.categoryId,
      isActive: prod.isActive,
    });
    setEditingProduct(prod);
  };

  const handleSaveProduct = async (e) => {
    e.preventDefault();
    try {
      const payload = {
        name: productForm.name.trim(),
        description: productForm.description.trim(),
        sku: productForm.sku.trim(),
        price: parseFloat(productForm.price),
        stockQuantity: parseInt(productForm.stockQuantity, 10),
        imageUrl: productForm.imageUrl.trim() || null,
        categoryId: productForm.categoryId,
        isActive: productForm.isActive,
      };

      if (editingProduct === 'new') {
        const res = await api.createProduct(payload);
        if (res.success) {
          showSuccess('Product created successfully!');
        }
      } else {
        const res = await api.updateProduct(editingProduct.id, payload);
        if (res.success) {
          showSuccess('Product updated successfully!');
        }
      }

      setEditingProduct(null);
      loadTabData('products');
      if (onDataChanged) onDataChanged();
    } catch (err) {
      showError(err.message || 'Failed to save product.');
    }
  };

  const handleDeleteProduct = async (id) => {
    if (!window.confirm('Are you sure you want to delete this product?')) return;
    try {
      const res = await api.deleteProduct(id);
      if (res.success) {
        showSuccess('Product deleted successfully.');
        loadTabData('products');
        if (onDataChanged) onDataChanged();
      }
    } catch (err) {
      showError(err.message || 'Failed to delete product.');
    }
  };

  // CATEGORY ACTIONS
  const handleSaveCategory = async (e) => {
    e.preventDefault();
    try {
      const res = await api.createCategory({
        name: categoryForm.name.trim(),
        description: categoryForm.description.trim(),
        slug: categoryForm.slug.trim() || undefined,
        imageUrl: categoryForm.imageUrl.trim() || undefined,
      });

      if (res.success) {
        showSuccess('Category created successfully!');
        setEditingCategory(null);
        if (onDataChanged) onDataChanged();
      }
    } catch (err) {
      showError(err.message || 'Failed to create category.');
    }
  };

  const handleDeleteCategory = async (id) => {
    if (!window.confirm('Are you sure you want to delete this category?')) return;
    try {
      const res = await api.deleteCategory(id);
      if (res.success) {
        showSuccess('Category deleted successfully.');
        if (onDataChanged) onDataChanged();
      }
    } catch (err) {
      showError(err.message || 'Failed to delete category.');
    }
  };

  // ORDER ACTIONS
  const handleUpdateOrderStatus = async (orderId, status) => {
    try {
      const res = await api.updateOrderStatus(orderId, status);
      if (res.success) {
        showSuccess(`Order status updated to ${status}.`);
        loadTabData('orders');
        if (onDataChanged) onDataChanged();
      }
    } catch (err) {
      showError(err.message || 'Failed to update order status.');
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div
        className="modal-content"
        onClick={(e) => e.stopPropagation()}
        style={{ maxWidth: 1000, height: '85vh', display: 'flex', flexDirection: 'column', padding: 0 }}
      >
        {/* Top bar */}
        <div
          style={{
            padding: '20px 24px',
            borderBottom: '1px solid var(--border-subtle)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
          }}
        >
          <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
            <div
              style={{
                width: 36,
                height: 36,
                borderRadius: '10px',
                background: 'rgba(99, 102, 241, 0.2)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: '#818cf8',
              }}
            >
              <Package size={20} />
            </div>
            <div>
              <h2 style={{ fontSize: '1.25rem' }}>E-Commerce Admin Dashboard</h2>
              <p style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>
                Role-based full backend administration
              </p>
            </div>
          </div>

          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <button className="btn-icon" onClick={() => loadTabData(activeTab)}>
              <RefreshCw size={16} />
            </button>
            <button className="btn-icon" onClick={onClose}>
              <X size={18} />
            </button>
          </div>
        </div>

        {/* Tab navigation */}
        <div
          style={{
            display: 'flex',
            gap: 10,
            padding: '12px 24px',
            borderBottom: '1px solid var(--border-subtle)',
            background: 'rgba(15, 23, 42, 0.5)',
          }}
        >
          <button
            className={`btn btn-sm ${activeTab === 'products' ? 'btn-primary' : 'btn-secondary'}`}
            onClick={() => setActiveTab('products')}
          >
            <Package size={15} />
            <span>Products ({products.length})</span>
          </button>
          <button
            className={`btn btn-sm ${activeTab === 'categories' ? 'btn-primary' : 'btn-secondary'}`}
            onClick={() => setActiveTab('categories')}
          >
            <FolderTree size={15} />
            <span>Categories ({categories.length})</span>
          </button>
          <button
            className={`btn btn-sm ${activeTab === 'orders' ? 'btn-primary' : 'btn-secondary'}`}
            onClick={() => setActiveTab('orders')}
          >
            <ShoppingBag size={15} />
            <span>All Orders ({orders.length})</span>
          </button>
          <button
            className={`btn btn-sm ${activeTab === 'users' ? 'btn-primary' : 'btn-secondary'}`}
            onClick={() => setActiveTab('users')}
          >
            <Users size={15} />
            <span>Registered Users ({users.length})</span>
          </button>
        </div>

        {/* Content body */}
        <div style={{ flex: 1, overflowY: 'auto', padding: 24 }}>
          {loading ? (
            <div style={{ padding: 40, textAlign: 'center', color: 'var(--text-muted)' }}>
              Loading data from .NET Backend...
            </div>
          ) : activeTab === 'products' ? (
            <div>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
                <h3 style={{ fontSize: '1.1rem' }}>Products Inventory (CRUD)</h3>
                <button className="btn btn-primary btn-sm" onClick={handleOpenNewProduct}>
                  <Plus size={16} />
                  <span>Add New Product</span>
                </button>
              </div>

              <div style={{ overflowX: 'auto' }}>
                <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.875rem' }}>
                  <thead>
                    <tr style={{ borderBottom: '1px solid var(--border-subtle)', textAlign: 'left', color: 'var(--text-muted)' }}>
                      <th style={{ padding: '10px 12px' }}>Product</th>
                      <th style={{ padding: '10px 12px' }}>SKU</th>
                      <th style={{ padding: '10px 12px' }}>Category</th>
                      <th style={{ padding: '10px 12px' }}>Price</th>
                      <th style={{ padding: '10px 12px' }}>Stock</th>
                      <th style={{ padding: '10px 12px', textAlign: 'right' }}>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {products.map((prod) => (
                      <tr
                        key={prod.id}
                        style={{
                          borderBottom: '1px solid rgba(255, 255, 255, 0.04)',
                          transition: 'background 0.2s',
                        }}
                      >
                        <td style={{ padding: '12px', display: 'flex', alignItems: 'center', gap: 10 }}>
                          <img
                            src={prod.imageUrl || 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=800'}
                            alt={prod.name}
                            style={{ width: 36, height: 36, borderRadius: 6, objectFit: 'cover' }}
                          />
                          <div>
                            <p style={{ fontWeight: 600 }}>{prod.name}</p>
                          </div>
                        </td>
                        <td style={{ padding: '12px', fontFamily: 'monospace', color: 'var(--text-muted)' }}>
                          {prod.sku}
                        </td>
                        <td style={{ padding: '12px' }}>
                          <span className="badge badge-primary">{prod.categoryName}</span>
                        </td>
                        <td style={{ padding: '12px', fontWeight: 600 }}>${Number(prod.price).toFixed(2)}</td>
                        <td style={{ padding: '12px' }}>
                          <span className={`badge ${prod.stockQuantity > 0 ? 'badge-success' : 'badge-danger'}`}>
                            {prod.stockQuantity} in stock
                          </span>
                        </td>
                        <td style={{ padding: '12px', textAlign: 'right' }}>
                          <div style={{ display: 'inline-flex', gap: 6 }}>
                            <button
                              className="btn-icon"
                              onClick={() => handleEditProduct(prod)}
                              title="Edit product"
                            >
                              <Edit size={15} />
                            </button>
                            <button
                              className="btn-icon"
                              onClick={() => handleDeleteProduct(prod.id)}
                              style={{ color: 'var(--danger)' }}
                              title="Delete product"
                            >
                              <Trash2 size={15} />
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          ) : activeTab === 'categories' ? (
            <div>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
                <h3 style={{ fontSize: '1.1rem' }}>Product Categories</h3>
                <button className="btn btn-primary btn-sm" onClick={() => setEditingCategory('new')}>
                  <Plus size={16} />
                  <span>Add Category</span>
                </button>
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))', gap: 16 }}>
                {categories.map((cat) => (
                  <div key={cat.id} className="glass-card" style={{ padding: 18 }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 8 }}>
                      <h4 style={{ fontSize: '1rem' }}>{cat.name}</h4>
                      <button
                        className="btn-icon"
                        onClick={() => handleDeleteCategory(cat.id)}
                        style={{ color: 'var(--danger)', border: 'none' }}
                      >
                        <Trash2 size={14} />
                      </button>
                    </div>
                    <p style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', marginBottom: 12 }}>
                      {cat.description || 'No description'}
                    </p>
                    <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.75rem', color: 'var(--text-muted)' }}>
                      <span>Slug: /{cat.slug}</span>
                      <span className="badge badge-primary">{cat.productCount} products</span>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          ) : activeTab === 'orders' ? (
            <div>
              <h3 style={{ fontSize: '1.1rem', marginBottom: 16 }}>Customer Orders Across Platform</h3>
              <div style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
                {orders.map((order) => (
                  <div key={order.id} className="glass-card" style={{ padding: 18 }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 10 }}>
                      <div>
                        <span style={{ fontFamily: 'monospace', color: 'var(--text-muted)', fontSize: '0.8rem' }}>
                          #{order.id.slice(0, 8)}
                        </span>
                        <h4 style={{ fontSize: '0.95rem', marginTop: 2 }}>
                          Buyer: {order.userName || 'Customer'} ({order.userEmail})
                        </h4>
                      </div>

                      <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                        <span style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>Update Status:</span>
                        <select
                          className="select"
                          value={order.statusText || order.status}
                          onChange={(e) => handleUpdateOrderStatus(order.id, e.target.value)}
                          style={{ width: 'auto', padding: '4px 10px', fontSize: '0.8rem' }}
                        >
                          <option value="Pending">Pending</option>
                          <option value="Processing">Processing</option>
                          <option value="Shipped">Shipped</option>
                          <option value="Delivered">Delivered</option>
                          <option value="Cancelled">Cancelled</option>
                        </select>
                      </div>
                    </div>

                    <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: 8 }}>
                      <span>Items: </span>
                      {order.items.map((i) => `${i.productName} (x${i.quantity})`).join(', ')}
                    </div>

                    <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.85rem', color: 'var(--text-muted)', borderTop: '1px solid rgba(255,255,255,0.05)', paddingTop: 8 }}>
                      <span>Shipping Address: {order.shippingAddress}</span>
                      <span style={{ fontWeight: 700, color: 'var(--primary)' }}>${Number(order.totalAmount).toFixed(2)}</span>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          ) : (
            <div>
              <h3 style={{ fontSize: '1.1rem', marginBottom: 16 }}>Registered Users Directory</h3>
              <div style={{ overflowX: 'auto' }}>
                <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.875rem' }}>
                  <thead>
                    <tr style={{ borderBottom: '1px solid var(--border-subtle)', textAlign: 'left', color: 'var(--text-muted)' }}>
                      <th style={{ padding: '10px 12px' }}>Name</th>
                      <th style={{ padding: '10px 12px' }}>Username</th>
                      <th style={{ padding: '10px 12px' }}>Email</th>
                      <th style={{ padding: '10px 12px' }}>Role</th>
                      <th style={{ padding: '10px 12px' }}>Joined</th>
                    </tr>
                  </thead>
                  <tbody>
                    {users.map((u) => (
                      <tr key={u.id} style={{ borderBottom: '1px solid rgba(255, 255, 255, 0.04)' }}>
                        <td style={{ padding: '12px', fontWeight: 600 }}>{u.firstName} {u.lastName}</td>
                        <td style={{ padding: '12px', fontFamily: 'monospace' }}>{u.username}</td>
                        <td style={{ padding: '12px', color: 'var(--text-secondary)' }}>{u.email}</td>
                        <td style={{ padding: '12px' }}>
                          <span className={`badge ${u.role === 'Admin' ? 'badge-primary' : 'badge-success'}`}>
                            {u.role}
                          </span>
                        </td>
                        <td style={{ padding: '12px', color: 'var(--text-muted)', fontSize: '0.8rem' }}>
                          {new Date(u.createdAtUtc).toLocaleDateString()}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </div>

        {/* Product Add / Edit Modal */}
        {editingProduct && (
          <div className="modal-overlay" onClick={() => setEditingProduct(null)}>
            <div className="modal-content" onClick={(e) => e.stopPropagation()} style={{ maxWidth: 520, padding: 24 }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
                <h3 style={{ fontSize: '1.2rem' }}>
                  {editingProduct === 'new' ? 'Add New Product' : 'Edit Product'}
                </h3>
                <button className="btn-icon" onClick={() => setEditingProduct(null)}>
                  <X size={18} />
                </button>
              </div>

              <form onSubmit={handleSaveProduct}>
                <div className="form-group">
                  <label className="form-label">Product Name</label>
                  <input
                    type="text"
                    className="input"
                    value={productForm.name}
                    onChange={(e) => setProductForm({ ...productForm, name: e.target.value })}
                    required
                  />
                </div>

                <div className="form-group">
                  <label className="form-label">Category</label>
                  <select
                    className="select"
                    value={productForm.categoryId}
                    onChange={(e) => setProductForm({ ...productForm, categoryId: e.target.value })}
                    required
                  >
                    {categories.map((c) => (
                      <option key={c.id} value={c.id}>
                        {c.name}
                      </option>
                    ))}
                  </select>
                </div>

                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: 10 }}>
                  <div className="form-group">
                    <label className="form-label">SKU</label>
                    <input
                      type="text"
                      className="input"
                      value={productForm.sku}
                      onChange={(e) => setProductForm({ ...productForm, sku: e.target.value })}
                      required
                    />
                  </div>
                  <div className="form-group">
                    <label className="form-label">Price ($)</label>
                    <input
                      type="number"
                      step="0.01"
                      className="input"
                      value={productForm.price}
                      onChange={(e) => setProductForm({ ...productForm, price: e.target.value })}
                      required
                    />
                  </div>
                  <div className="form-group">
                    <label className="form-label">Stock Qty</label>
                    <input
                      type="number"
                      className="input"
                      value={productForm.stockQuantity}
                      onChange={(e) => setProductForm({ ...productForm, stockQuantity: e.target.value })}
                      required
                    />
                  </div>
                </div>

                <div className="form-group">
                  <label className="form-label">Image URL</label>
                  <input
                    type="url"
                    className="input"
                    value={productForm.imageUrl}
                    onChange={(e) => setProductForm({ ...productForm, imageUrl: e.target.value })}
                    placeholder="https://images.unsplash.com/..."
                  />
                </div>

                <div className="form-group">
                  <label className="form-label">Description</label>
                  <textarea
                    className="textarea"
                    rows="3"
                    value={productForm.description}
                    onChange={(e) => setProductForm({ ...productForm, description: e.target.value })}
                    required
                  />
                </div>

                <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 10, marginTop: 16 }}>
                  <button type="button" className="btn btn-secondary" onClick={() => setEditingProduct(null)}>
                    Cancel
                  </button>
                  <button type="submit" className="btn btn-primary">
                    <Save size={16} />
                    <span>Save Product</span>
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}

        {/* Category Add Modal */}
        {editingCategory && (
          <div className="modal-overlay" onClick={() => setEditingCategory(null)}>
            <div className="modal-content" onClick={(e) => e.stopPropagation()} style={{ maxWidth: 440, padding: 24 }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
                <h3 style={{ fontSize: '1.2rem' }}>Add New Category</h3>
                <button className="btn-icon" onClick={() => setEditingCategory(null)}>
                  <X size={18} />
                </button>
              </div>

              <form onSubmit={handleSaveCategory}>
                <div className="form-group">
                  <label className="form-label">Category Name</label>
                  <input
                    type="text"
                    className="input"
                    value={categoryForm.name}
                    onChange={(e) => setCategoryForm({ ...categoryForm, name: e.target.value })}
                    required
                  />
                </div>
                <div className="form-group">
                  <label className="form-label">Description</label>
                  <input
                    type="text"
                    className="input"
                    value={categoryForm.description}
                    onChange={(e) => setCategoryForm({ ...categoryForm, description: e.target.value })}
                  />
                </div>
                <div className="form-group">
                  <label className="form-label">Slug (Optional)</label>
                  <input
                    type="text"
                    className="input"
                    placeholder="e.g. smart-devices"
                    value={categoryForm.slug}
                    onChange={(e) => setCategoryForm({ ...categoryForm, slug: e.target.value })}
                  />
                </div>

                <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 10, marginTop: 16 }}>
                  <button type="button" className="btn btn-secondary" onClick={() => setEditingCategory(null)}>
                    Cancel
                  </button>
                  <button type="submit" className="btn btn-primary">
                    <span>Create Category</span>
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
