import { useEffect, useState } from 'react';
import {
    Box, Button, Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
    Paper, IconButton, Dialog, DialogTitle, DialogContent, DialogActions, TextField,
    Typography, Autocomplete, Chip, Switch, FormControlLabel, Alert,
} from '@mui/material';
import { Add, Edit, Delete, Assessment } from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import Layout from '../components/Layout';
import { getSurveys, createSurvey, updateSurvey, deleteSurvey } from '../api/surveyApi';
import { getQuestions } from '../api/questionApi';
import { getUsers } from '../api/userApi';
import type { Survey } from '../types/survey';
import type { Question } from '../types/question';
import type { User } from '../types/user';
import { useCrudPage } from '../hooks/useCrudPage';
import { useSnackbar } from '../context/SnackbarContext';
import { extractErrorMessage } from '../api/errorHelper';

function SurveysPage() {
    const navigate = useNavigate();
    const { showError } = useSnackbar();
    const {
        items: surveys, dialogOpen, setDialogOpen, editingId, setEditingId,
        error, setError, saving, handleDelete, runSave,
    } = useCrudPage<Survey>({
        fetchAll: getSurveys,
        remove: deleteSurvey,
        deleteConfirmMessage: 'Bu anketi silmek istediğinize emin misiniz?',
    });
    const [questions, setQuestions] = useState<Question[]>([]);
    const [users, setUsers] = useState<User[]>([]);

    const [title, setTitle] = useState('');
    const [description, setDescription] = useState('');
    const [startDate, setStartDate] = useState('');
    const [endDate, setEndDate] = useState('');
    const [isActive, setIsActive] = useState(true);
    const [selectedQuestions, setSelectedQuestions] = useState<Question[]>([]);
    const [selectedUsers, setSelectedUsers] = useState<User[]>([]);

    useEffect(() => {
        Promise.all([getQuestions(), getUsers('User')])
            .then(([questionsData, usersData]) => {
                setQuestions(questionsData);
                setUsers(usersData);
            })
            .catch((err) => showError(extractErrorMessage(err)));
    }, [showError]);

    const openCreateDialog = () => {
        setEditingId(null);
        setTitle('');
        setDescription('');
        setStartDate('');
        setEndDate('');
        setIsActive(true);
        setSelectedQuestions([]);
        setSelectedUsers([]);
        setError('');
        setDialogOpen(true);
    };

    const openEditDialog = (survey: Survey) => {
        setEditingId(survey.id);
        setTitle(survey.title);
        setDescription(survey.description);
        setStartDate(survey.startDate.slice(0, 10));
        setEndDate(survey.endDate.slice(0, 10));
        setIsActive(survey.isActive);
        setSelectedQuestions(
            survey.questions
                .sort((a, b) => a.order - b.order)
                .map((q) => questions.find((full) => full.id === q.questionId))
                .filter((q): q is Question => !!q)
        );
        setSelectedUsers(
            survey.assignedUsers
                .map((u) => users.find((full) => full.id === u.userId))
                .filter((u): u is User => !!u)
        );
        setError('');
        setDialogOpen(true);
    };

    const handleSave = () => {
        if (selectedQuestions.length === 0) {
            setError('En az bir soru seçmelisiniz.');
            return;
        }
        runSave(async () => {
            const payload = {
                title,
                description,
                startDate: new Date(startDate + 'T00:00:00').toISOString(),
                endDate: new Date(endDate + 'T23:59:59').toISOString(),
                isActive,
                questionIds: selectedQuestions.map((q) => q.id),
                assignedUserIds: selectedUsers.map((u) => u.id),
            };
            if (editingId) {
                await updateSurvey(editingId, payload);
            } else {
                await createSurvey(payload);
            }
        });
    };

    return (
        <Layout>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
                <Typography variant="h5">Anketler</Typography>
                <Button variant="contained" startIcon={<Add />} onClick={openCreateDialog}>
                    Yeni Anket
                </Button>
            </Box>

            <TableContainer component={Paper}>
                <Table>
                    <TableHead>
                        <TableRow>
                            <TableCell>Başlık</TableCell>
                            <TableCell>Tarih Aralığı</TableCell>
                            <TableCell>Durum</TableCell>
                            <TableCell>Soru / Kullanıcı</TableCell>
                            <TableCell align="right">İşlemler</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {surveys.map((survey) => (
                            <TableRow key={survey.id}>
                                <TableCell>{survey.title}</TableCell>
                                <TableCell>
                                    {survey.startDate.slice(0, 10)} - {survey.endDate.slice(0, 10)}
                                </TableCell>
                                <TableCell>
                                    <Chip
                                        label={survey.isActive ? 'Aktif' : 'Pasif'}
                                        color={survey.isActive ? 'success' : 'default'}
                                        size="small"
                                    />
                                </TableCell>
                                <TableCell>
                                    {survey.questions.length} soru / {survey.assignedUsers.length} kullanıcı
                                </TableCell>
                                <TableCell align="right">
                                    <IconButton onClick={() => navigate(`/admin/surveys/${survey.id}/report`)}>
                                        <Assessment fontSize="small" />
                                    </IconButton>
                                    <IconButton onClick={() => openEditDialog(survey)}>
                                        <Edit fontSize="small" />
                                    </IconButton>
                                    <IconButton onClick={() => handleDelete(survey.id)}>
                                        <Delete fontSize="small" />
                                    </IconButton>
                                </TableCell>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>

            <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} fullWidth maxWidth="sm">
                <DialogTitle>{editingId ? 'Anketi Düzenle' : 'Yeni Anket'}</DialogTitle>
                <DialogContent>
                    <TextField
                        label="Başlık"
                        fullWidth
                        margin="normal"
                        value={title}
                        onChange={(e) => setTitle(e.target.value)}
                    />
                    <TextField
                        label="Açıklama"
                        fullWidth
                        margin="normal"
                        multiline
                        rows={2}
                        value={description}
                        onChange={(e) => setDescription(e.target.value)}
                    />
                    <TextField
                        label="Başlangıç Tarihi"
                        type="date"
                        fullWidth
                        margin="normal"
                        slotProps={{ inputLabel: { shrink: true } }}
                        value={startDate}
                        onChange={(e) => setStartDate(e.target.value)}
                    />
                    <TextField
                        label="Bitiş Tarihi"
                        type="date"
                        fullWidth
                        margin="normal"
                        slotProps={{
                            inputLabel: { shrink: true },
                            htmlInput: { min: startDate || undefined },
                        }}
                        value={endDate}
                        onChange={(e) => setEndDate(e.target.value)}
                    />
                    <FormControlLabel
                        control={<Switch checked={isActive} onChange={(e) => setIsActive(e.target.checked)} />}
                        label="Aktif"
                    />
                    <Autocomplete
                        multiple
                        options={questions}
                        getOptionLabel={(q) => q.text}
                        value={selectedQuestions}
                        onChange={(_, newValue) => setSelectedQuestions(newValue)}
                        renderInput={(params) => (
                            <TextField {...params} label="Sorular (seçim sırası = anket sırası)" margin="normal" />
                        )}
                    />
                    <Autocomplete
                        multiple
                        options={users}
                        getOptionLabel={(u) => u.email}
                        value={selectedUsers}
                        onChange={(_, newValue) => setSelectedUsers(newValue)}
                        renderInput={(params) => (
                            <TextField {...params} label="Atanacak Kullanıcılar" margin="normal" />
                        )}
                    />
                </DialogContent>
                <DialogActions sx={{ flexDirection: 'column', alignItems: 'stretch', px: 3, pb: 2 }}>
                    {error && (
                        <Alert severity="error" sx={{ mb: 1 }}>
                            {error}
                        </Alert>
                    )}
                    <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1 }}>
                        <Button onClick={() => setDialogOpen(false)}>İptal</Button>
                        <Button variant="contained" onClick={handleSave} disabled={saving}>
                            {saving ? 'Kaydediliyor...' : 'Kaydet'}
                        </Button>
                    </Box>
                </DialogActions>
            </Dialog>
        </Layout>
    );
}

export default SurveysPage;