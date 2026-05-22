import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuthStore } from '@/stores/auth.store'
import { cn } from '@/utils'
import {
  BarChart3, Beef, Home, AlertTriangle, Package,
  LogOut, User, ChevronRight, Activity
} from 'lucide-react'

const navItems = [
  { to: '/', icon: Home, label: 'Dashboard', end: true },
  { to: '/animales', icon: Beef, label: 'Animales' },
  { to: '/lotes', icon: Package, label: 'Lotes' },
  { to: '/analitica', icon: BarChart3, label: 'Analítica' },
  { to: '/alertas', icon: AlertTriangle, label: 'Alertas' },
]

export default function AppLayout() {
  const { user, logout } = useAuthStore()
  const navigate = useNavigate()

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <div className="flex h-screen bg-background overflow-hidden">
      {/* ── Sidebar ── */}
      <aside className="w-60 flex-shrink-0 flex flex-col border-r border-border bg-card">
        {/* Logo */}
        <div className="h-14 flex items-center gap-2.5 px-4 border-b border-border">
          <div className="w-7 h-7 rounded-lg bg-primary/20 flex items-center justify-center">
            <Activity className="w-4 h-4 text-primary" />
          </div>
          <span className="font-semibold text-sm tracking-tight">SmartFeedLot</span>
        </div>

        {/* Nav */}
        <nav className="flex-1 px-2 py-3 space-y-0.5 overflow-y-auto">
          {navItems.map(({ to, icon: Icon, label, end }) => (
            <NavLink
              key={to}
              to={to}
              end={end}
              className={({ isActive }) =>
                cn(
                  'flex items-center gap-2.5 px-3 py-2 rounded-md text-sm font-medium transition-colors',
                  isActive
                    ? 'bg-primary/10 text-primary'
                    : 'text-muted-foreground hover:text-foreground hover:bg-secondary'
                )
              }
            >
              <Icon className="w-4 h-4 flex-shrink-0" />
              {label}
            </NavLink>
          ))}
        </nav>

        {/* User footer */}
        <div className="p-2 border-t border-border">
          <div className="flex items-center gap-2.5 px-3 py-2 rounded-md hover:bg-secondary cursor-pointer group">
            <div className="w-7 h-7 rounded-full bg-primary/20 flex items-center justify-center flex-shrink-0">
              <User className="w-3.5 h-3.5 text-primary" />
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-xs font-medium truncate">{user?.nombre ?? 'Usuario'}</p>
              <p className="text-xs text-muted-foreground truncate">{user?.roles?.[0] ?? ''}</p>
            </div>
            <button
              onClick={handleLogout}
              className="opacity-0 group-hover:opacity-100 transition-opacity"
              title="Cerrar sesión"
            >
              <LogOut className="w-3.5 h-3.5 text-muted-foreground hover:text-foreground" />
            </button>
          </div>
        </div>
      </aside>

      {/* ── Main content ── */}
      <main className="flex-1 flex flex-col overflow-hidden">
        <div className="flex-1 overflow-y-auto">
          <Outlet />
        </div>
      </main>
    </div>
  )
}
