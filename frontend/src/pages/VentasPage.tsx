import { useState } from 'react'
import { format } from 'date-fns'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import {
  Plus, X, ShoppingCart, CheckCircle2, Beef,
  Trash2, DollarSign, AlertTriangle, Package,
} from 'lucide-react'
import { useVentas, useCrearVenta, useCompradores, useAnimals } from '@/hooks/useFeedlot'
import { lotesService } from '@/services/feedlot.service'
import {
  PageHeader, Card, CardContent,
  Skeleton, EmptyState, Button,
  Dialog, DialogHeader, DialogTitle, DialogDescription,
  FormField, Input, Alert, Badge,
  MoneyInput, CustomSelect,
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
    precioVenta: z.number().min(0),
    pesoVentaKg: z.number().positive(),
  })).min(1),
})
type VentaForm = z.infer<typeof ventaSchema>

interface LoteInfo {
  loteId: string
  loteCodigo: string
  loteNombre: string
}

interface AnimalRow {
  animalId: string
  codigo: string
  nombre?: string
  precioVenta: number
  pesoVentaKg: number
  loteActual?: LoteInfo
}

// ─── Modal de confirmación de retiro del lote ────────────────────────────────
function ConfirmarRetiroLoteModal({
  open,
  animalesConLote,
  onAceptar,
  onCancelar,
}: {
  open: boolean
  animalesConLote: AnimalRow[]
  onAceptar: () => void
  onCancelar: () => void
}) {
  const lotesCodigos = [
    ...new Set(animalesConLote.map(a => a.loteActual!.loteCodigo)),
  ]

  return (
    <Dialog open={open} onClose={onCancelar}>
      <div className="rounded-xl border border-amber-500/30 bg-card shadow-xl w-full max-w-md mx-4">
        <div className="flex items-center gap-3 px-5 py-4 border-b border-border">
          <div className="w-9 h-9 rounded-lg bg-amber-500/10 flex items-center justify-center flex-shrink-0">
            <AlertTriangle className="w-4 h-4 text-amber-400" />
          </div>
          <div>
            <h2 className="text-sm font-semibold">Animales en lote activo</h2>
            <p className="text-xs text-muted-foreground mt-0.5">Confirmación requerida</p>
          </div>
        </div>

        <div className="p-5 space-y-4">
          <p className="text-sm leading-relaxed">
            {animalesConLote.length === 1
              ? `El animal `
              : `${animalesConLote.length} animales `}
            pertenecen actualmente al lote{' '}
            <span className="font-semibold text-amber-400">
              {lotesCodigos.join(', ')}
            </span>.
          </p>

          <p className="text-sm text-muted-foreground leading-relaxed">
            ¿Desea retirarlos de su lote actual para proceder con la venta?
          </p>

          {/* Lista de animales afectados */}
          <div className="rounded-lg border border-border/50 bg-secondary/30 p-3 space-y-1.5 max-h-40 overflow-y-auto">
            {animalesConLote.map(a => (
              <div key={a.animalId} className="flex items-center justify-between text-xs">
                <span className="font-mono font-medium">{a.codigo}</span>
                <div className="flex items-center gap-1.5 text-muted-foreground">
                  <Package className="w-3 h-3" />
                  <span>{a.loteActual?.loteCodigo}</span>
                </div>
              </div>
            ))}
          </div>

          <div className="rounded-lg border border-amber-500/20 bg-amber-500/5 px-3 py-2">
            <p className="text-[10px] text-amber-400 leading-relaxed">
              El sistema retirará los animales de su lote con motivo{' '}
              <strong>Venta</strong>. El lote original no se cerrará automáticamente.
            </p>
          </div>

          <div className="flex gap-2">
            <Button variant="outline" className="flex-1" onClick={onCancelar}>
              Cancelar
            </Button>
            <Button
              className="flex-1 bg-amber-500 hover:bg-amber-500/90 text-white"
              onClick={onAceptar}
            >
              <ShoppingCart className="w-3.5 h-3.5" />
              Sí, retirar y vender
            </Button>
          </div>
        </div>
      </div>
    </Dialog>
  )
}

