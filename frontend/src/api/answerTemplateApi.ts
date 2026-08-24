import axiosInstance from './axiosInstance';
import type {
    AnswerTemplate,
    CreateAnswerTemplateRequest,
    UpdateAnswerTemplateRequest,
} from '../types/answerTemplate';

export const getAnswerTemplates = async (): Promise<AnswerTemplate[]> => {
    const response = await axiosInstance.get<AnswerTemplate[]>('/AnswerTemplates');
    return response.data;
};

export const createAnswerTemplate = async (
    data: CreateAnswerTemplateRequest
): Promise<AnswerTemplate> => {
    const response = await axiosInstance.post<AnswerTemplate>('/AnswerTemplates', data);
    return response.data;
};

export const updateAnswerTemplate = async (
    id: string,
    data: UpdateAnswerTemplateRequest
): Promise<AnswerTemplate> => {
    const response = await axiosInstance.put<AnswerTemplate>(`/AnswerTemplates/${id}`, data);
    return response.data;
};

export const deleteAnswerTemplate = async (id: string): Promise<void> => {
    await axiosInstance.delete(`/AnswerTemplates/${id}`);
};