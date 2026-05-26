import { useState } from 'react'
import { format, subDays } from 'date-fns'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import {
  DollarSign, Plus, X, CheckCircle2, ChevronDown, Beef,
  Wrench, FileText, CalendarDays, Users
} from 'lucide-react'
import { useLotes, useCostosTotalesLote, useRegistrarCostoOperativo } from '@/hooks/useFeedlot'
import {
  PageHeader, Card, CardHeader, CardTitle, CardContent,
  Skeleton, EmptyState, StatCard, Button,
  Dialog, DialogHeader, DialogTitle, DialogDescription,
  FormField, Input, Alert,
} from '@/components/ui'
import { fmt } from '@/utils'
import type { LoteResumen, CostoDetalle } from '@/types'

const hoy = format(new Date(), 'yyyy-MM-dd')
const hace30 = format(subDays(new Date(), 30), 'yyyy-MM-dd')

const registrarCostoSchema = z.object({
  categoria: z.enum(['ManoDeObra', 'CIF'], { required_error: 'Selecciona una categoría' }),
  concepto: z.string().min(3, 'Mínimo 3 caracteres').max(200, 'Máximo 200 caracteres'),
  fecha: z.string().min(1, 'La fecha es requerida'),
  monto: z.number({ invalid_type_error: 'Ingresa un número' }).positive('Debe ser mayor a cero'),
  moneda: z.string().length(3, 'Código ISO de 3 caracteres').default('COP'),
  observaciones: z.string().max(500, 'Máximo 500 caracteres').optional(),
})
type RegistrarCostoForm = z.infer<typeof registrarCostoSchema>

function SelectorLote({
  lotes, value, onChange,
}: {
  lotes: LoteResumen[]
  value: string
  onChange: (id: string) => void
}) {
  return (
    <div className="relative">
      <select
        value={value}
        onChange={e => onChange(e.target.value)}
        className="h-9 pl-3 pr-8 rounded-md border border-input bg-card text-sm
          focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring
          appearance-none cursor-pointer [&>option]:bg-card"
      >
        <option value="">Seleccionar lote...</option>
        {lotes.map(l => (
          <option key={l.id} value={l.id}>
            {l.codigo} — {l.nombre} ({l.animalesActuales} animales)
          </option>
        ))}
      </select>
      <ChevronDown className="absolute right-2 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-muted-foreground pointer-events-none" />
    </div>
  )
}

function RegistrarCostoModal({
  open, onClose, loteId, lotes,
}: {
  open: boolean
  onClose: () => void
  loteId: string
  lotes: LoteResumen[]
}) {
  const [exito, setExito] = useState(false)
  const [errorApi, setErrorApi] = useState<string>()
  const registrarCosto = useRegistrarCostoOperativo()
  const lote = lotes.find(l => l.id === loteId)

  const { register, handleSubmit, reset, watch, setValue, formState: { errors, isSubmitting } } =
    useForm<RegistrarCostoForm>({
      resolver: zodResolver(registrarCostoSchema),
      defaultValues: { moneda: 'COP', fecha: hoy },
    })

  const categoria = watch('categoria')

  const handleClose = () => { reset(); setExito(false); setErrorApi(undefined); onClose() }

  const onSubmit = async (data: RegistrarCostoForm) => {
    setErrorApi(undefined)
    try {
      await registrarCosto.mutateAsync({
        loteId,
        categoria: data.categoria,
        concepto: data.concepto,
        fecha: data.fecha,
        monto: data.monto,
        moneda: data.moneda,
        observaciones: data.observaciones || undefined,
        registradoPorId: '00000000-0000-0000-0000-000000000000',
      })
      setExito(true)
      setTimeout(() => handleClose(), 1500)
    } catch (err: any) {
      setErrorApi(err?.response?.data?.error ?? err?.response?.data?.detail ?? 'Error al registrar el costo.')
    }
  }

  return (
    <Dialog open={open} onClose={handleClose}>
      <div className="rounded-xl border border-border bg-card shadow-xl w-[480px]">
        <div className="flex items-center justify-between px-5 py-4 border-b border-border">
          <DialogHeader className="mb-0">
            <DialogTitle>Registrar costo operativo</DialogTitle>
            <DialogDescription>
              {lote ? `${lote.codigo} — ${lote.nombre}` : 'Selecciona un lote'}
            </DialogDescription>
          </DialogHeader>
          <button onClick={handleClose} className="text-muted-foreground hover:text-foreground ml-4">
            <X className="w-4 h-4" />
          </button>
        </div>

        <div className="p-5">
          {exito ? (
            <div className="flex flex-col items-center py-6 gap-3 animate-fade-in">
              <div className="w-12 h-12 rounded-full bg-emerald-500/10 flex items-center justify-center">
                <CheckCircle2 className="w-6 h-6 text-emerald-400" />
              </div>
              <p className="text-sm font-medium">¡Costo registrado!</p>
              <p className="text-xs text-muted-foreground">El costo se prorrateará entre los animales del lote.</p>
            </div>
          ) : (
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              <FormField label="Categoría" error={errors.categoria?.message} required>
                <div className="flex gap-2">
                  {(['ManoDeObra', 'CIF'] as const).map(cat => (
                    <button
                      key={cat}
                      type="button"
                      onClick={() => setValue('categoria', cat)}
                      className={`flex-1 flex items-center justify-center gap-2 h-9 rounded-md border text-sm font-medium transition-colors ${
                        categoria === cat
                          ? 'border-primary bg-primary/10 text-primary'
                          : 'border-input text-muted-foreground hover:border-border hover:text-foreground'
                      }`}
                    >
                      {cat === 'ManoDeObra' ? <Wrench className="w-3.5 h-3.5" /> : <FileText className="w-3.5 h-3.5" />}
                      {cat === 'ManoDeObra' ? 'Mano de obra' : 'CIF'}
                    </button>
                  ))}
                </div>
              </FormField>

              <FormField label="Concepto" error={errors.concepto?.message} required
                hint="Ej: Suministrar alimentación, Gasolina moto bomba">
                <Input {...register('concepto')} placeholder="Describe el concepto del costo"
                  className={errors.concepto ? 'border-destructive' : ''} />
              </FormField>

              <div className="grid grid-cols-2 gap-3">
                <FormField label="Fecha" error={errors.fecha?.message} required>
                  <Input {...register('fecha')} type="date" max={hoy}
                    className={errors.fecha ? 'border-destructive' : ''} />
                </FormField>
                <FormField label="Monto" error={errors.monto?.message} required>
                  <Input {...register('monto', { valueAsNumber: true })}
                    type="number" min={1} step={100} placeholder="0"
                    className={errors.monto ? 'border-destructive' : ''} />
                </FormField>
              </div>

              <FormField label="Observaciones" error={errors.observaciones?.message}
                hint="Opcional">
                <Input {...register('observaciones')} placeholder="Detalles adicionales"
                  className={errors.observaciones ? 'border-destructive' : ''} />
              </FormField>

              {errorApi && <Alert variant="destructive">{errorApi}</Alert>}

              <div className="flex gap-2 pt-1">
                <Button type="button" variant="outline" className="flex-1" onClick={handleClose} disabled={isSubmitting}>
                  Cancelar
                </Button>
                <Button type="submit" className="flex-1" loading={isSubmitting}>
                  <DollarSign className="w-3.5 h-3.5" />
                  Registrar costo
                </Button>
              </div>
            </form>
          )}
        </div>
      </div>
    </Dialog>
  )
}

