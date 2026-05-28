import { useState } from 'react'
import { format } from 'date-fns'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Plus, X, ShoppingCart, CheckCircle2, ChevronDown, Beef, Trash2, DollarSign } from 'lucide-react'
import { useVentas, useCrearVenta, useCompradores, useAnimals } from '@/hooks/useFeedlot'
import {
  PageHeader, Card, CardContent,
  Skeleton, EmptyState, Button,
  Dialog, DialogHeader, DialogTitle, DialogDescription,
  FormField, Input, Alert, Badge,
} from '@/components/ui'
import { fmt } from '@/utils'
import type { Venta, Comprador, AnimalResumen } from '@/types'

const hoy = format(new Date(), 'yyyy-MM-dd')

const ventaSchema = z.object({
  compradorId: z.string().min(1, 'Selecciona un comprador'),
  fecha: z.string().min(1, 'La fecha es requerida'),
  moneda: z.string().length(3, 'Código ISO').default('COP'),
  descripcion: z.string().max(500).optional().or(z.literal('')),
  animales: z.array(z.object({
    animalId: z.string().min(1),
    precioVenta: z.number().min(0, 'No puede ser negativo'),
    pesoVentaKg: z.number().positive('Debe ser mayor a cero'),
  })).min(1, 'Agrega al menos un animal'),
})
type VentaForm = z.infer<typeof ventaSchema>

interface AnimalRow {
  animalId: string
  codigo: string
  nombre?: string
  precioVenta: number
  pesoVentaKg: number
}

