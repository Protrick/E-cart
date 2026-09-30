import React, { createContext, useContext, useState, useEffect } from 'react';
import { api } from '../services/api';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [token, setToken] = useState(() => localStorage.getItem('ecart_token'));
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function loadUser() {
      if (token) {
        try {
          const res = await api.getCurrentUser();
          if (res.success && res.data) {
            setUser(res.data);
          } else {
            logout();
          }
        } catch {
          logout();
        }
      }
      setLoading(false);
    }

    loadUser();

    const handleUnauthorized = () => {
      logout();
    };

    window.addEventListener('ecart_unauthorized', handleUnauthorized);
    return () => window.removeEventListener('ecart_unauthorized', handleUnauthorized);
  }, [token]);

  const login = async (credentials) => {
    const res = await api.login(credentials);
    if (res.success && res.data) {
      localStorage.setItem('ecart_token', res.data.token);
      setToken(res.data.token);
      setUser(res.data.user);
      return res.data;
    }
    throw new Error(res.message || 'Login failed');
  };

  const register = async (userData) => {
    const res = await api.register(userData);
    if (res.success && res.data) {
      localStorage.setItem('ecart_token', res.data.token);
      setToken(res.data.token);
      setUser(res.data.user);
      return res.data;
    }
    throw new Error(res.message || 'Registration failed');
  };

  const logout = () => {
    localStorage.removeItem('ecart_token');
    setToken(null);
    setUser(null);
  };

  const isAdmin = user?.role === 'Admin';

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        loading,
        isAdmin,
        login,
        register,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
