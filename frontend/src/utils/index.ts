import { type ClassValue, clsx } from 'clsx'
import { twMerge } from 'tailwind-merge'
import { format, parseISO } from 'date-fns'
import { es } from 'date-fns/locale'
import type { ClasificacionGmd, EstadoProductivo, EstadoSanitario, SeveridadEvento } from '@/types'

// ─── Tailwind class merger ────────────────────────────────────────────────────
export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

// ─── Formato de números ───────────────────────────────────────────────────────
export const fmt = {
  kg: (v: number) => `${v.toFixed(2)} kg`,
  kgDia: (v: number) => `${v.toFixed(3)} kg/día`,
  pct: (v: number) => `${v.toFixed(1)}%`,
  cop: (v: number) =>
    new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(v),
  decimal: (v: number, d = 2) => v.toFixed(d),
  fecha: (iso: string) => format(parseISO(iso), 'dd/MM/yyyy', { locale: es }),
  fechaLarga: (iso: string) => format(parseISO(iso), "d 'de' MMMM yyyy", { locale: es }),
}

// ─── Colores por clasificación productiva ────────────────────────────────────
export const gmdColor: Record<ClasificacionGmd, string> = {
  Excelente: 'text-emerald-400',
  Bueno: 'text-blue-400',
  Regular: 'text-amber-400',
  Deficiente: 'text-rose-400',
}

export const gmdBadgeColor: Record<ClasificacionGmd, string> = {
  Excelente: 'bg-emerald-500/10 text-emerald-400 border-emerald-500/20',
  Bueno: 'bg-blue-500/10 text-blue-400 border-blue-500/20',
  Regular: 'bg-amber-500/10 text-amber-400 border-amber-500/20',
  Deficiente: 'bg-rose-500/10 text-rose-400 border-rose-500/20',
}

export const estadoProductivoColor: Record<EstadoProductivo, string> = {
  EnEngorde: 'bg-emerald-500/10 text-emerald-400 border-emerald-500/20',
  Vendido: 'bg-blue-500/10 text-blue-400 border-blue-500/20',
  Muerto: 'bg-zinc-500/10 text-zinc-400 border-zinc-500/20',
  Retirado: 'bg-orange-500/10 text-orange-400 border-orange-500/20',
}

export const estadoSanitarioColor: Record<EstadoSanitario, string> = {
  Sano: 'bg-emerald-500/10 text-emerald-400 border-emerald-500/20',
  EnTratamiento: 'bg-amber-500/10 text-amber-400 border-amber-500/20',
  EnRetiro: 'bg-orange-500/10 text-orange-400 border-orange-500/20',
  Restringido: 'bg-rose-500/10 text-rose-400 border-rose-500/20',
}

export const severidadColor: Record<SeveridadEvento, string> = {
  Leve: 'bg-blue-500/10 text-blue-400 border-blue-500/20',
  Moderado: 'bg-amber-500/10 text-amber-400 border-amber-500/20',
  Grave: 'bg-orange-500/10 text-orange-400 border-orange-500/20',
  Critico: 'bg-rose-500/10 text-rose-400 border-rose-500/20',
}

// ─── Recharts colors ──────────────────────────────────────────────────────────
export const CHART_COLORS = {
  primary: '#4ade80',    // emerald-400
  secondary: '#60a5fa',  // blue-400
  warning: '#fbbf24',    // amber-400
  danger: '#f87171',     // rose-400
  muted: '#71717a',      // zinc-500
}
