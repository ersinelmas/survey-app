export interface SurveyQuestionItem {
    questionId: string;
    questionText: string;
    order: number;
}

export interface SurveyAssignedUser {
    userId: string;
    email: string;
    isCompleted: boolean;
}

export interface Survey {
    id: string;
    title: string;
    description: string;
    startDate: string;
    endDate: string;
    isActive: boolean;
    questions: SurveyQuestionItem[];
    assignedUsers: SurveyAssignedUser[];
}

export interface CreateSurveyRequest {
    title: string;
    description: string;
    startDate: string;
    endDate: string;
    isActive: boolean;
    questionIds: string[];
    assignedUserIds: string[];
}

export interface UpdateSurveyRequest extends CreateSurveyRequest { }

export interface UserCompletion {
    userId: string;
    email: string;
    completedAt: string | null;
}

export interface QuestionResponseSummary {
    questionId: string;
    questionText: string;
    userAnswers: { userEmail: string; selectedOptionText: string }[];
}

export interface SurveyReport {
    surveyId: string;
    title: string;
    totalAssigned: number;
    totalCompleted: number;
    completedByUsers: UserCompletion[];
    pendingUsers: UserCompletion[];
    questionSummaries: QuestionResponseSummary[];
}