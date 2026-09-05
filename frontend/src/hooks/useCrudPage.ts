import { useCallback, useEffect, useState } from 'react';
import { extractErrorMessage } from '../api/errorHelper';
import { useSnackbar } from '../context/SnackbarContext';

interface UseCrudPageOptions<T> {
    fetchAll: () => Promise<T[]>;
    remove: (id: string) => Promise<void>;
    deleteConfirmMessage: string;
}

export function useCrudPage<T>({ fetchAll, remove, deleteConfirmMessage }: UseCrudPageOptions<T>) {
    const { showError } = useSnackbar();
    const [items, setItems] = useState<T[]>([]);
    const [dialogOpen, setDialogOpen] = useState(false);
    const [editingId, setEditingId] = useState<string | null>(null);
    const [error, setError] = useState('');
    const [saving, setSaving] = useState(false);

    const reload = useCallback(async () => {
        try {
            const data = await fetchAll();
            setItems(data);
        } catch (err) {
            showError(extractErrorMessage(err));
        }
    }, [fetchAll, showError]);

    useEffect(() => {
        reload();
    }, [reload]);

    const handleDelete = async (id: string) => {
        if (!confirm(deleteConfirmMessage)) return;
        try {
            await remove(id);
            await reload();
        } catch (err) {
            showError(extractErrorMessage(err));
        }
    };

    const runSave = async (action: () => Promise<void>) => {
        if (saving) return;
        setSaving(true);
        setError('');
        try {
            await action();
            setDialogOpen(false);
            await reload();
        } catch (err) {
            setError(extractErrorMessage(err));
        } finally {
            setSaving(false);
        }
    };

    return {
        items,
        reload,
        dialogOpen,
        setDialogOpen,
        editingId,
        setEditingId,
        error,
        setError,
        saving,
        handleDelete,
        runSave,
    };
}
