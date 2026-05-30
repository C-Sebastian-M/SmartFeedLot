import { useState } from 'react'
import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuthStore } from '@/stores/auth.store'
import { cn } from '@/utils'
import {
  BarChart3, Beef, Home, AlertTriangle, Package,
  LogOut, User, Activity, DollarSign, Menu,
  Building2, ShoppingCart, HandCoins, UserPlus, Landmark, ClipboardList,
} from 'lucide-react'

const navItems = [
  { to: '/', icon: Home, label: 'Dashboard', end: true },
  { to: '/animales', icon: Beef, label: 'Animales' },
  { to: '/lotes', icon: Package, label: 'Lotes' },
  { to: '/costos', icon: DollarSign, label: 'Costos' },
  { to: '/finanzas', icon: DollarSign, label: 'Finanzas' },
  { to: '/prestamos', icon: Landmark, label: 'Préstamos' },
  { to: '/inversion', icon: ClipboardList, label: 'Inversión' },
  { to: '/operacion', icon: ClipboardList, label: 'Operación' },
  { to: '/analitica', icon: BarChart3, label: 'Analítica' },
  { to: '/alertas', icon: AlertTriangle, label: 'Alertas' },
  { to: '/proveedores', icon: Building2, label: 'Proveedores' },
  { to: '/compras', icon: ShoppingCart, label: 'Compras' },
  { to: '/ventas', icon: HandCoins, label: 'Ventas' },
  { to: '/compradores', icon: UserPlus, label: 'Compradores' },
]

function SidebarContent({ onNavClick }: { onNavClick?: () => void }) {
  const { user, logout } = useAuthStore()
  const navigate = useNavigate()

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <>
      <div className="h-14 flex items-center gap-2.5 px-4 border-b border-border">
        <div className="w-7 h-7 rounded-lg bg-primary/20 flex items-center justify-center">
          <Activity className="w-4 h-4 text-primary" />
        </div>
        <span className="font-semibold text-sm tracking-tight">SmartFeedLot</span>
      </div>
      <nav className="flex-1 px-2 py-3 space-y-0.5 overflow-y-auto">
        {navItems.map(({ to, icon: Icon, label, end }) => (
          <NavLink
            key={to}
            to={to}
            end={end}
            onClick={onNavClick}
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
    </>
  )
}

export default function AppLayout() {
  const [sidebarOpen, setSidebarOpen] = useState(false)

  return (
    <div className="flex h-screen bg-background overflow-hidden">
      {/* ── Overlay (mobile) ── */}
      {sidebarOpen && (
        <div
          className="fixed inset-0 bg-black/50 z-40 md:hidden"
          onClick={() => setSidebarOpen(false)}
        />
      )}

      {/* ── Sidebar (desktop: always visible; mobile: drawer) ── */}
      <aside className={`
        fixed md:static inset-y-0 left-0 z-50 w-60 flex-shrink-0 flex flex-col
        border-r border-border bg-card transition-transform duration-200
        ${sidebarOpen ? 'translate-x-0' : '-translate-x-full md:translate-x-0'}
      `}>
        <SidebarContent onNavClick={() => setSidebarOpen(false)} />
      </aside>

      {/* ── Main content ── */}
      <main className="flex-1 flex flex-col overflow-hidden">
        {/* ── Mobile top bar ── */}
        <div className="md:hidden flex items-center gap-2 h-12 px-3 border-b border-border bg-card">
          <button
            onClick={() => setSidebarOpen(true)}
            className="p-1.5 rounded-md text-muted-foreground hover:text-foreground hover:bg-secondary"
          >
            <Menu className="w-5 h-5" />
          </button>
          <span className="text-sm font-semibold tracking-tight">SmartFeedLot</span>
        </div>

        <div className="flex-1 overflow-y-auto">
          <Outlet />
        </div>
      </main>
    </div>
  )
}
