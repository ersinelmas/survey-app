import axiosInstance from './axiosInstance';
import type {
    AnswerTemplate,
    CreateAnswerTemplateRequest,
    UpdateAnswerTemplateRequest,
} from '../types/answerTemplate';
import type { PagedResult } from '../types/common';

export const getAnswerTemplates = async (): Promise<AnswerTemplate[]> => {
    const response = await axiosInstance.get<AnswerTemplate[]>('/AnswerTemplates');
    return response.data;
};

export const getAnswerTemplatesPaged = async (
    page: number,
    pageSize: number
): Promise<PagedResult<AnswerTemplate>> => {
    const response = await axiosInstance.get<PagedResult<AnswerTemplate>>('/AnswerTemplates', {
        params: { page, pageSize },
    });
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