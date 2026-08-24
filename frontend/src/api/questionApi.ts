import axiosInstance from './axiosInstance';
import type { Question, CreateQuestionRequest, UpdateQuestionRequest } from '../types/question';

export const getQuestions = async (): Promise<Question[]> => {
    const response = await axiosInstance.get<Question[]>('/Questions');
    return response.data;
};

export const createQuestion = async (data: CreateQuestionRequest): Promise<Question> => {
    const response = await axiosInstance.post<Question>('/Questions', data);
    return response.data;
};

export const updateQuestion = async (id: string, data: UpdateQuestionRequest): Promise<Question> => {
    const response = await axiosInstance.put<Question>(`/Questions/${id}`, data);
    return response.data;
};

export const deleteQuestion = async (id: string): Promise<void> => {
    await axiosInstance.delete(`/Questions/${id}`);
};