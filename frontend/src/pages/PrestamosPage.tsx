import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Plus, X, CheckCircle2, ChevronDown, ChevronRight, Landmark } from 'lucide-react'
import { usePrestamos, useCrearPrestamo } from '@/hooks/useFeedlot'
import {
  PageHeader, Card, Skeleton, EmptyState, Button,
  Dialog, DialogHeader, DialogTitle, DialogDescription,
  FormField, Input, Alert, Badge,
  MoneyInput,
} from '@/components/ui'
import { fmt } from '@/utils'
import type { Prestamo } from '@/types'

const prestamoSchema = z.object({
  monto: z.coerce.number().positive('Debe ser mayor a cero'),
  moneda: z.string().length(3).default('COP'),
  tasaMensual: z.coerce.number().min(0, 'No puede ser negativa').max(100, 'Máximo 100%'),
  nCuotas: z.coerce.number().int().positive('Debe ser mayor a cero'),
  fechaInicio: z.string().min(1, 'Requerida'),
  descripcion: z.string().min(3, 'Mínimo 3 caracteres').max(500),
})
type PrestamoForm = z.infer<typeof prestamoSchema>

function CrearPrestamoModal({ open, onClose }: { open: boolean; onClose: () => void }) {
  const [exito, setExito] = useState(false)
  const [errorApi, setErrorApi] = useState<string>()
  const [cuotaEstimada, setCuotaEstimada] = useState<number | null>(null)
  const mutation = useCrearPrestamo()

  const { register, handleSubmit, reset, watch, formState: { errors, isSubmitting } } =
    useForm<PrestamoForm>({
      resolver: zodResolver(prestamoSchema),
      defaultValues: { moneda: 'COP', fechaInicio: new Date().toISOString().slice(0, 10) },
    })

  const monto = watch('monto')
  const tasa = watch('tasaMensual')
  const nCuotas = watch('nCuotas')

  const calcularEstimacion = () => {
    if (!monto || monto <= 0 || !tasa || tasa <= 0 || !nCuotas || nCuotas <= 0) {
      setCuotaEstimada(null)
      return
    }
    const i = tasa / 100
    const factor = Math.pow(1 + i, -nCuotas)
    const cuota = Math.round(monto * i / (1 - factor))
    setCuotaEstimada(cuota)
  }

  const handleClose = () => { reset(); setExito(false); setErrorApi(undefined); setCuotaEstimada(null); onClose() }

  const onSubmit = async (data: PrestamoForm) => {
    setErrorApi(undefined)
    try {
      await mutation.mutateAsync(data)
      setExito(true)
      setTimeout(() => handleClose(), 1500)
    } catch (err: any) {
      setErrorApi(err?.response?.data?.error ?? 'Error al crear préstamo.')
    }
  }

  return (
    <Dialog open={open} onClose={handleClose}>
      <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[480px] mx-4">
        <div className="flex items-center justify-between px-5 py-4 border-b border-border">
          <DialogHeader className="mb-0">
            <DialogTitle>Nuevo préstamo</DialogTitle>
            <DialogDescription>Registra un crédito y genera su tabla de amortización</DialogDescription>
          </DialogHeader>
          <button onClick={handleClose} className="text-muted-foreground hover:text-foreground ml-4"><X className="w-4 h-4" /></button>
        </div>
        <div className="p-5 max-h-[70vh] overflow-y-auto">
          {exito ? (
            <div className="flex flex-col items-center py-6 gap-3 animate-fade-in">
              <div className="w-12 h-12 rounded-full bg-emerald-500/10 flex items-center justify-center"><CheckCircle2 className="w-6 h-6 text-emerald-400" /></div>
              <p className="text-sm font-medium">¡Préstamo creado!</p>
              <p className="text-xs text-muted-foreground">La tabla de amortización se generó automáticamente.</p>
            </div>
          ) : (
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              <FormField label="Descripción" error={errors.descripcion?.message} required>
                <Input {...register('descripcion')} placeholder="Ej: Préstamo BBVA para inversión" />
              </FormField>
              <div className="grid grid-cols-2 gap-3">
                <FormField label="Monto" error={errors.monto?.message} required>
                  <MoneyInput {...register('monto')} min={1} placeholder="20000000" />
                </FormField>
                <FormField label="Moneda" error={errors.moneda?.message} required>
                  <Input {...register('moneda')} placeholder="COP" />
                </FormField>
              </div>
              <div className="grid grid-cols-2 gap-3">
                <FormField label="Tasa mensual (%)" error={errors.tasaMensual?.message} required hint="Ej: 1.79">
                  <Input {...register('tasaMensual')} type="number" min={0} step={0.01} placeholder="1.79" onBlur={calcularEstimacion} />
                </FormField>
                <FormField label="N° de cuotas" error={errors.nCuotas?.message} required>
                  <Input {...register('nCuotas')} type="number" min={1} step={1} placeholder="12" onBlur={calcularEstimacion} />
                </FormField>
              </div>
              <FormField label="Fecha de inicio" error={errors.fechaInicio?.message} required>
                <Input {...register('fechaInicio')} type="date" />
              </FormField>

              {cuotaEstimada !== null && (
                <div className="rounded-lg border border-primary/30 bg-primary/5 p-3 text-sm text-center">
                  <p className="text-muted-foreground">Cuota estimada (sistema francés)</p>
                  <p className="text-lg font-bold text-primary">{fmt.cop(cuotaEstimada)} /mes</p>
                </div>
              )}

              {errorApi && <Alert variant="destructive">{errorApi}</Alert>}

              <div className="flex gap-2 pt-1">
                <Button type="button" variant="outline" className="flex-1" onClick={handleClose} disabled={isSubmitting}>Cancelar</Button>
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

export default function PrestamosPage() {
  const [modalAbierto, setModalAbierto] = useState(false)
  const [expandidoId, setExpandidoId] = useState<string | null>(null)
  const { data: prestamos, isLoading } = usePrestamos()
  const prestamosArray = (prestamos as Prestamo[] | undefined) ?? []

  return (
    <div className="flex flex-col h-full animate-fade-in">
      <PageHeader
        title="Préstamos"
        description="Créditos y tabla de amortización"
        action={
          <Button size="sm" onClick={() => setModalAbierto(true)}>
            <Plus className="w-3.5 h-3.5" /> Nuevo préstamo
          </Button>
        }
      />

      <div className="flex-1 overflow-y-auto p-6">
        {isLoading ? (
          <div className="space-y-3">{Array.from({ length: 3 }).map((_, i) => <Skeleton key={i} className="h-24 rounded-lg" />)}</div>
        ) : prestamosArray.length === 0 ? (
          <EmptyState icon={<Landmark className="w-5 h-5" />} title="Sin préstamos" description="Aún no hay préstamos registrados."
            action={<Button size="sm" onClick={() => setModalAbierto(true)}><Plus className="w-3.5 h-3.5" /> Registrar primero</Button>} />
        ) : (
          <div className="space-y-4">
            {prestamosArray.map(p => {
              const expandido = expandidoId === p.id
              const totalPagado = p.cuotas.filter(c => c.pagada).length
              const saldoRestante = p.cuotas[p.cuotas.length - 1]?.saldoPendiente ?? p.capital

              return (
                <Card key={p.id} className="overflow-hidden">
                  <div className="p-5">
                    <div className="flex items-start justify-between">
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-2 flex-wrap">
                          <p className="text-sm font-medium">{p.descripcion}</p>
                          <Badge className="bg-primary/10 text-primary border-primary/20 text-[10px]">
                            {p.nCuotas} cuotas · {p.tasaMensual}% mensual
                          </Badge>
                        </div>
                        <div className="flex items-center gap-4 mt-2 text-xs text-muted-foreground">
                          <span>Capital: <strong>{fmt.cop(p.capital)}</strong></span>
                          <span>Inicio: <strong>{fmt.fecha(p.fechaInicio)}</strong></span>
                          <span>Cuotas pagadas: <strong>{totalPagado}/{p.nCuotas}</strong></span>
                        </div>
                      </div>
                      <button onClick={() => setExpandidoId(expandido ? null : p.id)}
                        className="p-1.5 rounded-md text-muted-foreground hover:text-foreground hover:bg-secondary ml-3">
                        {expandido ? <ChevronDown className="w-4 h-4" /> : <ChevronRight className="w-4 h-4" />}
                      </button>
                    </div>
                  </div>

                  {expandido && (
                    <div className="border-t border-border">
                      <div className="p-5">
                        <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 mb-4">
                          <div className="rounded-lg bg-secondary/30 p-3 text-center">
                            <p className="text-[10px] text-muted-foreground uppercase tracking-wide">Capital</p>
                            <p className="text-sm font-bold">{fmt.cop(p.capital)}</p>
                          </div>
                          <div className="rounded-lg bg-secondary/30 p-3 text-center">
                            <p className="text-[10px] text-muted-foreground uppercase tracking-wide">Cuota mensual</p>
                            <p className="text-sm font-bold">{fmt.cop(p.cuotas[0]?.cuota ?? 0)}</p>
                          </div>
                          <div className="rounded-lg bg-secondary/30 p-3 text-center">
                            <p className="text-[10px] text-muted-foreground uppercase tracking-wide">Pagado</p>
                            <p className="text-sm font-bold text-emerald-400">{fmt.cop(p.cuotas.filter(c => c.pagada).reduce((s, c) => s + c.cuota, 0))}</p>
                          </div>
                          <div className="rounded-lg bg-secondary/30 p-3 text-center">
                            <p className="text-[10px] text-muted-foreground uppercase tracking-wide">Saldo pendiente</p>
                            <p className="text-sm font-bold text-amber-400">{fmt.cop(saldoRestante)}</p>
                          </div>
                        </div>
                        <div className="overflow-x-auto">
                          <table className="w-full text-xs">
                            <thead>
                              <tr className="border-b border-border">
                                {['#', 'Vencimiento', 'Cuota', 'Interés', 'Abono capital', 'Saldo', 'Estado'].map(h => (
                                  <th key={h} className="text-left px-3 py-2 text-muted-foreground font-medium uppercase tracking-wide text-[9px] whitespace-nowrap">{h}</th>
                                ))}
                              </tr>
                            </thead>
                            <tbody>
                              {p.cuotas.map((c) => (
                                <tr key={c.id} className={`border-b border-border/30 hover:bg-muted/20 transition-colors ${c.pagada ? 'bg-emerald-500/5' : ''}`}>
                                  <td className="px-3 py-2 font-medium tabular-nums">{c.numeroCuota}</td>
                                  <td className="px-3 py-2 text-muted-foreground">{fmt.fecha(c.fechaVencimiento)}</td>
                                  <td className="px-3 py-2 tabular-nums font-medium">{fmt.cop(c.cuota)}</td>
                                  <td className="px-3 py-2 tabular-nums text-muted-foreground">{fmt.cop(c.interes)}</td>
                                  <td className="px-3 py-2 tabular-nums">{fmt.cop(c.abonoCapital)}</td>
                                  <td className="px-3 py-2 tabular-nums">{fmt.cop(c.saldoPendiente)}</td>
                                  <td className="px-3 py-2">
                                    <Badge className={`text-[9px] ${c.pagada ? 'bg-emerald-500/10 text-emerald-400' : 'bg-zinc-500/10 text-zinc-400'} border-0`}>
                                      {c.pagada ? 'Pagada' : 'Pendiente'}
                                    </Badge>
                                  </td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        </div>
                      </div>
                    </div>
                  )}
                </Card>
              )
            })}
          </div>
        )}
      </div>

      <CrearPrestamoModal open={modalAbierto} onClose={() => setModalAbierto(false)} />
    </div>
  )
}