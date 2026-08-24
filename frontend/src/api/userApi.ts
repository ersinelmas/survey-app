import axiosInstance from './axiosInstance';
import type { User } from '../types/user';

export const getUsers = async (): Promise<User[]> => {
    const response = await axiosInstance.get<User[]>('/Users');
    return response.data;
};