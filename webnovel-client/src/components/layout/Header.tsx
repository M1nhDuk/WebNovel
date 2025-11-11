import React, { useState } from 'react';
import './CSS/Header.css';
import { FaSearch, FaHeart, FaBell, FaUserCircle } from 'react-icons/fa';
import { Link, useNavigate } from 'react-router-dom';




const Header: React.FC = () => {
    const [searchQuery, setSearchQuery] = useState('');
    const navigate = useNavigate();

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
                <Link to="/account-settings" className="user-profile" title="Account Setting">
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