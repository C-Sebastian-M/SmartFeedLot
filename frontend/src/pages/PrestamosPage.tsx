import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import {
  Plus, X, CheckCircle2, ChevronDown, ChevronRight,
  Landmark, AlertCircle, Clock, Check, Undo2,
} from 'lucide-react'
import { usePrestamos, useCrearPrestamo, useRegistrarPagoCuota, useAnularPagoCuota } from '@/hooks/useFeedlot'
import {
  PageHeader, Card, CardContent, Skeleton, EmptyState, Button,
  Dialog, DialogHeader, DialogTitle, DialogDescription,
  FormField, Input, Alert, Badge,
  MoneyInput,
} from '@/components/ui'
import { fmt } from '@/utils'
import type { Prestamo, CuotaAmortizacion } from '@/types'

// ─── Helpers ──────────────────────────────────────────────────────────────────

function diasHastaFecha(fechaIso: string): number {
  const hoy = new Date()
  hoy.setHours(0, 0, 0, 0)
  const [y, m, d] = fechaIso.split('-').map(Number)
  const fecha = new Date(y, m - 1, d)
  return Math.round((fecha.getTime() - hoy.getTime()) / 86_400_000)
}

function etiquetaVencimiento(dias: number): { texto: string; color: string } {
  if (dias < 0) return { texto: `Venció hace ${Math.abs(dias)} días`, color: 'text-rose-400' }
  if (dias === 0) return { texto: 'Vence hoy', color: 'text-amber-400' }
  if (dias <= 7) return { texto: `Vence en ${dias} días`, color: 'text-amber-400' }
  return { texto: `Vence en ${dias} días`, color: 'text-muted-foreground' }
}

// ─── Modal crear préstamo ─────────────────────────────────────────────────────

const schema = z.object({
  monto: z.coerce.number().positive('Debe ser mayor a cero'),
  moneda: z.string().length(3).default('COP'),
  tasaMensual: z.coerce.number().min(0).max(100),
  nCuotas: z.coerce.number().int().positive(),
  fechaInicio: z.string().min(1, 'Requerida'),
  descripcion: z.string().min(3).max(500),
})
type Form = z.infer<typeof schema>

