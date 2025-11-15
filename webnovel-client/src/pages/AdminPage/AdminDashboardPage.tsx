import React, { useState } from 'react';
import { useAuth } from '../../hooks/useAuth';
import { Navigate } from 'react-router-dom';
import './AdminDashboardPage.css';
import {
    FaUsers, FaBook, FaTags, FaChartLine, FaExclamationTriangle, FaCog
} from 'react-icons/fa';
import UserManagementPanel from './UserManagementPanel';


const AdminDashboardPage: React.FC = () => {
    const { user, isLoading } = useAuth();
    const [selectedMenu, setSelectedMenu] = useState('users');

    if (isLoading) {
        return <div className="admin-loading-container">Loading...</div>;
    }

    if (!user || user.role !== 'Admin') {
        return <Navigate to="/" replace />;
    }

    const renderContent = () => {
        switch (selectedMenu) {
            case 'users':
                return <UserManagementPanel />; 
            case 'publication_content':
                return (
                    <>
                        <h2>Publication Content Management</h2>
                        <p>Functionality: Delete Series/Novel/Chapter (AdminNovelController).</p>
                    </>
                );
            case 'publication_content':
                return (
                    <>
                        <h2>Publication Content Management</h2>
                        <p>Functionality: Delete Series/Novel/Chapter (AdminNovelController).</p>
                    </>
                );
            case 'publication_meta':
                return (
                    <>
                        <h2>Category & Tag Management</h2>
                        <p>Functionality: CRUD Categories, Tags, Statuses (AdminNovelController).</p>
                    </>
                );
            default:
                return <h2 className="admin-welcome">Welcome, {user.username}. Select an item from the sidebar.</h2>;
        }
    };

    return (
        <div className="admin-page-container">
            <aside className="admin-sidebar">
                <h1 className="admin-sidebar-header">Admin Dashboard</h1>
                <ul>
                    <li className={selectedMenu === 'users' ? 'active' : ''} onClick={() => setSelectedMenu('users')}>
                        <FaUsers /> <span>User Management</span>
                    </li>
                    <li className={selectedMenu === 'publication_content' ? 'active' : ''} onClick={() => setSelectedMenu('publication_content')}>
                        <FaBook /> <span>Publication Content</span>
                    </li>
                    <li className={selectedMenu === 'publication_meta' ? 'active' : ''} onClick={() => setSelectedMenu('publication_meta')}>
                        <FaTags /> <span>Categories & Tags</span>
                    </li>
                    <li className={selectedMenu === 'reports' ? 'active' : ''} onClick={() => setSelectedMenu('reports')}>
                        <FaExclamationTriangle /> <span>Report Processing</span>
                    </li>
                    <li className="menu-divider"></li>
                    <li className={selectedMenu === 'analytics' ? 'active' : ''} onClick={() => setSelectedMenu('analytics')}>
                        <FaChartLine /> <span>Statistics & Analytics</span>
                    </li>
                    <li className={selectedMenu === 'settings' ? 'active' : ''} onClick={() => setSelectedMenu('settings')}>
                        <FaCog /> <span>System Settings</span>
                    </li>
                </ul>
            </aside>
            <main className="admin-main-content">
                {renderContent()}
            </main>
        </div>
    );
};

export default AdminDashboardPage;