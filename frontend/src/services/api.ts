import axios from 'axios'

const api = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' },
})

// Request interceptor: agrega el token JWT a cada request.
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('feedlot_token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Response interceptor: redirige al login si el token expiró.
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('feedlot_token')
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)

export default api
