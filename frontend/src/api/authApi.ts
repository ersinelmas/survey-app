import axiosInstance from './axiosInstance';
import type { LoginRequest, RegisterRequest, AuthResponse } from '../types/auth';

export const login = async (data: LoginRequest): Promise<AuthResponse> => {
    const response = await axiosInstance.post<AuthResponse>('/Auth/login', data);
    return response.data;
};

export const register = async (data: RegisterRequest): Promise<AuthResponse> => {
    const response = await axiosInstance.post<AuthResponse>('/Auth/register', data);
    return response.data;
};