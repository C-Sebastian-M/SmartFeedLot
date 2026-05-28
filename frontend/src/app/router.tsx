import { createBrowserRouter, RouterProvider, Navigate } from 'react-router-dom'
import { useAuthStore } from '@/stores/auth.store'
import AppLayout from '@/layouts/AppLayout'
import LoginPage from '@/pages/LoginPage'
import DashboardPage from '@/pages/DashboardPage'
import AnimalesPage from '@/pages/AnimalesPage'
import AnimalDetallePage from '@/pages/AnimalDetallePage'
import LotesPage from '@/pages/LotesPage'
import LoteDetallePage from '@/pages/LoteDetallePage'
import AnaliticaPage from '@/pages/AnaliticaPage'
import AlertasPage from '@/pages/AlertasPage'
import CostosPage from '@/pages/CostosPage'

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
        { path: 'animales/:id', element: <AnimalDetallePage /> },
        { path: 'lotes', element: <LotesPage /> },
        { path: 'lotes/:id', element: <LoteDetallePage /> },
        { path: 'analitica', element: <AnaliticaPage /> },
        { path: 'alertas', element: <AlertasPage /> },
        { path: 'costos', element: <CostosPage /> },
      ],
    },
  ] as any,
  // @ts-expect-error — v7_startTransition existe en runtime aunque no en los tipos de esta versión
  { future: { v7_startTransition: true } }
)

export default function AppRouter() {
  return <RouterProvider router={router} />
}
