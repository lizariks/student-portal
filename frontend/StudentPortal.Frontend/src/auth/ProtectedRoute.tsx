import { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from './useAuth';

interface Props {
  children: ReactNode;
  requiredRole?: string;
}

export function ProtectedRoute({ children, requiredRole }: Props) {
  const { isAuthenticated, isLoading, roles } = useAuth();

  if (isLoading) return null;
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  if (requiredRole && !roles.includes(requiredRole)) {
    return <div className="p-8 text-red-600">Access denied.</div>;
  }

  return <>{children}</>;
}