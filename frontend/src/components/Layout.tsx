import { AppBar, Toolbar, Typography, Button, Box, Avatar, Tabs, Tab } from '@mui/material';
import { useNavigate, useLocation } from 'react-router-dom';
import type { ReactNode } from 'react';
import { useAuth } from '../context/AuthContext';

const adminTabs = [
    { label: 'Panel', path: '/admin' },
    { label: 'Cevap Şablonları', path: '/admin/answer-templates' },
    { label: 'Sorular', path: '/admin/questions' },
    { label: 'Anketler', path: '/admin/surveys' },
];

function Layout({ children }: { children: ReactNode }) {
    const { email, role, logout } = useAuth();
    const navigate = useNavigate();
    const location = useLocation();

    const handleLogout = () => {
        logout();
        navigate('/login');
    };

    const handleHomeClick = () => {
        navigate(role === 'Admin' ? '/admin' : '/my-surveys');
    };

    const avatarLetter = email ? email.charAt(0).toUpperCase() : '?';
    const avatarColor = role === 'Admin' ? '#C77A1F' : '#0F7A6C';

    const activeTab = [...adminTabs].reverse().find((t) => location.pathname.startsWith(t.path))?.path ?? false;

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
                {role === 'Admin' && (
                    <Tabs
                        value={activeTab}
                        textColor="inherit"
                        indicatorColor="secondary"
                        variant="scrollable"
                        scrollButtons={false}
                        sx={{
                            minHeight: 40,
                            borderTop: '1px solid rgba(255,255,255,0.12)',
                            '& .MuiTab-root': {
                                minHeight: 40,
                                textTransform: 'none',
                                color: 'rgba(255,255,255,0.7)',
                                '&.Mui-selected': { color: '#fff' },
                            },
                        }}
                    >
                        {adminTabs.map((t) => (
                            <Tab key={t.path} label={t.label} value={t.path} onClick={() => navigate(t.path)} />
                        ))}
                    </Tabs>
                )}
            </AppBar>
            <Box sx={{ padding: 3 }}>{children}</Box>
        </Box>
    );
}

export default Layout;
