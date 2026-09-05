import { createContext, useContext, useState, type ReactNode } from 'react';
import { login as loginApi, register as registerApi, logout as logoutApi } from '../api/authApi';
import type { LoginRequest, RegisterRequest } from '../types/auth';

interface AuthContextType {
    token: string | null;
    email: string | null;
    role: string | null;
    isAuthenticated: boolean;
    login: (data: LoginRequest) => Promise<void>;
    register: (data: RegisterRequest) => Promise<void>;
    logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
    const [token, setToken] = useState<string | null>(localStorage.getItem('token'));
    const [email, setEmail] = useState<string | null>(localStorage.getItem('email'));
    const [role, setRole] = useState<string | null>(localStorage.getItem('role'));

    const login = async (data: LoginRequest) => {
        const response = await loginApi(data);
        saveAuth(response.token, response.refreshToken, response.email, response.role);
    };

    const register = async (data: RegisterRequest) => {
        const response = await registerApi(data);
        saveAuth(response.token, response.refreshToken, response.email, response.role);
    };

    const saveAuth = (newToken: string, newRefreshToken: string, newEmail: string, newRole: string) => {
        localStorage.setItem('token', newToken);
        localStorage.setItem('refreshToken', newRefreshToken);
        localStorage.setItem('email', newEmail);
        localStorage.setItem('role', newRole);
        setToken(newToken);
        setEmail(newEmail);
        setRole(newRole);
    };

    const clearAuth = () => {
        localStorage.removeItem('token');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('email');
        localStorage.removeItem('role');
        setToken(null);
        setEmail(null);
        setRole(null);
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
            value={{ token, email, role, isAuthenticated: !!token, login, register, logout }}
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