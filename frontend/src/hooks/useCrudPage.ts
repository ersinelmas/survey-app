import { useCallback, useEffect, useState } from 'react';
import { extractErrorMessage } from '../api/errorHelper';
import { useSnackbar } from '../context/SnackbarContext';
import type { PagedResult } from '../types/common';

interface UseCrudPageOptions<T> {
    fetchPage: (page: number, pageSize: number) => Promise<PagedResult<T>>;
    remove: (id: string) => Promise<void>;
    deleteConfirmMessage: string;
    initialPageSize?: number;
}

export function useCrudPage<T>({
    fetchPage,
    remove,
    deleteConfirmMessage,
    initialPageSize = 10,
}: UseCrudPageOptions<T>) {
    const { showError } = useSnackbar();
    const [items, setItems] = useState<T[]>([]);
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(initialPageSize);
    const [totalCount, setTotalCount] = useState(0);
    const [dialogOpen, setDialogOpen] = useState(false);
    const [editingId, setEditingId] = useState<string | null>(null);
    const [error, setError] = useState('');
    const [saving, setSaving] = useState(false);

    const reload = useCallback(async () => {
        try {
            const data = await fetchPage(page, pageSize);
            setItems(data.items);
            setTotalCount(data.totalCount);
        } catch (err) {
            showError(extractErrorMessage(err));
        }
    }, [fetchPage, page, pageSize, showError]);

    useEffect(() => {
        reload();
    }, [reload]);

    const changePageSize = (newPageSize: number) => {
        setPageSize(newPageSize);
        setPage(1);
    };

    const handleDelete = async (id: string) => {
        if (!confirm(deleteConfirmMessage)) return;
        try {
            await remove(id);
            if (items.length === 1 && page > 1) {
                setPage(page - 1);
            } else {
                await reload();
            }
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
        page,
        setPage,
        pageSize,
        setPageSize: changePageSize,
        totalCount,
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
