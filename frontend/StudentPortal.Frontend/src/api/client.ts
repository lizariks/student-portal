import axios from 'axios';
import { getAccessToken, isTokenExpiringSoon, refreshTokens } from '../auth/authService';

const client = axios.create({ baseURL: '/api' });

client.interceptors.request.use(async (config) => {
  if (isTokenExpiringSoon()) await refreshTokens();
  const token = getAccessToken();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

export default client;