function CrearVentaModal({ open, onClose }: { open: boolean; onClose: () => void }) {
  const [exito, setExito] = useState(false)
  const [errorApi, setErrorApi] = useState<string>()
  const [animalesRows, setAnimalesRows] = useState<AnimalRow[]>([])
  const crearVenta = useCrearVenta()
  const { data: compradoresData } = useCompradores()
  const { data: animalsData } = useAnimals({ page: 1, pageSize: 200, estadoProductivo: 'EnEngorde' })

  const compradores = ((compradoresData as Comprador[] | undefined) ?? []).map(c => ({
    id: c.id, label: c.nombre,
  }))
  const animalesDisponibles = ((animalsData?.items as AnimalResumen[] | undefined) ?? [])
    .filter(a => !animalesRows.some(r => r.animalId === a.id))

  const { register, reset, watch, setValue, formState: { errors, isSubmitting } } =
    useForm<VentaForm>({
      resolver: zodResolver(ventaSchema),
      defaultValues: { moneda: 'COP', fecha: hoy, animales: [] },
    })

  const handleClose = () => { reset(); setExito(false); setErrorApi(undefined); setAnimalesRows([]); onClose() }

  const agregarAnimal = (animalId: string) => {
    const animal = animalesDisponibles.find(a => a.id === animalId)
    if (!animal) return
    setAnimalesRows(prev => [...prev, {
      animalId: animal.id,
      codigo: animal.codigoIdentificacion,
      nombre: animal.nombre,
      precioVenta: 0,
      pesoVentaKg: animal.pesoActualKg,
    }])
  }

  const quitarAnimal = (animalId: string) => {
    setAnimalesRows(prev => prev.filter(r => r.animalId !== animalId))
  }

  const actualizarRow = (animalId: string, campo: 'precioVenta' | 'pesoVentaKg', valor: number) => {
    setAnimalesRows(prev => prev.map(r => r.animalId === animalId ? { ...r, [campo]: valor } : r))
  }

  const montoTotal = animalesRows.reduce((sum, r) => sum + r.precioVenta, 0)

  const onSubmit = async () => {
    setErrorApi(undefined)
    if (!watch('compradorId')) { setErrorApi('Selecciona un comprador'); return }
    if (animalesRows.length === 0) { setErrorApi('Agrega al menos un animal'); return }
    if (animalesRows.some(r => r.precioVenta <= 0)) { setErrorApi('Todos los animales deben tener un precio de venta'); return }

    try {
      await crearVenta.mutateAsync({
        compradorId: watch('compradorId'),
        fecha: watch('fecha'),
        moneda: watch('moneda'),
        descripcion: watch('descripcion') || undefined,
        animales: animalesRows.map(r => ({
          animalId: r.animalId,
          precioVenta: r.precioVenta,
          pesoVentaKg: r.pesoVentaKg,
        })),
      })
      setExito(true)
      setTimeout(() => handleClose(), 1500)
    } catch (err: any) {
      setErrorApi(err?.response?.data?.error ?? err?.response?.data?.detail ?? 'Error al registrar la venta.')
    }
  }

  return (
    <Dialog open={open} onClose={handleClose}>
      <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[560px] mx-4">
        <div className="flex items-center justify-between px-5 py-4 border-b border-border">
          <DialogHeader className="mb-0">
            <DialogTitle>Registrar venta</DialogTitle>
            <DialogDescription>Ingresa los datos de la venta y los animales vendidos</DialogDescription>
          </DialogHeader>
          <button onClick={handleClose} className="text-muted-foreground hover:text-foreground ml-4">
            <X className="w-4 h-4" />
          </button>
        </div>

        <div className="p-5 max-h-[70vh] overflow-y-auto">
          {exito ? (
            <div className="flex flex-col items-center py-6 gap-3 animate-fade-in">
              <div className="w-12 h-12 rounded-full bg-emerald-500/10 flex items-center justify-center">
                <CheckCircle2 className="w-6 h-6 text-emerald-400" />
              </div>
              <p className="text-sm font-medium">¡Venta registrada!</p>
              <p className="text-xs text-muted-foreground">Los animales fueron marcados como vendidos.</p>
            </div>
          ) : (
            <div className="space-y-4">
              <div className="grid grid-cols-2 gap-3">
                <FormField label="Comprador" error={errors.compradorId?.message} required>
                  <div className="relative">
                    <select value={watch('compradorId')} onChange={e => setValue('compradorId', e.target.value)}
                      className="h-9 pl-3 pr-8 rounded-md border border-input bg-card text-sm w-full appearance-none cursor-pointer focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring [&>option]:bg-card">
                      <option value="">Seleccionar...</option>
                      {compradores.map(c => <option key={c.id} value={c.id}>{c.label}</option>)}
                    </select>
                    <ChevronDown className="absolute right-2 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-muted-foreground pointer-events-none" />
                  </div>
                </FormField>
                <FormField label="Fecha" error={errors.fecha?.message} required>
                  <Input {...register('fecha')} type="date" max={hoy}
                    className={errors.fecha ? 'border-destructive' : ''} />
                </FormField>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <FormField label="Moneda" error={errors.moneda?.message} required>
                  <Input {...register('moneda')} placeholder="COP"
                    className={errors.moneda ? 'border-destructive' : ''} />
                </FormField>
                <FormField label="Descripción">
                  <Input {...register('descripcion')} placeholder="Notas de la venta" />
                </FormField>
              </div>

              {/* Selector de animales */}
              <div>
                <div className="flex items-center justify-between mb-2">
                  <p className="text-[10px] font-semibold text-muted-foreground uppercase tracking-widest">
                    Animales a vender ({animalesRows.length})
                  </p>
                  {animalesDisponibles.length > 0 && (
                    <div className="relative">
                      <select onChange={e => { if (e.target.value) { agregarAnimal(e.target.value); e.target.value = '' } }}
                        className="h-7 pl-2 pr-6 rounded-md border border-input bg-card text-xs appearance-none cursor-pointer focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring [&>option]:bg-card">
                        <option value="">+ Agregar animal</option>
                        {animalesDisponibles.map(a => (
                          <option key={a.id} value={a.id}>{a.codigoIdentificacion}{a.nombre ? ` — ${a.nombre}` : ''} ({fmt.kg(a.pesoActualKg)})</option>
                        ))}
                      </select>
                      <ChevronDown className="absolute right-1 top-1/2 -translate-y-1/2 w-3 h-3 text-muted-foreground pointer-events-none" />
                    </div>
                  )}
                </div>

                {animalesRows.length === 0 ? (
                  <p className="text-xs text-muted-foreground text-center py-4 border border-dashed border-border rounded-lg">
                    {animalesDisponibles.length === 0
                      ? 'No hay animales disponibles para vender'
                      : 'Agrega animales desde el selector'}
                  </p>
                ) : (
                  <div className="space-y-2 max-h-48 overflow-y-auto">
                    {animalesRows.map(row => (
                      <div key={row.animalId} className="flex items-center gap-2 p-2 rounded-lg border border-border/50 bg-secondary/20">
                        <div className="flex-1 min-w-0">
                          <p className="text-xs font-medium truncate">{row.codigo}{row.nombre ? ` — ${row.nombre}` : ''}</p>
                        </div>
                        <input type="number" placeholder="$ venta" value={row.precioVenta || ''}
                          onChange={e => actualizarRow(row.animalId, 'precioVenta', Number(e.target.value))}
                          className="w-24 h-7 px-2 rounded border border-input bg-card text-xs text-right" min={0} step={1000} />
                        <input type="number" placeholder="kg" value={row.pesoVentaKg || ''}
                          onChange={e => actualizarRow(row.animalId, 'pesoVentaKg', Number(e.target.value))}
                          className="w-16 h-7 px-2 rounded border border-input bg-card text-xs text-right" min={0} step={0.1} />
                        <button onClick={() => quitarAnimal(row.animalId)}
                          className="p-1 rounded text-muted-foreground hover:text-destructive">
                          <Trash2 className="w-3 h-3" />
                        </button>
                      </div>
                    ))}
                  </div>
                )}

                {animalesRows.length > 0 && (
                  <div className="flex justify-between items-center mt-2 pt-2 border-t border-border">
                    <span className="text-xs text-muted-foreground">{animalesRows.length} animal{animalesRows.length > 1 ? 'es' : ''}</span>
                    <span className="text-sm font-semibold">Total: {fmt.cop(montoTotal)}</span>
                  </div>
                )}
              </div>

              {errorApi && <Alert variant="destructive">{errorApi}</Alert>}

              <div className="flex gap-2 pt-1">
                <Button type="button" variant="outline" className="flex-1" onClick={handleClose} disabled={isSubmitting}>
                  Cancelar
                </Button>
                <Button type="button" className="flex-1" loading={isSubmitting} onClick={onSubmit}>
                  <ShoppingCart className="w-3.5 h-3.5" />
                  Registrar venta
                </Button>
              </div>
            </div>
          )}
        </div>
      </div>
    </Dialog>
  )
}

