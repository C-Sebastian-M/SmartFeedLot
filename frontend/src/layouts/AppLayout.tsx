import { useState } from 'react'
import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuthStore } from '@/stores/auth.store'
import { useThemeStore } from '@/stores/theme.store'
import { useAlertCounts } from '@/hooks/useAlertCounts'
import { cn } from '@/utils'
import {
  BarChart3, Beef, Home, AlertTriangle, Package,
  LogOut, Activity, DollarSign, Menu, X,
  Building2, ShoppingCart, HandCoins, UserPlus, Landmark, ClipboardList,
  PiggyBank, TrendingUp, Trees, ChevronRight, Moon, Sun,
} from 'lucide-react'

// ─── Nav structure ────────────────────────────────────────────────────────────

interface NavItem {
  to: string
  icon: typeof Home
  label: string
  end?: boolean
}

interface NavGroup {
  label?: string
  items: NavItem[]
}

const navGroups: NavGroup[] = [
  {
    items: [
      { to: '/', icon: Home, label: 'Dashboard', end: true },
    ],
  },
  {
    label: 'Producción',
    items: [
      { to: '/animales', icon: Beef, label: 'Animales' },
      { to: '/lotes', icon: Package, label: 'Lotes' },
      { to: '/porcino', icon: PiggyBank, label: 'Porcino' },
      { to: '/operacion', icon: Trees, label: 'Campo' },
    ],
  },
  {
    label: 'Finanzas',
    items: [
      { to: '/finanzas', icon: DollarSign, label: 'Movimientos' },
      { to: '/prestamos', icon: Landmark, label: 'Préstamos' },
      { to: '/inversion', icon: ClipboardList, label: 'Inversión' },
      { to: '/costos', icon: Activity, label: 'Costeo' },
    ],
  },
  {
    label: 'Comercial',
    items: [
      { to: '/ventas', icon: HandCoins, label: 'Ventas' },
      { to: '/compras', icon: ShoppingCart, label: 'Compras' },
      { to: '/proveedores', icon: Building2, label: 'Proveedores' },
      { to: '/compradores', icon: UserPlus, label: 'Compradores' },
    ],
  },
  {
    label: 'Análisis',
    items: [
      { to: '/precios-mercado', icon: TrendingUp, label: 'Precios Mercado' },
      { to: '/analitica', icon: BarChart3, label: 'Analítica' },
      { to: '/alertas', icon: AlertTriangle, label: 'Alertas' },
    ],
  },
]

// ─── Badge component ──────────────────────────────────────────────────────────

function NavBadge({ count, variant = 'red' }: { count: number; variant?: 'red' | 'amber' }) {
  if (count === 0) return null
  const label = count > 99 ? '99+' : String(count)
  return (
    <span
      className={cn(
        'ml-auto flex-shrink-0 h-4 min-w-[1rem] px-1 rounded-full',
        'text-[9px] font-bold leading-4 text-center tabular-nums',
        variant === 'red'
          ? 'bg-rose-500/20 text-rose-400'
          : 'bg-amber-500/20 text-amber-400'
      )}
    >
      {label}
    </span>
  )
}

// ─── Sidebar content ──────────────────────────────────────────────────────────

