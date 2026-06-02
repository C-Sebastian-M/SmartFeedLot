import { useState } from 'react'
import { format } from 'date-fns'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Plus, X, ShoppingCart, CheckCircle2, ChevronDown, Beef, Package } from 'lucide-react'
import { useCompras, useCrearCompra, useProveedores, useLotes } from '@/hooks/useFeedlot'
import {
  PageHeader, Card, CardContent,
  Skeleton, EmptyState, Button,
  Dialog, DialogHeader, DialogTitle, DialogDescription,
  FormField, Input, Alert,
  MoneyInput, CustomSelect,
} from '@/components/ui'
import { fmt } from '@/utils'
import type { Compra, Proveedor, LoteResumen } from '@/types'

const hoy = format(new Date(), 'yyyy-MM-dd')

const compraSchema = z.object({
  proveedorId: z.string().min(1, 'Selecciona un proveedor'),
  fecha: z.string().min(1, 'La fecha es requerida'),
  tipoCompra: z.enum(['Ganado', 'Insumo']),
  costoTotal: z.number({ invalid_type_error: 'Ingresa un número' }).positive('Debe ser mayor a cero'),
  moneda: z.string().length(3, 'Código ISO de 3 caracteres').default('COP'),
  descripcion: z.string().max(500, 'Máximo 500 caracteres').optional().or(z.literal('')),
  cantidadCabezas: z.number().positive().optional().or(z.nan()).or(z.literal(undefined)),
  precioPorCabeza: z.number().positive().optional().or(z.nan()).or(z.literal(undefined)),
  pesoPromedioKg: z.number().positive().optional().or(z.nan()).or(z.literal(undefined)),
  loteId: z.string().optional().or(z.literal('')),
  tipoInsumo: z.string().max(30).optional().or(z.literal('')),
  cantidadInsumo: z.number().positive().optional().or(z.nan()).or(z.literal(undefined)),
  unidadMedida: z.string().max(20).optional().or(z.literal('')),
})
type CompraForm = z.infer<typeof compraSchema>



