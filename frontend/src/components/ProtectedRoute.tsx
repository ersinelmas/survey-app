import { Navigate } from 'react-router-dom';
import type { ReactNode } from 'react';
import { useAuth } from '../context/AuthContext';

interface ProtectedRouteProps {
    children: ReactNode;
    requiredRole?: string;
}

function ProtectedRoute({ children, requiredRole }: ProtectedRouteProps) {
    const { isAuthenticated, role } = useAuth();

    if (!isAuthenticated) {
        return <Navigate to="/login" replace />;
    }

    if (requiredRole && role !== requiredRole) {
        return <Navigate to="/" replace />;
    }

    return <>{children}</>;
}

export default ProtectedRoute;