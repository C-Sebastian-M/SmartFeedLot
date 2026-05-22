import React from 'react'
import { cn } from '@/utils'

// ─── Card ─────────────────────────────────────────────────────────────────────
export function Card({ className, ...props }: React.HTMLAttributes<HTMLDivElement>) {
  return (
    <div
      className={cn('rounded-lg border border-border bg-card text-card-foreground', className)}
      {...props}
    />
  )
}

export function CardHeader({ className, ...props }: React.HTMLAttributes<HTMLDivElement>) {
  return <div className={cn('flex flex-col space-y-1 p-5', className)} {...props} />
}

export function CardTitle({ className, ...props }: React.HTMLAttributes<HTMLHeadingElement>) {
  return <h3 className={cn('text-sm font-semibold leading-none tracking-tight', className)} {...props} />
}

export function CardDescription({ className, ...props }: React.HTMLAttributes<HTMLParagraphElement>) {
  return <p className={cn('text-xs text-muted-foreground', className)} {...props} />
}

export function CardContent({ className, ...props }: React.HTMLAttributes<HTMLDivElement>) {
  return <div className={cn('p-5 pt-0', className)} {...props} />
}

// ─── Badge ────────────────────────────────────────────────────────────────────
interface BadgeProps extends React.HTMLAttributes<HTMLSpanElement> {
  variant?: 'default' | 'outline'
}

export function Badge({ className, variant = 'outline', ...props }: BadgeProps) {
  return (
    <span
      className={cn(
        'inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium border transition-colors',
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
          'h-7 px-2.5 text-xs': size === 'sm',
          'h-9 px-4 text-sm': size === 'md',
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

// ─── Input — usa forwardRef para compatibilidad con react-hook-form ───────────
export const Input = React.forwardRef<
  HTMLInputElement,
  React.InputHTMLAttributes<HTMLInputElement>
>(({ className, ...props }, ref) => (
  <input
    ref={ref}
    className={cn(
      'flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm transition-colors',
      'placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring',
      'disabled:cursor-not-allowed disabled:opacity-50',
      className
    )}
    {...props}
  />
))
Input.displayName = 'Input'

// ─── Label ────────────────────────────────────────────────────────────────────
export function Label({ className, ...props }: React.LabelHTMLAttributes<HTMLLabelElement>) {
  return (
    <label
      className={cn('text-xs font-medium leading-none text-muted-foreground', className)}
      {...props}
    />
  )
}

// ─── Skeleton loader ──────────────────────────────────────────────────────────
export function Skeleton({ className, ...props }: React.HTMLAttributes<HTMLDivElement>) {
  return (
    <div
      className={cn('animate-pulse rounded-md bg-muted', className)}
      {...props}
    />
  )
}

// ─── Stat card ────────────────────────────────────────────────────────────────
interface StatCardProps {
  label: string
  value: string | number
  delta?: string
  deltaPositive?: boolean
  icon?: React.ReactNode
  className?: string
  loading?: boolean
}

export function StatCard({ label, value, delta, deltaPositive, icon, className, loading }: StatCardProps) {
  if (loading) {
    return (
      <Card className={cn('p-5', className)}>
        <Skeleton className="h-3 w-24 mb-3" />
        <Skeleton className="h-7 w-32 mb-2" />
        <Skeleton className="h-3 w-16" />
      </Card>
    )
  }

  return (
    <Card className={cn('p-5 transition-all hover:border-border/80', className)}>
      <div className="flex items-start justify-between">
        <div className="space-y-1">
          <p className="text-xs font-medium text-muted-foreground uppercase tracking-wide">{label}</p>
          <p className="text-2xl font-bold tabular-nums">{value}</p>
          {delta && (
            <p className={cn('text-xs font-medium', deltaPositive ? 'text-emerald-400' : 'text-rose-400')}>
              {delta}
            </p>
          )}
        </div>
        {icon && (
          <div className="w-9 h-9 rounded-lg bg-primary/10 flex items-center justify-center text-primary flex-shrink-0">
            {icon}
          </div>
        )}
      </div>
    </Card>
  )
}

// ─── Empty state ──────────────────────────────────────────────────────────────
interface EmptyStateProps {
  icon?: React.ReactNode
  title: string
  description?: string
  action?: React.ReactNode
}

export function EmptyState({ icon, title, description, action }: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center py-16 px-4 text-center animate-fade-in">
      {icon && (
        <div className="w-12 h-12 rounded-xl bg-muted flex items-center justify-center mb-4 text-muted-foreground">
          {icon}
        </div>
      )}
      <h3 className="text-sm font-semibold mb-1">{title}</h3>
      {description && <p className="text-xs text-muted-foreground mb-4 max-w-xs">{description}</p>}
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

export function PageHeader({ title, description, action }: PageHeaderProps) {
  return (
    <div className="flex items-center justify-between px-6 py-4 border-b border-border">
      <div>
        <h1 className="text-base font-semibold">{title}</h1>
        {description && <p className="text-xs text-muted-foreground mt-0.5">{description}</p>}
      </div>
      {action}
    </div>
  )
}
