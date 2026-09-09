import { createContext, useContext, useState, type ReactNode } from 'react';
import { login as loginApi, register as registerApi, logout as logoutApi } from '../api/authApi';
import type { LoginRequest, RegisterRequest } from '../types/auth';

interface AuthContextType {
    token: string | null;
    email: string | null;
    isAdmin: boolean;
    isAuthenticated: boolean;
    login: (data: LoginRequest) => Promise<void>;
    register: (data: RegisterRequest) => Promise<void>;
    logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
    const [token, setToken] = useState<string | null>(localStorage.getItem('token'));
    const [email, setEmail] = useState<string | null>(localStorage.getItem('email'));
    const [isAdmin, setIsAdmin] = useState<boolean>(localStorage.getItem('isAdmin') === 'true');

    const login = async (data: LoginRequest) => {
        const response = await loginApi(data);
        saveAuth(response.token, response.refreshToken, response.email, response.isAdmin);
    };

    const register = async (data: RegisterRequest) => {
        const response = await registerApi(data);
        saveAuth(response.token, response.refreshToken, response.email, response.isAdmin);
    };

    const saveAuth = (newToken: string, newRefreshToken: string, newEmail: string, newIsAdmin: boolean) => {
        localStorage.setItem('token', newToken);
        localStorage.setItem('refreshToken', newRefreshToken);
        localStorage.setItem('email', newEmail);
        localStorage.setItem('isAdmin', String(newIsAdmin));
        setToken(newToken);
        setEmail(newEmail);
        setIsAdmin(newIsAdmin);
    };

    const clearAuth = () => {
        localStorage.removeItem('token');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('email');
        localStorage.removeItem('isAdmin');
        setToken(null);
        setEmail(null);
        setIsAdmin(false);
    };

    const logout = () => {
        const storedRefreshToken = localStorage.getItem('refreshToken');
        if (storedRefreshToken) {
            logoutApi(storedRefreshToken).catch(() => {
                // Sunucuya ulaşılamasa bile kullanıcı için yerel oturum sonlandırılır.
            });
        }
        clearAuth();
    };

    return (
        <AuthContext.Provider
            value={{ token, email, isAdmin, isAuthenticated: !!token, login, register, logout }}
        >
            {children}
        </AuthContext.Provider>
    );
}

export function useAuth() {
    const context = useContext(AuthContext);
    if (!context) {
        throw new Error('useAuth must be used within AuthProvider');
    }
    return context;
}
