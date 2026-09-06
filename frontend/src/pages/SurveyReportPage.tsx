import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
    Box, Typography, Paper, Table, TableBody, TableCell, TableContainer, TableHead,
    TableRow, Chip, Grid,
} from '@mui/material';
import Layout from '../components/Layout';
import BackButton from '../components/BackButton';
import CompletionRing from '../components/CompletionRing';
import { getSurveyReport } from '../api/surveyApi';
import type { SurveyReport } from '../types/survey';
import { useSnackbar } from '../context/SnackbarContext';
import { extractErrorMessage } from '../api/errorHelper';
import { fontFamilyMono } from '../theme';

function SurveyReportPage() {
    const { surveyId } = useParams<{ surveyId: string }>();
    const navigate = useNavigate();
    const { showError } = useSnackbar();
    const [report, setReport] = useState<SurveyReport | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        if (!surveyId) return;
        getSurveyReport(surveyId)
            .then(setReport)
            .catch((err) => showError(extractErrorMessage(err)))
            .finally(() => setLoading(false));
    }, [surveyId, showError]);

    if (loading) {
        return (
            <Layout>
                <Typography>Yükleniyor...</Typography>
            </Layout>
        );
    }

    if (!report) {
        return (
            <Layout>
                <Typography color="error">Rapor bulunamadı.</Typography>
            </Layout>
        );
    }

    const completionRate =
        report.totalAssigned > 0
            ? Math.round((report.totalCompleted / report.totalAssigned) * 100)
            : 0;

    return (
        <Layout>
            <BackButton label="Anketlere Dön" onClick={() => navigate('/admin/surveys')} />

            <Typography variant="h5" sx={{ mb: 3 }}>
                {report.title} - Raporu
            </Typography>

            <Paper sx={{ p: 3, mb: 3, display: 'flex', alignItems: 'center', gap: 4, flexWrap: 'wrap' }}>
                <CompletionRing percent={completionRate} size={128} />
                <Box sx={{ display: 'flex', gap: 5 }}>
                    <Box>
                        <Typography sx={{ fontFamily: fontFamilyMono, fontSize: '2rem', fontWeight: 600, lineHeight: 1 }}>
                            {report.totalAssigned}
                        </Typography>
                        <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>Atanan Kullanıcı</Typography>
                    </Box>
                    <Box>
                        <Typography sx={{ fontFamily: fontFamilyMono, fontSize: '2rem', fontWeight: 600, lineHeight: 1, color: 'secondary.main' }}>
                            {report.totalCompleted}
                        </Typography>
                        <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>Tamamlayan</Typography>
                    </Box>
                </Box>
            </Paper>

            <Grid container spacing={2} sx={{ mb: 3 }}>
                <Grid size={{ xs: 12, sm: 6 }}>
                    <Typography variant="h6" sx={{ mb: 1 }}>Tamamlayanlar</Typography>
                    {report.completedByUsers.length === 0 && (
                        <Typography variant="body2" color="text.secondary">Henüz kimse tamamlamadı.</Typography>
                    )}
                    {report.completedByUsers.map((u) => (
                        <Chip
                            key={u.userId}
                            label={u.email}
                            size="small"
                            sx={{ mr: 1, mb: 1, bgcolor: 'secondary.main', color: '#fff' }}
                        />
                    ))}
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                    <Typography variant="h6" sx={{ mb: 1 }}>Bekleyenler</Typography>
                    {report.pendingUsers.length === 0 && (
                        <Typography variant="body2" color="text.secondary">Herkes tamamladı.</Typography>
                    )}
                    {report.pendingUsers.map((u) => (
                        <Chip
                            key={u.userId}
                            label={u.email}
                            variant="outlined"
                            size="small"
                            sx={{ mr: 1, mb: 1 }}
                        />
                    ))}
                </Grid>
            </Grid>

            <Typography variant="h6" sx={{ mb: 1 }}>Soru Bazında Cevaplar</Typography>
            {report.questionSummaries.map((q, index) => (
                <TableContainer component={Paper} key={q.questionId} sx={{ mb: 2 }}>
                    <Box
                        sx={{
                            display: 'flex',
                            alignItems: 'baseline',
                            gap: 1.5,
                            px: 2,
                            py: 1.5,
                            bgcolor: '#F0F2F5',
                            borderBottom: '1px solid',
                            borderColor: 'divider',
                        }}
                    >
                        <Typography
                            sx={{ fontFamily: fontFamilyMono, fontSize: '0.8125rem', color: 'text.secondary', flexShrink: 0 }}
                        >
                            {String(index + 1).padStart(2, '0')}
                        </Typography>
                        <Typography variant="h6" sx={{ fontSize: '1rem' }}>{q.questionText}</Typography>
                    </Box>
                    <Table size="small" sx={{ tableLayout: 'fixed' }}>
                        <TableHead>
                            <TableRow>
                                <TableCell sx={{ width: '50%' }}>KULLANICI</TableCell>
                                <TableCell sx={{ width: '50%' }}>CEVAP</TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {q.userAnswers.length === 0 && (
                                <TableRow>
                                    <TableCell colSpan={2}>
                                        <Typography variant="body2" color="text.secondary">
                                            Henüz cevap yok.
                                        </Typography>
                                    </TableCell>
                                </TableRow>
                            )}
                            {q.userAnswers.map((a, idx) => (
                                <TableRow key={idx}>
                                    <TableCell>{a.userEmail}</TableCell>
                                    <TableCell>{a.selectedOptionText}</TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                </TableContainer>
            ))}
        </Layout>
    );
}

export default SurveyReportPage;