import { useEffect, useState } from 'react';
import {
    Box, Button, Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
    Paper, IconButton, Dialog, DialogTitle, DialogContent, DialogActions, TextField,
    Typography, MenuItem, TablePagination, Chip,
} from '@mui/material';
import { Add, Edit, Delete, ContentCopy, Public, PublicOff } from '@mui/icons-material';
import Layout from '../components/Layout';
import EmptyState from '../components/EmptyState';
import ResponsiveListCard from '../components/ResponsiveListCard';
import {
    getQuestionsPaged, createQuestion, updateQuestion, deleteQuestion, duplicateQuestion, setQuestionIsDefault,
} from '../api/questionApi';
import { getAnswerTemplates } from '../api/answerTemplateApi';
import type { Question } from '../types/question';
import type { AnswerTemplate } from '../types/answerTemplate';
import { useCrudPage } from '../hooks/useCrudPage';
import { useAuth } from '../context/AuthContext';
import { useSnackbar } from '../context/SnackbarContext';
import { extractErrorMessage } from '../api/errorHelper';

function QuestionsPage() {
    const { isAdmin } = useAuth();
    const { showError } = useSnackbar();
    const {
        items: questions, page, setPage, pageSize, setPageSize, totalCount,
        dialogOpen, setDialogOpen, editingId, setEditingId,
        error, setError, saving, handleDelete, runSave, reload,
    } = useCrudPage<Question>({
        fetchPage: getQuestionsPaged,
        remove: deleteQuestion,
        deleteConfirmMessage: 'Bu soruyu silmek istediğinize emin misiniz?',
    });
    const [templates, setTemplates] = useState<AnswerTemplate[]>([]);
    const [text, setText] = useState('');
    const [answerTemplateId, setAnswerTemplateId] = useState('');
    const canModify = (question: Question) => question.isMine || (isAdmin && question.isDefault);

    const handleDuplicate = async (id: string) => {
        try {
            await duplicateQuestion(id);
            await reload();
        } catch (err) {
            showError(extractErrorMessage(err));
        }
    };

    const handleSetIsDefault = async (id: string, isDefault: boolean) => {
        try {
            await setQuestionIsDefault(id, isDefault);
            await reload();
        } catch (err) {
            showError(extractErrorMessage(err));
        }
    };

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
            <Box sx={{ display: 'flex', flexWrap: 'wrap', justifyContent: 'space-between', gap: 1, mb: 2 }}>
                <Typography variant="h5">Sorular</Typography>
                <Button variant="contained" startIcon={<Add />} onClick={openCreateDialog}>
                    Yeni Soru
                </Button>
            </Box>

            {totalCount === 0 ? (
                <EmptyState message="Henüz bir soru oluşturulmamış." />
            ) : (
                <>
                    <Box sx={{ display: { xs: 'block', sm: 'none' } }}>
                        {questions.map((question) => (
                            <ResponsiveListCard
                                key={question.id}
                                title={
                                    <>
                                        {question.text}
                                        {question.isDefault && (
                                            <Chip label="Varsayılan" size="small" sx={{ ml: 1 }} />
                                        )}
                                    </>
                                }
                                fields={[{ label: 'CEVAP ŞABLONU', value: question.answerTemplateName }]}
                                actions={
                                    <>
                                        {isAdmin && (question.isMine || question.isDefault) && (
                                            <IconButton
                                                onClick={() => handleSetIsDefault(question.id, !question.isDefault)}
                                                title={question.isDefault ? 'Varsayılanlıktan çıkar' : 'Varsayılan yap'}
                                            >
                                                {question.isDefault ? <PublicOff fontSize="small" /> : <Public fontSize="small" />}
                                            </IconButton>
                                        )}
                                        {canModify(question) ? (
                                            <>
                                                <IconButton onClick={() => openEditDialog(question)}>
                                                    <Edit fontSize="small" />
                                                </IconButton>
                                                <IconButton onClick={() => handleDelete(question.id)}>
                                                    <Delete fontSize="small" />
                                                </IconButton>
                                            </>
                                        ) : (
                                            <IconButton onClick={() => handleDuplicate(question.id)} title="Kendime kopyala">
                                                <ContentCopy fontSize="small" />
                                            </IconButton>
                                        )}
                                    </>
                                }
                            />
                        ))}
                    </Box>
                    <TableContainer component={Paper} sx={{ display: { xs: 'none', sm: 'block' } }}>
                        <Table sx={{ minWidth: 480 }}>
                            <TableHead>
                                <TableRow>
                                    <TableCell>SORU METNİ</TableCell>
                                    <TableCell>CEVAP ŞABLONU</TableCell>
                                    <TableCell align="right">İŞLEMLER</TableCell>
                                </TableRow>
                            </TableHead>
                            <TableBody>
                                {questions.map((question) => (
                                    <TableRow key={question.id}>
                                        <TableCell>
                                            {question.text}
                                            {question.isDefault && (
                                                <Chip label="Varsayılan" size="small" sx={{ ml: 1 }} />
                                            )}
                                        </TableCell>
                                        <TableCell>{question.answerTemplateName}</TableCell>
                                        <TableCell align="right">
                                            {isAdmin && (question.isMine || question.isDefault) && (
                                                <IconButton
                                                    onClick={() => handleSetIsDefault(question.id, !question.isDefault)}
                                                    title={question.isDefault ? 'Varsayılanlıktan çıkar' : 'Varsayılan yap'}
                                                >
                                                    {question.isDefault ? <PublicOff fontSize="small" /> : <Public fontSize="small" />}
                                                </IconButton>
                                            )}
                                            {canModify(question) ? (
                                                <>
                                                    <IconButton onClick={() => openEditDialog(question)}>
                                                        <Edit fontSize="small" />
                                                    </IconButton>
                                                    <IconButton onClick={() => handleDelete(question.id)}>
                                                        <Delete fontSize="small" />
                                                    </IconButton>
                                                </>
                                            ) : (
                                                <IconButton onClick={() => handleDuplicate(question.id)} title="Kendime kopyala">
                                                    <ContentCopy fontSize="small" />
                                                </IconButton>
                                            )}
                                        </TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
                    </TableContainer>
                    <Paper sx={{ mt: { xs: 1.5, sm: 0 } }}>
                        <TablePagination
                            component="div"
                            count={totalCount}
                            page={page - 1}
                            onPageChange={(_, newPage) => setPage(newPage + 1)}
                            rowsPerPage={pageSize}
                            onRowsPerPageChange={(e) => setPageSize(parseInt(e.target.value, 10))}
                            rowsPerPageOptions={[10, 20, 50]}
                            labelRowsPerPage="Sayfa başına"
                            labelDisplayedRows={({ from, to, count }) => `${from}-${to} / ${count}`}
                        />
                    </Paper>
                </>
            )}

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