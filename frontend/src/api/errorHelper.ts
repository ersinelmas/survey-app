export function extractErrorMessage(err: any): string {
    const data = err?.response?.data;

    if (!data) return 'Beklenmeyen bir hata oluştu.';

    if (data.message) return data.message;

    if (data.errors) {
        const firstField = Object.keys(data.errors)[0];
        const firstMessage = data.errors[firstField]?.[0];
        if (firstMessage) return firstMessage;
    }

    return 'Beklenmeyen bir hata oluştu.';
}