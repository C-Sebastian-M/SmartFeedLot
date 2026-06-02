import React, { useState, useEffect } from 'react'
import { cn } from '@/utils'

// ─── Card ─────────────────────────────────────────────────────────────────────
export function Card({ className, ...props }: React.HTMLAttributes<HTMLDivElement>) {
  return (
    <div
      className={cn('rounded-lg border border-border/60 bg-card text-card-foreground shadow-sm', className)}
      {...props}
    />
  )
}

export function CardHeader({ className, ...props }: React.HTMLAttributes<HTMLDivElement>) {
  return <div className={cn('flex flex-col space-y-1 px-5 py-4', className)} {...props} />
}

export function CardTitle({ className, ...props }: React.HTMLAttributes<HTMLHeadingElement>) {
  return <h3 className={cn('text-sm font-semibold leading-none tracking-tight', className)} {...props} />
}

export function CardDescription({ className, ...props }: React.HTMLAttributes<HTMLParagraphElement>) {
  return <p className={cn('text-xs text-muted-foreground', className)} {...props} />
}

export function CardContent({ className, ...props }: React.HTMLAttributes<HTMLDivElement>) {
  return <div className={cn('px-5 pb-4 pt-0', className)} {...props} />
}

// ─── Badge ────────────────────────────────────────────────────────────────────
interface BadgeProps extends React.HTMLAttributes<HTMLSpanElement> {
  variant?: 'default' | 'outline'
}

export function Badge({ className, variant = 'outline', ...props }: BadgeProps) {
  return (
    <span
      className={cn(
        'inline-flex items-center rounded-md px-1.5 py-0.5 text-[11px] font-medium border transition-colors',
        variant === 'outline' && 'bg-transparent',
        className
      )}
      {...props}
    />
  )
}

// ─── Button ───────────────────────────────────────────────────────────────────
interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'default' | 'secondary' | 'ghost' | 'destructive' | 'outline'
  size?: 'sm' | 'md' | 'lg' | 'icon'
  loading?: boolean
}

export function Button({
  className, variant = 'default', size = 'md', loading, children, disabled, ...props
}: ButtonProps) {
  return (
    <button
      disabled={disabled || loading}
      className={cn(
        'inline-flex items-center justify-center gap-2 rounded-md font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:pointer-events-none disabled:opacity-50',
        {
          'bg-primary text-primary-foreground hover:bg-primary/90': variant === 'default',
          'bg-secondary text-secondary-foreground hover:bg-secondary/80': variant === 'secondary',
          'hover:bg-accent hover:text-accent-foreground': variant === 'ghost',
          'bg-destructive text-destructive-foreground hover:bg-destructive/90': variant === 'destructive',
          'border border-border bg-transparent hover:bg-accent': variant === 'outline',
        },
        {
          'h-7 px-2.5 text-[11px] tracking-wide': size === 'sm',
          'h-8 px-3.5 text-sm': size === 'md',
          'h-10 px-6 text-sm': size === 'lg',
          'h-8 w-8 p-0': size === 'icon',
        },
        className
      )}
      {...props}
    >
      {loading && (
        <svg className="w-3.5 h-3.5 animate-spin" fill="none" viewBox="0 0 24 24">
          <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
          <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
        </svg>
      )}
      {children}
    </button>
  )
}

// ─── Input ────────────────────────────────────────────────────────────────────
export const Input = React.forwardRef<
  HTMLInputElement,
  React.InputHTMLAttributes<HTMLInputElement>
>(({ className, ...props }, ref) => (
  <input
    ref={ref}
    className={cn(
      'flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm transition-colors',
      'placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring',
      'disabled:cursor-not-allowed disabled:opacity-50',
      className
    )}
    {...props}
  />
))
Input.displayName = 'Input'

// ─── Textarea ─────────────────────────────────────────────────────────────────
export const Textarea = React.forwardRef<
  HTMLTextAreaElement,
  React.TextareaHTMLAttributes<HTMLTextAreaElement>
>(({ className, ...props }, ref) => (
  <textarea
    ref={ref}
    className={cn(
      'flex min-h-[80px] w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm transition-colors',
      'placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring',
      'disabled:cursor-not-allowed disabled:opacity-50 resize-none',
      className
    )}
    {...props}
  />
))
Textarea.displayName = 'Textarea'

// ─── Label ────────────────────────────────────────────────────────────────────
export function Label({ className, ...props }: React.LabelHTMLAttributes<HTMLLabelElement>) {
  return (
    <label
      className={cn('text-[11px] font-medium leading-none text-muted-foreground/80 uppercase tracking-wide', className)}
      {...props}
    />
  )
}

// ─── Skeleton ─────────────────────────────────────────────────────────────────
export function Skeleton({ className, ...props }: React.HTMLAttributes<HTMLDivElement>) {
  return (
    <div className={cn('animate-pulse rounded-md bg-muted', className)} {...props} />
  )
}

