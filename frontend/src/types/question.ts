export type QuestionType = 'SingleChoice' | 'MultipleChoice' | 'FreeText';

export interface Question {
    id: string;
    text: string;
    type: QuestionType;
    answerTemplateId: string | null;
    answerTemplateName: string | null;
    isDefault: boolean;
    isMine: boolean;
}

export interface CreateQuestionRequest {
    text: string;
    type: QuestionType;
    answerTemplateId: string | null;
}

export interface UpdateQuestionRequest {
    text: string;
    type: QuestionType;
    answerTemplateId: string | null;
}
