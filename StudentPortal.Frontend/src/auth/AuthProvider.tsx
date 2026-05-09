import { createContext, useEffect, useState, useCallback, ReactNode } from 'react';
import * as authService from './authService';

export interface AuthContextType {
  isAuthenticated: boolean;
  isLoading: boolean;
  name: string | undefined;
  email: string | undefined;
  roles: string[];
  login: (username: string, password: string) => Promise<void>;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    authService.initFromStorage()
      .then(setIsAuthenticated)
      .finally(() => setIsLoading(false));
  }, []);

  const login = useCallback(async (username: string, password: string) => {
    await authService.login(username, password);
    setIsAuthenticated(true);
  }, []);

  const logout = useCallback(() => {
    authService.logout();
    setIsAuthenticated(false);
  }, []);

  const { name, email, roles } = authService.getUserInfo();

  return (
    <AuthContext.Provider value={{ isAuthenticated, isLoading, name, email, roles, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}