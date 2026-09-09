import axiosInstance from './axiosInstance';
import type {
    AssignedSurvey, SurveyFillDetail, SubmitSurveyRequest, PublicSurveyDetail, SubmitPublicSurveyRequest,
} from '../types/surveyFilling';

export const getMyActiveSurveys = async (): Promise<AssignedSurvey[]> => {
    const response = await axiosInstance.get<AssignedSurvey[]>('/my-surveys');
    return response.data;
};

export const getSurveyToFill = async (surveyId: string): Promise<SurveyFillDetail> => {
    const response = await axiosInstance.get<SurveyFillDetail>(`/my-surveys/${surveyId}`);
    return response.data;
};

export const submitSurvey = async (surveyId: string, data: SubmitSurveyRequest): Promise<void> => {
    await axiosInstance.post(`/my-surveys/${surveyId}/submit`, data);
};

export const getPublicSurvey = async (surveyId: string): Promise<PublicSurveyDetail> => {
    const response = await axiosInstance.get<PublicSurveyDetail>(`/public/surveys/${surveyId}`);
    return response.data;
};

export const submitPublicSurvey = async (surveyId: string, data: SubmitPublicSurveyRequest): Promise<void> => {
    await axiosInstance.post(`/public/surveys/${surveyId}/submit`, data);
};