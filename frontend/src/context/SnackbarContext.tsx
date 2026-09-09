import { createContext, useContext, useState, type ReactNode } from 'react';
import { Snackbar, Alert, type AlertColor } from '@mui/material';

interface SnackbarContextType {
    showError: (message: string) => void;
    showSuccess: (message: string) => void;
}

const SnackbarContext = createContext<SnackbarContextType | undefined>(undefined);

export function SnackbarProvider({ children }: { children: ReactNode }) {
    const [message, setMessage] = useState('');
    const [severity, setSeverity] = useState<AlertColor>('error');
    const [open, setOpen] = useState(false);

    const showError = (msg: string) => {
        setMessage(msg);
        setSeverity('error');
        setOpen(true);
    };

    const showSuccess = (msg: string) => {
        setMessage(msg);
        setSeverity('success');
        setOpen(true);
    };

    const handleClose = () => setOpen(false);

    return (
        <SnackbarContext.Provider value={{ showError, showSuccess }}>
            {children}
            <Snackbar open={open} autoHideDuration={5000} onClose={handleClose}>
                <Alert onClose={handleClose} severity={severity} sx={{ width: '100%' }}>
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
