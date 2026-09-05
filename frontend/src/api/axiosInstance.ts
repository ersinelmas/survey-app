import axios from 'axios';

const axiosInstance = axios.create({
    baseURL: 'http://localhost:5092/api',
});

const AUTH_ENDPOINTS = ['/Auth/login', '/Auth/register', '/Auth/refresh'];

axiosInstance.interceptors.request.use((config) => {
    const token = localStorage.getItem('token');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

const clearSessionAndRedirect = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('email');
    localStorage.removeItem('role');
    if (window.location.pathname !== '/login') {
        window.location.href = '/login';
    }
};

let refreshPromise: Promise<string> | null = null;

const performRefresh = async (): Promise<string> => {
    const storedRefreshToken = localStorage.getItem('refreshToken');
    if (!storedRefreshToken) {
        throw new Error('No refresh token available');
    }

    const response = await axios.post(`${axiosInstance.defaults.baseURL}/Auth/refresh`, {
        refreshToken: storedRefreshToken,
    });

    localStorage.setItem('token', response.data.token);
    localStorage.setItem('refreshToken', response.data.refreshToken);
    return response.data.token;
};

axiosInstance.interceptors.response.use(
    (response) => response,
    async (error) => {
        const originalRequest = error.config;

        const isAuthEndpoint = AUTH_ENDPOINTS.some((endpoint) => originalRequest?.url?.includes(endpoint));

        if (error.response?.status === 401 && !isAuthEndpoint && !originalRequest._retry) {
            originalRequest._retry = true;
            try {
                refreshPromise ??= performRefresh().finally(() => {
                    refreshPromise = null;
                });
                const newToken = await refreshPromise;
                originalRequest.headers.Authorization = `Bearer ${newToken}`;
                return axiosInstance(originalRequest);
            } catch {
                clearSessionAndRedirect();
                return Promise.reject(error);
            }
        }

        if (error.response?.status === 401) {
            clearSessionAndRedirect();
        }

        return Promise.reject(error);
    }
);

export default axiosInstance;