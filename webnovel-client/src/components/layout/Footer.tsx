import React from 'react';
import { FaGithub, FaHeart, FaTwitter } from 'react-icons/fa';
import './CSS/Footer.css';

const Footer: React.FC = () => {
    const currentYear = new Date().getFullYear();

    return (
        <footer className="main-footer">
            <div className="footer-container">

                <div className="footer-section footer-about">
                    <div className="footer-logo">W E B N O V E L</div>
                    <p>
                        A platform for readers and writers of web novels.
                    </p>
                    <p className="footer-copyright">
                        &copy; {currentYear} WebNovel Project. All rights reserved.
                    </p>
                </div>


                <div className="footer-section footer-social">
                    <h4>Connect</h4>
                    <div className="social-icons">
                        <a href="#" aria-label="Github"><FaGithub /></a>
                        <a href="#" aria-label="Twitter"><FaTwitter /></a>
                        <a href="#" aria-label="Sponsor"><FaHeart /></a>
                    </div>
                </div>
            </div>
        </footer>
    );
};

export default Footer;