function CrearPrestamoModal({ open, onClose }: { open: boolean; onClose: () => void }) {
  const [exito, setExito] = useState(false)
  const [errorApi, setErrorApi] = useState<string>()
  const [cuotaEstimada, setCuotaEstimada] = useState<number | null>(null)
  const mutation = useCrearPrestamo()

  const { register, handleSubmit, reset, watch, formState: { errors, isSubmitting } } =
    useForm<Form>({
      resolver: zodResolver(schema),
      defaultValues: { moneda: 'COP', fechaInicio: new Date().toISOString().slice(0, 10) },
    })

  const monto = watch('monto')
  const tasa = watch('tasaMensual')
  const n = watch('nCuotas')

  const calcular = () => {
    if (!monto || !tasa || tasa <= 0 || !n || n <= 0) { setCuotaEstimada(null); return }
    const i = tasa / 100
    setCuotaEstimada(Math.round(monto * i / (1 - Math.pow(1 + i, -n))))
  }

  const handleClose = () => { reset(); setExito(false); setErrorApi(undefined); setCuotaEstimada(null); onClose() }

  const onSubmit = async (data: Form) => {
    setErrorApi(undefined)
    try {
      await mutation.mutateAsync(data)
      setExito(true)
      setTimeout(handleClose, 1500)
    } catch (err: any) {
      setErrorApi(err?.response?.data?.error ?? 'Error al crear préstamo.')
    }
  }

  return (
    <Dialog open={open} onClose={handleClose}>
      <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[480px] mx-4">
        <div className="flex items-center justify-between px-5 py-4 border-b border-border/50">
          <DialogHeader className="mb-0">
            <DialogTitle>Nuevo préstamo</DialogTitle>
            <DialogDescription>Genera la tabla de amortización automáticamente</DialogDescription>
          </DialogHeader>
          <button onClick={handleClose} className="text-muted-foreground hover:text-foreground ml-4"><X className="w-4 h-4" /></button>
        </div>
        <div className="p-5 max-h-[75vh] overflow-y-auto">
          {exito ? (
            <div className="flex flex-col items-center py-8 gap-3">
              <div className="w-12 h-12 rounded-full bg-emerald-500/10 flex items-center justify-center">
                <CheckCircle2 className="w-6 h-6 text-emerald-400" />
              </div>
              <p className="text-sm font-medium">¡Préstamo creado!</p>
              <p className="text-xs text-muted-foreground">La tabla de amortización se generó automáticamente.</p>
            </div>
          ) : (
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              <FormField label="Descripción" error={errors.descripcion?.message} required>
                <Input {...register('descripcion')} placeholder="Ej: Crédito Banco Agrario" autoFocus />
              </FormField>
              <div className="grid grid-cols-2 gap-3">
                <FormField label="Monto" error={errors.monto?.message} required>
                  <MoneyInput {...register('monto')} placeholder="20.000.000" onBlur={calcular} />
                </FormField>
                <FormField label="Moneda" error={errors.moneda?.message} required>
                  <Input {...register('moneda')} placeholder="COP" />
                </FormField>
              </div>
              <div className="grid grid-cols-2 gap-3">
                <FormField label="Tasa mensual (%)" error={errors.tasaMensual?.message} required hint="Ej: 1.79">
                  <Input {...register('tasaMensual')} type="number" min={0} step={0.01} placeholder="1.79" onBlur={calcular} />
                </FormField>
                <FormField label="N° de cuotas" error={errors.nCuotas?.message} required>
                  <Input {...register('nCuotas')} type="number" min={1} placeholder="12" onBlur={calcular} />
                </FormField>
              </div>
              <FormField label="Fecha de inicio" error={errors.fechaInicio?.message} required>
                <Input {...register('fechaInicio')} type="date" />
              </FormField>

              {cuotaEstimada !== null && (
                <div className="rounded-lg border border-primary/20 bg-primary/5 p-3 text-center">
                  <p className="text-xs text-muted-foreground mb-0.5">Cuota estimada · sistema francés</p>
                  <p className="text-xl font-bold text-primary">{fmt.cop(cuotaEstimada)}<span className="text-sm font-normal text-muted-foreground ml-1">/mes</span></p>
                </div>
              )}

              {errorApi && <Alert variant="destructive">{errorApi}</Alert>}
              <div className="flex gap-2 pt-1">
                <Button type="button" variant="outline" className="flex-1" onClick={handleClose}>Cancelar</Button>
                <Button type="submit" className="flex-1" loading={isSubmitting}>
                  <Landmark className="w-3.5 h-3.5" /> Crear préstamo
                </Button>
              </div>
            </form>
          )}
        </div>
      </div>
    </Dialog>
  )
}

// ─── Modal pagar cuota ────────────────────────────────────────────────────────

function PagarCuotaModal({
  open, onClose, prestamo, cuota
}: {
  open: boolean
  onClose: () => void
  prestamo: Prestamo
  cuota: CuotaAmortizacion | null
}) {
  const [fecha, setFecha] = useState(new Date().toISOString().slice(0, 10))
  const [errorApi, setErrorApi] = useState<string>()
  const mutation = useRegistrarPagoCuota()

  const handleClose = () => { setFecha(new Date().toISOString().slice(0, 10)); setErrorApi(undefined); onClose() }

  const onConfirm = async () => {
    if (!cuota) return
    setErrorApi(undefined)
    try {
      await mutation.mutateAsync({ prestamoId: prestamo.id, cuotaId: cuota.id, fechaPago: fecha })
      handleClose()
    } catch (err: any) {
      setErrorApi(err?.response?.data?.error ?? err?.response?.data?.detail ?? 'Error al registrar el pago.')
    }
  }

  if (!cuota) return null

  return (
    <Dialog open={open} onClose={handleClose}>
      <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[380px] mx-4 p-5">
        <DialogHeader className="mb-4">
          <DialogTitle>Registrar pago</DialogTitle>
          <DialogDescription>
            Cuota #{cuota.numeroCuota} · {fmt.cop(cuota.cuota)} · vence {fmt.fecha(cuota.fechaVencimiento)}
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4">
          {/* Desglose de la cuota */}
          <div className="rounded-lg border border-border/50 divide-y divide-border/30">
            <div className="flex justify-between px-3 py-2 text-xs">
              <span className="text-muted-foreground">Cuota total</span>
              <span className="font-semibold">{fmt.cop(cuota.cuota)}</span>
            </div>
            <div className="flex justify-between px-3 py-2 text-xs">
              <span className="text-muted-foreground">Interés</span>
              <span className="text-amber-400">{fmt.cop(cuota.interes)}</span>
            </div>
            <div className="flex justify-between px-3 py-2 text-xs">
              <span className="text-muted-foreground">Abono capital</span>
              <span className="text-emerald-400">{fmt.cop(cuota.abonoCapital)}</span>
            </div>
            <div className="flex justify-between px-3 py-2 text-xs">
              <span className="text-muted-foreground">Saldo tras pago</span>
              <span className="font-semibold">{fmt.cop(cuota.saldoPendiente)}</span>
            </div>
          </div>

          <FormField label="Fecha de pago" required>
            <Input
              type="date"
              value={fecha}
              max={new Date().toISOString().slice(0, 10)}
              onChange={e => setFecha(e.target.value)}
            />
          </FormField>

          {errorApi && <Alert variant="destructive">{errorApi}</Alert>}

          <div className="flex gap-2">
            <Button variant="outline" className="flex-1" onClick={handleClose}>Cancelar</Button>
            <Button className="flex-1" loading={mutation.isPending} onClick={onConfirm}>
              <Check className="w-3.5 h-3.5" /> Confirmar pago
            </Button>
          </div>
        </div>
      </div>
    </Dialog>
  )
}

