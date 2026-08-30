import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
    Box, Typography, Paper, RadioGroup, Radio, FormControlLabel, FormControl,
    FormLabel, Button, Alert,
} from '@mui/material';
import Layout from '../components/Layout';
import { getSurveyToFill, submitSurvey } from '../api/surveyFillingApi';
import type { SurveyFillDetail } from '../types/surveyFilling';
import { extractErrorMessage } from '../api/errorHelper';

function FillSurveyPage() {
    const { surveyId } = useParams<{ surveyId: string }>();
    const navigate = useNavigate();
    const [survey, setSurvey] = useState<SurveyFillDetail | null>(null);
    const [answers, setAnswers] = useState<Record<string, string>>({});
    const [error, setError] = useState('');
    const [submitting, setSubmitting] = useState(false);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        if (!surveyId) return;
        getSurveyToFill(surveyId)
            .then((data) => {
                setSurvey(data);
                setLoading(false);
            })
            .catch(() => {
                setError('Bu anket şu anda görüntülenemiyor.');
                setLoading(false);
            });
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
            await submitSurvey(surveyId, {
                answers: Object.entries(answers).map(([questionId, selectedOptionId]) => ({
                    questionId,
                    selectedOptionId,
                })),
            });
            navigate('/my-surveys');
        } catch (err) {
            setError(extractErrorMessage(err));
        } finally {
            setSubmitting(false);
        }
    };

    if (loading) {
        return (
            <Layout>
                <Typography>Yükleniyor...</Typography>
            </Layout>
        );
    }

    if (!survey) {
        return (
            <Layout>
                <Alert severity="error">{error || 'Anket bulunamadı.'}</Alert>
            </Layout>
        );
    }

    return (
        <Layout>
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

            <Box sx={{ mt: 2 }}>
                <Button variant="contained" onClick={handleSubmit} disabled={submitting}>
                    {submitting ? 'Gönderiliyor...' : 'Anketi Gönder'}
                </Button>
            </Box>
        </Layout>
    );
}

export default FillSurveyPage;