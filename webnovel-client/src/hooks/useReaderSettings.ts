import { useContext } from 'react';
import { ReaderSettingsContext } from '../context/ReaderSettingsContext';

export const useReaderSettings = () => {
    const context = useContext(ReaderSettingsContext);
    if (context === undefined) {
        throw new Error('useReaderSettings must be used within a ReaderSettingsProvider');
    }
    return context;
};