import {
    AppBar, Toolbar, Typography, Box, Avatar, Tabs, Tab, IconButton, Menu, MenuItem, ListItemIcon, Divider,
    Drawer, List, ListItemButton, ListItemText,
} from '@mui/material';
import { KeyboardArrowDown, Person, Logout, Menu as MenuIcon } from '@mui/icons-material';
import { useNavigate, useLocation } from 'react-router-dom';
import { useState, type ReactNode } from 'react';
import { useAuth } from '../context/AuthContext';

const tabs = [
    { label: 'Panel', path: '/dashboard' },
    { label: 'Cevap Şablonları', path: '/answer-templates' },
    { label: 'Sorular', path: '/questions' },
    { label: 'Anketler', path: '/surveys' },
    { label: 'Doldurmam Gerekenler', path: '/my-surveys' },
];

function Layout({ children }: { children: ReactNode }) {
    const { email, logout } = useAuth();
    const navigate = useNavigate();
    const location = useLocation();
    const [menuAnchor, setMenuAnchor] = useState<null | HTMLElement>(null);
    const [drawerOpen, setDrawerOpen] = useState(false);

    const handleLogout = () => {
        setMenuAnchor(null);
        logout();
        navigate('/login');
    };

    const handleProfile = () => {
        setMenuAnchor(null);
        navigate('/profile');
    };

    const avatarLetter = email ? email.charAt(0).toUpperCase() : '?';

    const activeTab = [...tabs].reverse().find((t) => location.pathname.startsWith(t.path))?.path ?? false;

    return (
        <Box>
            <AppBar position="static">
                <Toolbar sx={{ display: 'flex', justifyContent: 'space-between', gap: 1, px: { xs: 1.5, sm: 2 } }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, minWidth: 0 }}>
                        <IconButton
                            color="inherit"
                            onClick={() => setDrawerOpen(true)}
                            sx={{ display: { xs: 'inline-flex', sm: 'none' } }}
                        >
                            <MenuIcon />
                        </IconButton>
                        <Box
                            onClick={() => navigate('/dashboard')}
                            sx={{ display: 'flex', alignItems: 'center', gap: 1, cursor: 'pointer', minWidth: 0 }}
                        >
                        <Box
                            component="img"
                            src="/favicon.svg"
                            alt="Survey App logo"
                            sx={{ width: 28, height: 28, filter: 'brightness(0) invert(1)', flexShrink: 0 }}
                        />
                        <Typography variant="h6" noWrap sx={{ fontSize: { xs: '1.05rem', sm: '1.25rem' } }}>
                            Survey App
                        </Typography>
                        </Box>
                    </Box>
                    <Box sx={{ display: 'flex', alignItems: 'center' }}>
                        <IconButton
                            color="inherit"
                            onClick={(e) => setMenuAnchor(e.currentTarget)}
                            sx={{ display: 'flex', alignItems: 'center', gap: 0.5, minWidth: 0, borderRadius: 2, px: 1 }}
                        >
                            <Avatar
                                sx={{
                                    width: 32,
                                    height: 32,
                                    bgcolor: '#0F7A6C',
                                    fontSize: 14,
                                    border: '1px solid white',
                                    flexShrink: 0,
                                }}
                            >
                                {avatarLetter}
                            </Avatar>
                            <Typography
                                variant="body2"
                                noWrap
                                sx={{ display: { xs: 'none', sm: 'block' }, maxWidth: 200, color: 'inherit', textTransform: 'none' }}
                            >
                                {email}
                            </Typography>
                            <KeyboardArrowDown sx={{ color: 'inherit', fontSize: 20 }} />
                        </IconButton>
                        <Menu anchorEl={menuAnchor} open={!!menuAnchor} onClose={() => setMenuAnchor(null)}>
                            <MenuItem onClick={handleProfile}>
                                <ListItemIcon>
                                    <Person fontSize="small" />
                                </ListItemIcon>
                                Profilim
                            </MenuItem>
                            <Divider />
                            <MenuItem onClick={handleLogout}>
                                <ListItemIcon>
                                    <Logout fontSize="small" />
                                </ListItemIcon>
                                Çıkış Yap
                            </MenuItem>
                        </Menu>
                    </Box>
                </Toolbar>
                <Tabs
                    value={activeTab}
                    textColor="inherit"
                    indicatorColor="secondary"
                    variant="scrollable"
                    scrollButtons={false}
                    sx={{
                        display: { xs: 'none', sm: 'flex' },
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
                    {tabs.map((t) => (
                        <Tab key={t.path} label={t.label} value={t.path} onClick={() => navigate(t.path)} />
                    ))}
                </Tabs>
            </AppBar>
            <Drawer anchor="left" open={drawerOpen} onClose={() => setDrawerOpen(false)}>
                <Box sx={{ width: 260 }} role="presentation">
                    <List>
                        {tabs.map((t) => (
                            <ListItemButton
                                key={t.path}
                                selected={activeTab === t.path}
                                onClick={() => {
                                    setDrawerOpen(false);
                                    navigate(t.path);
                                }}
                            >
                                <ListItemText primary={t.label} />
                            </ListItemButton>
                        ))}
                    </List>
                </Box>
            </Drawer>
            <Box sx={{ padding: { xs: 2, sm: 3 } }}>{children}</Box>
        </Box>
    );
}

export default Layout;
