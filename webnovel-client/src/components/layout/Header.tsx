import React from 'react';
import './CSS/Header.css';
import { FaSearch, FaHeart, FaBell, FaUserCircle } from 'react-icons/fa';
import { Link } from 'react-router-dom';

const Header: React.FC = () => {
    return (
        <header className="top-header">

            <div className="header-left">
                <Link to="/" className="header-logo-link" title="Trang ch?">
                    <div className="header-logo">W</div>
                </Link>

                <div className="search-bar">
                    <FaSearch className="search-icon" />
                    <input type="text" placeholder="Search book, name, author..." />
                </div>
            </div>


            <div className="header-right">
                <Link to="/profile" className="user-profile">
                    <FaUserCircle className="user-avatar" />
                    <span>Username</span>
                </Link>
                <Link to="/favorites" className="header-icon-btn" title="Favorites">
                    <FaHeart />
                </Link>
                <Link to="/notifications" className="header-icon-btn" title="Notifications">
                    <FaBell />
                </Link>
            </div>
        </header>
    );
};

export default Header;