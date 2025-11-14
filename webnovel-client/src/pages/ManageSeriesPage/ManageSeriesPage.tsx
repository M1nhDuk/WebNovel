import React, { useState, useEffect, useCallback } from 'react';
import type { Key } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import apiClient from '../../api/apiClient';
import { API_ROUTES } from '../../api/apiRoutes';

import type {
    NovelSeriesDetailDto,
    ChapterDetailDto,
    NovelDetailDto,
    EditingItem,
    SeriesContextMenuProps,
    NovelContextMenuProps,
    ReorderableItem,
    ReorderableListProps,
    SeriesHierarchyProps
} from '../../types/series';


import './ManageSeriesPage.css';

import {
    FaPencilAlt,
    FaPlus,
    FaBook,
    FaFileAlt,
    FaPlusSquare,
    FaMinusSquare,
    FaListOl,
    FaTrash,
    FaGripVertical
} from 'react-icons/fa';

import EditSeriesForm from './EditSeriesForm';
import AddNovelForm from './AddNovelForm';
import EditNovelForm from './EditNovelForm';
import AddChapterForm from './AddChapterForm';
import EditChapterForm from './EditChapterForm';








// --- Component ContextMenu (Series) ---
const SeriesContextMenu: React.FC<SeriesContextMenuProps> = ({
    visible, x, y, seriesType, onEdit, onAddVolume, onReorder, onAddChapter, onDeleteSeries
}) => {
    if (!visible) return null;
    const style = { top: `${y}px`, left: `${x}px` };
    const handleMenuClick = (e: React.MouseEvent, action: () => void) => {
        e.stopPropagation();
        action();
    };
    return (
        <div className="series-context-menu" style={style} onClick={(e) => e.stopPropagation()}>
            <button className="context-menu-item" onClick={(e) => handleMenuClick(e, onEdit)}>
                <FaPencilAlt /> Edit Series
            </button>
            {seriesType === 'Series' && (
                <>
                    <button className="context-menu-item" onClick={(e) => handleMenuClick(e, onAddVolume)}>
                        <FaPlus /> Add Volume
                    </button>
                    <button className="context-menu-item" onClick={(e) => handleMenuClick(e, onReorder)}>
                        <FaListOl /> Reorder Volumes
                    </button>
                </>
            )}
            {seriesType === 'TRADITIONAL' && (
                <>
                    <button className="context-menu-item" onClick={(e) => handleMenuClick(e, onAddChapter)}>
                        <FaPlus /> Add Chapter
                    </button>
                    <button className="context-menu-item" onClick={(e) => handleMenuClick(e, onReorder)}>
                        <FaListOl /> Reorder Chapters
                    </button>
                </>
            )}

            <button
                className="context-menu-item"
                style={{ color: '#e74c3c' }}
                onClick={(e) => handleMenuClick(e, onDeleteSeries)}
            >
                <FaTrash /> Delete Series
            </button>
        </div>
    );
};




// --- Component ContextMenu (Novel) ---
const NovelContextMenu: React.FC<NovelContextMenuProps> = ({
    visible, x, y, onEdit, onAddChapter, onReorderChapters, onDelete
}) => {
    if (!visible) return null;
    const style = { top: `${y}px`, left: `${x}px` };
    const handleMenuClick = (e: React.MouseEvent, action: () => void) => {
        e.stopPropagation();
        action();
    };

    return (
        <div className="series-context-menu" style={style} onClick={(e) => e.stopPropagation()}>
            <button className="context-menu-item" onClick={(e) => handleMenuClick(e, onEdit)}>
                <FaPencilAlt /> Edit Volume
            </button>
            <button className="context-menu-item" onClick={(e) => handleMenuClick(e, onAddChapter)}>
                <FaPlus /> Add Chapter
            </button>
            <button className="context-menu-item" onClick={(e) => handleMenuClick(e, onReorderChapters)}>
                <FaListOl /> Reorder Chapters
            </button>
            <button
                className="context-menu-item"
                style={{ color: '#e74c3c' }}
                onClick={(e) => handleMenuClick(e, onDelete)}
            >
                <FaTrash /> Delete Volume
            </button>
        </div>
    );
};




