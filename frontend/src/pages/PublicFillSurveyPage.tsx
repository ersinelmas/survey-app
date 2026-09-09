import { useEffect, useState } from 'react';
import { useParams, useNavigate, Link as RouterLink } from 'react-router-dom';
import {
    Box, Typography, Paper, RadioGroup, Radio, FormControlLabel, FormControl,
    FormLabel, Button, Alert, Container, Link,
} from '@mui/material';
import { getPublicSurvey, submitPublicSurvey } from '../api/surveyFillingApi';
import type { PublicSurveyDetail } from '../types/surveyFilling';
import { extractErrorMessage } from '../api/errorHelper';
import { useAuth } from '../context/AuthContext';

function getOrCreateRespondentToken(surveyId: string): string {
    const key = `public-survey-token-${surveyId}`;
    try {
        const existing = localStorage.getItem(key);
        if (existing) return existing;
        const newToken = crypto.randomUUID();
        localStorage.setItem(key, newToken);
        return newToken;
    } catch {
        return crypto.randomUUID();
    }
}

function PublicFillSurveyPage() {
    const { surveyId } = useParams<{ surveyId: string }>();
    const navigate = useNavigate();
    const { isAuthenticated } = useAuth();
    const [survey, setSurvey] = useState<PublicSurveyDetail | null>(null);
    const [answers, setAnswers] = useState<Record<string, string>>({});
    const [error, setError] = useState('');
    const [submitting, setSubmitting] = useState(false);
    const [submitted, setSubmitted] = useState(false);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        if (!surveyId) return;
        getPublicSurvey(surveyId)
            .then(setSurvey)
            .catch(() => setError('Bu anket şu anda görüntülenemiyor.'))
            .finally(() => setLoading(false));
    }, [surveyId]);

    const handleAnswerChange = (questionId: string, optionId: string) => {
        setAnswers((prev) => ({ ...prev, [questionId]: optionId }));
    };

    const handleSubmit = async () => {
        if (!survey || !surveyId || submitting) return;

        if (Object.keys(answers).length !== survey.questions.length) {
            setError('Lütfen tüm soruları cevaplayın.');
            return;
        }

        setSubmitting(true);
        setError('');
        try {
            await submitPublicSurvey(surveyId, {
                answers: Object.entries(answers).map(([questionId, selectedOptionId]) => ({
                    questionId,
                    selectedOptionId,
                })),
                respondentToken: getOrCreateRespondentToken(surveyId),
            });
            setSubmitted(true);
        } catch (err) {
            setError(extractErrorMessage(err));
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <Container maxWidth="sm" sx={{ py: { xs: 3, sm: 5 } }}>
            {loading && <Typography>Yükleniyor...</Typography>}

            {!loading && !survey && (
                <Alert severity="error">{error || 'Anket bulunamadı.'}</Alert>
            )}

            {!loading && survey && survey.requireLogin && !isAuthenticated && (
                <Paper sx={{ p: 3 }}>
                    <Typography variant="h6" sx={{ mb: 1 }}>{survey.title}</Typography>
                    <Alert severity="info" sx={{ mb: 2 }}>
                        Bu anketi yanıtlamak için giriş yapmanız gerekiyor.
                    </Alert>
                    <Box sx={{ display: 'flex', gap: 1 }}>
                        <Button
                            variant="contained"
                            component={RouterLink}
                            to={`/login?redirect=/public/surveys/${surveyId}`}
                        >
                            Giriş Yap
                        </Button>
                        <Button
                            variant="outlined"
                            component={RouterLink}
                            to={`/register?redirect=/public/surveys/${surveyId}`}
                        >
                            Üye Ol
                        </Button>
                    </Box>
                </Paper>
            )}

            {!loading && survey && (!survey.requireLogin || isAuthenticated) && !submitted && (
                <>
                    <Typography variant="h5">{survey.title}</Typography>
                    <Typography variant="body1" color="text.secondary" sx={{ mb: 3 }}>
                        {survey.description}
                    </Typography>

                    {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

                    {survey.questions.map((question, index) => (
                        <Paper key={question.questionId} sx={{ p: 2, mb: 2 }}>
                            <FormControl>
                                <FormLabel>
                                    {index + 1}. {question.text}
                                </FormLabel>
                                <RadioGroup
                                    value={answers[question.questionId] || ''}
                                    onChange={(e) => handleAnswerChange(question.questionId, e.target.value)}
                                >
                                    {question.options.map((option) => (
                                        <FormControlLabel
                                            key={option.optionId}
                                            value={option.optionId}
                                            control={<Radio />}
                                            label={option.text}
                                        />
                                    ))}
                                </RadioGroup>
                            </FormControl>
                        </Paper>
                    ))}

                    <Button variant="contained" onClick={handleSubmit} disabled={submitting}>
                        {submitting ? 'Gönderiliyor...' : 'Anketi Gönder'}
                    </Button>
                </>
            )}

            {submitted && (
                <Paper sx={{ p: 3, textAlign: 'center' }}>
                    <Typography variant="h6" sx={{ mb: 1 }}>Teşekkürler!</Typography>
                    <Typography color="text.secondary" sx={{ mb: 2 }}>
                        Yanıtınız kaydedildi.
                    </Typography>
                    <Link component="button" onClick={() => navigate('/')}>Ana sayfaya dön</Link>
                </Paper>
            )}
        </Container>
    );
}

export default PublicFillSurveyPage;
