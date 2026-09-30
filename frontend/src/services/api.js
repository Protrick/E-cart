const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

class ApiError extends Error {
  constructor(message, status, errors = []) {
    super(message);
    this.status = status;
    this.errors = errors;
  }
}

async function request(endpoint, options = {}) {
  const token = localStorage.getItem('ecart_token');
  const headers = {
    'Content-Type': 'application/json',
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...options.headers,
  };

  const url = `${API_BASE_URL}${endpoint}`;

  try {
    const response = await fetch(url, {
      ...options,
      headers,
    });

    const data = await response.json().catch(() => null);

    if (!response.ok) {
      if (response.status === 401) {
        // Trigger auth event if token expired
        window.dispatchEvent(new CustomEvent('ecart_unauthorized'));
      }

      const errorMessage = data?.message || `Request failed with status ${response.status}`;
      const errors = data?.errors || [];
      throw new ApiError(errorMessage, response.status, errors);
    }

    return data;
  } catch (err) {
    if (err instanceof ApiError) {
      throw err;
    }
    throw new ApiError(err.message || 'Network error occurred. Make sure backend is running.', 0);
  }
}

export const api = {
  // Health
  checkHealth: () => request('/health'),

  // Auth
  login: (credentials) =>
    request('/auth/login', {
      method: 'POST',
      body: JSON.stringify(credentials),
    }),

  register: (userData) =>
    request('/auth/register', {
      method: 'POST',
      body: JSON.stringify(userData),
    }),

  getCurrentUser: () => request('/auth/me'),

  getUsers: () => request('/auth/users'),

  // Categories
  getCategories: () => request('/categories'),

  getCategoryById: (id) => request(`/categories/${id}`),

  createCategory: (category) =>
    request('/categories', {
      method: 'POST',
      body: JSON.stringify(category),
    }),

  updateCategory: (id, category) =>
    request(`/categories/${id}`, {
      method: 'PUT',
      body: JSON.stringify(category),
    }),

  deleteCategory: (id) =>
    request(`/categories/${id}`, {
      method: 'DELETE',
    }),

  // Products
  getProducts: (params = {}) => {
    const query = new URLSearchParams();
    if (params.search) query.append('search', params.search);
    if (params.categoryId) query.append('categoryId', params.categoryId);
    if (params.minPrice) query.append('minPrice', params.minPrice);
    if (params.maxPrice) query.append('maxPrice', params.maxPrice);
    if (params.inStockOnly) query.append('inStockOnly', 'true');
    if (params.sortBy) query.append('sortBy', params.sortBy);
    if (params.isDescending !== undefined) query.append('isDescending', params.isDescending);
    if (params.pageNumber) query.append('pageNumber', params.pageNumber);
    if (params.pageSize) query.append('pageSize', params.pageSize);

    const queryString = query.toString();
    return request(`/products${queryString ? `?${queryString}` : ''}`);
  },

  getProductById: (id) => request(`/products/${id}`),

  createProduct: (product) =>
    request('/products', {
      method: 'POST',
      body: JSON.stringify(product),
    }),

  updateProduct: (id, product) =>
    request(`/products/${id}`, {
      method: 'PUT',
      body: JSON.stringify(product),
    }),

  deleteProduct: (id) =>
    request(`/products/${id}`, {
      method: 'DELETE',
    }),

  // Orders
  createOrder: (orderData) =>
    request('/orders', {
      method: 'POST',
      body: JSON.stringify(orderData),
    }),

  getMyOrders: () => request('/orders/my-orders'),

  getAllOrders: () => request('/orders'),

  getOrderById: (id) => request(`/orders/${id}`),

  updateOrderStatus: (id, status) =>
    request(`/orders/${id}/status`, {
      method: 'PATCH',
      body: JSON.stringify({ status }),
    }),
};
