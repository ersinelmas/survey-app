import { useState } from 'react';
import {
    Box, Button, Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
    Paper, IconButton, Dialog, DialogTitle, DialogContent, DialogActions, TextField,
    Typography, Chip, TablePagination,
} from '@mui/material';
import { Add, Edit, Delete, ContentCopy } from '@mui/icons-material';
import Layout from '../components/Layout';
import EmptyState from '../components/EmptyState';
import {
    getAnswerTemplatesPaged, createAnswerTemplate, updateAnswerTemplate, deleteAnswerTemplate, duplicateAnswerTemplate,
} from '../api/answerTemplateApi';
import type { AnswerTemplate, UpdateAnswerOptionRequest } from '../types/answerTemplate';
import { useCrudPage } from '../hooks/useCrudPage';
import { useAuth } from '../context/AuthContext';
import { useSnackbar } from '../context/SnackbarContext';
import { extractErrorMessage } from '../api/errorHelper';

function AnswerTemplatesPage() {
    const { isAdmin } = useAuth();
    const { showError } = useSnackbar();
    const {
        items: templates, page, setPage, pageSize, setPageSize, totalCount,
        dialogOpen, setDialogOpen, editingId, setEditingId,
        error, setError, saving, handleDelete, runSave, reload,
    } = useCrudPage<AnswerTemplate>({
        fetchPage: getAnswerTemplatesPaged,
        remove: deleteAnswerTemplate,
        deleteConfirmMessage: 'Bu cevap şablonunu silmek istediğinize emin misiniz?',
    });
    const canModify = (template: AnswerTemplate) => template.isMine || (isAdmin && template.isDefault);

    const handleDuplicate = async (id: string) => {
        try {
            await duplicateAnswerTemplate(id);
            await reload();
        } catch (err) {
            showError(extractErrorMessage(err));
        }
    };
    const [name, setName] = useState('');
    const [options, setOptions] = useState<UpdateAnswerOptionRequest[]>([
        { id: null, text: '', order: 1 },
        { id: null, text: '', order: 2 },
    ]);

    const openCreateDialog = () => {
        setEditingId(null);
        setName('');
        setOptions([
            { id: null, text: '', order: 1 },
            { id: null, text: '', order: 2 },
        ]);
        setError('');
        setDialogOpen(true);
    };

    const openEditDialog = (template: AnswerTemplate) => {
        setEditingId(template.id);
        setName(template.name);
        setOptions(template.options.map((o) => ({ id: o.id, text: o.text, order: o.order })));
        setError('');
        setDialogOpen(true);
    };

    const addOption = () => {
        if (options.length >= 4) return;
        setOptions([...options, { id: null, text: '', order: options.length + 1 }]);
    };

    const removeOption = (index: number) => {
        if (options.length <= 2) return;
        setOptions(options.filter((_, i) => i !== index));
    };

    const updateOptionText = (index: number, text: string) => {
        const updated = [...options];
        updated[index] = { ...updated[index], text };
        setOptions(updated);
    };

    const handleSave = () =>
        runSave(async () => {
            if (editingId) {
                await updateAnswerTemplate(editingId, { name, options });
            } else {
                await createAnswerTemplate({
                    name,
                    options: options.map((o) => ({ text: o.text, order: o.order })),
                });
            }
        });

    return (
        <Layout>
            <Box sx={{ display: 'flex', flexWrap: 'wrap', justifyContent: 'space-between', gap: 1, mb: 2 }}>
                <Typography variant="h5">Cevap Şablonları</Typography>
                <Button variant="contained" startIcon={<Add />} onClick={openCreateDialog}>
                    Yeni Şablon
                </Button>
            </Box>

            {totalCount === 0 ? (
                <EmptyState message="Henüz bir cevap şablonu tanımlanmamış." />
            ) : (
                <TableContainer component={Paper}>
                    <Table sx={{ minWidth: 480 }}>
                        <TableHead>
                            <TableRow>
                                <TableCell>AD</TableCell>
                                <TableCell>ŞIKLAR</TableCell>
                                <TableCell align="right">İŞLEMLER</TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {templates.map((template) => (
                                <TableRow key={template.id}>
                                    <TableCell>
                                        {template.name}
                                        {template.isDefault && (
                                            <Chip label="Varsayılan" size="small" sx={{ ml: 1 }} />
                                        )}
                                    </TableCell>
                                    <TableCell>
                                        {template.options
                                            .sort((a, b) => a.order - b.order)
                                            .map((o) => (
                                                <Chip key={o.id} label={o.text} size="small" variant="outlined" sx={{ mr: 0.5 }} />
                                            ))}
                                    </TableCell>
                                    <TableCell align="right">
                                        {canModify(template) ? (
                                            <>
                                                <IconButton onClick={() => openEditDialog(template)}>
                                                    <Edit fontSize="small" />
                                                </IconButton>
                                                <IconButton onClick={() => handleDelete(template.id)}>
                                                    <Delete fontSize="small" />
                                                </IconButton>
                                            </>
                                        ) : (
                                            <IconButton onClick={() => handleDuplicate(template.id)} title="Kendime kopyala">
                                                <ContentCopy fontSize="small" />
                                            </IconButton>
                                        )}
                                    </TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
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
                </TableContainer>
            )}

            <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} fullWidth maxWidth="sm">
                <DialogTitle>{editingId ? 'Şablonu Düzenle' : 'Yeni Şablon'}</DialogTitle>
                <DialogContent>
                    {error && <Typography color="error" sx={{ mb: 1 }}>{error}</Typography>}
                    <TextField
                        label="Şablon Adı"
                        fullWidth
                        margin="normal"
                        value={name}
                        onChange={(e) => setName(e.target.value)}
                    />
                    {options.map((option, index) => (
                        <Box key={index} sx={{ display: 'flex', gap: 1, alignItems: 'center', mt: 1 }}>
                            <TextField
                                label={`Şık ${index + 1}`}
                                fullWidth
                                value={option.text}
                                onChange={(e) => updateOptionText(index, e.target.value)}
                            />
                            <IconButton onClick={() => removeOption(index)} disabled={options.length <= 2}>
                                <Delete fontSize="small" />
                            </IconButton>
                        </Box>
                    ))}
                    <Button onClick={addOption} disabled={options.length >= 4} sx={{ mt: 1 }}>
                        + Şık Ekle
                    </Button>
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

export default AnswerTemplatesPage;