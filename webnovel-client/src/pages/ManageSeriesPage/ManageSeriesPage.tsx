import React, { useState, useEffect, useCallback } from 'react';
import type { Key } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import apiClient from '../../api/apiClient';
import { API_ROUTES } from '../../api/apiRoutes';
import type { NovelSeriesDetailDto, ChapterDetailDto } from '../../types/series';
import './ManageSeriesPage.css';

import {
    FaPencilAlt,
    FaPlus,
    FaBook,
    FaFileAlt,
    FaPlusSquare,
    FaMinusSquare,
    FaListOl
} from 'react-icons/fa';

<button title="Add Chapter"><FaPlus /></button>
import EditSeriesForm from './EditSeriesForm';
import AddNovelForm from './AddNovelForm';
import EditNovelForm from './EditNovelForm'; 
import AddChapterForm from './AddChapterForm';
import EditChapterForm from './EditChapterForm';

type EditingItem = {
    type: 'series' | 'novel' | 'chapter' | 'add-novel' | 'add-chapter';
    id: number;
    parentId?: number;
};

// --- Component ContextMenu ---
interface SeriesContextMenuProps {
    visible: boolean; x: number; y: number; seriesType: 'Series' | 'TRADITIONAL';
    onEdit: () => void; onAddVolume: () => void; onReorder: () => void; onAddChapter: () => void;
}
const SeriesContextMenu: React.FC<SeriesContextMenuProps> = ({
    visible, x, y, seriesType, onEdit, onAddVolume, onReorder, onAddChapter
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
                <button className="context-menu-item" onClick={(e) => handleMenuClick(e, onAddChapter)}>
                    <FaPlus /> Add Chapter
                </button>
            )}
        </div>
    );
};

// --- Component Hierarchy---
interface SeriesHierarchyProps {
    series: NovelSeriesDetailDto;
    setEditingItem: (item: EditingItem) => void;
    onRefresh: () => void;
}

