import React, { useState } from 'react';
import { X, Lock, Mail, User, Shield, Sparkles } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import { useToast } from './Toast';

export function AuthModal({ isOpen, onClose }) {
  const { login, register } = useAuth();
  const { showSuccess, showError } = useToast();

  const [isLoginTab, setIsLoginTab] = useState(true);
  const [loading, setLoading] = useState(false);

  // Form states
  const [usernameOrEmail, setUsernameOrEmail] = useState('');
  const [password, setPassword] = useState('');
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [role, setRole] = useState('Customer');

  if (!isOpen) return null;

  const handleQuickFill = (type) => {
    if (type === 'admin') {
      setIsLoginTab(true);
      setUsernameOrEmail('admin@ecart.com');
      setPassword('Admin@12345');
    } else {
      setIsLoginTab(true);
      setUsernameOrEmail('customer@ecart.com');
      setPassword('Customer@12345');
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);

    try {
      if (isLoginTab) {
        if (!usernameOrEmail || !password) {
          showError('Please provide username/email and password.');
          setLoading(false);
          return;
        }

        await login({ usernameOrEmail, password });
        showSuccess('Welcome back! Signed in successfully.');
        onClose();
      } else {
        if (!username || !email || !password || !firstName || !lastName) {
          showError('Please fill out all registration fields.');
          setLoading(false);
          return;
        }

        await register({
          username,
          email,
          password,
          firstName,
          lastName,
          role,
        });
        showSuccess('Account registered and signed in!');
        onClose();
      }
    } catch (err) {
      showError(err.message || (err.errors ? err.errors.join(', ') : 'Authentication failed.'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div
        className="modal-content"
        onClick={(e) => e.stopPropagation()}
        style={{ maxWidth: 440, padding: 32 }}
      >
        {/* Header */}
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
          <div>
            <h2 style={{ fontSize: '1.4rem' }}>{isLoginTab ? 'Welcome Back' : 'Create Account'}</h2>
            <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem' }}>
              {isLoginTab ? 'Access your orders and account' : 'Join E-Cart for full store access'}
            </p>
          </div>
          <button className="btn-icon" onClick={onClose}>
            <X size={18} />
          </button>
        </div>

        {/* Quick Demo Logins */}
        <div
          style={{
            background: 'rgba(99, 102, 241, 0.08)',
            border: '1px solid rgba(99, 102, 241, 0.25)',
            borderRadius: 'var(--radius-sm)',
            padding: '12px 14px',
            marginBottom: 20,
          }}
        >
          <div style={{ display: 'flex', alignItems: 'center', gap: 6, marginBottom: 8, fontSize: '0.8rem', color: '#818cf8', fontWeight: 600 }}>
            <Sparkles size={14} />
            <span>Quick Demo Credentials</span>
          </div>
          <div style={{ display: 'flex', gap: 8 }}>
            <button
              type="button"
              className="btn btn-secondary btn-sm"
              onClick={() => handleQuickFill('admin')}
              style={{ flex: 1, fontSize: '0.75rem', padding: '6px 8px' }}
            >
              <Shield size={14} color="#8b5cf6" />
              <span>Admin Demo</span>
            </button>
            <button
              type="button"
              className="btn btn-secondary btn-sm"
              onClick={() => handleQuickFill('customer')}
              style={{ flex: 1, fontSize: '0.75rem', padding: '6px 8px' }}
            >
              <User size={14} color="#06b6d4" />
              <span>Customer Demo</span>
            </button>
          </div>
        </div>

        {/* Tab switch */}
        <div
          style={{
            display: 'flex',
            background: 'var(--bg-input)',
            borderRadius: 'var(--radius-sm)',
            padding: 4,
            marginBottom: 20,
          }}
        >
          <button
            type="button"
            className="btn btn-sm"
            onClick={() => setIsLoginTab(true)}
            style={{
              flex: 1,
              background: isLoginTab ? 'var(--primary)' : 'transparent',
              color: isLoginTab ? 'white' : 'var(--text-secondary)',
              borderRadius: 6,
            }}
          >
            Sign In
          </button>
          <button
            type="button"
            className="btn btn-sm"
            onClick={() => setIsLoginTab(false)}
            style={{
              flex: 1,
              background: !isLoginTab ? 'var(--primary)' : 'transparent',
              color: !isLoginTab ? 'white' : 'var(--text-secondary)',
              borderRadius: 6,
            }}
          >
            Register
          </button>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit}>
          {isLoginTab ? (
            <>
              <div className="form-group">
                <label className="form-label">Username or Email</label>
                <div style={{ position: 'relative' }}>
                  <Mail size={16} style={{ position: 'absolute', left: 12, top: 12, color: 'var(--text-muted)' }} />
                  <input
                    type="text"
                    className="input"
                    placeholder="e.g. admin@ecart.com"
                    style={{ paddingLeft: 38 }}
                    value={usernameOrEmail}
                    onChange={(e) => setUsernameOrEmail(e.target.value)}
                    required
                  />
                </div>
              </div>

              <div className="form-group">
                <label className="form-label">Password</label>
                <div style={{ position: 'relative' }}>
                  <Lock size={16} style={{ position: 'absolute', left: 12, top: 12, color: 'var(--text-muted)' }} />
                  <input
                    type="password"
                    className="input"
                    placeholder="Enter password"
                    style={{ paddingLeft: 38 }}
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    required
                  />
                </div>
              </div>
            </>
          ) : (
            <>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                <div className="form-group">
                  <label className="form-label">First Name</label>
                  <input
                    type="text"
                    className="input"
                    placeholder="First name"
                    value={firstName}
                    onChange={(e) => setFirstName(e.target.value)}
                    required
                  />
                </div>
                <div className="form-group">
                  <label className="form-label">Last Name</label>
                  <input
                    type="text"
                    className="input"
                    placeholder="Last name"
                    value={lastName}
                    onChange={(e) => setLastName(e.target.value)}
                    required
                  />
                </div>
              </div>

              <div className="form-group">
                <label className="form-label">Username</label>
                <input
                  type="text"
                  className="input"
                  placeholder="Choose username"
                  value={username}
                  onChange={(e) => setUsername(e.target.value)}
                  required
                />
              </div>

              <div className="form-group">
                <label className="form-label">Email Address</label>
                <input
                  type="email"
                  className="input"
                  placeholder="name@domain.com"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  required
                />
              </div>

              <div className="form-group">
                <label className="form-label">Password</label>
                <input
                  type="password"
                  className="input"
                  placeholder="At least 6 characters"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  required
                />
              </div>

              <div className="form-group">
                <label className="form-label">Role</label>
                <select className="select" value={role} onChange={(e) => setRole(e.target.value)}>
                  <option value="Customer">Customer (Standard User)</option>
                  <option value="Admin">Admin (Full Control)</option>
                </select>
              </div>
            </>
          )}

          <button
            type="submit"
            className="btn btn-primary"
            disabled={loading}
            style={{ width: '100%', marginTop: 8, padding: '12px' }}
          >
            {loading ? 'Authenticating...' : isLoginTab ? 'Sign In with JWT' : 'Create Account'}
          </button>
        </form>
      </div>
    </div>
  );
}
