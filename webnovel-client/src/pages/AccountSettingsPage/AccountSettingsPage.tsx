import React, { useState } from 'react';
import apiClient from '../../api/apiClient';
import { API_ROUTES } from '../../api/apiRoutes';
import './AccountSettingsPage.css'; 

const ChangeUsernameForm: React.FC = () => {
    const [newUsername, setNewUsername] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError(null);
        setSuccess(null);

        try {
            const response = await apiClient.post(API_ROUTES.AUTH.CHANGE_USERNAME, { newUsername });


            const { accessToken, refreshToken } = response.data;


            localStorage.setItem('accessToken', accessToken);
            localStorage.setItem('refreshToken', refreshToken);

            setSuccess('Your new account name is: ' + newUsername);
            setNewUsername('');


        } catch (err: any) {
            setError(err.response?.data?.message || 'Erro when changing nick name.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <form onSubmit={handleSubmit} className="settings-form">
            <div className="form-group">
                <label htmlFor="newUsername">New use name</label>
                <input
                    type="text"
                    id="newUsername"
                    value={newUsername}
                    onChange={(e) => setNewUsername(e.target.value)}
                    required
                    minLength={6}
                />
            </div>

            <div className="form-footer">
                {success && <div className="settings-message success">{success}</div>}
                {error && <div className="settings-message error">{error}</div>}
                <button type="submit" disabled={loading}>
                    {loading ? 'Saving...' : 'Save'}
                </button>
            </div>
        </form>
    );
};

// --- Component Form Password ---
const ChangePasswordForm: React.FC = () => {
    const [oldPassword, setOldPassword] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError(null);
        setSuccess(null);

        if (newPassword !== confirmPassword) {
            setError('New password not match')
            setLoading(false);
            return;
        }

        try {
            await apiClient.post(API_ROUTES.AUTH.CHANGE_PASSWORD, {
                oldPassword,
                newPassword,
                confirmPassword
            });

            setSuccess('Change password sucessfully!');
            setOldPassword('');
            setNewPassword('');
            setConfirmPassword('');

        } catch (err: any) {
            setError(err.response?.data?.message || 'Erro when changing password');
        } finally {
            setLoading(false);
        }
    };

    return (
        <form onSubmit={handleSubmit} className="settings-form">
            <div className="form-group">
                <label htmlFor="oldPassword">Currrent Password</label>
                <input
                    type="password"
                    id="oldPassword"
                    value={oldPassword}
                    onChange={(e) => setOldPassword(e.target.value)}
                    required
                />
            </div>
            <div className="form-group">
                <label htmlFor="newPassword">New Password</label>
                <input
                    type="password"
                    id="newPassword"
                    value={newPassword}
                    onChange={(e) => setNewPassword(e.target.value)}
                    required
                    minLength={6}
                />
            </div>
            <div className="form-group">
                <label htmlFor="confirmPassword">Comfirm New Password</label>
                <input
                    type="password"
                    id="confirmPassword"
                    value={confirmPassword}
                    onChange={(e) => setConfirmPassword(e.target.value)}
                    required
                />
            </div>

            <div className="form-footer">
                {success && <div className="settings-message success">{success}</div>}
                {error && <div className="settings-message error">{error}</div>}
                <button type="submit" disabled={loading}>
                    {loading ? 'Saving...' : 'Save'}
                </button>
            </div>
        </form>
    );
};


// --- Component Trang Chính ---
const AccountSettingsPage: React.FC = () => {
    return (
        <div className="settings-container">
            <div className="settings-box">
                <div className="settings-header">
                    Change UseName
                </div>
                <ChangeUsernameForm />
            </div>

            <div className="settings-box">
                <div className="settings-header">
                    Change Password
                </div>
                <ChangePasswordForm />
            </div>
        </div>
    );
};

export default AccountSettingsPage;