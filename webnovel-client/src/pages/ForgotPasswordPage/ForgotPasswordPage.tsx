import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import apiClient from '../../api/apiClient';
import { API_ROUTES } from '../../api/apiRoutes';
import '../AuthForm.css';

const ForgotPasswordPage: React.FC = () => {
    const [email, setEmail] = useState('');
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);
    const [loading, setLoading] = useState(false);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError(null);
        setSuccess(null);

        try {
            await apiClient.post(API_ROUTES.AUTH.FORGOT_PASSWORD, { email });

            
            setSuccess("Email sending, check your mail box");
            setEmail('');
        } catch (err: any) {
            console.error("Forgot password failed:", err);
            setError("Erro.");
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="auth-container">
            <form onSubmit={handleSubmit} className="auth-form">
                <h2>Forgot Password</h2>

                {error && <div className="auth-error">{error}</div>}
                {success && <div className="auth-success">{success}</div>}

                {!success && (
                    <p style={{ fontSize: '0.9rem', textAlign: 'center', color: 'var(--text-secondary)' }}>
                        Enter your email account to recieve new password.
                    </p>
                )}

                <div className="form-group">
                    <label htmlFor="email">Email</label>
                    <input
                        type="email"
                        id="email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        required
                        disabled={!!success}
                    />
                </div>

                <button type="submit" disabled={loading || !!success}>
                    {loading ? 'Sending...' : 'Sending new password'}
                </button>

                <div className="form-links">
                    <Link to="/login">Login</Link>
                </div>
            </form>
        </div>
    );
};

export default ForgotPasswordPage;