// ─── Dialog ───────────────────────────────────────────────────────────────────
// Dialog propio sin dependencias externas — portal sobre un backdrop.
interface DialogProps {
  open: boolean
  onClose: () => void
  children: React.ReactNode
}

export function Dialog({ open, onClose, children }: DialogProps) {
  // Cerrar con Escape
  React.useEffect(() => {
    if (!open) return
    const handler = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose()
    }
    document.addEventListener('keydown', handler)
    return () => document.removeEventListener('keydown', handler)
  }, [open, onClose])

  if (!open) return null

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      {/* Backdrop */}
      <div
        className="absolute inset-0 bg-black/60 backdrop-blur-sm animate-fade-in"
        onClick={onClose}
      />
      {/* Panel */}
      <div className="relative z-10 w-full max-w-md mx-4 animate-slide-up">
        {children}
      </div>
    </div>
  )
}

export function DialogHeader({ className, ...props }: React.HTMLAttributes<HTMLDivElement>) {
  return <div className={cn('flex flex-col space-y-1 mb-5', className)} {...props} />
}

export function DialogTitle({ className, ...props }: React.HTMLAttributes<HTMLHeadingElement>) {
  return <h2 className={cn('text-sm font-semibold', className)} {...props} />
}

export function DialogDescription({ className, ...props }: React.HTMLAttributes<HTMLParagraphElement>) {
  return <p className={cn('text-xs text-muted-foreground', className)} {...props} />
}

// ─── Form field wrapper ───────────────────────────────────────────────────────
interface FormFieldProps {
  label: string
  error?: string
  required?: boolean
  children: React.ReactNode
  hint?: string
}

export function FormField({ label, error, required, children, hint }: FormFieldProps) {
  return (
    <div className="space-y-1.5">
      <Label>
        {label}
        {required && <span className="text-destructive ml-0.5">*</span>}
      </Label>
      {children}
      {hint && !error && (
        <p className="text-[10px] text-muted-foreground">{hint}</p>
      )}
      {error && (
        <p className="text-[10px] text-destructive">{error}</p>
      )}
    </div>
  )
}

// ─── Alert ────────────────────────────────────────────────────────────────────
interface AlertProps extends React.HTMLAttributes<HTMLDivElement> {
  variant?: 'default' | 'destructive' | 'success'
}

export function Alert({ className, variant = 'default', ...props }: AlertProps) {
  return (
    <div
      className={cn(
        'rounded-lg border px-4 py-3 text-xs',
        {
          'border-border bg-muted/50 text-foreground': variant === 'default',
          'border-destructive/30 bg-destructive/10 text-destructive': variant === 'destructive',
          'border-emerald-500/30 bg-emerald-500/10 text-emerald-400': variant === 'success',
        },
        className
      )}
      {...props}
    />
  )
}

// ─── Stat card ────────────────────────────────────────────────────────────────
interface StatCardProps {
  label: string
  value: string | number
  sub?: string
  delta?: string
  deltaPositive?: boolean
  icon?: React.ReactNode
  className?: string
  loading?: boolean
}

export function StatCard({ label, value, sub, delta, deltaPositive, icon: _icon, className, loading }: StatCardProps) {
  if (loading) {
    return (
      <Card className={cn('p-4', className)}>
        <Skeleton className="h-3 w-20 mb-2.5" />
        <Skeleton className="h-7 w-24" />
      </Card>
    )
  }

  return (
    <Card className={cn('p-4', className)}>
      <p className="text-[10px] font-semibold uppercase tracking-widest text-muted-foreground/50 mb-1.5">{label}</p>
      <p className="text-2xl font-bold tabular-nums tracking-tight leading-none">{value}</p>
      {sub && <p className="text-[11px] text-muted-foreground mt-1.5">{sub}</p>}
      {delta && (
        <p className={cn('text-xs font-medium mt-1.5 flex items-center gap-1', deltaPositive ? 'text-emerald-400' : 'text-rose-400')}>
          <span>{deltaPositive ? '↑' : '↓'}</span>{delta}
        </p>
      )}
    </Card>
  )
}


// ─── MoneyInput ───────────────────────────────────────────────────────────────
// Formatea mientras escribe: 1000000 → 1.000.000
export const MoneyInput = React.forwardRef<
  HTMLInputElement,
  Omit<React.InputHTMLAttributes<HTMLInputElement>, 'onChange' | 'value' | 'type'> & {
    value?: number | string
    onChange?: (e: React.ChangeEvent<HTMLInputElement>) => void
  }
