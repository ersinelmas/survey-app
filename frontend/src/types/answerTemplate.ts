export interface AnswerOption {
    id: string;
    text: string;
    order: number;
}

export interface AnswerTemplate {
    id: string;
    name: string;
    options: AnswerOption[];
}

export interface CreateAnswerOptionRequest {
    text: string;
    order: number;
}

export interface CreateAnswerTemplateRequest {
    name: string;
    options: CreateAnswerOptionRequest[];
}

export interface UpdateAnswerOptionRequest {
    id: string | null;
    text: string;
    order: number;
}

export interface UpdateAnswerTemplateRequest {
    name: string;
    options: UpdateAnswerOptionRequest[];
}