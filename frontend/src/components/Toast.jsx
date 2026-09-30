import React, { createContext, useContext, useState, useCallback } from 'react';
import { CheckCircle2, AlertTriangle, AlertCircle, Info, X } from 'lucide-react';

const ToastContext = createContext(null);

export function ToastProvider({ children }) {
  const [toasts, setToasts] = useState([]);

  const addToast = useCallback((message, type = 'info', duration = 4000) => {
    const id = Date.now() + Math.random().toString();
    setToasts((prev) => [...prev, { id, message, type }]);

    if (duration > 0) {
      setTimeout(() => {
        removeToast(id);
      }, duration);
    }
  }, []);

  const removeToast = useCallback((id) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  }, []);

  const showSuccess = (msg) => addToast(msg, 'success');
  const showError = (msg) => addToast(msg, 'error', 5000);
  const showWarning = (msg) => addToast(msg, 'warning', 5000);
  const showInfo = (msg) => addToast(msg, 'info');

  return (
    <ToastContext.Provider value={{ addToast, removeToast, showSuccess, showError, showWarning, showInfo }}>
      {children}
      <div
        style={{
          position: 'fixed',
          bottom: 24,
          right: 24,
          display: 'flex',
          flexDirection: 'column',
          gap: 10,
          zIndex: 9999,
          maxWidth: 400,
          width: '100%',
          pointerEvents: 'none',
        }}
      >
        {toasts.map((t) => (
          <div
            key={t.id}
            style={{
              pointerEvents: 'auto',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'space-between',
              gap: 12,
              padding: '12px 16px',
              borderRadius: '10px',
              background: '#1e293b',
              border: `1px solid ${
                t.type === 'success'
                  ? '#10b981'
                  : t.type === 'error'
                  ? '#ef4444'
                  : t.type === 'warning'
                  ? '#f59e0b'
                  : '#6366f1'
              }`,
              color: '#f8fafc',
              boxShadow: '0 10px 25px -5px rgba(0,0,0,0.5)',
              fontSize: '0.9rem',
              animation: 'fadeIn 0.2s ease-out',
            }}
          >
            <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
              {t.type === 'success' && <CheckCircle2 size={18} color="#10b981" />}
              {t.type === 'error' && <AlertCircle size={18} color="#ef4444" />}
              {t.type === 'warning' && <AlertTriangle size={18} color="#f59e0b" />}
              {t.type === 'info' && <Info size={18} color="#6366f1" />}
              <span>{t.message}</span>
            </div>
            <button
              onClick={() => removeToast(t.id)}
              style={{
                background: 'transparent',
                border: 'none',
                color: '#94a3b8',
                cursor: 'pointer',
                display: 'flex',
                alignItems: 'center',
              }}
            >
              <X size={16} />
            </button>
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  );
}

export function useToast() {
  const context = useContext(ToastContext);
  if (!context) {
    throw new Error('useToast must be used within a ToastProvider');
  }
  return context;
}