// --- Component ReorderableList ---
const ReorderableList: React.FC<ReorderableListProps> = ({ items, listTitle, onSave, onCancel }) => {
    const [currentItems, setCurrentItems] = useState<ReorderableItem[]>(items);
    const [draggingItemIndex, setDraggingItemIndex] = useState<number | null>(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);

    const handleDragStart = (index: number) => {
        setDraggingItemIndex(index);
    };

    const handleDragOver = (e: React.DragEvent, index: number) => {
        e.preventDefault();
        if (draggingItemIndex === null || draggingItemIndex === index) {
            return;
        }

        const draggedItem = currentItems[draggingItemIndex];
        const newItems = [...currentItems];

        newItems.splice(draggingItemIndex, 1);
        newItems.splice(index, 0, draggedItem);

        setDraggingItemIndex(index);
        setCurrentItems(newItems);
    };

    const handleDragEnd = () => {
        setDraggingItemIndex(null);
    };

    const handleSaveReorder = async () => {
        setLoading(true);
        setError(null);
        setSuccess(null);

        const orderedIds = currentItems.map(item => item.id);

        try {
            await onSave(orderedIds);
            setSuccess('Order saved successfully!');
            setTimeout(onCancel, 1000); 
        } catch (err: any) {
            setError(err.response?.data?.message || "Could not save new order.");
            setLoading(false);
        }
    };

    return (
        <div className="create-series-form">
            <h2>{listTitle}</h2>
            <p>Click and drag items to reorder them.</p>

            {error && <div className="form-message error">{error}</div>}
            {success && <div className="form-message success">{success}</div>}

            <ul className="reorder-list">
                {currentItems.map((item, index) => (
                    <li
                        key={item.id}
                        className={`reorder-item ${draggingItemIndex === index ? 'dragging' : ''}`}
                        draggable={!loading}
                        onDragStart={() => handleDragStart(index)}
                        onDragOver={(e) => handleDragOver(e, index)}
                        onDragEnd={handleDragEnd}
                    >
                        <FaGripVertical className="reorder-drag-handle" />
                        <span className="reorder-item-title">{item.title}</span>
                    </li>
                ))}
            </ul>

            <div className="form-actions">
                <button type="button" onClick={handleSaveReorder} disabled={loading}>
                    {loading ? 'Saving...' : 'Save Order'}
                </button>
                <button type="button" className="cancel-btn" onClick={onCancel} disabled={loading}>
                    Cancel
                </button>
            </div>
        </div>
    );
};



