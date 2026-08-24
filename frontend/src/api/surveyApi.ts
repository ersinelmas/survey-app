import axiosInstance from './axiosInstance';
import type { Survey, CreateSurveyRequest, UpdateSurveyRequest, SurveyReport } from '../types/survey';

export const getSurveys = async (): Promise<Survey[]> => {
    const response = await axiosInstance.get<Survey[]>('/Surveys');
    return response.data;
};

export const createSurvey = async (data: CreateSurveyRequest): Promise<Survey> => {
    const response = await axiosInstance.post<Survey>('/Surveys', data);
    return response.data;
};

export const updateSurvey = async (id: string, data: UpdateSurveyRequest): Promise<Survey> => {
    const response = await axiosInstance.put<Survey>(`/Surveys/${id}`, data);
    return response.data;
};

export const deleteSurvey = async (id: string): Promise<void> => {
    await axiosInstance.delete(`/Surveys/${id}`);
};

export const getSurveyReport = async (id: string): Promise<SurveyReport> => {
    const response = await axiosInstance.get<SurveyReport>(`/Surveys/${id}/report`);
    return response.data;
};