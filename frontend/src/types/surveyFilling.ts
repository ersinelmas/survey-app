import type { QuestionType } from './question';

export interface AssignedSurvey {
    surveyId: string;
    title: string;
    description: string;
    endDate: string;
}

export interface SurveyFillOption {
    optionId: string;
    text: string;
}

export interface SurveyFillQuestion {
    questionId: string;
    text: string;
    type: QuestionType;
    options: SurveyFillOption[];
}

export interface SurveyFillDetail {
    surveyId: string;
    title: string;
    description: string;
    questions: SurveyFillQuestion[];
}

export interface SubmitAnswer {
    questionId: string;
    selectedOptionIds: string[];
    textValue: string | null;
}

export interface SubmitSurveyRequest {
    answers: SubmitAnswer[];
}

export interface PublicSurveyDetail {
    surveyId: string;
    title: string;
    description: string;
    requireLogin: boolean;
    questions: SurveyFillQuestion[];
}

export interface SubmitPublicSurveyRequest {
    answers: SubmitAnswer[];
    respondentToken: string | null;
}
