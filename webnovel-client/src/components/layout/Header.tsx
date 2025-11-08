import React from 'react';
import './CSS/Header.css';
import { FaSearch, FaHeart, FaBell, FaUserCircle } from 'react-icons/fa';

const Header: React.FC = () => {
    return (
        <header className="top-header">

            <div className="header-left">
                <div className="header-logo">W</div>
                <div className="search-bar">
                    <FaSearch className="search-icon" />
                    <input type="text" placeholder="Search book, name, author..." />
                </div>
            </div>


            <div className="header-right">
                <a href="/profile" className="user-profile">
                    <FaUserCircle className="user-avatar" />
                    <span>Username</span>
                </a>
                <a href="/favorites" className="header-icon-btn" title="Favorites">
                    <FaHeart />
                </a>
                <a href="/notifications" className="header-icon-btn" title="Notifications">
                    <FaBell />
                </a>
            </div>
        </header>
    );
};

export default Header;