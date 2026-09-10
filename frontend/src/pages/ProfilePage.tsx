import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
    Box, Paper, Typography, TextField, Button, Divider, Alert,
    Dialog, DialogTitle, DialogContent, DialogActions, DialogContentText,
} from '@mui/material';
import Layout from '../components/Layout';
import BackButton from '../components/BackButton';
import { useAuth } from '../context/AuthContext';
import { useSnackbar } from '../context/SnackbarContext';
import { changePassword, deleteAccount } from '../api/authApi';
import { extractErrorMessage } from '../api/errorHelper';

function ProfilePage() {
    const { email, logout } = useAuth();
    const { showSuccess } = useSnackbar();
    const navigate = useNavigate();

    const [currentPassword, setCurrentPassword] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [passwordError, setPasswordError] = useState('');
    const [savingPassword, setSavingPassword] = useState(false);

    const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
    const [deletePassword, setDeletePassword] = useState('');
    const [deleteError, setDeleteError] = useState('');
    const [deleting, setDeleting] = useState(false);

    const handleChangePassword = async (e: React.FormEvent) => {
        e.preventDefault();
        setPasswordError('');
        setSavingPassword(true);
        try {
            await changePassword({ currentPassword, newPassword });
            setCurrentPassword('');
            setNewPassword('');
            showSuccess('Şifreniz güncellendi.');
        } catch (err) {
            setPasswordError(extractErrorMessage(err));
        } finally {
            setSavingPassword(false);
        }
    };

    const openDeleteDialog = () => {
        setDeletePassword('');
        setDeleteError('');
        setDeleteDialogOpen(true);
    };

    const handleDeleteAccount = async () => {
        setDeleteError('');
        setDeleting(true);
        try {
            await deleteAccount({ password: deletePassword });
            logout();
            navigate('/login');
            showSuccess('Hesabınız silindi.');
        } catch (err) {
            setDeleteError(extractErrorMessage(err));
        } finally {
            setDeleting(false);
        }
    };

    return (
        <Layout>
            <BackButton label="Panele Dön" onClick={() => navigate('/dashboard')} />
            <Box sx={{ maxWidth: 480, mx: 'auto', display: 'flex', flexDirection: 'column', gap: 3 }}>
                <Paper sx={{ p: 3 }}>
                    <Typography variant="h6" sx={{ mb: 0.5 }}>Hesabım</Typography>
                    <Typography variant="body2" color="text.secondary">{email}</Typography>
                </Paper>

                <Paper sx={{ p: 3 }}>
                    <Typography variant="h6" sx={{ mb: 2 }}>Şifre Değiştir</Typography>
                    {passwordError && <Alert severity="error" sx={{ mb: 2 }}>{passwordError}</Alert>}
                    <form onSubmit={handleChangePassword}>
                        <TextField
                            label="Mevcut Şifre"
                            type="password"
                            fullWidth
                            margin="normal"
                            value={currentPassword}
                            onChange={(e) => setCurrentPassword(e.target.value)}
                            required
                        />
                        <TextField
                            label="Yeni Şifre"
                            type="password"
                            fullWidth
                            margin="normal"
                            value={newPassword}
                            onChange={(e) => setNewPassword(e.target.value)}
                            required
                        />
                        <Button type="submit" variant="contained" disabled={savingPassword} sx={{ mt: 2 }}>
                            Şifreyi Güncelle
                        </Button>
                    </form>
                </Paper>

                <Paper sx={{ p: 3, borderColor: 'error.main', borderWidth: 1, borderStyle: 'solid' }}>
                    <Typography variant="h6" color="error" sx={{ mb: 1 }}>Hesabı Sil</Typography>
                    <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                        Hesabınızı sildiğinizde size ait tüm anketler, sorular ve cevap şablonları kalıcı olarak silinir.
                        Anketlerinize başka kullanıcıların verdiği cevaplar ve tamamlama kayıtları da bu anketlerle birlikte silinir.
                        Bu işlem geri alınamaz.
                    </Typography>
                    <Divider sx={{ mb: 2 }} />
                    <Button variant="outlined" color="error" onClick={openDeleteDialog}>
                        Hesabımı Sil
                    </Button>
                </Paper>
            </Box>

            <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)} fullWidth maxWidth="xs">
                <DialogTitle>Hesabını silmek istediğine emin misin?</DialogTitle>
                <DialogContent>
                    <DialogContentText sx={{ mb: 2 }}>
                        Bu işlem geri alınamaz. Onaylamak için şifreni gir.
                    </DialogContentText>
                    {deleteError && <Alert severity="error" sx={{ mb: 2 }}>{deleteError}</Alert>}
                    <TextField
                        label="Şifre"
                        type="password"
                        fullWidth
                        autoFocus
                        value={deletePassword}
                        onChange={(e) => setDeletePassword(e.target.value)}
                    />
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setDeleteDialogOpen(false)}>Vazgeç</Button>
                    <Button
                        color="error"
                        variant="contained"
                        disabled={deleting || !deletePassword}
                        onClick={handleDeleteAccount}
                    >
                        Hesabımı Kalıcı Olarak Sil
                    </Button>
                </DialogActions>
            </Dialog>
        </Layout>
    );
}

export default ProfilePage;