// ─── Modal crear venta ────────────────────────────────────────────────────────
function CrearVentaModal({ open, onClose }: { open: boolean; onClose: () => void }) {
  const [exito, setExito] = useState(false)
  const [errorApi, setErrorApi] = useState<string>()
  const [animalesRows, setAnimalesRows] = useState<AnimalRow[]>([])
  const [precioPorKg, setPrecioPorKg] = useState(0)
  const [pendienteConfirmacion, setPendienteConfirmacion] = useState(false)
  const [verificandoLotes, setVerificandoLotes] = useState(false)

  const crearVenta = useCrearVenta()
  const { data: compradoresData } = useCompradores()
  const { data: animalsData } = useAnimals({
    page: 1,
    pageSize: 200,
    estadoProductivo: 'EnEngorde',
  })

  const compradores = ((compradoresData as Comprador[] | undefined) ?? []).map(c => ({
    value: c.id,
    label: c.nombre,
  }))

  const animalesDisponibles = (
    (animalsData?.items as AnimalResumen[] | undefined) ?? []
  ).filter(a => !animalesRows.some(r => r.animalId === a.id))

  const { register, reset, watch, setValue, formState: { errors } } = useForm<VentaForm>({
    resolver: zodResolver(ventaSchema),
    defaultValues: { moneda: 'COP', fecha: hoy, animales: [] },
  })

  const handleClose = () => {
    reset()
    setExito(false)
    setErrorApi(undefined)
    setAnimalesRows([])
    setPendienteConfirmacion(false)
    onClose()
  }

  const calcularPrecioVenta = (pesoKg: number) =>
    precioPorKg > 0 ? Math.round(pesoKg * precioPorKg) : 0

  const agregarAnimal = (animalId: string) => {
    const animal = animalesDisponibles.find(a => a.id === animalId)
    if (!animal) return
    setAnimalesRows(prev => [
      ...prev,
      {
        animalId: animal.id,
        codigo: animal.codigoIdentificacion,
        nombre: animal.nombre,
        precioVenta: calcularPrecioVenta(animal.pesoActualKg),
        pesoVentaKg: animal.pesoActualKg,
        loteActual: undefined, // se resuelve al iniciar la venta
      },
    ])
  }

  const quitarAnimal = (animalId: string) => {
    setAnimalesRows(prev => prev.filter(r => r.animalId !== animalId))
  }

  const actualizarRow = (
    animalId: string,
    campo: 'precioVenta' | 'pesoVentaKg',
    valor: number,
  ) => {
    setAnimalesRows(prev =>
      prev.map(r => {
        if (r.animalId !== animalId) return r
        if (campo === 'pesoVentaKg')
          return { ...r, pesoVentaKg: valor, precioVenta: calcularPrecioVenta(valor) }
        return { ...r, [campo]: valor }
      }),
    )
  }

  const montoTotal = animalesRows.reduce((s, r) => s + r.precioVenta, 0)
  const animalesConLote = animalesRows.filter(r => r.loteActual !== undefined)

  // ── Paso 1: validar form y consultar lotes al backend ─────────────────────
  const iniciarVenta = async () => {
    setErrorApi(undefined)
    if (!watch('compradorId')) { setErrorApi('Selecciona un comprador'); return }
    if (animalesRows.length === 0) { setErrorApi('Agrega al menos un animal'); return }
    if (animalesRows.some(r => r.precioVenta <= 0)) {
      setErrorApi('Todos los animales deben tener un precio de venta mayor a cero')
      return
    }

    setVerificandoLotes(true)
    try {
      // Consultar al backend qué animales tienen lote activo
      const mapaLotes = await lotesService.consultarLotesAnimales(
        animalesRows.map(r => r.animalId),
      )

      // Enriquecer las filas con la info de lote
      const rowsEnriquecidas = animalesRows.map(r => ({
        ...r,
        loteActual: mapaLotes[r.animalId] ?? undefined,
      }))
      setAnimalesRows(rowsEnriquecidas)

      const conLote = rowsEnriquecidas.filter(r => r.loteActual !== undefined)

      if (conLote.length > 0) {
        // Escenario 2: hay animales con lote → mostrar confirmación
        setPendienteConfirmacion(true)
      } else {
        // Escenario 1: todos sin lote → flujo directo
        await ejecutarVenta(rowsEnriquecidas)
      }
    } catch {
      setErrorApi('Error al verificar los lotes de los animales. Intenta de nuevo.')
    } finally {
      setVerificandoLotes(false)
    }
  }

  // ── Paso 2: ejecutar la venta (el backend retira del lote automáticamente) ─
  const ejecutarVenta = async (rows?: AnimalRow[]) => {
    setPendienteConfirmacion(false)
    setErrorApi(undefined)
    const filasAUsar = rows ?? animalesRows
    try {
      await crearVenta.mutateAsync({
        compradorId: watch('compradorId'),
        fecha: watch('fecha'),
        moneda: watch('moneda'),
        descripcion: watch('descripcion') || undefined,
        animales: filasAUsar.map(r => ({
          animalId: r.animalId,
          precioVenta: r.precioVenta,
          pesoVentaKg: r.pesoVentaKg,
        })),
      })
      setExito(true)
      setTimeout(handleClose, 1500)
    } catch (err: any) {
      setErrorApi(
        err?.response?.data?.error ??
        err?.response?.data?.detail ??
        'Error al registrar la venta.',
      )
    }
  }

  const isLoading = verificandoLotes || crearVenta.isPending

  return (
    <>
      <Dialog open={open && !pendienteConfirmacion} onClose={handleClose}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[560px] mx-4">
          <div className="flex items-center justify-between px-5 py-4 border-b border-border">
            <DialogHeader className="mb-0">
              <DialogTitle>Registrar venta</DialogTitle>
              <DialogDescription>
                Selecciona animales e ingresa los datos de la venta
              </DialogDescription>
            </DialogHeader>
            <button
              onClick={handleClose}
              className="text-muted-foreground hover:text-foreground ml-4"
            >
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
                <p className="text-xs text-muted-foreground">
                  Los animales fueron marcados como vendidos.
                </p>
              </div>
            ) : (
              <div className="space-y-4">
                {/* Datos generales */}
                <div className="grid grid-cols-2 gap-3">
                  <FormField label="Comprador" error={errors.compradorId?.message} required>
                    <CustomSelect
                      value={watch('compradorId') ?? ''}
                      onChange={v => setValue('compradorId', v)}
                      options={compradores}
                      placeholder="Seleccionar..."
                    />
                  </FormField>
                  <FormField label="Fecha" error={errors.fecha?.message} required>
                    <Input
                      {...register('fecha')}
                      type="date"
                      max={hoy}
                      className={errors.fecha ? 'border-destructive' : ''}
                    />
                  </FormField>
                </div>

                <div className="grid grid-cols-2 gap-3">
                  <FormField label="Moneda" required>
                    <Input {...register('moneda')} placeholder="COP" />
                  </FormField>
                  <FormField label="Precio por kg ($)" hint="Calcula el precio automáticamente">
                    <MoneyInput
                      min={0}
                      value={precioPorKg || ''}
                      onChange={e => {
                        const val = Number(e.target.value)
                        setPrecioPorKg(val)
                        setAnimalesRows(prev =>
                          prev.map(r => ({
                            ...r,
                            precioVenta: Math.round(val * r.pesoVentaKg),
                          })),
                        )
                      }}
                      className="flex h-9 w-full rounded-md border bg-transparent px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring border-input"
                      placeholder="Ej: 5500"
                    />
                  </FormField>
                  <FormField label="Descripción" className="col-span-2">
                    <Input
                      {...register('descripcion')}
                      placeholder="Notas de la venta"
                    />
                  </FormField>
                </div>

                {/* Selector de animales */}
                <div>
                  <div className="flex items-center justify-between mb-2">
                    <p className="text-[10px] font-semibold text-muted-foreground uppercase tracking-widest">
                      Animales a vender ({animalesRows.length})
                    </p>
                    {animalesDisponibles.length > 0 && (
                      <CustomSelect
                        value=""
                        onChange={v => { if (v) agregarAnimal(v) }}
                        options={animalesDisponibles.map(a => ({
                          value: a.id,
                          label: `${a.codigoIdentificacion}${a.nombre ? ` — ${a.nombre}` : ''} (${fmt.kg(a.pesoActualKg)})`,
                        }))}
                        placeholder="+ Agregar animal"
                        className="h-7 text-xs"
                      />
                    )}
                  </div>

                  {animalesRows.length === 0 ? (
                    <p className="text-xs text-muted-foreground text-center py-4 border border-dashed border-border rounded-lg">
                      {animalesDisponibles.length === 0
                        ? 'No hay animales en engorde disponibles'
                        : 'Agrega animales desde el selector'}
                    </p>
                  ) : (
                    <div className="space-y-2 max-h-52 overflow-y-auto">
                      {animalesRows.map(row => (
                        <div
                          key={row.animalId}
                          className="flex items-center gap-2 p-2 rounded-lg border bg-secondary/20 border-border/50"
                        >
                          <div className="flex-1 min-w-0">
                            <p className="text-xs font-medium truncate">
                              {row.codigo}
                              {row.nombre ? ` — ${row.nombre}` : ''}
                            </p>
                          </div>
                          <MoneyInput
                            placeholder="$ venta"
                            value={row.precioVenta || ''}
                            onChange={e =>
                              actualizarRow(row.animalId, 'precioVenta', Number(e.target.value))
                            }
                            className="w-24 h-7 px-2 rounded border border-input bg-card text-xs text-right"
                            min={0}
                            step={1000}
                          />
                          <input
                            type="number"
                            placeholder="kg"
                            value={row.pesoVentaKg || ''}
                            onChange={e =>
                              actualizarRow(row.animalId, 'pesoVentaKg', Number(e.target.value))
                            }
                            className="w-16 h-7 px-2 rounded border border-input bg-card text-xs text-right"
                            min={0}
                            step={0.1}
                          />
                          <button
                            onClick={() => quitarAnimal(row.animalId)}
                            className="p-1 rounded text-muted-foreground hover:text-destructive"
                          >
                            <Trash2 className="w-3 h-3" />
                          </button>
                        </div>
                      ))}
                    </div>
                  )}

                  {animalesRows.length > 0 && (
                    <div className="flex justify-between items-center mt-2 pt-2 border-t border-border">
                      <span className="text-xs text-muted-foreground">
                        {animalesRows.length} animal{animalesRows.length > 1 ? 'es' : ''}
                      </span>
                      <span className="text-sm font-semibold">
                        Total: {fmt.cop(montoTotal)}
                      </span>
                    </div>
                  )}
                </div>

                {errorApi && <Alert variant="destructive">{errorApi}</Alert>}

                <div className="flex gap-2 pt-1">
                  <Button
                    type="button"
                    variant="outline"
                    className="flex-1"
                    onClick={handleClose}
                    disabled={isLoading}
                  >
                    Cancelar
                  </Button>
                  <Button
                    type="button"
                    className="flex-1"
                    loading={isLoading}
                    onClick={iniciarVenta}
                  >
                    <ShoppingCart className="w-3.5 h-3.5" />
                    Continuar
                  </Button>
                </div>
              </div>
            )}
          </div>
        </div>
      </Dialog>

      {/* Modal de confirmación — aparece ENCIMA del modal de venta */}
      <ConfirmarRetiroLoteModal
        open={pendienteConfirmacion}
        animalesConLote={animalesConLote}
        onAceptar={() => ejecutarVenta()}
        onCancelar={() => setPendienteConfirmacion(false)}
      />
    </>
  )
}

