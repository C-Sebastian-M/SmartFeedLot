import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { LoginResponse } from '@/types'

interface AuthUser {
  id: string
  email: string
  nombre: string
  roles: string[]
}

interface AuthState {
  token: string | null
  user: AuthUser | null
  isAuthenticated: boolean
  login: (response: LoginResponse) => void
  logout: () => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      user: null,
      isAuthenticated: false,

      login: (response: LoginResponse) => {
        // El interceptor de Axios lee directamente de localStorage.
        // El store persiste en su propia key de localStorage vía zustand/persist.
        localStorage.setItem('feedlot_token', response.token)
        set({
          token: response.token,
          user: response.usuario,
          isAuthenticated: true,
        })
      },

      logout: () => {
        localStorage.removeItem('feedlot_token')
        set({ token: null, user: null, isAuthenticated: false })
      },
    }),
    {
      name: 'feedlot-auth',
      // Solo persistir token y user — isAuthenticated se deriva al rehidratar.
      partialize: (state) => ({
        token: state.token,
        user: state.user,
      }),
      // Al rehidratar desde localStorage: si hay token, marcar como autenticado.
      onRehydrateStorage: () => (state) => {
        if (state?.token) {
          state.isAuthenticated = true
          // Sincronizar con la key que usa el interceptor de Axios.
          localStorage.setItem('feedlot_token', state.token)
        }
      },
    }
  )
)
