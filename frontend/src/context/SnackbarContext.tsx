import { createContext, useContext, useState, type ReactNode } from 'react';
import { Snackbar, Alert } from '@mui/material';

interface SnackbarContextType {
    showError: (message: string) => void;
}

const SnackbarContext = createContext<SnackbarContextType | undefined>(undefined);

export function SnackbarProvider({ children }: { children: ReactNode }) {
    const [message, setMessage] = useState('');
    const [open, setOpen] = useState(false);

    const showError = (msg: string) => {
        setMessage(msg);
        setOpen(true);
    };

    const handleClose = () => setOpen(false);

    return (
        <SnackbarContext.Provider value={{ showError }}>
            {children}
            <Snackbar open={open} autoHideDuration={5000} onClose={handleClose}>
                <Alert onClose={handleClose} severity="error" sx={{ width: '100%' }}>
                    {message}
                </Alert>
            </Snackbar>
        </SnackbarContext.Provider>
    );
}

export function useSnackbar() {
    const context = useContext(SnackbarContext);
    if (!context) {
        throw new Error('useSnackbar must be used within SnackbarProvider');
    }
    return context;
}
