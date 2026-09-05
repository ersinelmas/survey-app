import { useState } from 'react';
import {
    Box, Button, Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
    Paper, IconButton, Dialog, DialogTitle, DialogContent, DialogActions, TextField,
    Typography, Chip,
} from '@mui/material';
import { Add, Edit, Delete } from '@mui/icons-material';
import Layout from '../components/Layout';
import {
    getAnswerTemplates, createAnswerTemplate, updateAnswerTemplate, deleteAnswerTemplate,
} from '../api/answerTemplateApi';
import type { AnswerTemplate, UpdateAnswerOptionRequest } from '../types/answerTemplate';
import { useCrudPage } from '../hooks/useCrudPage';

function AnswerTemplatesPage() {
    const {
        items: templates, dialogOpen, setDialogOpen, editingId, setEditingId,
        error, setError, saving, handleDelete, runSave,
    } = useCrudPage<AnswerTemplate>({
        fetchAll: getAnswerTemplates,
        remove: deleteAnswerTemplate,
        deleteConfirmMessage: 'Bu cevap şablonunu silmek istediğinize emin misiniz?',
    });
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
            <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
                <Typography variant="h5">Cevap Şablonları</Typography>
                <Button variant="contained" startIcon={<Add />} onClick={openCreateDialog}>
                    Yeni Şablon
                </Button>
            </Box>

            <TableContainer component={Paper}>
                <Table>
                    <TableHead>
                        <TableRow>
                            <TableCell>Ad</TableCell>
                            <TableCell>Şıklar</TableCell>
                            <TableCell align="right">İşlemler</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {templates.map((template) => (
                            <TableRow key={template.id}>
                                <TableCell>{template.name}</TableCell>
                                <TableCell>
                                    {template.options
                                        .sort((a, b) => a.order - b.order)
                                        .map((o) => (
                                            <Chip key={o.id} label={o.text} size="small" sx={{ mr: 0.5 }} />
                                        ))}
                                </TableCell>
                                <TableCell align="right">
                                    <IconButton onClick={() => openEditDialog(template)}>
                                        <Edit fontSize="small" />
                                    </IconButton>
                                    <IconButton onClick={() => handleDelete(template.id)}>
                                        <Delete fontSize="small" />
                                    </IconButton>
                                </TableCell>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>

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