// ─── Página principal ─────────────────────────────────────────────────────────
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
            {Array.from({ length: 4 }).map((_, i) => (
              <Skeleton key={i} className="h-16 rounded-lg" />
            ))}
          </div>
        ) : ventasArray.length === 0 ? (
          <EmptyState
            icon={<DollarSign className="w-5 h-5" />}
            title="Sin ventas"
            description="Al registrar una venta, los animales se marcan como vendidos automáticamente."
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
                        <th
                          key={h}
                          className="text-left px-4 py-3 text-muted-foreground font-medium uppercase tracking-wide text-[10px] whitespace-nowrap"
                        >
                          {h}
                        </th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {ventasArray.map((v, i) => (
                      <tr
                        key={v.id}
                        className={`border-b border-border/40 hover:bg-muted/20 transition-colors ${
                          i === ventasArray.length - 1 ? 'border-b-0' : ''
                        }`}
                      >
                        <td className="px-4 py-3 text-muted-foreground tabular-nums">
                          {fmt.fecha(v.fecha)}
                        </td>
                        <td className="px-4 py-2.5 font-medium">{v.nombreComprador}</td>
                        <td className="px-4 py-3">
                          <div className="flex flex-wrap gap-1 items-center">
                            <Badge className="bg-primary/10 text-primary border-primary/20 text-[10px]">
                              <Beef className="w-3 h-3 mr-1" />
                              {v.totalAnimales}
                            </Badge>
                            {v.items.slice(0, 3).map(item => (
                              <span
                                key={item.id}
                                className="text-[10px] text-muted-foreground font-mono"
                              >
                                {item.codigoAnimal}
                                {item.nombreAnimal ? ` (${item.nombreAnimal})` : ''}
                              </span>
                            ))}
                            {v.items.length > 3 && (
                              <span className="text-[10px] text-muted-foreground">
                                +{v.items.length - 3} más
                              </span>
                            )}
                          </div>
                        </td>
                        <td className="px-4 py-3 tabular-nums font-medium">
                          {fmt.cop(v.montoTotal)}
                        </td>
                        <td className="px-4 py-3 text-muted-foreground text-xs max-w-[200px] truncate">
                          {v.descripcion ?? '—'}
                        </td>
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
