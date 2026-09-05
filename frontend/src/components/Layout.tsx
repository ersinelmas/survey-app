import { AppBar, Toolbar, Typography, Button, Box, Avatar } from '@mui/material';
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

    const avatarLetter = email ? email.charAt(0).toUpperCase() : '?';
    const avatarColor = role === 'Admin' ? '#C77A1F' : '#0F7A6C';

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
                            sx={{ width: 28, height: 28, filter: 'brightness(0) invert(1)' }}
                        />
                        <Typography variant="h6">Survey App</Typography>
                    </Box>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            <Avatar
                                sx={{
                                    width: 32,
                                    height: 32,
                                    bgcolor: avatarColor,
                                    fontSize: 14,
                                    border: '1px solid white',
                                }}
                            >
                                {avatarLetter}
                            </Avatar>
                            <Typography variant="body2">{email}</Typography>
                        </Box>
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