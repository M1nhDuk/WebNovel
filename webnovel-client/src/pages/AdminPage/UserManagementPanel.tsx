import React, { useState, useEffect, useCallback } from 'react';
import apiClient from '../../api/apiClient';
import type { AdminUserDto } from '../../types/series'; 
import type { PagedResult } from '../../types/series';
import Pagination from '../../components/common/Pagination';
import { useAuth } from '../../hooks/useAuth';
import './CSS/UserManagementPanel.css'; 

const GATEWAY_URL = 'https://localhost:8000';
const PAGE_SIZE = 10;

const UserManagementPanel: React.FC = () => {
    const { user } = useAuth(); 
    const [users, setUsers] = useState<AdminUserDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);


    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(0);
    const [search, setSearch] = useState('');
    const [roleFilter, setRoleFilter] = useState('');
    const [verifiedFilter, setVerifiedFilter] = useState('');


    const [editingRole, setEditingRole] = useState<Record<string, string>>({});

    // Hàm lấy URL ảnh
    const getImageUrl = (coverPath: string | undefined | null) => {
        if (!coverPath) {
  
            return `${GATEWAY_URL}/uploads/default_avatar_thumb.png`;
        }
        const formattedPath = coverPath.startsWith('/') ? coverPath : `/${coverPath}`;
        return `${GATEWAY_URL}${formattedPath}`;
    };

    // Hàm fetch dữ liệu người dùng
    const fetchUsers = useCallback(async (page: number) => {
        setLoading(true);
        setError(null);
        try {
            const params = new URLSearchParams();
            params.append('page', page.toString());
            params.append('pageSize', PAGE_SIZE.toString());
            if (search) params.append('search', search);
            if (roleFilter) params.append('role', roleFilter);
            if (verifiedFilter) params.append('isVerified', verifiedFilter);

            
            const response = await apiClient.get<PagedResult<AdminUserDto>>(
                '/api/admin/users',
                { params }
            );

            setUsers(response.data.items);
            setCurrentPage(response.data.pageNumber);
            setTotalPages(Math.ceil(response.data.totalRecords / PAGE_SIZE));
            setEditingRole({}); 
        } catch (err: any) {
            setError(err.response?.data?.message || "Failed to fetch users.");
        } finally {
            setLoading(false);
        }
    }, [search, roleFilter, verifiedFilter]);

    // Load dữ liệu khi component mount hoặc filter/page thay đổi
    useEffect(() => {
        fetchUsers(currentPage);
    }, [fetchUsers, currentPage]);

    // Reset về trang 1 khi filter
    useEffect(() => {
        setCurrentPage(1);
    }, [search, roleFilter, verifiedFilter]);



    // --- Hàm xử lý Actions ---

    const handleRoleChange = (userId: string, newRole: string) => {
        setEditingRole(prev => ({ ...prev, [userId]: newRole }));
    };

    const saveRoleChange = async (userId: string) => {
        const newRole = editingRole[userId];
        if (!newRole) return;

        try {
            await apiClient.put(`/api/admin/users/${userId}/role`, { newRole });
            setEditingRole(prev => {
                const newState = { ...prev };
                delete newState[userId];
                return newState;
            });
            
            fetchUsers(currentPage);
        } catch (err: any) {
            setError(err.response?.data?.message || "Failed to update role.");
        }
    };

    const handleLockToggle = async (userId: string, isLocked: boolean) => {
        const action = isLocked ? 'unlock' : 'lock';
        if (!window.confirm(`Are you sure you want to ${action} this user?`)) return;

        try {
            await apiClient.post(`/api/admin/users/${userId}/${action}`);
            fetchUsers(currentPage);
        } catch (err: any) {
            setError(err.response?.data?.message || `Failed to ${action} user.`);
        }
    };

    const handleDeleteUser = async (userId: string, username: string | null) => {
        if (!window.confirm(`DELETE user "${username || userId}"? This action is IRREVERSIBLE.`)) return;

        try {
            await apiClient.delete(`/api/admin/users/${userId}`);
            fetchUsers(1); 
        } catch (err: any) {
            setError(err.response?.data?.message || "Failed to delete user.");
        }
    };

    const handlePageChange = (page: number) => {
        setCurrentPage(page);
    };

    // Format ngày 
    const formatDate = (dateString: string | null) => {
        if (!dateString) return 'N/A';
        return new Date(dateString).toLocaleDateString('en-UK', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
        });
    };

    return (
        <div className="admin-panel-container">
            {/* <h2>User Management</h2> (Tiêu đề đã có ở AdminDashboardPage) */}

            {error && <div className="auth-error" style={{ marginBottom: '15px' }}>{error}</div>}

            <div className="admin-controls">
                <input
                    type="text"
                    placeholder="Search by Username/Email..."
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                />
                <select value={roleFilter} onChange={(e) => setRoleFilter(e.target.value)}>
                    <option value="">All Roles</option>
                    <option value="Admin">Admin</option>
                    <option value="User">User</option>
                </select>
                <select value={verifiedFilter} onChange={(e) => setVerifiedFilter(e.target.value)}>
                    <option value="">All Statuses</option>
                    <option value="true">Verified</option>
                    <option value="false">Not Verified</option>
                </select>
            </div>

            {loading ? (
                <div>Loading users...</div>
            ) : (
                <>
                    <table className="admin-user-table">
                        <thead>
                            <tr>
                                <th>User</th>
                                <th>Email</th>
                                <th>Role</th>
                                <th>Status</th>
                                <th>Joined</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {users.map(u => (
                                <tr key={u.userId}>
                                    <td>
                                        <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                                            <img
                                                src={getImageUrl(u.avatarThumbnail)}
                                                alt="avatar"
                                                className="user-avatar-thumb"
                                            />
                                            {u.username}
                                        </div>
                                    </td>
                                    <td>{u.email}</td>
                                    <td className="admin-actions">
                                        <select
                                            value={editingRole[u.userId] || u.role || 'User'}
                                            onChange={(e) => handleRoleChange(u.userId, e.target.value)}
                                            disabled={u.userId === user?.userId} 
                                        >
                                            <option value="User">User</option>
                                            <option value="Admin">Admin</option>
                                        </select>
                                        {editingRole[u.userId] && (
                                            <button className="btn-save" onClick={() => saveRoleChange(u.userId)}>Save</button>
                                        )}
                                    </td>
                                    <td>
                                        {u.isLocked ? (
                                            <span className="user-status-badge badge-locked">Locked</span>
                                        ) : u.isEmailConfirmed ? (
                                            <span className="user-status-badge badge-verified">Verified</span>
                                        ) : (
                                            <span className="user-status-badge badge-unverified">Not Verified</span>
                                        )}
                                    </td>
                                    <td>{formatDate(u.createdAt)}</td>
                                    <td className="admin-actions">
                                        <button
                                            className={u.isLocked ? 'btn-unlock' : 'btn-lock'}
                                            onClick={() => handleLockToggle(u.userId, u.isLocked)}
                                            disabled={u.userId === user?.userId} // Admin không thể tự khóa mình
                                        >
                                            {u.isLocked ? 'Unlock' : 'Lock'}
                                        </button>
                                        <button
                                            className="btn-delete"
                                            onClick={() => handleDeleteUser(u.userId, u.username)}
                                            disabled={u.userId === user?.userId} // Admin không thể tự xóa mình
                                        >
                                            Delete
                                        </button>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>

                    {totalPages > 1 && (
                        <div className="admin-pagination">
                            <Pagination
                                currentPage={currentPage}
                                totalPages={totalPages}
                                onPageChange={handlePageChange}
                            />
                        </div>
                    )}
                </>
            )}
        </div>
    );
};

export default UserManagementPanel;