import { create } from 'zustand'
import { persist } from 'zustand/middleware'

type Theme = 'dark' | 'light'

interface ThemeStore {
  theme: Theme
  toggle: () => void
  setTheme: (t: Theme) => void
}

function applyTheme(theme: Theme) {
  const root = document.documentElement
  if (theme === 'dark') {
    root.classList.add('dark')
  } else {
    root.classList.remove('dark')
  }
}

export const useThemeStore = create<ThemeStore>()(
  persist(
    (set, get) => ({
      theme: 'dark',
      toggle: () => {
        const next = get().theme === 'dark' ? 'light' : 'dark'
        applyTheme(next)
        set({ theme: next })
      },
      setTheme: (t) => {
        applyTheme(t)
        set({ theme: t })
      },
    }),
    { name: 'smartfeedlot-theme' }
  )
)

// Aplica el tema guardado al cargar (antes de que React monte)
const saved = localStorage.getItem('smartfeedlot-theme')
if (saved) {
  try {
    const parsed = JSON.parse(saved)
    applyTheme(parsed?.state?.theme ?? 'dark')
  } catch {
    applyTheme('dark')
  }
}