const SeriesHierarchy: React.FC<SeriesHierarchyProps> = ({ series, setEditingItem }) => {
    const [expandedNovels, setExpandedNovels] = useState<Set<number>>(new Set());
    const [isSeriesExpanded, setIsSeriesExpanded] = useState(true);
    const [contextMenu, setContextMenu] = useState({ visible: false, x: 0, y: 0 });

    const handleContextMenu = (event: React.MouseEvent) => {
        event.preventDefault(); event.stopPropagation();
        setContextMenu({ visible: true, x: event.clientX, y: event.clientY });
    };
    const closeContextMenu = () => { setContextMenu({ visible: false, x: 0, y: 0 }); };

    useEffect(() => {
        document.addEventListener('click', closeContextMenu);
        return () => { document.removeEventListener('click', closeContextMenu); };
    }, []);

    const toggleNovelExpand = (novelId: number) => {
        setExpandedNovels(prev => {
            const newSet = new Set(prev);
            if (newSet.has(novelId)) newSet.delete(novelId);
            else newSet.add(novelId);
            return newSet;
        });
    };

    const handleEditSeries = () => { setEditingItem({ type: 'series', id: series.series_Id }); closeContextMenu(); };
    const handleAddVolume = () => { setEditingItem({ type: 'add-novel', id: series.series_Id }); closeContextMenu(); };
    const handleReorder = () => { console.log("Reorder clicked"); closeContextMenu(); };
    const handleAddChapter = () => { setEditingItem({ type: 'add-chapter', id: series.series_Id }); closeContextMenu(); };

    const renderSeriesFlow = () => (
        <>
            {series.novels.map(novel => {
                const isExpanded = expandedNovels.has(novel.novel_Id);
                return (
                    <div key={novel.novel_Id} className="tree-item novel">
                        <div className="tree-item-label">
                            <button className="tree-toggle-btn" onClick={() => toggleNovelExpand(novel.novel_Id)}>
                                {isExpanded ? <FaMinusSquare /> : <FaPlusSquare />}
                            </button>
                            <FaBook />
                            <span>{novel.title}</span>
                            <div className="tree-item-actions">
                                <button
                                    title="Edit Volume"
                                    onClick={(e) => {
                                        e.stopPropagation();
                                        setEditingItem({ type: 'novel', id: novel.novel_Id });
                                    }}
                                >
                                    <FaPencilAlt />
                                </button>
                                <button
                                    title="Add Chapter"
                                    onClick={(e) => {
                                        e.stopPropagation();
                                        // Gán parentId là novel.novel_Id
                                        setEditingItem({ type: 'add-chapter', id: series.series_Id, parentId: novel.novel_Id });
                                    }}
                                >
                                    <FaPlus />
                                </button>
                            </div>
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
            {(series as any).chapters.map((chapter: ChapterDetailDto) => (
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
                        </div>
                    </div>
                </div>
            ))}
            {(series as any).chapters.length === 0 && (<div className="tree-item-label empty">No chapters yet.</div>)}
        </div>
    );

    return (
        <div className="hierarchy-container">
            <SeriesContextMenu
                visible={contextMenu.visible} x={contextMenu.x} y={contextMenu.y}
                seriesType={series.type}
                onEdit={handleEditSeries} onAddVolume={handleAddVolume}
                onReorder={handleReorder} onAddChapter={handleAddChapter}
            />
            <div className="tree-item series">
                <div className="tree-item-label" onContextMenu={handleContextMenu}>
                    <button className="tree-toggle-btn" onClick={() => setIsSeriesExpanded(p => !p)}>
                        {isSeriesExpanded ? <FaMinusSquare /> : <FaPlusSquare />}
                    </button>
                    <Link to={`/series/${series.series_Id}`} target="_blank" title="View public page"
                        onClick={(e) => e.stopPropagation()}
                        onContextMenu={(e) => { e.stopPropagation(); handleContextMenu(e); }}
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


    const fetchSeriesData = useCallback(async () => {
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
            setSeries(response.data);
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
        if (item.type === 'add-novel' || item.type === 'add-chapter') {
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
            fetchSeriesData().then(() => {
                if (id) {
                    setEditingItem({ type: 'series', id: parseInt(id, 10) });
                }
            });
        }
    }, [id, fetchSeriesData]);

    

    const handleRefreshData = () => {
        fetchSeriesData();
    };


    const renderEditorPanel = () => {
        if (!series) return <div className="editor-placeholder">Loading...</div>;
        if (!editingItem) return <div className="editor-placeholder">Select an item to edit.</div>;

        const key: Key = editingItem.type === 'series'
            ? series.series_Id
            : `${editingItem.type}-${editingItem.id}`;

        switch (editingItem.type) {
            case 'series':
                return <EditSeriesForm key={key} series={series} onSeriesUpdate={handleRefreshData} />;

            case 'add-novel':
                return <AddNovelForm key={key} seriesId={series.series_Id} onNovelCreated={() => {
                    handleRefreshData();

                    // Sau khi tạo xong, quay lại edit series
                    setEditingItem({ type: 'series', id: series.series_Id });
                }} />;

            case 'novel':
                const novelToEdit = series.novels.find(n => n.novel_Id === editingItem.id);
                if (!novelToEdit) return <div className="editor-placeholder">Error: Novel not found.</div>;

                return <EditNovelForm
                    key={key}
                    seriesId={series.series_Id}
                    novel={novelToEdit}
                    onNovelUpdated={() => {
                        handleRefreshData();
                    }}
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
                    onChapterUpdated={() => {
                        handleRefreshData();
                        handleSetEditingItem({ type: 'series', id: series.series_Id });
                    }}
                    onCancel={() => handleSetEditingItem({ type: 'series', id: series.series_Id })}
                />;

            case 'add-chapter':
                return <AddChapterForm
                    key={key}
                    seriesId={series.series_Id}
                    seriesType={series.type}
                    novelId={editingItem.parentId}
                    onChapterCreated={() => {
                        handleRefreshData();
                        handleSetEditingItem({ type: 'series', id: series.series_Id });
                    }}
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
                />
            </aside>
            <main className="editor-panel">
                {renderEditorPanel()}
            </main>
        </div>
    );
};
export default ManageSeriesPage;