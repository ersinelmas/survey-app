import { AppBar, Toolbar, Typography, Button, Box } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import type { ReactNode } from 'react';
import { useAuth } from '../context/AuthContext';

function Layout({ children }: { children: ReactNode }) {
    const { email, role, logout } = useAuth();
    const navigate = useNavigate();

    const handleLogout = () => {
        logout();
        navigate('/login');
    };

    return (
        <Box>
            <AppBar position="static">
                <Toolbar sx={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Typography variant="h6">Survey App</Typography>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                        <Typography variant="body2">{email} ({role})</Typography>
                        <Button color="inherit" onClick={handleLogout}>
                            Çıkış Yap
                        </Button>
                    </Box>
                </Toolbar>
            </AppBar>
            <Box sx={{ padding: 3 }}>{children}</Box>
        </Box>
    );
}

export default Layout;