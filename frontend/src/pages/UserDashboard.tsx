import { useEffect, useState } from 'react';
import {
    Typography, Card, CardContent, CardActions, Button, Grid,
} from '@mui/material';
import { useNavigate } from 'react-router-dom';
import Layout from '../components/Layout';
import { getMyActiveSurveys } from '../api/surveyFillingApi';
import type { AssignedSurvey } from '../types/surveyFilling';

function UserDashboard() {
    const navigate = useNavigate();
    const [surveys, setSurveys] = useState<AssignedSurvey[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        getMyActiveSurveys().then((data) => {
            setSurveys(data);
            setLoading(false);
        });
    }, []);

    return (
        <Layout>
            <Typography variant="h5" sx={{ mb: 2 }}>
                Doldurmanız Gereken Anketler
            </Typography>

            {loading && <Typography>Yükleniyor...</Typography>}

            {!loading && surveys.length === 0 && (
                <Typography color="text.secondary">
                    Şu anda doldurmanız gereken bir anket bulunmuyor.
                </Typography>
            )}

            <Grid container spacing={2}>
                {surveys.map((survey) => (
                    <Grid size={{ xs: 12, sm: 6, md: 4 }} key={survey.surveyId}>
                        <Card>
                            <CardContent>
                                <Typography variant="h6">{survey.title}</Typography>
                                <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                                    {survey.description}
                                </Typography>
                                <Typography variant="caption" color="text.secondary">
                                    Son tarih: {survey.endDate.slice(0, 10)}
                                </Typography>
                            </CardContent>
                            <CardActions>
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
                ))}
            </Grid>
        </Layout>
    );
}

export default UserDashboard;