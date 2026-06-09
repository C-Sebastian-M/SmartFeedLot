import axios from 'axios'

// En producción (Vercel), VITE_API_URL apunta al backend desplegado en Railway/Render.
// En desarrollo, el proxy de Vite reescribe /api → http://localhost:5000/api.
const baseURL = import.meta.env.VITE_API_URL
  ? `${import.meta.env.VITE_API_URL}/api`
  : '/api'

const api = axios.create({
  baseURL,
  headers: { 'Content-Type': 'application/json' },
})

// Agrega el JWT Bearer token a cada request.
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('feedlot_token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Redirige al login si el token expiró.
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
