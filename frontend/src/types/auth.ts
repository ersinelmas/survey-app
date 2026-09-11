export interface LoginRequest {
    email: string;
    password: string;
}

export interface RegisterRequest {
    email: string;
    password: string;
}

export interface AuthResponse {
    token: string;
    refreshToken: string;
    email: string;
    isAdmin: boolean;
}

export interface ChangePasswordRequest {
    currentPassword: string;
    newPassword: string;
}

export interface DeleteAccountRequest {
    password: string;
}

export interface DataExport {
    account: {
        id: string;
        email: string;
        isAdmin: boolean;
        createdAt: string;
    };
    ownedSurveys: {
        id: string;
        title: string;
        description: string;
        startDate: string;
        endDate: string;
        isActive: boolean;
        isPublic: boolean;
        questions: string[];
        assignedUserCount: number;
    }[];
    ownedQuestions: {
        id: string;
        text: string;
        type: string;
    }[];
    ownedAnswerTemplates: {
        id: string;
        name: string;
        options: string[];
    }[];
    myResponses: {
        surveyTitle: string;
        questionText: string;
        answer: string;
        answeredAt: string;
    }[];
    myAssignments: {
        surveyTitle: string;
        isCompleted: boolean;
        completedAt: string | null;
    }[];
}