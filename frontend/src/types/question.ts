export interface Question {
    id: string;
    text: string;
    answerTemplateId: string;
    answerTemplateName: string;
    isDefault: boolean;
    isMine: boolean;
}

export interface CreateQuestionRequest {
    text: string;
    answerTemplateId: string;
}

export interface UpdateQuestionRequest {
    text: string;
    answerTemplateId: string;
}