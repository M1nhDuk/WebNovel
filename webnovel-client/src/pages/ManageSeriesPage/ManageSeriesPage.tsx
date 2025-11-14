import React, { useState, useEffect, useCallback } from 'react';
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

import EditSeriesForm from './EditSeriesForm';
// import EditNovelForm from './EditNovelForm';
// import EditChapterForm from './EditChapterForm';

type EditingItem = {
    type: 'series' | 'novel' | 'chapter';
    id: number;
    parentId?: number;
};

// COMPONENT CONTEXT MENU

interface SeriesContextMenuProps {
    visible: boolean;
    x: number;
    y: number;
    seriesType: 'Series' | 'TRADITIONAL';
    onEdit: () => void;
    onAddVolume: () => void;
    onReorder: () => void;
    onAddChapter: () => void;
}

const SeriesContextMenu: React.FC<SeriesContextMenuProps> = ({
    visible, x, y, seriesType, onEdit, onAddVolume, onReorder, onAddChapter
}) => {
    if (!visible) return null;

    const style = { top: `${y}px`, left: `${x}px` };

    // Ngăn click vào menu làm đóng menu
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


// COMPONENT CỘT TRÁI

interface SeriesHierarchyProps {
    series: NovelSeriesDetailDto;
    setEditingItem: (item: EditingItem) => void;
    onRefresh: () => void;
}

const SeriesHierarchy: React.FC<SeriesHierarchyProps> = ({ series, setEditingItem, onRefresh }) => {

    const [expandedNovels, setExpandedNovels] = useState<Set<number>>(new Set());

    
    const [isSeriesExpanded, setIsSeriesExpanded] = useState(true);

  
    const [contextMenu, setContextMenu] = useState({ visible: false, x: 0, y: 0 });

    // ---Context Menu ---
    const handleContextMenu = (event: React.MouseEvent) => {
        event.preventDefault(); 
        event.stopPropagation();
        setContextMenu({ visible: true, x: event.clientX, y: event.clientY });
    };

    const closeContextMenu = () => {
        setContextMenu({ visible: false, x: 0, y: 0 });
    };

    // Effect để đóng menu khi click ra ngoài
    useEffect(() => {
        document.addEventListener('click', closeContextMenu);
        return () => {
            document.removeEventListener('click', closeContextMenu);
        };
    }, []);

    const toggleNovelExpand = (novelId: number) => {
        setExpandedNovels(prev => {
            const newSet = new Set(prev);
            if (newSet.has(novelId)) {
                newSet.delete(novelId);
            } else {
                newSet.add(novelId);
            }
            return newSet;
        });
    };

    // --- Handlers cho các nút trong Context Menu ---
    const handleEditSeries = () => {
        setEditingItem({ type: 'series', id: series.series_Id });
        closeContextMenu();
    };

    const handleAddVolume = () => {
        console.log("Add Volume clicked");
        closeContextMenu();
    };

    const handleReorder = () => {
        console.log("Reorder clicked");
        closeContextMenu();
    };

    const handleAddChapter = () => {
        console.log("Add Chapter clicked (for Traditional)");
        closeContextMenu();
    };

    // (Hàm renderSeriesFlow và renderTraditionalFlow giữ nguyên)
    const renderSeriesFlow = () => (
        <>
            {series.novels.map(novel => {
                const isExpanded = expandedNovels.has(novel.novel_Id);
                return (
                    <div key={novel.novel_Id} className="tree-item novel">
                        <div className="tree-item-label">
                            <button
                                className="tree-toggle-btn"
                                onClick={() => toggleNovelExpand(novel.novel_Id)}
                            >
                                {isExpanded ? <FaMinusSquare /> : <FaPlusSquare />}
                            </button>
                            <FaBook />
                            <span>{novel.title}</span>
                            <div className="tree-item-actions">
                                <button title="Edit Volume"><FaPencilAlt /></button>
                                <button title="Add Chapter"><FaPlus /></button>
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
                                                <button title="Edit Chapter"><FaPencilAlt /></button>
                                            </div>
                                        </div>
                                    </div>
                                ))}
                                {novel.chapters.length === 0 && (
                                    <div className="tree-item-label empty">No chapters yet.</div>
                                )}
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
                            <button title="Edit Chapter"><FaPencilAlt /></button>
                        </div>
                    </div>
                </div>
            ))}
            {(series as any).chapters.length === 0 && (
                <div className="tree-item-label empty">No chapters yet.</div>
            )}
        </div>
    );

    return (
        <div className="hierarchy-container">
            <SeriesContextMenu
                visible={contextMenu.visible}
                x={contextMenu.x}
                y={contextMenu.y}
                seriesType={series.type}
                onEdit={handleEditSeries}
                onAddVolume={handleAddVolume}
                onReorder={handleReorder}
                onAddChapter={handleAddChapter}
            />

            <div className="tree-item series">
                <div
                    className="tree-item-label"
                    onContextMenu={handleContextMenu}
                >
                    <button
                        className="tree-toggle-btn"
                        onClick={() => setIsSeriesExpanded(p => !p)} 
                    >
                        {isSeriesExpanded ? <FaMinusSquare /> : <FaPlusSquare />}
                    </button>

            
                    <Link
                        to={`/series/${series.series_Id}`}
                        target="_blank"
                        title="View public page"
                        onClick={(e) => e.stopPropagation()}
                        onContextMenu={(e) => {
                            e.stopPropagation(); 
                            handleContextMenu(e); 
                        }}
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


// COMPONENT TRANG CHÍNH 

const ManageSeriesPage: React.FC = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();

    const [series, setSeries] = useState<NovelSeriesDetailDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [editingItem, setEditingItem] = useState<EditingItem | null>(null);

    const fetchSeriesData = useCallback(async (forceReload = false) => {
        if (!id) {
            setError("Series ID is missing.");
            setLoading(false);
            return;
        }

        if (!series || forceReload) {
            setLoading(true);
        }

        try {
            const response = await apiClient.get<NovelSeriesDetailDto>(
                API_ROUTES.SERIES.GET_BY_ID(id)
            );
            setSeries(response.data);

            if (!editingItem || forceReload) {
                setEditingItem({ type: 'series', id: response.data.series_Id });
            }
        } catch (err) {
            console.error("Failed to fetch series details:", err);
            setError("Could not load series details.");
            navigate('/profile');
        } finally {
            setLoading(false);
        }
    }, [id, navigate, editingItem, series]);

    useEffect(() => {
        fetchSeriesData(false);
    }, [id, fetchSeriesData]);

    const handleRefresh = () => {
        fetchSeriesData(true);
    };

    const renderEditorPanel = () => {
        if (!editingItem || !series) return <div className="editor-placeholder">Select an item to edit.</div>;

        switch (editingItem.type) {
            case 'series':
                return <EditSeriesForm series={series} onSeriesUpdate={handleRefresh} />;
            case 'novel':
                return <div className="editor-placeholder">Novel Editor (Coming Soon)</div>;
            case 'chapter':
                return <div className="editor-placeholder">Chapter Editor (Coming Soon)</div>;
            default:
                return <div className="editor-placeholder">Select an item to edit.</div>;
        }
    };

    if (loading) return <div className="manage-page-wrapper">Loading Management Studio...</div>;
    if (error) return <div className="manage-page-wrapper error">{error}</div>;
    if (!series) return <div className="manage-page-wrapper">Series not found.</div>;

    return (
        <div className="manage-series-layout">
            <aside className="hierarchy-panel">
                <SeriesHierarchy series={series} setEditingItem={setEditingItem} onRefresh={handleRefresh} />
            </aside>
            <main className="editor-panel">
                {renderEditorPanel()}
            </main>
        </div>
    );
};

export default ManageSeriesPage;