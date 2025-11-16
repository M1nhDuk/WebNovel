import React, { useState, useEffect, useCallback } from 'react';
import apiClient from '../../api/apiClient';
import { metaPanelApiConfig, type ViewType } from '../../api/metaPanelConfig';
import type { CategoryDto, NovelStatusDto, TagDto } from '../../types/filters';
import Pagination from '../../components/common/Pagination';
import './CSS/PublicationMetaPanel.css';


const PAGE_SIZE = 25;

type MetadataItem = (CategoryDto | NovelStatusDto | TagDto) & { id: number; name: string; description?: string };

// Kiểu cho item đang chỉnh sửa
type EditingItem = {
    id: number;
    name: string;
    description: string;
} | null;


const PublicationMetaPanel: React.FC = () => {
    const [view, setView] = useState<ViewType>('categories');

    const [allItems, setAllItems] = useState<MetadataItem[]>([]);
    const [pagedItems, setPagedItems] = useState<MetadataItem[]>([]);
    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(0);

    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    // State cho form tạo mới
    const [newItemName, setNewItemName] = useState('');
    const [newItemDesc, setNewItemDesc] = useState('');

    // State cho chỉnh sửa inline
    const [editingItem, setEditingItem] = useState<EditingItem>(null);


    //fetch dữ liệu
    const fetchData = useCallback(async () => {
        setLoading(true);
        setError(null);
        setEditingItem(null);
        resetNewForm();

        try {
            const config = metaPanelApiConfig[view];
            const response = await apiClient.get<any[]>(config.GET);

            // Chuẩn hóa dữ liệu về 'id', 'name', 'description'
            const normalizedData = response.data.map(item => ({
                ...item,
                id: item.category_id || item.tagId || item.statusId,
                name: item[config.dtoKey],
                description: item.description || ''
            }));

            setAllItems(normalizedData);
            setCurrentPage(1);
        } catch (err: any) {
            setError(err.response?.data?.message || `Failed to fetch ${view}.`);
        } finally {
            setLoading(false);
        }

    }, [view]);

    // Fetch data khi 'view' thay đổi
    useEffect(() => {
        fetchData();
    }, [fetchData]);


    // useEffect để tính toán phân trang (Không đổi)
    useEffect(() => {
        const total = Math.ceil(allItems.length / PAGE_SIZE);
        setTotalPages(total);

        const startIndex = (currentPage - 1) * PAGE_SIZE;
        const endIndex = startIndex + PAGE_SIZE;

        setPagedItems(allItems.slice(startIndex, endIndex));
    }, [allItems, currentPage]);

    const resetNewForm = () => {
        setNewItemName('');
        setNewItemDesc('');
    };


    //xử lý đổi trang 
    const handlePageChange = (page: number) => {
        setCurrentPage(page);
    };

    const handleCreate = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!newItemName.trim()) {
            setError('Name cannot be empty.');
            return;
        }

        const config = metaPanelApiConfig[view];
        const payload: any = { [config.dtoKey]: newItemName };
        if (config.hasDescription) {
            payload.Description = newItemDesc;
        }

        try {
            await apiClient.post(config.CREATE, payload);
            fetchData();
        } catch (err: any) {
            setError(err.response?.data?.message || `Failed to create ${view}.`);
        }
    };

    const handleUpdate = async () => {
        if (!editingItem || !editingItem.name.trim()) {
            setError('Name cannot be empty.');
            return;
        }

        const config = metaPanelApiConfig[view];
        const payload: any = { [config.dtoKey]: editingItem.name };
        if (config.hasDescription) {
            payload.Description = editingItem.description;
        }

        try {
            await apiClient.put(config.UPDATE(editingItem.id), payload);
            fetchData();
        } catch (err: any) {
            setError(err.response?.data?.message || `Failed to update ${view}.`);
        }
    };

    const handleDelete = async (id: number) => {
        if (!window.confirm('Are you sure you want to delete this item?')) return;

        try {
            await apiClient.delete(metaPanelApiConfig[view].DELETE(id));
            fetchData();
        } catch (err: any) {
            setError(err.response?.data?.message || `Failed to delete ${view}.`);
        }
    };

    // --- Xử lý UI --- 

    const startEdit = (item: MetadataItem) => {
        setEditingItem({
            id: item.id,
            name: item.name,
            description: item.description || ''
        });
    };

    const cancelEdit = () => {
        setEditingItem(null);
    };

    const config = metaPanelApiConfig[view];

    return (
        <div className="meta-panel-container">
            {/* Thanh Nav */}
            <div className="meta-nav">
                <button
                    className={`meta-nav-button ${view === 'categories' ? 'active' : ''}`}
                    onClick={() => setView('categories')}>
                    Categories
                </button>
                <button
                    className={`meta-nav-button ${view === 'tags' ? 'active' : ''}`}
                    onClick={() => setView('tags')}>
                    Tags
                </button>
                <button
                    className={`meta-nav-button ${view === 'statuses' ? 'active' : ''}`}
                    onClick={() => setView('statuses')}>
                    Statuses
                </button>
            </div>

            {error && <div className="auth-error" style={{ marginBottom: '15px' }}>{error}</div>}

            {/* Form Tạo Mới */}
            <form className="meta-form" onSubmit={handleCreate}>
                <div className="form-group">
                    <label htmlFor="newName">New {view.slice(0, -1)} Name</label>
                    <input
                        type="text"
                        id="newName"
                        value={newItemName}
                        onChange={(e) => setNewItemName(e.target.value)}
                        placeholder={`Enter new ${view.slice(0, -1)} name`}
                    />
                </div>
                {config.hasDescription && (
                    <div className="form-group">
                        <label htmlFor="newDesc">Description</label>
                        <input
                            type="text"
                            id="newDesc"
                            value={newItemDesc}
                            onChange={(e) => setNewItemDesc(e.target.value)}
                            placeholder="Enter description (optional)"
                        />
                    </div>
                )}
                <button type="submit" disabled={loading}>Create</button>
            </form>

            {/* Bảng Dữ Liệu */}
            {loading ? (
                <div>Loading...</div>
            ) : (
                <>
                    <table className="meta-table">
                        <thead>
                            <tr>
                                <th>ID</th>
                                <th>Name</th>
                                {config.hasDescription && <th>Description</th>}
                                <th style={{ width: '180px' }}>Actions</th>
                            </tr>
                        </thead>
                        <tbody>

                            {pagedItems.map(item => (
                                <tr key={item.id}>
                                    {editingItem?.id === item.id ? (
                                        // Chế độ Chỉnh sửa
                                        <>
                                            <td>{item.id}</td>
                                            <td>
                                                <input
                                                    type="text"
                                                    value={editingItem.name}
                                                    onChange={(e) => setEditingItem({ ...editingItem, name: e.target.value })}
                                                />
                                            </td>
                                            {config.hasDescription && (
                                                <td>
                                                    <input
                                                        type="text"
                                                        value={editingItem.description}
                                                        onChange={(e) => setEditingItem({ ...editingItem, description: e.target.value })}
                                                    />
                                                </td>
                                            )}
                                            <td className="actions">
                                                <button className="btn-save" onClick={handleUpdate}>Save</button>
                                                <button className="btn-cancel" onClick={cancelEdit}>Cancel</button>
                                            </td>
                                        </>
                                    ) : (
                                        // Chế độ Hiển thị
                                        <>
                                            <td>{item.id}</td>
                                            <td>{item.name}</td>
                                            {config.hasDescription && <td>{item.description}</td>}
                                            <td className="actions">
                                                <button className="btn-edit" onClick={() => startEdit(item)}>Edit</button>
                                                <button className="btn-delete" onClick={() => handleDelete(item.id)}>Delete</button>
                                            </td>
                                        </>
                                    )}
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

export default PublicationMetaPanel;