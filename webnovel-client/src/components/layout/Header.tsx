import React, { useState, useEffect, useRef } from 'react';
import './CSS/Header.css';
import {
    FaSearch,
    FaHeart,
    FaBell,
    FaUserCircle,
    FaHistory,
    FaBookmark,
    FaCog,
    FaSignOutAlt,
    FaPencilAlt,
    FaUser,
} from 'react-icons/fa';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';

// --- User Dropdown Menu Component ---
interface UserMenuProps {
    onLogout: () => void;
    onClose: () => void;
    user: any;
}

const UserDropdownMenu: React.FC<UserMenuProps> = ({ onLogout, onClose, user }) => {
    return (
        <div className="user-dropdown-menu">
            <ul>
                {/* ProfilePage */}
                <li>
                    <Link to="/profile" onClick={onClose}>
                        <FaUser />
                        <span> Account Profile </span>
                    </Link>
                </li>
                {/* ReadingHistoryPage */}
                <li>
                    <Link to="/history" onClick={onClose}>
                        <FaHistory />
                        <span> Reading History </span>
                    </Link>
                </li>
                {/* Bookmarks */}
                <li>
                    <Link to="/bookmarks" onClick={onClose}>
                        <FaBookmark />
                        <span> Bookmark</span>
                    </Link>
                </li>

                <li>
                    <Link to="/create-series" onClick={onClose}>
                        <FaPencilAlt />
                        <span> Create Stories </span>
                    </Link>
                </li>

                {/* --- Divider --- */}
                <li className="menu-divider"></li>

                {/* --- ADMIN LINK --- */}
                {user && user.role === 'Admin' && (
                    <li>
                        <Link to="/admin" onClick={onClose}>
                            <FaCog />
                            <span> Admin Dashboard </span>
                        </Link>
                    </li>
                )}

                {/* Account Settings) */}
                <li>
                    <Link to="/account-settings" onClick={onClose}>
                        <FaCog />
                        <span> Change UseName/Password </span>
                    </Link>
                </li>

                {/* Logout */}
                <li>
                    <a href="#" onClick={(e) => { e.preventDefault(); onLogout(); onClose(); }}>
                        <FaSignOutAlt />
                        <span> Log Out </span>
                    </a>
                </li>
            </ul>
        </div>
    );
};

const Header: React.FC = () => {
    const [searchQuery, setSearchQuery] = useState('');
    const [isMenuOpen, setIsMenuOpen] = useState(false);
    const menuRef = useRef<HTMLDivElement>(null);

    const navigate = useNavigate();

    const { user, isLoading, logout, unreadGeneralCount, unreadChapterCount, clearChapterNotifications } = useAuth();

    const handleSearch = (event: React.KeyboardEvent<HTMLInputElement>) => {
        if (event.key === 'Enter' && searchQuery.trim() !== '') {
            navigate(`/browse?q=${encodeURIComponent(searchQuery.trim())}`);
            setSearchQuery('');
        }
    };

    // Chỉ cho phép toggle nếu đã đăng nhập
    const handleToggleMenu = () => {
        if (user) {
            setIsMenuOpen(prev => !prev);
        }
    };

    // Đóng menu khi click ra ngoài
    const handleClickOutside = (event: MouseEvent) => {
        if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
            setIsMenuOpen(false);
        }
    };

    useEffect(() => {
        document.addEventListener('mousedown', handleClickOutside);
        return () => {
            document.removeEventListener('mousedown', handleClickOutside);
        };
    }, []);


    return (
        <header className="top-header">

            <div className="header-left">
                <Link to="/" className="header-logo-link" title="Trang chủ">
                    <div className="header-logo">W</div>
                </Link>

                <div className="search-bar">
                    <FaSearch className="search-icon" />
                    <input
                        type="text"
                        placeholder="Search book, name, author..."
                        value={searchQuery}
                        onChange={(e) => setSearchQuery(e.target.value)}
                        onKeyDown={handleSearch}
                    />
                </div>
            </div>

            <div className="header-right">
                {isLoading ? (
                    <div>Loading...</div>
                ) : user ? (

                    // ĐÃ ĐĂNG NHẬP
                    <>
                        <div
                            className="user-profile-wrapper"
                            onClick={handleToggleMenu}
                            ref={menuRef}
                        >
                            {/* Profile Display */}
                            <div className="user-profile" title="My Profile">
                                {user.avatarThumbnail ? (
                                    <img src={user.avatarThumbnail} alt={user.username} className="user-avatar-img" />
                                ) : (
                                    <FaUserCircle className="user-avatar" />
                                )}
                                <span>{user.username}</span>
                            </div>

                            {/* Dropdown Menu */}
                            {isMenuOpen && (
                                <UserDropdownMenu onLogout={logout} onClose={() => setIsMenuOpen(false)} user={user} />
                            )}
                        </div>

                        {/*ICON TIM */}
                            <Link
                                to="/favorites"
                                className="header-icon-btn notification-icon-wrapper"
                                title="Favorites - New Chapters"
                                onClick={clearChapterNotifications} 
                            >
                                <FaHeart />
                                {unreadChapterCount > 0 && (
                                    <span className="notification-badge">
                                        {unreadChapterCount > 99 ? '99+' : unreadChapterCount}
                                    </span>
                                )}
                            </Link>

                        {/* ICON CHUÔNG --- */}
                        <Link to="/notifications" className="header-icon-btn notification-icon-wrapper" title="Notifications">
                            <FaBell />
                            {unreadGeneralCount > 0 && (
                                <span className="notification-badge">
                                    {unreadGeneralCount > 99 ? '99+' : unreadGeneralCount}
                                </span>
                            )}
                        </Link>
                    </>
                ) : (

                    // CHƯA ĐĂNG NHẬP
                    <>
                        <Link to="/login" className="header-icon-btn">Login</Link>
                        <Link to="/register" className="header-icon-btn">Register</Link>
                    </>
                )}
            </div>
        </header>
    );
};

export default Header;