>(({ value, onChange, className, ...props }, ref) => {
  const fmt = (v: number | string | undefined): string => {
    if (v === undefined || v === '' || v === null) return ''
    const n = typeof v === 'number' ? v : parseFloat(String(v).replace(/\./g, ''))
    if (isNaN(n) || n === 0) return ''
    return new Intl.NumberFormat('es-CO', { maximumFractionDigits: 0, useGrouping: true }).format(n).replace(/,/g, '.')
  }

  const [display, setDisplay] = useState(() => fmt(value))

  useEffect(() => {
    const external = String(value ?? '').replace(/\./g, '')
    const current = display.replace(/\./g, '')
    if (current !== external) setDisplay(fmt(value))
  }, [value]) // eslint-disable-line

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const raw = e.target.value.replace(/[^0-9]/g, '')
    const n = raw === '' ? 0 : parseInt(raw, 10)
    setDisplay(raw === '' ? '' : new Intl.NumberFormat('es-CO', { maximumFractionDigits: 0, useGrouping: true }).format(n).replace(/,/g, '.'))
    if (onChange) {
      const syn = Object.create(e)
      syn.target = { ...e.target, value: raw, name: props.name ?? '' }
      onChange(syn as React.ChangeEvent<HTMLInputElement>)
    }
  }

  return (
    <input ref={ref} type="text" inputMode="numeric" value={display} onChange={handleChange}
      className={cn(
        'flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm text-right tabular-nums transition-colors',
        'placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring',
        'disabled:cursor-not-allowed disabled:opacity-50', className
      )}
      {...props} />
  )
})
MoneyInput.displayName = 'MoneyInput'

// ─── Empty state ──────────────────────────────────────────────────────────────
interface EmptyStateProps {
  icon?: React.ReactNode
  title: string
  description?: string
  action?: React.ReactNode
}

export function EmptyState({ icon, title, description, action }: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center py-12 text-center">
      {icon && <div className="text-muted-foreground/30 mb-3">{icon}</div>}
      <p className="text-sm font-medium text-muted-foreground mb-1">{title}</p>
      {description && <p className="text-[11px] text-muted-foreground/60 mb-3 max-w-xs leading-relaxed">{description}</p>}
      {action}
    </div>
  )
}

// ─── Page header ──────────────────────────────────────────────────────────────
interface PageHeaderProps {
  title: string
  description?: string
  action?: React.ReactNode
}

export function PageHeader({ title, description: _desc, action }: PageHeaderProps) {
  return (
    <div className="flex items-center justify-between px-6 border-b border-border/50 flex-shrink-0 h-12">
      <h1 className="text-sm font-semibold tracking-tight">{title}</h1>
      {action && <div className="flex items-center gap-2">{action}</div>}
    </div>
  )
}

// ─── CustomSelect ──────────────────────────────────────────────────────────────
// Reemplaza el <select> nativo que en macOS Chrome muestra popup blanco en dark mode
interface SelectOption { value: string; label: string }

interface CustomSelectProps {
  value: string
  onChange: (value: string) => void
  options: SelectOption[]
  placeholder?: string
  className?: string
  disabled?: boolean
}

export function CustomSelect({ value, onChange, options, placeholder = 'Seleccionar...', className, disabled }: CustomSelectProps) {
  const [open, setOpen] = useState(false)
  const ref = React.useRef<HTMLDivElement>(null)

  const selected = options.find(o => o.value === value)

  useEffect(() => {
    function handleClick(e: MouseEvent) {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false)
    }
    document.addEventListener('mousedown', handleClick)
    return () => document.removeEventListener('mousedown', handleClick)
  }, [])

  return (
    <div ref={ref} className={cn('relative', className)}>
      <button
        type="button"
        disabled={disabled}
        onClick={() => setOpen(v => !v)}
        className={cn(
          'flex h-9 w-full items-center justify-between rounded-md border border-input bg-card px-3 py-1 text-sm text-foreground',
          'focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring',
          'disabled:cursor-not-allowed disabled:opacity-50',
        )}
      >
        <span className={selected ? 'text-foreground' : 'text-muted-foreground'}>
          {selected ? selected.label : placeholder}
        </span>
        <svg className={cn('w-3.5 h-3.5 text-muted-foreground transition-transform', open && 'rotate-180')} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M6 9l6 6 6-6"/></svg>
      </button>

      {open && (
        <div className="absolute z-50 mt-1 w-full rounded-md border border-border bg-card shadow-lg overflow-hidden">
          <div className="max-h-56 overflow-y-auto py-1">
            {options.map(opt => (
              <div
                key={opt.value}
                onClick={() => { onChange(opt.value); setOpen(false) }}
                className={cn(
                  'px-3 py-2 text-sm cursor-pointer select-none transition-colors',
                  opt.value === value
                    ? 'bg-primary/10 text-primary font-medium'
                    : 'text-foreground hover:bg-secondary',
                )}
              >
                {opt.label}
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  )
}

