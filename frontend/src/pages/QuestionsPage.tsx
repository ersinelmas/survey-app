import { useEffect, useState } from 'react';
import {
    Box, Button, Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
    Paper, IconButton, Dialog, DialogTitle, DialogContent, DialogActions, TextField,
    Typography, MenuItem,
} from '@mui/material';
import { Add, Edit, Delete } from '@mui/icons-material';
import Layout from '../components/Layout';
import { getQuestions, createQuestion, updateQuestion, deleteQuestion } from '../api/questionApi';
import { getAnswerTemplates } from '../api/answerTemplateApi';
import type { Question } from '../types/question';
import type { AnswerTemplate } from '../types/answerTemplate';

function QuestionsPage() {
    const [questions, setQuestions] = useState<Question[]>([]);
    const [templates, setTemplates] = useState<AnswerTemplate[]>([]);
    const [dialogOpen, setDialogOpen] = useState(false);
    const [editingId, setEditingId] = useState<string | null>(null);
    const [text, setText] = useState('');
    const [answerTemplateId, setAnswerTemplateId] = useState('');
    const [error, setError] = useState('');
    const [saving, setSaving] = useState(false);

    const loadData = async () => {
        const [questionsData, templatesData] = await Promise.all([
            getQuestions(),
            getAnswerTemplates(),
        ]);
        setQuestions(questionsData);
        setTemplates(templatesData);
    };

    useEffect(() => {
        loadData();
    }, []);

    const openCreateDialog = () => {
        setEditingId(null);
        setText('');
        setAnswerTemplateId('');
        setError('');
        setDialogOpen(true);
    };

    const openEditDialog = (question: Question) => {
        setEditingId(question.id);
        setText(question.text);
        setAnswerTemplateId(question.answerTemplateId);
        setError('');
        setDialogOpen(true);
    };

    const handleSave = async () => {
        if (saving) return;
        if (!answerTemplateId) {
            setError('Lütfen bir cevap şablonu seçin.');
            return;
        }
        setSaving(true);
        setError('');
        try {
            if (editingId) {
                await updateQuestion(editingId, { text, answerTemplateId });
            } else {
                await createQuestion({ text, answerTemplateId });
            }
            setDialogOpen(false);
            loadData();
        } catch (err) {
            setError('Kayıt sırasında bir hata oluştu.');
        } finally {
            setSaving(false);
        }
    };

    const handleDelete = async (id: string) => {
        if (!confirm('Bu soruyu silmek istediğinize emin misiniz?')) return;
        await deleteQuestion(id);
        loadData();
    };

    return (
        <Layout>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
                <Typography variant="h5">Sorular</Typography>
                <Button variant="contained" startIcon={<Add />} onClick={openCreateDialog}>
                    Yeni Soru
                </Button>
            </Box>

            <TableContainer component={Paper}>
                <Table>
                    <TableHead>
                        <TableRow>
                            <TableCell>Soru Metni</TableCell>
                            <TableCell>Cevap Şablonu</TableCell>
                            <TableCell align="right">İşlemler</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {questions.map((question) => (
                            <TableRow key={question.id}>
                                <TableCell>{question.text}</TableCell>
                                <TableCell>{question.answerTemplateName}</TableCell>
                                <TableCell align="right">
                                    <IconButton onClick={() => openEditDialog(question)}>
                                        <Edit fontSize="small" />
                                    </IconButton>
                                    <IconButton onClick={() => handleDelete(question.id)}>
                                        <Delete fontSize="small" />
                                    </IconButton>
                                </TableCell>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>

            <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} fullWidth maxWidth="sm">
                <DialogTitle>{editingId ? 'Soruyu Düzenle' : 'Yeni Soru'}</DialogTitle>
                <DialogContent>
                    {error && <Typography color="error" sx={{ mb: 1 }}>{error}</Typography>}
                    <TextField
                        label="Soru Metni"
                        fullWidth
                        margin="normal"
                        value={text}
                        onChange={(e) => setText(e.target.value)}
                    />
                    <TextField
                        select
                        label="Cevap Şablonu"
                        fullWidth
                        margin="normal"
                        value={answerTemplateId}
                        onChange={(e) => setAnswerTemplateId(e.target.value)}
                    >
                        {templates.map((template) => (
                            <MenuItem key={template.id} value={template.id}>
                                {template.name}
                            </MenuItem>
                        ))}
                    </TextField>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setDialogOpen(false)}>İptal</Button>
                    <Button variant="contained" onClick={handleSave} disabled={saving}>
                        {saving ? 'Kaydediliyor...' : 'Kaydet'}
                    </Button>
                </DialogActions>
            </Dialog>
        </Layout>
    );
}

export default QuestionsPage;