// ─── Modal anular pago ────────────────────────────────────────────────────────

function AnularPagoModal({
  open, onClose, prestamo, cuota
}: {
  open: boolean
  onClose: () => void
  prestamo: Prestamo
  cuota: CuotaAmortizacion | null
}) {
  const [errorApi, setErrorApi] = useState<string>()
  const mutation = useAnularPagoCuota()

  const handleClose = () => { setErrorApi(undefined); onClose() }

  const onConfirm = async () => {
    if (!cuota) return
    setErrorApi(undefined)
    try {
      await mutation.mutateAsync({ prestamoId: prestamo.id, cuotaId: cuota.id })
      handleClose()
    } catch (err: any) {
      setErrorApi(err?.response?.data?.error ?? err?.response?.data?.detail ?? 'Error al anular el pago.')
    }
  }

  if (!cuota) return null

  return (
    <Dialog open={open} onClose={handleClose}>
      <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[360px] mx-4 p-5">
        <DialogHeader className="mb-4">
          <DialogTitle>Anular pago</DialogTitle>
          <DialogDescription>
            Cuota #{cuota.numeroCuota} · pagada el {cuota.fechaPago ? fmt.fecha(cuota.fechaPago) : '—'}
          </DialogDescription>
        </DialogHeader>
        <p className="text-sm text-muted-foreground mb-4">
          La cuota volverá a marcarse como <strong>pendiente</strong>. Esta acción solo debe usarse si el pago fue registrado por error.
        </p>
        {errorApi && <Alert variant="destructive" className="mb-3">{errorApi}</Alert>}
        <div className="flex gap-2">
          <Button variant="outline" className="flex-1" onClick={handleClose}>Cancelar</Button>
          <Button variant="destructive" className="flex-1" loading={mutation.isPending} onClick={onConfirm}>
            <Undo2 className="w-3.5 h-3.5" /> Anular pago
          </Button>
        </div>
      </div>
    </Dialog>
  )
}

// ─── Tarjeta de préstamo ──────────────────────────────────────────────────────

