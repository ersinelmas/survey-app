import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
    Box, Typography, Paper, RadioGroup, Radio, Checkbox, FormControlLabel, FormControl,
    FormLabel, FormGroup, TextField, Button, Alert,
} from '@mui/material';
import Layout from '../components/Layout';
import BackButton from '../components/BackButton';
import { getSurveyToFill, submitSurvey } from '../api/surveyFillingApi';
import type { SurveyFillDetail, SurveyFillQuestion, SubmitAnswer } from '../types/surveyFilling';
import { extractErrorMessage } from '../api/errorHelper';

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

function FillSurveyPage() {
    const { surveyId } = useParams<{ surveyId: string }>();
    const navigate = useNavigate();
    const [survey, setSurvey] = useState<SurveyFillDetail | null>(null);
    const [answers, setAnswers] = useState<Record<string, QuestionAnswer>>({});
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
            await submitSurvey(surveyId, {
                answers: survey.questions.map((q) => toSubmitAnswer(q.questionId, q, answers[q.questionId])),
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
                <BackButton label="Anketlerime Dön" onClick={() => navigate('/my-surveys')} />
                <Typography>Yükleniyor...</Typography>
            </Layout>
        );
    }

    if (!survey) {
        return (
            <Layout>
                <BackButton label="Anketlerime Dön" onClick={() => navigate('/my-surveys')} />
                <Alert severity="error">{error || 'Anket bulunamadı.'}</Alert>
            </Layout>
        );
    }

    return (
        <Layout>
            <BackButton label="Anketlerime Dön" onClick={() => navigate('/my-surveys')} />
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

            <Box sx={{ mt: 2 }}>
                <Button variant="contained" onClick={handleSubmit} disabled={submitting}>
                    {submitting ? 'Gönderiliyor...' : 'Anketi Gönder'}
                </Button>
            </Box>
        </Layout>
    );
}

export default FillSurveyPage;