function CrearCompraModal({ open, onClose }: { open: boolean; onClose: () => void }) {
  const [exito, setExito] = useState(false)
  const [errorApi, setErrorApi] = useState<string>()
  const crearCompra = useCrearCompra()
  const { data: proveedores } = useProveedores()
  const { data: lotes } = useLotes(true)

  const proveedoresList = ((proveedores as Proveedor[] | undefined) ?? []).map(p => ({
    id: p.id, label: p.nombre,
  }))
  const lotesList = ((lotes as LoteResumen[] | undefined) ?? []).map(l => ({
    id: l.id, label: `${l.codigo} — ${l.nombre} (${l.animalesActuales} anim.)`,
  }))

  const { register, handleSubmit, reset, watch, setValue, formState: { errors, isSubmitting } } =
    useForm<CompraForm>({
      resolver: zodResolver(compraSchema),
      defaultValues: { moneda: 'COP', fecha: hoy, tipoCompra: 'Ganado' },
    })

  const tipoCompra = watch('tipoCompra')

  const handleClose = () => { reset(); setExito(false); setErrorApi(undefined); onClose() }

  const onSubmit = async (data: CompraForm) => {
    setErrorApi(undefined)
    try {
      const payload: any = {
        proveedorId: data.proveedorId,
        fecha: data.fecha,
        tipoCompra: data.tipoCompra,
        costoTotal: data.costoTotal,
        moneda: data.moneda,
        descripcion: data.descripcion || undefined,
      }
      if (data.tipoCompra === 'Ganado') {
        payload.cantidadCabezas = data.cantidadCabezas
        payload.precioPorCabeza = data.precioPorCabeza
        payload.pesoPromedioKg = data.pesoPromedioKg
        payload.loteId = data.loteId || undefined
      } else {
        payload.tipoInsumo = data.tipoInsumo || undefined
        payload.cantidadInsumo = data.cantidadInsumo
        payload.unidadMedida = data.unidadMedida || undefined
      }
      await crearCompra.mutateAsync(payload)
      setExito(true)
      setTimeout(() => handleClose(), 1500)
    } catch (err: any) {
      setErrorApi(err?.response?.data?.error ?? err?.response?.data?.detail ?? 'Error al registrar la compra.')
    }
  }

  return (
    <Dialog open={open} onClose={handleClose}>
      <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[520px] mx-4">
        <div className="flex items-center justify-between px-5 py-4 border-b border-border">
          <DialogHeader className="mb-0">
            <DialogTitle>Registrar compra</DialogTitle>
            <DialogDescription>Ingresa los detalles de la compra de ganado o insumos</DialogDescription>
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
              <p className="text-sm font-medium">¡Compra registrada!</p>
            </div>
          ) : (
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              <FormField label="Proveedor" error={errors.proveedorId?.message} required>
                <CustomSelect
                  value={watch('proveedorId') ?? ''}
                  onChange={v => setValue('proveedorId', v)}
                  options={proveedoresList.map(p => ({ value: p.id, label: p.label }))}
                  placeholder="Seleccionar proveedor..."
                />
              </FormField>

              <div className="grid grid-cols-2 gap-3">
                <FormField label="Fecha" error={errors.fecha?.message} required>
                  <Input {...register('fecha')} type="date" max={hoy}
                    className={errors.fecha ? 'border-destructive' : ''} />
                </FormField>
                <FormField label="Tipo de compra" error={errors.tipoCompra?.message} required>
                  <div className="flex gap-2">
                    {(['Ganado', 'Insumo'] as const).map(t => (
                      <button
                        key={t}
                        type="button"
                        onClick={() => setValue('tipoCompra', t)}
                        className={`flex-1 flex items-center justify-center gap-2 h-9 rounded-md border text-sm font-medium transition-colors ${
                          tipoCompra === t
                            ? 'border-primary bg-primary/10 text-primary'
                            : 'border-input text-muted-foreground hover:border-border hover:text-foreground'
                        }`}
                      >
                        {t === 'Ganado' ? <Beef className="w-3.5 h-3.5" /> : <Package className="w-3.5 h-3.5" />}
                        {t}
                      </button>
                    ))}
                  </div>
                </FormField>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <FormField label="Costo total" error={errors.costoTotal?.message} required>
                  <MoneyInput {...register('costoTotal')}
                    min={1} placeholder="0"
                    className={errors.costoTotal ? 'border-destructive' : ''} />
                </FormField>
                <FormField label="Moneda" error={errors.moneda?.message} required>
                  <Input {...register('moneda')} placeholder="COP"
                    className={errors.moneda ? 'border-destructive' : ''} />
                </FormField>
              </div>

              <FormField label="Descripción" error={errors.descripcion?.message}>
                <Input {...register('descripcion')} placeholder="Notas sobre la compra"
                  className={errors.descripcion ? 'border-destructive' : ''} />
              </FormField>

              {tipoCompra === 'Ganado' && (
                <>
                  <div className="grid grid-cols-3 gap-3">
                    <FormField label="Cant. cabezas">
                      <Input {...register('cantidadCabezas', { valueAsNumber: true })}
                        type="number" min={1} placeholder="0" />
                    </FormField>
                    <FormField label="$ / cabeza">
                      <MoneyInput {...register('precioPorCabeza')}
                        min={0} placeholder="0" />
                    </FormField>
                    <FormField label="Peso prom. (kg)">
                      <Input {...register('pesoPromedioKg', { valueAsNumber: true })}
                        type="number" min={0} step={0.1} placeholder="0" />
                    </FormField>
                  </div>
                  <FormField label="Lote destino">
                    <CustomSelect
                      value={watch('loteId') ?? ''}
                      onChange={v => setValue('loteId', v)}
                      options={lotesList.map(l => ({ value: l.id, label: l.label }))}
                      placeholder="Seleccionar lote..."
                    />
                  </FormField>
                </>
              )}

              {tipoCompra === 'Insumo' && (
                <div className="grid grid-cols-3 gap-3">
                  <FormField label="Tipo insumo">
                    <Input {...register('tipoInsumo')} placeholder="Ej: Vacuna" />
                  </FormField>
                  <FormField label="Cantidad">
                    <Input {...register('cantidadInsumo', { valueAsNumber: true })}
                      type="number" min={0} step={0.1} placeholder="0" />
                  </FormField>
                  <FormField label="Unidad">
                    <Input {...register('unidadMedida')} placeholder="Ej: L, kg" />
                  </FormField>
                </div>
              )}

              {errorApi && <Alert variant="destructive">{errorApi}</Alert>}

              <div className="flex gap-2 pt-1">
                <Button type="button" variant="outline" className="flex-1" onClick={handleClose} disabled={isSubmitting}>
                  Cancelar
                </Button>
                <Button type="submit" className="flex-1" loading={isSubmitting}>
                  <ShoppingCart className="w-3.5 h-3.5" />
                  Registrar compra
                </Button>
              </div>
            </form>
          )}
        </div>
      </div>
    </Dialog>
  )
}

