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
    selectedOptionId: string;
}

export interface SubmitSurveyRequest {
    answers: SubmitAnswer[];
}