import React from 'react';
import { useReaderSettings } from '../../hooks/useReaderSettings';
import './ReaderSettingsPanel.css';
import { FaTimes } from 'react-icons/fa';

interface ReaderSettingsPanelProps {
    isOpen: boolean;
    onClose: () => void;
}


const FONT_FAMILIES = ["Times New Roman", "Lora", "Roboto", "NotoSans", "Nunito"];
const ALIGNMENTS = ["left", "center", "right"];
const COLORS = [
    { name: 'White', bg: '#FFFFFF', text: '#000000' },
    { name: 'Beige', bg: '#F5F2EC', text: '#333333' },
    { name: 'Grey', bg: '#AAAAAA', text: '#000000' },
    { name: 'Black', bg: '#000000', text: '#FFFFFF' },
];


const FONT_STEP = 2;
const FONT_MIN = 12;
const FONT_MAX = 36;

const PADDING_STEP = 20;
const PADDING_MIN = 0;
const PADDING_MAX = 200; // Giống validation backend

const ReaderSettingsPanel: React.FC<ReaderSettingsPanelProps> = ({ isOpen, onClose }) => {
    const { settings, updateSetting, isLoading } = useReaderSettings();

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
                                    className={`color-dot ${settings.backgroundColor === color.bg ? 'active' : ''}`}
                                    style={{ backgroundColor: color.bg, borderColor: color.text }}
                                    onClick={() => {
                                        updateSetting('backgroundColor', color.bg);
                                        updateSetting('fontColor', color.text);
                                    }}
                                />
                            ))}
                        </div>
                    </div>

                    {/* Font Family */}
                    <div className="setting-group">
                        <label htmlFor="fontFamily">Font</label>
                        <select
                            id="fontFamily"
                            value={settings.fontFamily}
                            onChange={e => updateSetting('fontFamily', e.target.value)}
                        >
                            {FONT_FAMILIES.map(font => <option key={font} value={font}>{font}</option>)}
                        </select>
                    </div>

                    {/* Font Size (Stepper) */}
                    <div className="setting-group">
                        <label htmlFor="fontSize">Font Size</label>
                        <div className="numeric-stepper">
                            <button
                                type="button"
                                onClick={() => handleFontSizeChange('decrease')}
                                disabled={settings.fontSize <= FONT_MIN}
                            >
                                &lt;
                            </button>
                            <span className="value-display">{settings.fontSize}px</span>
                            <button
                                type="button"
                                onClick={() => handleFontSizeChange('increase')}
                                disabled={settings.fontSize >= FONT_MAX}
                            >
                                &gt;
                            </button>
                        </div>
                    </div>

                    {/* Page Margin (Stepper) */}
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
                            <span className="value-display">{settings.paddingPx}px</span>
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
                        <label>Text Align</label>
                        <select
                            id="alignment"
                            value={settings.aligment}
                            onChange={e => updateSetting('aligment', e.target.value)}
                        >
                            {ALIGNMENTS.map(align => <option key={align} value={align}>{align.charAt(0).toUpperCase() + align.slice(1)}</option>)}
                        </select>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default ReaderSettingsPanel;