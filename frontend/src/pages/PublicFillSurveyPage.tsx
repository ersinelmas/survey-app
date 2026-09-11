import { useEffect, useState } from 'react';
import { useParams, useNavigate, Link as RouterLink } from 'react-router-dom';
import {
    Box, Typography, Paper, RadioGroup, Radio, Checkbox, FormControlLabel, FormControl,
    FormLabel, FormGroup, TextField, Button, Alert, Container, Link,
} from '@mui/material';
import { getPublicSurvey, submitPublicSurvey } from '../api/surveyFillingApi';
import type { PublicSurveyDetail, SurveyFillQuestion, SubmitAnswer } from '../types/surveyFilling';
import { extractErrorMessage } from '../api/errorHelper';
import { useAuth } from '../context/AuthContext';

interface QuestionAnswer {
    optionIds: string[];
    text: string;
}

function isAnswered(question: SurveyFillQuestion, answer: QuestionAnswer | undefined): boolean {
    if (!answer) return false;
    if (question.type === 'FreeText') return answer.text.trim().length > 0;
    return answer.optionIds.length > 0;
}

function toSubmitAnswer(questionId: string, question: SurveyFillQuestion, answer: QuestionAnswer): SubmitAnswer {
    if (question.type === 'FreeText') {
        return { questionId, selectedOptionIds: [], textValue: answer.text.trim() };
    }
    return { questionId, selectedOptionIds: answer.optionIds, textValue: null };
}

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
    const [answers, setAnswers] = useState<Record<string, QuestionAnswer>>({});
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

    const handleSingleChoiceChange = (questionId: string, optionId: string) => {
        setAnswers((prev) => ({ ...prev, [questionId]: { optionIds: [optionId], text: '' } }));
    };

    const handleMultipleChoiceChange = (questionId: string, optionId: string, checked: boolean) => {
        setAnswers((prev) => {
            const current = prev[questionId]?.optionIds ?? [];
            const optionIds = checked ? [...current, optionId] : current.filter((id) => id !== optionId);
            return { ...prev, [questionId]: { optionIds, text: '' } };
        });
    };

    const handleTextChange = (questionId: string, text: string) => {
        setAnswers((prev) => ({ ...prev, [questionId]: { optionIds: [], text } }));
    };

    const handleSubmit = async () => {
        if (!survey || !surveyId || submitting) return;

        if (!survey.questions.every((q) => isAnswered(q, answers[q.questionId]))) {
            setError('Lütfen tüm soruları cevaplayın.');
            return;
        }

        setSubmitting(true);
        setError('');
        try {
            await submitPublicSurvey(surveyId, {
                answers: survey.questions.map((q) => toSubmitAnswer(q.questionId, q, answers[q.questionId])),
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
                            <FormControl fullWidth>
                                <FormLabel>
                                    {index + 1}. {question.text}
                                </FormLabel>
                                {question.type === 'SingleChoice' && (
                                    <RadioGroup
                                        value={answers[question.questionId]?.optionIds[0] || ''}
                                        onChange={(e) => handleSingleChoiceChange(question.questionId, e.target.value)}
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
                                )}
                                {question.type === 'MultipleChoice' && (
                                    <FormGroup>
                                        {question.options.map((option) => (
                                            <FormControlLabel
                                                key={option.optionId}
                                                control={
                                                    <Checkbox
                                                        checked={answers[question.questionId]?.optionIds.includes(option.optionId) ?? false}
                                                        onChange={(e) => handleMultipleChoiceChange(question.questionId, option.optionId, e.target.checked)}
                                                    />
                                                }
                                                label={option.text}
                                            />
                                        ))}
                                    </FormGroup>
                                )}
                                {question.type === 'FreeText' && (
                                    <TextField
                                        multiline
                                        minRows={2}
                                        sx={{ mt: 1 }}
                                        value={answers[question.questionId]?.text ?? ''}
                                        onChange={(e) => handleTextChange(question.questionId, e.target.value)}
                                    />
                                )}
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
