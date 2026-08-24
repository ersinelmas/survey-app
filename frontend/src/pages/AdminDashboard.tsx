import { Box, Button, Typography } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import Layout from '../components/Layout';

function AdminDashboard() {
    const navigate = useNavigate();

    return (
        <Layout>
            <Typography variant="h5" sx={{ mb: 2 }}>Admin Paneli</Typography>
            <Box sx={{ display: 'flex', gap: 2 }}>
                <Button variant="outlined" onClick={() => navigate('/admin/answer-templates')}>
                    Cevap Şablonları
                </Button>
                <Button variant="outlined" onClick={() => navigate('/admin/questions')}>
                    Sorular
                </Button>
                <Button variant="outlined" onClick={() => navigate('/admin/surveys')}>
                    Anketler
                </Button>
            </Box>
        </Layout>
    );
}

export default AdminDashboard;