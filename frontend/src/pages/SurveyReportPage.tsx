import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
    Box, Typography, Paper, Table, TableBody, TableCell, TableContainer, TableHead,
    TableRow, Chip, Grid, Button,
} from '@mui/material';
import { ArrowBack } from '@mui/icons-material';
import Layout from '../components/Layout';
import { getSurveyReport } from '../api/surveyApi';
import type { SurveyReport } from '../types/survey';

function SurveyReportPage() {
    const { surveyId } = useParams<{ surveyId: string }>();
    const navigate = useNavigate();
    const [report, setReport] = useState<SurveyReport | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        if (!surveyId) return;
        getSurveyReport(surveyId).then((data) => {
            setReport(data);
            setLoading(false);
        });
    }, [surveyId]);

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
            <Button startIcon={<ArrowBack />} onClick={() => navigate('/admin/surveys')} sx={{ mb: 2 }}>
                Anketlere Dön
            </Button>

            <Typography variant="h5" sx={{ mb: 3 }}>
                {report.title} - Raporu
            </Typography>

            <Grid container spacing={2} sx={{ mb: 3 }}>
                <Grid size={{ xs: 12, sm: 4 }}>
                    <Paper sx={{ p: 2, textAlign: 'center' }}>
                        <Typography variant="h4">{report.totalAssigned}</Typography>
                        <Typography variant="body2" color="text.secondary">Atanan Kullanıcı</Typography>
                    </Paper>
                </Grid>
                <Grid size={{ xs: 12, sm: 4 }}>
                    <Paper sx={{ p: 2, textAlign: 'center' }}>
                        <Typography variant="h4">{report.totalCompleted}</Typography>
                        <Typography variant="body2" color="text.secondary">Tamamlayan</Typography>
                    </Paper>
                </Grid>
                <Grid size={{ xs: 12, sm: 4 }}>
                    <Paper sx={{ p: 2, textAlign: 'center' }}>
                        <Typography variant="h4">%{completionRate}</Typography>
                        <Typography variant="body2" color="text.secondary">Tamamlanma Oranı</Typography>
                    </Paper>
                </Grid>
            </Grid>

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
                            color="success"
                            size="small"
                            sx={{ mr: 1, mb: 1 }}
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
            {report.questionSummaries.map((q) => (
                <TableContainer component={Paper} key={q.questionId} sx={{ mb: 2 }}>
                    <Box sx={{ p: 2, pb: 0 }}>
                        <Typography variant="subtitle1">{q.questionText}</Typography>
                    </Box>
                    <Table size="small">
                        <TableHead>
                            <TableRow>
                                <TableCell>Kullanıcı</TableCell>
                                <TableCell>Cevap</TableCell>
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