export default function VentasPage() {
  const [modalAbierto, setModalAbierto] = useState(false)
  const { data: ventas, isLoading } = useVentas()
  const ventasArray = (ventas as Venta[] | undefined) ?? []

  return (
    <div className="flex flex-col h-full animate-fade-in">
      <PageHeader
        title="Ventas"
        description="Registro de ventas de animales"
        action={
          <Button size="sm" onClick={() => setModalAbierto(true)}>
            <Plus className="w-3.5 h-3.5" />
            Nueva venta
          </Button>
        }
      />

      <div className="flex-1 overflow-y-auto p-6">
        {isLoading ? (
          <div className="space-y-3">
            {Array.from({ length: 4 }).map((_, i) => <Skeleton key={i} className="h-16 rounded-lg" />)}
          </div>
        ) : ventasArray.length === 0 ? (
          <EmptyState
            icon={<DollarSign className="w-5 h-5" />}
            title="Sin ventas"
            description="Aún no hay ventas registradas. Al registrar una venta, los animales se marcan como vendidos."
            action={
              <Button size="sm" onClick={() => setModalAbierto(true)}>
                <Plus className="w-3.5 h-3.5" />
                Registrar primera venta
              </Button>
            }
          />
        ) : (
          <Card>
            <CardContent className="p-0">
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-border">
                      {['Fecha', 'Comprador', 'Animales', 'Total', 'Descripción'].map(h => (
                        <th key={h} className="text-left px-4 py-3 text-muted-foreground font-medium uppercase tracking-wide text-[10px] whitespace-nowrap">
                          {h}
                        </th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {ventasArray.map((v, i) => (
                      <tr key={v.id}
                        className={`border-b border-border/40 hover:bg-secondary/30 transition-colors ${i === ventasArray.length - 1 ? 'border-b-0' : ''}`}
                      >
                        <td className="px-4 py-3 text-muted-foreground tabular-nums">{fmt.fecha(v.fecha)}</td>
                        <td className="px-4 py-3 font-medium">{v.nombreComprador}</td>
                        <td className="px-4 py-3">
                          <div className="flex flex-wrap gap-1">
                            <Badge className="bg-primary/10 text-primary border-primary/20 text-[10px]">
                              <Beef className="w-3 h-3 mr-1" />{v.totalAnimales}
                            </Badge>
                            {v.items.slice(0, 3).map(item => (
                              <span key={item.id} className="text-[10px] text-muted-foreground font-mono">
                                {item.codigoAnimal}{item.nombreAnimal ? ` (${item.nombreAnimal})` : ''}
                              </span>
                            ))}
                            {v.items.length > 3 && (
                              <span className="text-[10px] text-muted-foreground">+{v.items.length - 3} más</span>
                            )}
                          </div>
                        </td>
                        <td className="px-4 py-3 tabular-nums font-medium">{fmt.cop(v.montoTotal)}</td>
                        <td className="px-4 py-3 text-muted-foreground text-xs max-w-[200px] truncate">{v.descripcion ?? '—'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </CardContent>
          </Card>
        )}
      </div>

      <CrearVentaModal open={modalAbierto} onClose={() => setModalAbierto(false)} />
    </div>
  )
}
