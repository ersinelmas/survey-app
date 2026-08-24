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

    const handleHomeClick = () => {
        navigate(role === 'Admin' ? '/admin' : '/my-surveys');
    };

    return (
        <Box>
            <AppBar position="static">
                <Toolbar sx={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Box
                        onClick={handleHomeClick}
                        sx={{ display: 'flex', alignItems: 'center', gap: 1, cursor: 'pointer' }}
                    >
                        <Box
                            component="img"
                            src="/favicon.svg"
                            alt="Survey App logo"
                            sx={{ width: 28, height: 28, filter: 'brightness(0) invert(1)', }}
                        />
                        <Typography variant="h6">Survey App</Typography>
                    </Box>
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