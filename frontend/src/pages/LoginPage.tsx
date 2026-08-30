import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { Box, Button, TextField, Typography, Alert, Paper, Divider } from '@mui/material';
import { useAuth } from '../context/AuthContext';
import { extractErrorMessage } from '../api/errorHelper';

function LoginPage() {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const { login } = useAuth();
    const navigate = useNavigate();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError('');
        try {
            await login({ email, password });
            navigate('/');
        } catch (err) {
            setError(extractErrorMessage(err));
        }
    };

    return (
        <Box
            sx={{
                display: 'flex',
                justifyContent: 'center',
                alignItems: 'center',
                minHeight: '100vh',
            }}
        >
            <Paper elevation={3} sx={{ padding: 4, width: 350 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 1, mb: 1 }}>
                    <Box component="img" src="/favicon.svg" alt="Survey App logo" sx={{ width: 40, height: 40 }} />
                    <Typography variant="h5" sx={{ fontWeight: 600, color: 'primary.main' }}>
                        Survey App
                    </Typography>
                </Box>

                <Divider sx={{ mb: 3 }} />

                <Typography variant="h5" sx={{ mb: 2, fontWeight: 500 }}>
                    Giriş Yap
                </Typography>
                {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
                <form onSubmit={handleSubmit}>
                    <TextField
                        label="Email"
                        type="email"
                        fullWidth
                        margin="normal"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        required
                    />
                    <TextField
                        label="Şifre"
                        type="password"
                        fullWidth
                        margin="normal"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                    />
                    <Button type="submit" variant="contained" fullWidth sx={{ mt: 2 }}>
                        Giriş Yap
                    </Button>
                </form>
                <Typography variant="body2" sx={{ mt: 2, textAlign: 'center' }}>
                    Hesabınız yok mu? <Link to="/register">Kayıt Ol</Link>
                </Typography>
            </Paper>
        </Box>
    );
}

export default LoginPage;