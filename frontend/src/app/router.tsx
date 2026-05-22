import { createBrowserRouter, RouterProvider, Navigate } from 'react-router-dom'
import { useAuthStore } from '@/stores/auth.store'
import AppLayout from '@/layouts/AppLayout'
import LoginPage from '@/pages/LoginPage'
import DashboardPage from '@/pages/DashboardPage'
import AnimalesPage from '@/pages/AnimalesPage'
import LotesPage from '@/pages/LotesPage'

function RequireAuth({ children }: { children: React.ReactNode }) {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  if (!isAuthenticated) return <Navigate to="/login" replace />
  return <>{children}</>
}

const router = createBrowserRouter(
  [
    {
      path: '/login',
      element: <LoginPage />,
    },
    {
      path: '/',
      element: (
        <RequireAuth>
          <AppLayout />
        </RequireAuth>
      ),
      children: [
        { index: true, element: <DashboardPage /> },
        { path: 'animales', element: <AnimalesPage /> },
        { path: 'lotes', element: <LotesPage /> },
        {
          path: 'analitica',
          element: (
            <div className="p-6 text-muted-foreground text-sm">
              Analítica avanzada — próximamente
            </div>
          ),
        },
        {
          path: 'alertas',
          element: (
            <div className="p-6 text-muted-foreground text-sm">
              Panel de alertas — próximamente
            </div>
          ),
        },
      ],
    },
  ],
  {
    future: {
      v7_startTransition: true,
    },
  }
)

export default function AppRouter() {
  return <RouterProvider router={router} />
}
