import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import apiClient from '../../api/apiClient';
import { API_ROUTES } from '../../api/apiRoutes';
import '../AuthForm.css';


const RegisterPage: React.FC = () => {
    const [userName, setUserName] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');

    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);
    const [loading, setLoading] = useState(false);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError(null);
        setSuccess(null);

        //Check matching password
        if (password !== confirmPassword) {
            setError("Password not match");
            setLoading(false);
            return;
        }

        try {
            await apiClient.post(API_ROUTES.AUTH.REGISTER, {
                userName,
                email,
                password,
                confirmPassword
            });

            setSuccess("Sign Up successfully! Check your email to confirm account");
            setLoading(false);

            setUserName('');
            setEmail('');
            setPassword('');
            setConfirmPassword('');

        } catch (err: any) {
            console.error("Registration failed:", err);
            if (err.response && err.response.data) {
   
                setError(err.response.data.message || "Register failed");
            } else {
                setError("Register failed, check your connection");
            }
            setLoading(false);
        }
    };

    return (
        <div className="auth-container">
            <form onSubmit={handleSubmit} className="auth-form">
                <h2>Register Form</h2>

                {error && <div className="auth-error">{error}</div>}
                {success && <div className="auth-success">{success}</div>}

                <div className="form-group">
                    <label htmlFor="username">UserName</label>
                    <input
                        type="text"
                        id="username"
                        value={userName}
                        onChange={(e) => setUserName(e.target.value)}
                        required
                    />
                </div>

                <div className="form-group">
                    <label htmlFor="email">Email</label>
                    <input
                        type="email"
                        id="email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        required
                    />
                </div>

                <div className="form-group">
                    <label htmlFor="password">Password</label>
                    <input
                        type="password"
                        id="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                    />
                </div>

                <div className="form-group">
                    <label htmlFor="confirmPassword">Comfirm Password</label>
                    <input
                        type="password"
                        id="confirmPassword"
                        value={confirmPassword}
                        onChange={(e) => setConfirmPassword(e.target.value)}
                        required
                    />
                </div>

                <button type="submit" disabled={loading}>
                    {loading ? 'Loading...' : 'Sign Up'}
                </button>

                <div className="form-links">
                    <Link to="/login">Click here to Login Page</Link>
                </div>
            </form>
        </div>
    );
};

export default RegisterPage;