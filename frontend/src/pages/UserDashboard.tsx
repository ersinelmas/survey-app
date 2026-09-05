import { useEffect, useState } from 'react';
import {
    Box, Typography, Card, CardContent, CardActions, Button, Grid, Chip,
} from '@mui/material';
import { EventAvailable } from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import Layout from '../components/Layout';
import { getMyActiveSurveys } from '../api/surveyFillingApi';
import type { AssignedSurvey } from '../types/surveyFilling';
import { useSnackbar } from '../context/SnackbarContext';
import { extractErrorMessage } from '../api/errorHelper';
import { fontFamilyMono } from '../theme';

function daysUntil(endDate: string): number {
    const end = new Date(endDate.slice(0, 10) + 'T23:59:59');
    const now = new Date();
    return Math.ceil((end.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));
}

function UserDashboard() {
    const navigate = useNavigate();
    const { showError } = useSnackbar();
    const [surveys, setSurveys] = useState<AssignedSurvey[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        getMyActiveSurveys()
            .then(setSurveys)
            .catch((err) => showError(extractErrorMessage(err)))
            .finally(() => setLoading(false));
    }, [showError]);

    return (
        <Layout>
            <Typography variant="h5" sx={{ mb: 1 }}>
                Doldurmanız Gereken Anketler
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                Size atanan ve son tarihi geçmemiş anketler burada listelenir.
            </Typography>

            {loading && <Typography color="text.secondary">Yükleniyor...</Typography>}

            {!loading && surveys.length === 0 && (
                <Box
                    sx={{
                        border: '1px dashed',
                        borderColor: 'divider',
                        borderRadius: 1,
                        p: 4,
                        textAlign: 'center',
                    }}
                >
                    <Typography color="text.secondary">
                        Şu anda doldurmanız gereken bir anket bulunmuyor.
                    </Typography>
                </Box>
            )}

            <Grid container spacing={2}>
                {surveys.map((survey) => {
                    const remaining = daysUntil(survey.endDate);
                    const urgent = remaining <= 3;
                    return (
                        <Grid size={{ xs: 12, sm: 6, md: 4 }} key={survey.surveyId}>
                            <Card sx={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
                                <CardContent sx={{ flexGrow: 1 }}>
                                    <Typography variant="h6" sx={{ mb: 0.5 }}>{survey.title}</Typography>
                                    <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                                        {survey.description}
                                    </Typography>
                                    <Chip
                                        icon={<EventAvailable sx={{ fontSize: 16 }} />}
                                        size="small"
                                        variant="outlined"
                                        color={urgent ? 'warning' : 'default'}
                                        label={
                                            <span style={{ fontFamily: fontFamilyMono, fontSize: '0.75rem' }}>
                                                {survey.endDate.slice(0, 10)} · {remaining >= 0 ? `${remaining} gün kaldı` : 'süre doldu'}
                                            </span>
                                        }
                                    />
                                </CardContent>
                                <CardActions sx={{ px: 2, pb: 2 }}>
                                    <Button
                                        size="small"
                                        variant="contained"
                                        onClick={() => navigate(`/my-surveys/${survey.surveyId}`)}
                                    >
                                        Anketi Doldur
                                    </Button>
                                </CardActions>
                            </Card>
                        </Grid>
                    );
                })}
            </Grid>
        </Layout>
    );
}

export default UserDashboard;
