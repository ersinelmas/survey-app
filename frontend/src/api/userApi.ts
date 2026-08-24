import axiosInstance from './axiosInstance';
import type { User } from '../types/user';

export const getUsers = async (role?: string): Promise<User[]> => {
    const response = await axiosInstance.get<User[]>('/Users', {
        params: role ? { role } : {},
    });
    return response.data;
};