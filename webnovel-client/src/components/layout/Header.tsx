import React, { useState } from 'react';
import './CSS/Header.css';
import { FaSearch, FaHeart, FaBell, FaUserCircle } from 'react-icons/fa';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';

const Header: React.FC = () => {
    const [searchQuery, setSearchQuery] = useState('');
    const navigate = useNavigate();
    const { user, isLoading } = useAuth();

    const handleSearch = (event: React.KeyboardEvent<HTMLInputElement>) => {
        if (event.key === 'Enter' && searchQuery.trim() !== '') {
            navigate(`/browse?q=${encodeURIComponent(searchQuery.trim())}`);
            setSearchQuery('');
        }
    };

    return (
        <header className="top-header">

            <div className="header-left">
                <Link to="/" className="header-logo-link" title="Trang ch?">
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
                        <Link to="/profile" className="user-profile" title="My Profile">
                            {user.avatarThumbnail ? (
                                <img src={user.avatarThumbnail} alt={user.username} className="user-avatar-img" />
                            ) : (
                                <FaUserCircle className="user-avatar" />
                            )}
                            <span>{user.username}</span>
                        </Link>
                        <Link to="/favorites" className="header-icon-btn" title="Favorites">
                            <FaHeart />
                        </Link>
                        <Link to="/notifications" className="header-icon-btn" title="Notifications">
                            <FaBell />
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