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
import { useCrudPage } from '../hooks/useCrudPage';
import { useSnackbar } from '../context/SnackbarContext';
import { extractErrorMessage } from '../api/errorHelper';

function QuestionsPage() {
    const { showError } = useSnackbar();
    const {
        items: questions, dialogOpen, setDialogOpen, editingId, setEditingId,
        error, setError, saving, handleDelete, runSave,
    } = useCrudPage<Question>({
        fetchAll: getQuestions,
        remove: deleteQuestion,
        deleteConfirmMessage: 'Bu soruyu silmek istediğinize emin misiniz?',
    });
    const [templates, setTemplates] = useState<AnswerTemplate[]>([]);
    const [text, setText] = useState('');
    const [answerTemplateId, setAnswerTemplateId] = useState('');

    useEffect(() => {
        getAnswerTemplates()
            .then(setTemplates)
            .catch((err) => showError(extractErrorMessage(err)));
    }, [showError]);

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

    const handleSave = () => {
        if (!answerTemplateId) {
            setError('Lütfen bir cevap şablonu seçin.');
            return;
        }
        runSave(async () => {
            if (editingId) {
                await updateQuestion(editingId, { text, answerTemplateId });
            } else {
                await createQuestion({ text, answerTemplateId });
            }
        });
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