function PrestamoCard({ p, onPagar, onAnular }: {
  p: Prestamo
  onPagar: (c: CuotaAmortizacion) => void
  onAnular: (c: CuotaAmortizacion) => void
}) {
  const [expandido, setExpandido] = useState(false)

  const pagadas = p.cuotas.filter(c => c.pagada).length
  const pendientes = p.cuotas.filter(c => !c.pagada)
  const proximaCuota = pendientes[0] ?? null
  const diasProxima = proximaCuota ? diasHastaFecha(proximaCuota.fechaVencimiento) : null
  const etiqueta = diasProxima !== null ? etiquetaVencimiento(diasProxima) : null
  const totalPagadoMonto = p.cuotas.filter(c => c.pagada).reduce((s, c) => s + c.cuota, 0)
  const saldoRestante = proximaCuota?.saldoPendiente ?? (pagadas === p.nCuotas ? 0 : p.capital)
  const pct = Math.round((pagadas / p.nCuotas) * 100)
  const vencida = diasProxima !== null && diasProxima < 0

  return (
    <Card className="overflow-hidden">
      {/* Header del préstamo */}
      <div className="p-5">
        <div className="flex items-start gap-4">
          <div className={`w-9 h-9 rounded-lg flex items-center justify-center flex-shrink-0 ${vencida ? 'bg-rose-500/10' : pagadas === p.nCuotas ? 'bg-emerald-500/10' : 'bg-primary/10'}`}>
            {pagadas === p.nCuotas
              ? <CheckCircle2 className="w-4 h-4 text-emerald-400" />
              : vencida
                ? <AlertCircle className="w-4 h-4 text-rose-400" />
                : <Landmark className="w-4 h-4 text-primary" />
            }
          </div>

          <div className="flex-1 min-w-0">
            <div className="flex items-baseline gap-2 flex-wrap">
              <p className="text-sm font-semibold">{p.descripcion}</p>
              <span className="text-[10px] text-muted-foreground">{p.tasaMensual}% mensual · {p.nCuotas} cuotas</span>
            </div>

            {/* Progress bar */}
            <div className="mt-3">
              <div className="flex items-center justify-between mb-1.5">
                <span className="text-[11px] text-muted-foreground">
                  {pagadas === p.nCuotas ? '✓ Completado' : `${pagadas} de ${p.nCuotas} cuotas pagadas`}
                </span>
                <span className="text-[11px] font-medium tabular-nums">{pct}%</span>
              </div>
              <div className="h-1.5 bg-border rounded-full overflow-hidden">
                <div
                  className={`h-full rounded-full transition-all duration-500 ${pagadas === p.nCuotas ? 'bg-emerald-400' : 'bg-primary'}`}
                  style={{ width: `${pct}%` }}
                />
              </div>
            </div>

            {/* Resumen de montos */}
            <div className="flex items-center gap-4 mt-3 flex-wrap">
              <div>
                <p className="text-[9px] uppercase tracking-widest text-muted-foreground/50">Capital</p>
                <p className="text-xs font-semibold tabular-nums">{fmt.cop(p.capital)}</p>
              </div>
              <div>
                <p className="text-[9px] uppercase tracking-widest text-muted-foreground/50">Pagado</p>
                <p className="text-xs font-semibold tabular-nums text-emerald-400">{fmt.cop(totalPagadoMonto)}</p>
              </div>
              <div>
                <p className="text-[9px] uppercase tracking-widest text-muted-foreground/50">Saldo deuda</p>
                <p className="text-xs font-semibold tabular-nums text-amber-400">{fmt.cop(saldoRestante)}</p>
              </div>
              {proximaCuota && (
                <div className="ml-auto text-right">
                  <p className="text-[9px] uppercase tracking-widest text-muted-foreground/50">Próxima cuota</p>
                  <p className="text-xs font-bold tabular-nums">{fmt.cop(proximaCuota.cuota)}</p>
                  {etiqueta && (
                    <p className={`text-[10px] ${etiqueta.color}`}>{etiqueta.texto}</p>
                  )}
                </div>
              )}
            </div>

            {/* Alerta si hay cuota vencida sin pagar */}
            {vencida && (
              <div className="mt-3 flex items-center gap-2 text-xs text-rose-400 bg-rose-500/5 border border-rose-500/20 rounded-md px-3 py-2">
                <AlertCircle className="w-3.5 h-3.5 flex-shrink-0" />
                <span>Cuota #{proximaCuota?.numeroCuota} venció hace {Math.abs(diasProxima!)} días</span>
                <Button size="sm" className="ml-auto h-6 px-2 text-[10px] bg-rose-500/20 hover:bg-rose-500/30 text-rose-400 border-0"
                  onClick={() => proximaCuota && onPagar(proximaCuota)}>
                  Pagar ahora
                </Button>
              </div>
            )}
          </div>

          <button onClick={() => setExpandido(!expandido)}
            className="p-1.5 rounded-md text-muted-foreground hover:text-foreground hover:bg-muted/40 transition-colors flex-shrink-0 ml-2">
            {expandido ? <ChevronDown className="w-4 h-4" /> : <ChevronRight className="w-4 h-4" />}
          </button>
        </div>
      </div>

      {/* Tabla de cuotas */}
      {expandido && (
        <div className="border-t border-border/50">
          <div className="overflow-x-auto">
            <table className="w-full text-xs">
              <thead>
                <tr className="border-b border-border/40 bg-muted/20">
                  {['#', 'Vencimiento', 'Cuota', 'Interés', 'Capital', 'Saldo', 'Estado', ''].map(h => (
                    <th key={h} className="text-left px-3 py-2.5 text-[9px] font-semibold uppercase tracking-widest text-muted-foreground/50 whitespace-nowrap">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {p.cuotas.map((c, i) => {
                  const esProxima = !c.pagada && proximaCuota?.id === c.id
                  const diasC = !c.pagada ? diasHastaFecha(c.fechaVencimiento) : null
                  const esVencida = diasC !== null && diasC < 0

                  return (
                    <tr key={c.id}
                      className={`border-b border-border/20 transition-colors group
                        ${c.pagada ? 'bg-emerald-500/[0.03]' : ''}
                        ${esProxima && !esVencida ? 'bg-amber-500/[0.04]' : ''}
                        ${esVencida ? 'bg-rose-500/[0.04]' : ''}
                        ${!c.pagada && !esProxima ? 'hover:bg-muted/20' : ''}
                        ${i === p.cuotas.length - 1 ? 'border-b-0' : ''}
                      `}>
                      <td className="px-3 py-2.5 tabular-nums text-muted-foreground font-medium">{c.numeroCuota}</td>
                      <td className="px-3 py-2.5 tabular-nums text-muted-foreground">
                        {fmt.fecha(c.fechaVencimiento)}
                        {!c.pagada && diasC !== null && diasC <= 7 && (
                          <span className={`ml-1.5 text-[9px] ${esVencida ? 'text-rose-400' : 'text-amber-400'}`}>
                            {esVencida ? `−${Math.abs(diasC)}d` : `+${diasC}d`}
                          </span>
                        )}
                      </td>
                      <td className="px-3 py-2.5 tabular-nums font-semibold">{fmt.cop(c.cuota)}</td>
                      <td className="px-3 py-2.5 tabular-nums text-muted-foreground">{fmt.cop(c.interes)}</td>
                      <td className="px-3 py-2.5 tabular-nums text-muted-foreground">{fmt.cop(c.abonoCapital)}</td>
                      <td className="px-3 py-2.5 tabular-nums text-muted-foreground">{fmt.cop(c.saldoPendiente)}</td>
                      <td className="px-3 py-2.5">
                        {c.pagada ? (
                          <span className="inline-flex items-center gap-1 text-[10px] text-emerald-400">
                            <CheckCircle2 className="w-3 h-3" />
                            {c.fechaPago ? fmt.fecha(c.fechaPago) : 'Pagada'}
                          </span>
                        ) : esVencida ? (
                          <span className="inline-flex items-center gap-1 text-[10px] text-rose-400">
                            <AlertCircle className="w-3 h-3" />
                            Vencida
                          </span>
                        ) : esProxima ? (
                          <span className="inline-flex items-center gap-1 text-[10px] text-amber-400">
                            <Clock className="w-3 h-3" />
                            Próxima
                          </span>
                        ) : (
                          <span className="text-[10px] text-muted-foreground/50">Pendiente</span>
                        )}
                      </td>
                      <td className="px-3 py-2.5 text-right">
                        {c.pagada ? (
                          <button
                            onClick={() => onAnular(c)}
                            className="opacity-0 group-hover:opacity-100 transition-opacity inline-flex items-center gap-1 text-[10px] text-muted-foreground hover:text-foreground"
                            title="Anular pago">
                            <Undo2 className="w-3 h-3" /> Anular
                          </button>
                        ) : (
                          <button
                            onClick={() => onPagar(c)}
                            className={`transition-opacity inline-flex items-center gap-1 text-[10px] font-medium
                              ${esProxima || esVencida
                                ? 'opacity-100 text-primary hover:text-primary/80'
                                : 'opacity-0 group-hover:opacity-100 text-muted-foreground hover:text-foreground'
                              }`}
                            title="Registrar pago">
                            <Check className="w-3 h-3" /> Pagar
                          </button>
                        )}
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </Card>
  )
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function PrestamosPage() {
  const [modalCrear, setModalCrear] = useState(false)
  const [pagarState, setPagarState] = useState<{ prestamo: Prestamo; cuota: CuotaAmortizacion } | null>(null)
  const [anularState, setAnularState] = useState<{ prestamo: Prestamo; cuota: CuotaAmortizacion } | null>(null)

  const { data: prestamos, isLoading } = usePrestamos()
  const arr = (prestamos as Prestamo[] | undefined) ?? []

  // Resumen global
  const totalDeuda = arr.reduce((s, p) => {
    const proxima = p.cuotas.find(c => !c.pagada)
    return s + (proxima?.saldoPendiente ?? 0)
  }, 0)
  const cuotasVencidas = arr.flatMap(p => p.cuotas.filter(c => !c.pagada && diasHastaFecha(c.fechaVencimiento) < 0)).length

  return (
    <div className="flex flex-col h-full">
      <PageHeader
        title="Préstamos"
        action={
          <Button size="sm" onClick={() => setModalCrear(true)}>
            <Plus className="w-3.5 h-3.5" /> Nuevo préstamo
          </Button>
        }
      />

      <div className="flex-1 overflow-y-auto p-6">
        {isLoading ? (
          <div className="space-y-4">
            {[1, 2].map(i => <Skeleton key={i} className="h-36 rounded-lg" />)}
          </div>
        ) : arr.length === 0 ? (
          <EmptyState
            icon={<Landmark className="w-5 h-5" />}
            title="Sin préstamos registrados"
            description="Registra un crédito para generar su tabla de amortización automáticamente."
            action={<Button size="sm" onClick={() => setModalCrear(true)}><Plus className="w-3.5 h-3.5" /> Registrar préstamo</Button>}
          />
        ) : (
          <>
            {/* Resumen global si hay varios préstamos */}
            {arr.length > 1 && (
              <div className="grid grid-cols-2 sm:grid-cols-3 gap-3 mb-5">
                <div className="rounded-lg border border-border/50 p-4">
                  <p className="text-[10px] font-semibold uppercase tracking-widest text-muted-foreground/50 mb-1.5">Deuda total activa</p>
                  <p className="text-xl font-bold tabular-nums text-amber-400">{fmt.cop(totalDeuda)}</p>
                </div>
                <div className={`rounded-lg border p-4 ${cuotasVencidas > 0 ? 'border-rose-500/30' : 'border-border/50'}`}>
                  <p className="text-[10px] font-semibold uppercase tracking-widest text-muted-foreground/50 mb-1.5">Cuotas vencidas</p>
                  <p className={`text-xl font-bold tabular-nums ${cuotasVencidas > 0 ? 'text-rose-400' : ''}`}>{cuotasVencidas}</p>
                </div>
                <div className="rounded-lg border border-border/50 p-4">
                  <p className="text-[10px] font-semibold uppercase tracking-widest text-muted-foreground/50 mb-1.5">Préstamos activos</p>
                  <p className="text-xl font-bold tabular-nums">{arr.filter(p => p.cuotas.some(c => !c.pagada)).length}</p>
                </div>
              </div>
            )}

            <div className="space-y-4">
              {arr.map(p => (
                <PrestamoCard
                  key={p.id}
                  p={p}
                  onPagar={c => setPagarState({ prestamo: p, cuota: c })}
                  onAnular={c => setAnularState({ prestamo: p, cuota: c })}
                />
              ))}
            </div>
          </>
        )}
      </div>

      <CrearPrestamoModal open={modalCrear} onClose={() => setModalCrear(false)} />

      <PagarCuotaModal
        open={!!pagarState}
        onClose={() => setPagarState(null)}
        prestamo={pagarState?.prestamo ?? arr[0]}
        cuota={pagarState?.cuota ?? null}
      />

      <AnularPagoModal
        open={!!anularState}
        onClose={() => setAnularState(null)}
        prestamo={anularState?.prestamo ?? arr[0]}
        cuota={anularState?.cuota ?? null}
      />
    </div>
  )
}
