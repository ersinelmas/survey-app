import { useCallback, useEffect, useState } from 'react';
import { extractErrorMessage } from '../api/errorHelper';

interface UseCrudPageOptions<T> {
    fetchAll: () => Promise<T[]>;
    remove: (id: string) => Promise<void>;
    deleteConfirmMessage: string;
}

export function useCrudPage<T>({ fetchAll, remove, deleteConfirmMessage }: UseCrudPageOptions<T>) {
    const [items, setItems] = useState<T[]>([]);
    const [dialogOpen, setDialogOpen] = useState(false);
    const [editingId, setEditingId] = useState<string | null>(null);
    const [error, setError] = useState('');
    const [saving, setSaving] = useState(false);

    const reload = useCallback(async () => {
        const data = await fetchAll();
        setItems(data);
    }, [fetchAll]);

    useEffect(() => {
        reload();
    }, [reload]);

    const handleDelete = async (id: string) => {
        if (!confirm(deleteConfirmMessage)) return;
        try {
            await remove(id);
            await reload();
        } catch (err) {
            alert(extractErrorMessage(err));
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
