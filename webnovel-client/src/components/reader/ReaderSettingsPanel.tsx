import React from 'react';
import { useReaderSettings } from '../../hooks/useReaderSettings';
import './ReaderSettingsPanel.css';
import { FaTimes, FaAlignLeft, FaAlignCenter, FaAlignRight, FaAlignJustify } from 'react-icons/fa';

interface ReaderSettingsPanelProps {
    isOpen: boolean;
    onClose: () => void;
}

// Cập nhật danh sách font chữ và màu nền 
const FONT_FAMILIES = ["Noto Sans", "Times New Roman", "Merriweather", "Lora", "Roboto"];
const ALIGNMENTS = [
    { name: 'left', icon: <FaAlignLeft /> },
    { name: 'center', icon: <FaAlignCenter /> },
    { name: 'right', icon: <FaAlignRight /> },
    { name: 'justify', icon: <FaAlignJustify /> }
];
const COLORS = [
    { name: 'White', bg: '#FFFFFF', text: '#000000' },
    { name: 'LightGreen', bg: '#EFF3ED', text: '#000000' },
    { name: 'LightBlue', bg: '#E6F0F2', text: '#000000' },
    { name: 'LightYellow', bg: '#F8F4E6', text: '#000000' },
    { name: 'MintGreen', bg: '#D8E8E3', text: '#000000' }, // Màu được chọn trong ảnh
    { name: 'LightPink', bg: '#F4EBEF', text: '#000000' },
    { name: 'DarkGray', bg: '#333333', text: '#E0E0E0' },
    { name: 'Black', bg: '#000000', text: '#FFFFFF' },
];

//hằng số điều khiển
const FONT_STEP = 2;
const FONT_MIN = 12;
const FONT_MAX = 36;

const PADDING_STEP = 20;
const PADDING_MIN = 0;
const PADDING_MAX = 200;

const ReaderSettingsPanel: React.FC<ReaderSettingsPanelProps> = ({ isOpen, onClose }) => {
    const { settings, updateSetting } = useReaderSettings();

    if (!isOpen) return null;

    // Hàm xử lý cho nút bấm Cỡ chữ
    const handleFontSizeChange = (direction: 'increase' | 'decrease') => {
        const currentSize = settings.fontSize;
        const newSize = direction === 'increase' ? currentSize + FONT_STEP : currentSize - FONT_STEP;

        if (newSize >= FONT_MIN && newSize <= FONT_MAX) {
            updateSetting('fontSize', newSize);
        }
    };

    // Hàm xử lý cho nút bấm Lề trang 
    const handlePaddingChange = (direction: 'increase' | 'decrease') => {
        const currentPadding = settings.paddingPx;
        const newPadding = direction === 'increase' ? currentPadding + PADDING_STEP : currentPadding - PADDING_STEP;

        if (newPadding >= PADDING_MIN && newPadding <= PADDING_MAX) {
            updateSetting('paddingPx', newPadding);
        }
    };

    return (
        <div className="settings-modal-overlay" onClick={onClose}>
            <div className="settings-modal-content" onClick={e => e.stopPropagation()}>
                <div className="settings-modal-header">
                    <h4>Display Settings</h4>
                    <button onClick={onClose} className="settings-close-btn"><FaTimes /></button>
                </div>

                <div className="settings-modal-body">

                    {/* Color Scheme */}
                    <div className="setting-group">
                        <label>Color Scheme</label>
                        <div className="color-options">
                            {COLORS.map(color => (
                                <button
                                    key={color.name}
                                    className={`color-swatch ${settings.backgroundColor === color.bg ? 'active' : ''}`}
                                    style={{ backgroundColor: color.bg, borderColor: color.text === '#000000' ? '#e0e0e0' : color.text }}
                                    onClick={() => {
                                        updateSetting('backgroundColor', color.bg);
                                        updateSetting('fontColor', color.text);
                                    }}
                                />
                            ))}
                        </div>
                    </div>

                    {/* Font family */}
                    <div className="setting-group">
                        <label htmlFor="fontFamily">Font family</label>
                        <select
                            id="fontFamily"
                            value={settings.fontFamily}
                            onChange={e => updateSetting('fontFamily', e.target.value)}
                        >
                            {FONT_FAMILIES.map(font => <option key={font} value={font}>{font}</option>)}
                        </select>
                    </div>

                    {/*  Font Size */}
                    <div className="setting-group">
                        <label htmlFor="fontSize"> Font Size</label>
                        <div className="numeric-stepper">
                            <button
                                type="button"
                                onClick={() => handleFontSizeChange('decrease')}
                                disabled={settings.fontSize <= FONT_MIN}
                            >
                                &lt;
                            </button>
                            <input
                                type="text"
                                className="value-display"
                                value={`${settings.fontSize}px`}
                                readOnly
                            />
                            <button
                                type="button"
                                onClick={() => handleFontSizeChange('increase')}
                                disabled={settings.fontSize >= FONT_MAX}
                            >
                                &gt;
                            </button>
                        </div>
                    </div>

                    {/* Page Margin */}
                    <div className="setting-group">
                        <label htmlFor="paddingPx">Page Margin</label>
                        <div className="numeric-stepper">
                            <button
                                type="button"
                                onClick={() => handlePaddingChange('decrease')}
                                disabled={settings.paddingPx <= PADDING_MIN}
                            >
                                &lt;
                            </button>

                            <input
                                type="text"
                                className="value-display"
                                value={`${settings.paddingPx}px`}
                                readOnly
                            />
                            <button
                                type="button"
                                onClick={() => handlePaddingChange('increase')}
                                disabled={settings.paddingPx >= PADDING_MAX}
                            >
                                &gt;
                            </button>
                        </div>
                    </div>

                    {/* Text Alignment */}
                    <div className="setting-group">
                        <label>Aligment</label>
                        <div className="alignment-options">
                            {ALIGNMENTS.map(align => (
                                <button
                                    key={align.name}
                                    className={`align-btn ${settings.alignment === align.name ? 'active' : ''}`}
                                    onClick={() => updateSetting('alignment', align.name)}
                                    title={align.name.charAt(0).toUpperCase() + align.name.slice(1)}
                                >
                                    {align.icon}
                                </button>
                            ))}
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default ReaderSettingsPanel;