function TablaDetalles({ detalles, titulo }: { detalles: CostoDetalle[]; titulo: string }) {
  if (!detalles.length) return null

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle>{titulo}</CardTitle>
          <span className="text-xs text-muted-foreground">{detalles.length} registro{detalles.length > 1 ? 's' : ''}</span>
        </div>
      </CardHeader>
      <CardContent className="p-0">
        <div className="overflow-x-auto">
          <table className="w-full text-xs">
            <thead>
              <tr className="border-b border-border">
                {['Concepto', 'Fecha', 'Monto', 'Observaciones'].map(h => (
                  <th key={h} className="text-left px-4 py-3 text-muted-foreground font-medium uppercase tracking-wide text-[10px] whitespace-nowrap">
                    {h}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {detalles.map((d, i) => (
                <tr key={d.id}
                  className={`border-b border-border/40 hover:bg-secondary/30 transition-colors ${
                    i === detalles.length - 1 ? 'border-b-0' : ''
                  }`}
                >
                  <td className="px-4 py-2.5 font-medium">{d.concepto}</td>
                  <td className="px-4 py-2.5 text-muted-foreground tabular-nums">{fmt.fecha(d.fecha)}</td>
                  <td className="px-4 py-2.5 tabular-nums font-medium">{fmt.cop(d.monto)}</td>
                  <td className="px-4 py-2.5 text-muted-foreground">{d.observaciones ?? '—'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </CardContent>
    </Card>
  )
}

export default function CostosPage() {
  const [loteId, setLoteId] = useState('')
  const [desde, setDesde] = useState(hace30)
  const [hasta, setHasta] = useState(hoy)
  const [modalAbierto, setModalAbierto] = useState(false)

  const { data: lotes, isLoading: loadingLotes } = useLotes()
  const lotesArray = (lotes as LoteResumen[] | undefined) ?? []

  const { data: costeo, isLoading: loadingCosteo } = useCostosTotalesLote({
    loteId,
    desde,
    hasta,
  })

  return (
    <div className="flex flex-col h-full animate-fade-in">
      <PageHeader
        title="Costos operativos"
        description="Registro y consulta de costos de mano de obra y CIF por lote"
        action={
          <Button size="sm" onClick={() => setModalAbierto(true)} disabled={!loteId}>
            <Plus className="w-3.5 h-3.5" />
            Registrar costo
          </Button>
        }
      />

      {/* Controles */}
      <div className="flex items-center gap-3 px-6 py-3 border-b border-border flex-wrap">
        {loadingLotes ? (
          <Skeleton className="h-9 w-64" />
        ) : (
          <SelectorLote lotes={lotesArray} value={loteId} onChange={setLoteId} />
        )}

        <div className="flex items-center gap-2">
          <span className="text-xs text-muted-foreground">Desde</span>
          <input
            type="date"
            value={desde}
            max={hasta}
            onChange={e => setDesde(e.target.value)}
            className="h-9 px-3 rounded-md border border-input bg-card text-sm
              focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
          />
          <span className="text-xs text-muted-foreground">hasta</span>
          <input
            type="date"
            value={hasta}
            min={desde}
            max={hoy}
            onChange={e => setHasta(e.target.value)}
            className="h-9 px-3 rounded-md border border-input bg-card text-sm
              focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
          />
        </div>
      </div>

      {/* Contenido */}
      <div className="flex-1 overflow-y-auto p-6">
        {!loteId ? (
          <EmptyState
            icon={<DollarSign className="w-5 h-5" />}
            title="Selecciona un lote"
            description="Elige un lote para ver su desglose de costos operativos en el período seleccionado."
          />
        ) : loadingCosteo ? (
          <div className="space-y-4">
            <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
              {Array.from({ length: 4 }).map((_, i) => <Skeleton key={i} className="h-24 rounded-lg" />)}
            </div>
            <Skeleton className="h-48 rounded-lg" />
          </div>
        ) : !costeo ? (
          <EmptyState
            icon={<DollarSign className="w-5 h-5" />}
            title="Sin datos de costos"
            description="No hay costos registrados para este lote en el período seleccionado."
            action={
              <Button size="sm" onClick={() => setModalAbierto(true)}>
                <Plus className="w-3.5 h-3.5" />
                Registrar primer costo
              </Button>
            }
          />
        ) : (
          <div className="space-y-6">
            {/* KPIs */}
            <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
              <StatCard
                label="Alimento"
                value={fmt.cop(costeo.costoTotalAlimento)}
                sub={`${fmt.cop(costeo.costoAlimentoPorAnimal)}/animal`}
                icon={<Beef className="w-4 h-4" />}
              />
              <StatCard
                label="Mano de obra"
                value={fmt.cop(costeo.costoTotalManoDeObra)}
                sub={`${fmt.cop(costeo.costoManoDeObraPorAnimal)}/animal`}
                icon={<Wrench className="w-4 h-4" />}
              />
              <StatCard
                label="CIF"
                value={fmt.cop(costeo.costoTotalCif)}
                sub={`${fmt.cop(costeo.costoCifPorAnimal)}/animal`}
                icon={<FileText className="w-4 h-4" />}
              />
              <StatCard
                label="Costo total operativo"
                value={fmt.cop(costeo.costoOperativoTotal)}
                sub={`${fmt.cop(costeo.costoOperativoPorAnimal)}/animal · ${costeo.totalAnimales} animales`}
                icon={<DollarSign className="w-4 h-4" />}
                className="border-primary/30"
              />
            </div>

            {/* Consumo */}
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
              <Card className="p-5">
                <div className="flex items-center gap-2 text-muted-foreground mb-1">
                  <Users className="w-3.5 h-3.5" />
                  <p className="text-[10px] font-medium uppercase tracking-wide">Animales en lote</p>
                </div>
                <p className="text-xl font-bold tabular-nums">{costeo.totalAnimales}</p>
              </Card>
              <Card className="p-5">
                <div className="flex items-center gap-2 text-muted-foreground mb-1">
                  <CalendarDays className="w-3.5 h-3.5" />
                  <p className="text-[10px] font-medium uppercase tracking-wide">Período</p>
                </div>
                <p className="text-sm font-medium tabular-nums">
                  {fmt.fecha(costeo.desde)} — {fmt.fecha(costeo.hasta)}
                </p>
              </Card>
              <Card className="p-5">
                <div className="flex items-center gap-2 text-muted-foreground mb-1">
                  <Beef className="w-3.5 h-3.5" />
                  <p className="text-[10px] font-medium uppercase tracking-wide">Consumo total alimento</p>
                </div>
                <p className="text-xl font-bold tabular-nums">{fmt.kg(costeo.consumoTotalKg)}</p>
              </Card>
            </div>

            {/* Detalle Mano de Obra */}
            {costeo.detallesManoDeObra.length > 0 && (
              <TablaDetalles detalles={costeo.detallesManoDeObra} titulo="Detalle — Mano de obra" />
            )}

            {/* Detalle CIF */}
            {costeo.detallesCif.length > 0 && (
              <TablaDetalles detalles={costeo.detallesCif} titulo="Detalle — CIF" />
            )}
          </div>
        )}
      </div>

      <RegistrarCostoModal open={modalAbierto} onClose={() => setModalAbierto(false)} loteId={loteId} lotes={lotesArray} />
    </div>
  )
}
