import { Box, Typography, Card, CardActionArea, CardContent, Grid } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { ListAlt, HelpOutlined, Assignment, EventAvailable } from '@mui/icons-material';
import Layout from '../components/Layout';
import { fontFamilyMono } from '../theme';

const sections = [
    {
        step: '01',
        title: 'Cevap Şablonları',
        description: 'Anketlerde kullanılacak şık kalıplarını (Evet/Hayır, 1-4 puan vb.) tanımlayın.',
        icon: <ListAlt sx={{ fontSize: 32 }} />,
        path: '/answer-templates',
    },
    {
        step: '02',
        title: 'Sorular',
        description: 'Cevap şablonlarına bağlı soruları oluşturun ve yönetin.',
        icon: <HelpOutlined sx={{ fontSize: 32 }} />,
        path: '/questions',
    },
    {
        step: '03',
        title: 'Anketler',
        description: 'Soruları bir araya getirip kullanıcılara atayın, sonuçları raporlayın.',
        icon: <Assignment sx={{ fontSize: 32 }} />,
        path: '/surveys',
    },
    {
        step: '04',
        title: 'Doldurmam Gerekenler',
        description: 'Size atanan ve son tarihi geçmemiş anketleri görüntüleyip doldurun.',
        icon: <EventAvailable sx={{ fontSize: 32 }} />,
        path: '/my-surveys',
    },
];

function Dashboard() {
    const navigate = useNavigate();

    return (
        <Layout>
            <Typography variant="h5" sx={{ mb: 1 }}>
                Panel
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                Kendi anketlerinizi oluşturun ve size atanan anketleri doldurun.
            </Typography>

            <Grid container spacing={2}>
                {sections.map((section) => (
                    <Grid size={{ xs: 12, sm: 6, md: 4 }} key={section.path} sx={{ display: 'flex' }}>
                        <Card
                            sx={{
                                display: 'flex',
                                flexDirection: 'column',
                                width: '100%',
                                transition: 'border-color 0.15s ease',
                                '&:hover': { borderColor: 'secondary.main' },
                            }}
                        >
                            <CardActionArea
                                onClick={() => navigate(section.path)}
                                sx={{ height: '100%', display: 'flex', flexDirection: 'column', alignItems: 'stretch' }}
                            >
                                <CardContent sx={{ flexGrow: 1 }}>
                                    <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1.5 }}>
                                        <Box sx={{ color: 'secondary.main', display: 'flex' }}>{section.icon}</Box>
                                        <Typography
                                            sx={{
                                                fontFamily: fontFamilyMono,
                                                fontSize: '0.8125rem',
                                                color: 'text.secondary',
                                                letterSpacing: '0.05em',
                                            }}
                                        >
                                            {section.step}
                                        </Typography>
                                    </Box>
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

export default Dashboard;