export default function ComprasPage() {
  const [modalAbierto, setModalAbierto] = useState(false)
  const { data: compras, isLoading } = useCompras()
  const comprasArray = (compras as Compra[] | undefined) ?? []

  return (
    <div className="flex flex-col h-full animate-fade-in">
      <PageHeader
        title="Compras"
        description="Registro de compras de ganado e insumos"
        action={
          <Button size="sm" onClick={() => setModalAbierto(true)}>
            <Plus className="w-3.5 h-3.5" />
            Nueva compra
          </Button>
        }
      />

      <div className="flex-1 overflow-y-auto p-6">
        {isLoading ? (
          <div className="space-y-3">
            {Array.from({ length: 4 }).map((_, i) => <Skeleton key={i} className="h-16 rounded-lg" />)}
          </div>
        ) : comprasArray.length === 0 ? (
          <EmptyState
            icon={<ShoppingCart className="w-5 h-5" />}
            title="Sin compras"
            description="Aún no hay compras registradas."
            action={
              <Button size="sm" onClick={() => setModalAbierto(true)}>
                <Plus className="w-3.5 h-3.5" />
                Registrar primera compra
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
                      {['Fecha', 'Proveedor', 'Tipo', 'Detalle', 'Total', 'Descripción'].map(h => (
                        <th key={h} className="text-left px-4 py-3 text-muted-foreground font-medium uppercase tracking-wide text-[10px] whitespace-nowrap">
                          {h}
                        </th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {comprasArray.map((c, i) => (
                      <tr key={c.id}
                        className={`border-b border-border/40 hover:bg-muted/20 transition-colors ${i === comprasArray.length - 1 ? 'border-b-0' : ''}`}
                      >
                        <td className="px-4 py-3 text-muted-foreground tabular-nums">{fmt.fecha(c.fecha)}</td>
                        <td className="px-4 py-2.5 font-medium">{c.nombreProveedor}</td>
                        <td className="px-4 py-3">
                          <span className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-semibold uppercase tracking-wider ${
                            c.tipoCompra === 'Ganado'
                              ? 'bg-emerald-500/10 text-emerald-400'
                              : 'bg-blue-500/10 text-blue-400'
                          }`}>
                            {c.tipoCompra === 'Ganado' ? <Beef className="w-3 h-3" /> : <Package className="w-3 h-3" />}
                            {c.tipoCompra}
                          </span>
                        </td>
                        <td className="px-4 py-3 text-muted-foreground text-xs">
                          {c.tipoCompra === 'Ganado'
                            ? `${c.cantidadCabezas ?? '?'} cab. · ${fmt.kg(c.pesoPromedioKg ?? 0)}/cab`
                            : `${c.cantidadInsumo ?? '?'} ${c.unidadMedida ?? ''} ${c.tipoInsumo ?? ''}`
                          }
                        </td>
                        <td className="px-4 py-3 tabular-nums font-medium">{fmt.cop(c.costoTotal)}</td>
                        <td className="px-4 py-3 text-muted-foreground text-xs max-w-[200px] truncate">{c.descripcion ?? '—'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </CardContent>
          </Card>
        )}
      </div>

      <CrearCompraModal open={modalAbierto} onClose={() => setModalAbierto(false)} />
    </div>
  )
}
