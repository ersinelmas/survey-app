import { Box, Typography, Card, CardActionArea, CardContent, Grid } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { ListAlt, HelpOutlined, Assignment } from '@mui/icons-material';
import Layout from '../components/Layout';

const sections = [
    {
        title: 'Cevap Şablonları',
        description: 'Anketlerde kullanılacak şık kalıplarını (Evet/Hayır, 1-4 puan vb.) tanımlayın.',
        icon: <ListAlt sx={{ fontSize: 36, color: 'primary.main' }} />,
        path: '/admin/answer-templates',
    },
    {
        title: 'Sorular',
        description: 'Cevap şablonlarına bağlı soruları oluşturun ve yönetin.',
        icon: <HelpOutlined sx={{ fontSize: 36, color: 'primary.main' }} />,
        path: '/admin/questions',
    },
    {
        title: 'Anketler',
        description: 'Soruları bir araya getirip kullanıcılara atayın, sonuçları raporlayın.',
        icon: <Assignment sx={{ fontSize: 36, color: 'primary.main' }} />,
        path: '/admin/surveys',
    },
];

function AdminDashboard() {
    const navigate = useNavigate();

    return (
        <Layout>
            <Typography variant="h5" sx={{ mb: 1 }}>
                Admin Paneli
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                Anket sisteminizi yönetmek için aşağıdaki bölümlerden birini seçin.
            </Typography>

            <Grid container spacing={2}>
                {sections.map((section) => (
                    <Grid size={{ xs: 12, sm: 6, md: 4 }} key={section.path} sx={{ display: 'flex' }}>
                        <Card sx={{ display: 'flex', flexDirection: 'column', width: '100%' }}>
                            <CardActionArea
                                onClick={() => navigate(section.path)}
                                sx={{ height: '100%', display: 'flex', flexDirection: 'column', alignItems: 'stretch' }}
                            >
                                <CardContent sx={{ flexGrow: 1 }}>
                                    <Box sx={{ mb: 1 }}>{section.icon}</Box>
                                    <Typography variant="h6" sx={{ mb: 0.5 }}>
                                        {section.title}
                                    </Typography>
                                    <Typography variant="body2" color="text.secondary">
                                        {section.description}
                                    </Typography>
                                </CardContent>
                            </CardActionArea>
                        </Card>
                    </Grid>
                ))}
            </Grid>
        </Layout>
    );
}

export default AdminDashboard;