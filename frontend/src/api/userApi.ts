import axiosInstance from './axiosInstance';
import type { UserSearchResult } from '../types/user';

export const searchUsers = async (query: string): Promise<UserSearchResult[]> => {
    const response = await axiosInstance.get<UserSearchResult[]>('/Users/search', {
        params: { query },
    });
    return response.data;
};