function SidebarContent({ onNavClick }: { onNavClick?: () => void }) {
  const { user, logout } = useAuthStore()
  const navigate = useNavigate()
  const { theme, toggle: toggleTheme } = useThemeStore()
  const counts = useAlertCounts()

  // Map de ruta → configuración de badge
  const badges: Record<string, { count: number; variant: 'red' | 'amber' }> = {
    '/alertas': { count: counts.total, variant: 'red' },
    '/prestamos': { count: counts.cuotasVencidas, variant: 'red' },
    '/inversion': { count: counts.itemsPendientes, variant: 'amber' },
  }

  return (
    <div className="flex flex-col h-full">
      {/* Logo */}
      <div className="h-12 flex items-center gap-2.5 px-4 border-b border-border/50">
        <div className="w-6 h-6 rounded-md bg-primary flex items-center justify-center flex-shrink-0">
          <Activity className="w-3.5 h-3.5 text-primary-foreground" />
        </div>
        <span className="font-semibold text-sm tracking-tight">SmartFeedLot</span>
        {/* Dot indicator global si hay alertas */}
        {counts.total > 0 && (
          <span className="ml-auto w-2 h-2 rounded-full bg-rose-500 animate-pulse flex-shrink-0" />
        )}
      </div>

      {/* Nav */}
      <nav className="flex-1 px-2 py-3 overflow-y-auto space-y-4">
        {navGroups.map((group, gi) => (
          <div key={gi}>
            {group.label && (
              <p className="px-2.5 mb-1 text-[9px] font-bold uppercase tracking-[0.12em] text-muted-foreground/40">
                {group.label}
              </p>
            )}
            <div className="space-y-0.5">
              {group.items.map(({ to, icon: Icon, label, end }) => {
                const badge = badges[to]
                return (
                  <NavLink
                    key={to}
                    to={to}
                    end={end}
                    onClick={onNavClick}
                    className={({ isActive }) =>
                      cn(
                        'group flex items-center gap-2.5 px-2.5 py-1.5 rounded-md text-[13px] transition-colors',
                        isActive
                          ? 'bg-primary/10 text-foreground font-medium'
                          : 'text-muted-foreground hover:text-foreground hover:bg-muted/50 font-normal'
                      )
                    }
                  >
                    {({ isActive }) => (
                      <>
                        <Icon
                          className={cn(
                            'w-3.5 h-3.5 flex-shrink-0 transition-colors',
                            isActive
                              ? 'text-primary'
                              : 'text-muted-foreground/60 group-hover:text-muted-foreground'
                          )}
                        />
                        <span className="truncate">{label}</span>

                        {/* Badge de alerta (tiene prioridad sobre el chevron) */}
                        {badge && badge.count > 0 ? (
                          <NavBadge count={badge.count} variant={badge.variant} />
                        ) : isActive ? (
                          <ChevronRight className="w-3 h-3 ml-auto text-primary/50 flex-shrink-0" />
                        ) : null}
                      </>
                    )}
                  </NavLink>
                )
              })}
            </div>
          </div>
        ))}
      </nav>

      {/* User */}
      <div className="p-2 border-t border-border/50 space-y-1">
        <button
          onClick={toggleTheme}
          className="w-full flex items-center gap-2.5 px-2.5 py-1.5 rounded-md text-[13px] text-muted-foreground hover:text-foreground hover:bg-muted/50 transition-colors"
          title={theme === 'dark' ? 'Cambiar a modo claro' : 'Cambiar a modo oscuro'}
        >
          {theme === 'dark'
            ? <Sun className="w-3.5 h-3.5 flex-shrink-0" />
            : <Moon className="w-3.5 h-3.5 flex-shrink-0" />
          }
          <span>{theme === 'dark' ? 'Modo claro' : 'Modo oscuro'}</span>
        </button>

        <div className="flex items-center gap-2.5 px-2.5 py-2 rounded-md hover:bg-muted/50 cursor-pointer group transition-colors">
          <div className="w-6 h-6 rounded-full bg-muted flex items-center justify-center flex-shrink-0 text-[10px] font-bold text-muted-foreground">
            {(user?.nombre ?? 'U').charAt(0).toUpperCase()}
          </div>
          <div className="flex-1 min-w-0">
            <p className="text-xs font-medium truncate leading-tight">{user?.nombre ?? 'Usuario'}</p>
            <p className="text-[10px] text-muted-foreground/60 truncate leading-tight">{user?.roles?.[0] ?? ''}</p>
          </div>
          <button
            onClick={() => { logout(); navigate('/login') }}
            className="opacity-0 group-hover:opacity-100 transition-opacity p-1 rounded hover:bg-muted text-muted-foreground hover:text-foreground"
            title="Cerrar sesión"
          >
            <LogOut className="w-3 h-3" />
          </button>
        </div>
      </div>
    </div>
  )
}

// ─── Layout ───────────────────────────────────────────────────────────────────

export default function AppLayout() {
  const [open, setOpen] = useState(false)

  return (
    <div className="flex h-screen bg-background overflow-hidden">
      {open && (
        <div
          className="fixed inset-0 bg-black/60 z-40 md:hidden backdrop-blur-sm"
          onClick={() => setOpen(false)}
        />
      )}

      <aside className={cn(
        'fixed md:static inset-y-0 left-0 z-50 w-56 flex-shrink-0',
        'border-r border-border/50 bg-card',
        'transition-transform duration-200 ease-out',
        open ? 'translate-x-0' : '-translate-x-full md:translate-x-0'
      )}>
        <SidebarContent onNavClick={() => setOpen(false)} />
      </aside>

      <main className="flex-1 flex flex-col overflow-hidden min-w-0">
        <div className="md:hidden flex items-center justify-between h-12 px-4 border-b border-border/50 bg-card">
          <button
            onClick={() => setOpen(true)}
            className="p-1.5 rounded-md text-muted-foreground hover:text-foreground hover:bg-muted/50 transition-colors"
          >
            {open ? <X className="w-4 h-4" /> : <Menu className="w-4 h-4" />}
          </button>
          <div className="flex items-center gap-2">
            <span className="text-sm font-semibold tracking-tight">SmartFeedLot</span>
          </div>
          <div className="w-7" />
        </div>

        <div className="flex-1 overflow-y-auto">
          <Outlet />
        </div>
      </main>
    </div>
  )
}