// --- Component Hierarchy ---
const SeriesHierarchy: React.FC<SeriesHierarchyProps> = ({
    series, setEditingItem, onRefresh, onDeleteSeries, onReorderTrigger,
    onEditNovel, onAddChapterToNovel, onReorderChapters, onDeleteNovel,
    onDeleteChapter
}) => {
    const [expandedNovels, setExpandedNovels] = useState<Set<number>>(new Set());
    const [isSeriesExpanded, setIsSeriesExpanded] = useState(true);
    const [seriesContextMenu, setSeriesContextMenu] = useState({ visible: false, x: 0, y: 0 });
    const [novelContextMenu, setNovelContextMenu] = useState({ visible: false, x: 0, y: 0, novelId: 0 });

    const handleSeriesContextMenu = (event: React.MouseEvent) => {
        event.preventDefault(); event.stopPropagation();
        setSeriesContextMenu({ visible: true, x: event.clientX, y: event.clientY });
    };

    const handleNovelContextMenu = (event: React.MouseEvent, novelId: number) => {
        event.preventDefault(); event.stopPropagation();
        setNovelContextMenu({ visible: true, x: event.clientX, y: event.clientY, novelId: novelId });
    };

    const closeContextMenus = () => {
        setSeriesContextMenu({ visible: false, x: 0, y: 0 });
        setNovelContextMenu({ visible: false, x: 0, y: 0, novelId: 0 });
    };

    useEffect(() => {
        document.addEventListener('click', closeContextMenus);
        return () => { document.removeEventListener('click', closeContextMenus); };
    }, []);

    const toggleNovelExpand = (novelId: number) => {
        setExpandedNovels(prev => {
            const newSet = new Set(prev);
            if (newSet.has(novelId)) newSet.delete(novelId);
            else newSet.add(novelId);
            return newSet;
        });
    };

    const handleEditSeries = () => { setEditingItem({ type: 'series', id: series.series_Id }); closeContextMenus(); };
    const handleAddVolume = () => { setEditingItem({ type: 'add-novel', id: series.series_Id }); closeContextMenus(); };
    const handleReorder = () => { onReorderTrigger(); closeContextMenus(); };
    const handleAddChapter = () => { setEditingItem({ type: 'add-chapter', id: series.series_Id }); closeContextMenus(); };
    const handleDeleteSeries = () => { onDeleteSeries(); closeContextMenus(); };

    const handleEditNovel = () => { onEditNovel(novelContextMenu.novelId); closeContextMenus(); };
    const handleAddChapterToNovel = () => { onAddChapterToNovel(novelContextMenu.novelId); closeContextMenus(); };
    const handleReorderChapters = () => { onReorderChapters(novelContextMenu.novelId); closeContextMenus(); };
    const handleDeleteNovel = () => { onDeleteNovel(novelContextMenu.novelId); closeContextMenus(); };


    const renderSeriesFlow = () => (
        <>
            {series.novels.map(novel => {
                const isExpanded = expandedNovels.has(novel.novel_Id);
                return (
                    <div key={novel.novel_Id} className="tree-item novel">
                        <div
                            className="tree-item-label"
                            onContextMenu={(e) => handleNovelContextMenu(e, novel.novel_Id)}
                        >
                            <button className="tree-toggle-btn" onClick={() => toggleNovelExpand(novel.novel_Id)}>
                                {isExpanded ? <FaMinusSquare /> : <FaPlusSquare />}
                            </button>
                            <FaBook />
                            <span>{novel.title}</span>
                        </div>
                        {isExpanded && (
                            <div className="tree-item-children">
                                {novel.chapters.map(chapter => (
                                    <div key={chapter.chapter_id} className="tree-item chapter">
                                        <div className="tree-item-label">
                                            <FaFileAlt />
                                            <span>{chapter.title}</span>
                                            <div className="tree-item-actions">
                                                <button
                                                    title="Edit Chapter"
                                                    onClick={(e) => {
                                                        e.stopPropagation();
                                                        setEditingItem({ type: 'chapter', id: chapter.chapter_id, parentId: novel.novel_Id });
                                                    }}
                                                >
                                                    <FaPencilAlt />
                                                </button>
                                                <button
                                                    title="Delete Chapter"
                                                    style={{ color: '#e74c3c' }}
                                                    onClick={(e) => {
                                                        e.stopPropagation();
                                                        onDeleteChapter(chapter.chapter_id, novel.novel_Id);
                                                    }}
                                                >
                                                    <FaTrash />
                                                </button>
                                            </div>
                                        </div>
                                    </div>
                                ))}
                                {novel.chapters.length === 0 && (<div className="tree-item-label empty">No chapters yet.</div>)}
                            </div>
                        )}
                    </div>
                );
            })}
        </>
    );
    const renderTraditionalFlow = () => (
        <div className="tree-item-children">
            {(series.chapters || []).map((chapter: ChapterDetailDto) => (
                <div key={chapter.chapter_id} className="tree-item chapter">
                    <div className="tree-item-label">
                        <FaFileAlt />
                        <span>{chapter.title}</span>
                        <div className="tree-item-actions">
                            <button
                                title="Edit Chapter"
                                onClick={(e) => {
                                    e.stopPropagation();
                                    setEditingItem({ type: 'chapter', id: chapter.chapter_id, parentId: series.series_Id });
                                }}
                            >
                                <FaPencilAlt />
                            </button>
                            <button
                                title="Delete Chapter"
                                style={{ color: '#e74c3c' }}
                                onClick={(e) => {
                                    e.stopPropagation();
                                    onDeleteChapter(chapter.chapter_id, series.series_Id);
                                }}
                            >
                                <FaTrash />
                            </button>
                        </div>
                    </div>
                </div>
            ))}
            {(series.chapters || []).length === 0 && (<div className="tree-item-label empty">No chapters yet.</div>)}
        </div>
    );

    return (
        <div className="hierarchy-container">
            <SeriesContextMenu
                visible={seriesContextMenu.visible} x={seriesContextMenu.x} y={seriesContextMenu.y}
                seriesType={series.type}
                onEdit={handleEditSeries}
                onAddVolume={handleAddVolume}
                onReorder={handleReorder}
                onAddChapter={handleAddChapter}
                onDeleteSeries={handleDeleteSeries}
            />
            <NovelContextMenu
                visible={novelContextMenu.visible} x={novelContextMenu.x} y={novelContextMenu.y}
                onEdit={handleEditNovel}
                onAddChapter={handleAddChapterToNovel}
                onReorderChapters={handleReorderChapters}
                onDelete={handleDeleteNovel}
            />

            <div className="tree-item series">
                <div className="tree-item-label" onContextMenu={handleSeriesContextMenu}>
                    <button className="tree-toggle-btn" onClick={() => setIsSeriesExpanded(p => !p)}>
                        {isSeriesExpanded ? <FaMinusSquare /> : <FaPlusSquare />}
                    </button>
                    <Link to={`/series/${series.series_Id}`} target="_blank" title="View public page"
                        onClick={(e) => e.stopPropagation()}
                        onContextMenu={(e) => { e.stopPropagation(); handleSeriesContextMenu(e); }}
                    >
                        <strong>{series.series_title}</strong>
                    </Link>
                </div>
            </div>
            {isSeriesExpanded && (
                <div className="tree-item-children">
                    {series.type === 'Series' ? renderSeriesFlow() : renderTraditionalFlow()}
                </div>
            )}
        </div>
    );
};


