import React, { useState, useEffect, useCallback } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import apiClient from '../../api/apiClient';
import { API_ROUTES } from '../../api/apiRoutes';
import { useAuth } from '../../hooks/useAuth';
import type { PagedResult } from '../../types/series'; 
import type { NotificationDto, RemoveNotificationsDto } from '../../types/notifications';
import Pagination from '../../components/common/Pagination';
import { FaTrash, FaCheckDouble, FaRegEnvelope, FaRegEnvelopeOpen } from 'react-icons/fa';
import { formatDistanceToNow } from 'date-fns';
import { vi } from 'date-fns/locale';
import './NotificationsPage.css';

const PAGE_SIZE = 15;

const NotificationsPage: React.FC = () => {
    const { user, isLoading: userLoading, refreshUnreadCount } = useAuth();
    const navigate = useNavigate();

    const [notifications, setNotifications] = useState<NotificationDto[]>([]);
    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(0);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

  
    const fetchNotifications = useCallback(async (pageToFetch: number) => {
        if (!user) {
            setIsLoading(false);
            return;
        }
        setIsLoading(true);
        setError(null);

        try {
            const response = await apiClient.get<PagedResult<NotificationDto>>(
                API_ROUTES.USER.GET_NOTIFICATIONS,
                {
                    params: {
                        page: pageToFetch,
                        pageSize: PAGE_SIZE
                    }
                }
            );


            setNotifications(response.data.items);


            setCurrentPage(response.data.pageNumber);
            setTotalPages(Math.ceil(response.data.totalRecords / PAGE_SIZE));

        } catch (err: any) {
            console.error("Failed to fetch notifications:", err);
            setError(err.response?.data?.message || "Could not load notifications.");
            setNotifications([]);
        } finally {
            setIsLoading(false);
        }
    }, [user]);
    

    useEffect(() => {
        if (!userLoading && !user) {
            setError("You must be logged in to view your notifications.");
            setIsLoading(false);
        }
    }, [user, userLoading]);



    useEffect(() => {
        if (user) {
            fetchNotifications(currentPage);
        }
    }, [user, fetchNotifications, currentPage]);



    const handlePageChange = (page: number) => {
        if (page !== currentPage) {
            setCurrentPage(page);
            window.scrollTo(0, 0);
        }
    };



    const handleSelect = (id: string) => {
        setSelectedIds(prev => {
            const newSet = new Set(prev);
            if (newSet.has(id)) {
                newSet.delete(id);
            } else {
                newSet.add(id);
            }
            return newSet;
        });
    };



    const handleMarkAllAsRead = async () => {
        try {
            await apiClient.post(API_ROUTES.USER.MARK_ALL_AS_READ);
            fetchNotifications(currentPage);      
            refreshUnreadCount(); 
        } catch (err) {
            setError("Failed to mark all as read.");
        }
    };



    const handleDeleteSelected = async () => {
        if (selectedIds.size === 0) return;
        if (!window.confirm(`Delete ${selectedIds.size} notification(s)?`)) return;

        const payload: RemoveNotificationsDto = {
            notificationIds: Array.from(selectedIds)
        };

        try {
            await apiClient.delete(API_ROUTES.USER.DELETE_NOTIFICATIONS, { data: payload });
            setSelectedIds(new Set());

           
            if (notifications.length === selectedIds.size && currentPage > 1) {
                fetchNotifications(currentPage - 1);
            } else {
                fetchNotifications(currentPage);
            }
            refreshUnreadCount();

        } catch (err: any) {
            setError(err.response?.data?.message || "Failed to delete notifications.");
        }
    };



    const formatTime = (dateString: string) => {
        return formatDistanceToNow(new Date(dateString), { addSuffix: true, locale: vi });
    };


    if (userLoading || (isLoading && currentPage === 1)) {
        return <div className="notifications-page-container"><h1>Notifications</h1>Loading...</div>;
    }


    if (!user) {
        return (
            <div className="notifications-page-container">
                <h1>Notifications</h1>
                <div className="notifications-error">{error || "Login to view."}</div>
            </div>
        );
    }



    return (
        <div className="notifications-page-container">
            <h1>Notifications</h1>

            {error && <div className="notifications-error">{error}</div>}

            <div className="notifications-controls">
                <button onClick={handleMarkAllAsRead} disabled={isLoading}>
                    <FaCheckDouble /> Mark All as Read
                </button>
                <button
                    className="btn-delete"
                    onClick={handleDeleteSelected}
                    disabled={selectedIds.size === 0 || isLoading}
                >
                    <FaTrash /> Delete ({selectedIds.size})
                </button>
            </div>

            <div className="notifications-list">
                {notifications.length === 0 && !isLoading ? (
                    <div className="notifications-empty">
                        You have no notifications.
                    </div>
                ) : (
                    notifications.map(item => (
                        <div
                            key={item.notificationId}
                            className={`notification-item ${item.isRead ? 'is-read' : ''}`}
                        >
                            <input
                                type="checkbox"
                                className="notification-checkbox"
                                checked={selectedIds.has(item.notificationId)}
                                onChange={() => handleSelect(item.notificationId)}
                            />
                            <div className="notification-icon">
                                {item.isRead ? <FaRegEnvelopeOpen /> : <FaRegEnvelope />}
                            </div>
                            <div className="notification-details">
                                <span className="notification-message">
                                    {item.linkUrl ? (
                                        <Link to={item.linkUrl}>{item.message}</Link>
                                    ) : (
                                        <span>{item.message}</span>
                                    )}
                                </span>
                                <span className="notification-date">
                                    {formatTime(item.createdAt)}
                                </span>
                            </div>
                        </div>
                    ))
                )}
            </div>

            {!isLoading && totalPages > 1 && (
                <Pagination
                    currentPage={currentPage}
                    totalPages={totalPages}
                    onPageChange={handlePageChange}
                />
            )}
        </div>
    );
};

export default NotificationsPage;