// COMPONENT TRANG CHÍNH (CHA)
const ManageSeriesPage: React.FC = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();

    const [series, setSeries] = useState<NovelSeriesDetailDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [editingItem, setEditingItem] = useState<EditingItem | null>(null);


    const fetchSeriesData = useCallback(async (selectSeries = false) => {
        if (!id) {
            setError("Series ID is missing.");
            setLoading(false);
            return;
        }
        setLoading(true);
        try {
            const response = await apiClient.get<NovelSeriesDetailDto>(
                API_ROUTES.SERIES.GET_BY_ID(id)
            );

            // Sắp xếp chapter cho traditional series
            if (response.data.type === 'TRADITIONAL' && response.data.chapters) {
                response.data.chapters.sort((a, b) => a.chapter_number - b.chapter_number);
            }
            // Sắp xếp novel và chapter cho web series
            else if (response.data.type === 'Series' && response.data.novels) {
                response.data.novels.sort((a, b) => a.novel_number - b.novel_number);
                response.data.novels.forEach(novel => {
                    novel.chapters.sort((a, b) => a.chapter_number - b.chapter_number);
                });
            }

            setSeries(response.data);
            if (selectSeries) {
                setEditingItem({ type: 'series', id: parseInt(id, 10) });
            }
        } catch (err: any) {
            console.error("Failed to fetch series details:", err);
            if (err.response && (err.response.status === 401 || err.response.status === 403)) {
                setError("You are not authorized to manage this series.");
                navigate('/profile');
            } else if (err.response && err.response.status === 404) {
                setError("Series not found.");
                navigate('/profile');
            } else {
                setError("Could not load series details.");
            }
        } finally {
            setLoading(false);
        }
    }, [id, navigate]);



    const handleSetEditingItem = useCallback((item: EditingItem) => {
        if (['add-novel', 'add-chapter', 'reorder-novels', 'reorder-chapters'].includes(item.type)) {
            setEditingItem(item);
        } else {
            fetchSeriesData()
                .then(() => {
                    setEditingItem(item);
                })
                .catch(err => {
                    console.error("Failed to refresh data before editing:", err);
                    setError("Could not load item details. Please refresh and try again.");
                });
        }
    }, [fetchSeriesData]);

    useEffect(() => {
        setSeries(null);
        setEditingItem(null);
        if (id) {
            fetchSeriesData(true);
        }
    }, [id, fetchSeriesData]);



    const handleRefreshData = () => {
        fetchSeriesData(true);
    };

    // --- LOGIC HÀNH ĐỘNG ---

    const handleDeleteSeries = async () => {
        if (!series) return;
        if (window.confirm(`Are you sure you want to permanently delete "${series.series_title}"? This cannot be undone.`)) {
            setLoading(true);
            setError(null);
            try {
                await apiClient.delete(API_ROUTES.SERIES.DELETE(series.series_Id));
                navigate('/profile');
            } catch (err: any) {
                setError(err.response?.data?.message || "Could not delete the series.");
                setLoading(false);
            }
        }
    };

    const handleDeleteNovel = async (novelId: number) => {
        if (!series) return;
        const novel = series.novels.find(n => n.novel_Id === novelId);
        if (!novel) return;

        if (window.confirm(`Are you sure you want to delete volume "${novel.title}"?`)) {
            setLoading(true);
            try {
                await apiClient.delete(API_ROUTES.SERIES.CREATE_NOVEL(series.series_Id) + `/${novelId}`);
                handleRefreshData();
            } catch (err: any) {
                setError(err.response?.data?.message || "Could not delete the volume.");
                setLoading(false);
            }
        }
    };

    const handleDeleteChapter = async (chapterId: number, parentId: number) => {
        if (!series) return;

        let novelId: number | undefined;
        let seriesId: number | undefined;
        let endpoint: string;

        if (series.type === 'TRADITIONAL') {
            seriesId = parentId;
            endpoint = API_ROUTES.SERIES.CHAPTER_FOR_SERIES(seriesId, chapterId);
        } else {
            novelId = parentId;
            endpoint = API_ROUTES.SERIES.CHAPTER_FOR_NOVEL(novelId, chapterId);
        }

        if (window.confirm(`Are you sure you want to delete this chapter?`)) {
            setLoading(true);
            try {
                await apiClient.delete(endpoint);
                handleRefreshData();
            } catch (err: any) {
                setError(err.response?.data?.message || "Could not delete the chapter.");
                setLoading(false);
            }
        }
    };

    const handleReorderTrigger = () => {
        if (!series) return;
        if (series.type === 'TRADITIONAL') {
            setEditingItem({ type: 'reorder-chapters', id: series.series_Id });
        } else {
            setEditingItem({ type: 'reorder-novels', id: series.series_Id });
        }
    };


    const renderEditorPanel = () => {
        if (!series) return <div className="editor-placeholder">Loading...</div>;
        if (!editingItem) return <div className="editor-placeholder">Select an item to edit.</div>;

        const key: Key = editingItem.type === 'series'
            ? series.series_Id
            : `${editingItem.type}-${editingItem.id}-${editingItem.parentId || ''}`;

        switch (editingItem.type) {
            case 'series':
                return <EditSeriesForm key={key} series={series} onSeriesUpdate={handleRefreshData} />;

            case 'add-novel':
                return <AddNovelForm key={key} seriesId={series.series_Id} onNovelCreated={handleRefreshData} />;

            case 'reorder-novels':
                return <ReorderableList
                    key={key}
                    listTitle="Reorder Volumes"
                    items={series.novels 
                        .map(n => ({ id: n.novel_Id, title: n.title }))
                    }
                    onCancel={handleRefreshData} 
                    onSave={async (orderedIds) => {
                        const payload = {
                            series_Id: series.series_Id,
                            Novels: orderedIds.map((id, index) => ({
                                novel_id: id,
                                new_position: index + 1
                            }))
                        };
                        await apiClient.post(API_ROUTES.SERIES.CREATE_NOVEL(series.series_Id) + '/reorder', payload);
                    }}
                />;

            case 'reorder-chapters':
                const isTraditional = series.type === 'TRADITIONAL';
                const parentId = editingItem.id; // novelId hoặc seriesId

                let chapters: ChapterDetailDto[] = [];
                let saveUrl = '';
                let listTitle = '';

                if (isTraditional) {
                    chapters = series.chapters || [];
                    saveUrl = API_ROUTES.SERIES.CREATE_CHAPTER_FOR_SERIES(parentId) + '/reorder';
                    listTitle = `Reorder Chapters for "${series.series_title}"`;
                } else {
                    const novel = series.novels.find(n => n.novel_Id === parentId);
                    chapters = novel?.chapters || [];
                    saveUrl = API_ROUTES.SERIES.CREATE_CHAPTER_FOR_NOVEL(parentId) + '/reorder';
                    listTitle = `Reorder Chapters for "${novel?.title || 'Unknown Novel'}"`;
                }

                return <ReorderableList
                    key={key}
                    listTitle={listTitle}
                    items={chapters 
                        .map(c => ({ id: c.chapter_id, title: c.title }))
                    }
                    onCancel={handleRefreshData} 
                    onSave={async (orderedIds) => {
                        const payload = {
                            [isTraditional ? 'series_Id' : 'novel_Id']: parentId,
                            Chapters: orderedIds.map((id, index) => ({
                                chapter_id: id,
                                new_position: index + 1
                            }))
                        };
                        await apiClient.post(saveUrl, payload);
                    }}
                />;

            case 'novel':
                const novelToEdit = series.novels.find(n => n.novel_Id === editingItem.id);
                if (!novelToEdit) return <div className="editor-placeholder">Error: Novel not found.</div>;

                return <EditNovelForm
                    key={key}
                    seriesId={series.series_Id}
                    novel={novelToEdit}
                    onNovelUpdated={handleRefreshData}
                    onCancel={() => setEditingItem({ type: 'series', id: series.series_Id })}
                />;

            case 'chapter':
                if (!editingItem.parentId) {
                    return <div className="editor-placeholder">Error: Parent ID not found for chapter.</div>;
                }
                return <EditChapterForm
                    key={key}
                    seriesId={series.series_Id}
                    seriesType={series.type}
                    novelId={series.type === 'Series' ? editingItem.parentId : undefined}
                    chapterId={editingItem.id}
                    onChapterUpdated={handleRefreshData}
                    onCancel={() => setEditingItem({ type: 'series', id: series.series_Id })}
                />;

            case 'add-chapter':
                return <AddChapterForm
                    key={key}
                    seriesId={series.series_Id}
                    seriesType={series.type}
                    novelId={editingItem.parentId}
                    onChapterCreated={handleRefreshData}
                />;

            default:
                return <div className="editor-placeholder">Select an item to edit.</div>;
        }
    };



    if (loading && !series) return <div className="manage-page-wrapper">Loading Management Studio...</div>;
    if (error) return <div className="manage-page-wrapper error">{error}</div>;
    if (!series) return <div className="manage-page-wrapper">Series not found.</div>;

    return (
        <div className="manage-series-layout">
            <aside className="hierarchy-panel">
                <SeriesHierarchy
                    series={series}
                    setEditingItem={handleSetEditingItem}
                    onRefresh={handleRefreshData}
                    onDeleteSeries={handleDeleteSeries}
                    onReorderTrigger={handleReorderTrigger}
                    // Truyền các hàm cho Novel
                    onEditNovel={(novelId) => setEditingItem({ type: 'novel', id: novelId })}
                    onAddChapterToNovel={(novelId) => setEditingItem({ type: 'add-chapter', id: series.series_Id, parentId: novelId })}
                    onReorderChapters={(novelId) => setEditingItem({ type: 'reorder-chapters', id: novelId, parentId: series.series_Id })}
                    onDeleteNovel={handleDeleteNovel}
                    // Truyền hàm cho Chapter
                    onDeleteChapter={handleDeleteChapter}
                />
            </aside>
            <main className="editor-panel">
                {renderEditorPanel()}
            </main>
        </div>
    );
};
export default ManageSeriesPage;