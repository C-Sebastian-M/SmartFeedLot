import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Plus, ChevronDown, ChevronRight, CheckCircle2, Clock, DollarSign, X, Landmark, Pencil } from 'lucide-react'
import { useEtapasInversion, useCrearEtapaInversion, useAgregarItemInversion, useActualizarItemInversion } from '@/hooks/useFeedlot'
import {
  PageHeader, Card, CardHeader, CardTitle, CardContent,
  Skeleton, EmptyState, StatCard, Button,
  Dialog, DialogHeader, DialogTitle, DialogDescription,
  FormField, Alert,
  MoneyInput,
} from '@/components/ui'
import { fmt } from '@/utils'
import type { EtapaInversion, ItemInversion } from '@/types'

const etapaSchema = z.object({
  numero: z.coerce.number().int().min(1, 'Mín 1').max(5, 'Máx 5'),
  nombre: z.string().min(3, 'Mínimo 3 caracteres').max(200),
})
type EtapaForm = z.infer<typeof etapaSchema>

const itemSchema = z.object({
  producto: z.string().min(3, 'Mínimo 3 caracteres').max(300),
  monto: z.coerce.number().min(0, 'No negativo'),
  moneda: z.string().length(3).default('COP'),
  observacion: z.string().max(500).optional(),
  estado: z.enum(['OK', 'Pendiente']),
  porcentajeAvance: z.coerce.number().min(0).max(100),
})
type ItemForm = z.infer<typeof itemSchema>

function ItemRow({ item, onEditar }: { item: ItemInversion; onEditar: (item: ItemInversion) => void }) {
  return (
    <tr className="border-b border-border/30 hover:bg-muted/20 transition-colors group">
      <td className="px-4 py-2.5 text-xs text-muted-foreground w-8">{item.estado === 'OK' ? <CheckCircle2 className="w-3.5 h-3.5 text-emerald-400" /> : <Clock className="w-3.5 h-3.5 text-amber-400" />}</td>
      <td className="px-4 py-2.5 font-medium">{item.producto}</td>
      <td className="px-4 py-2.5 tabular-nums">{fmt.cop(item.monto)}</td>
      <td className="px-4 py-2.5">
        <div className="flex items-center gap-2">
          <div className="w-20 h-1.5 rounded-full bg-secondary">
            <div className={`h-full rounded-full transition-all ${item.porcentajeAvance >= 100 ? 'bg-emerald-400' : item.porcentajeAvance >= 50 ? 'bg-amber-400' : 'bg-muted-foreground/30'}`}
              style={{ width: `${item.porcentajeAvance}%` }} />
          </div>
          <span className="text-xs tabular-nums text-muted-foreground">{item.porcentajeAvance}%</span>
        </div>
      </td>
      <td className="px-4 py-2.5">
        <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-medium ${
          item.estado === 'OK'
            ? 'bg-emerald-500/10 text-emerald-400'
            : 'bg-amber-500/10 text-amber-400'
        }`}>{item.estado}</span>
      </td>
      <td className="px-4 py-2.5 text-xs text-muted-foreground max-w-[200px] truncate">{item.observacion ?? '—'}</td>
      <td className="px-4 py-2.5 w-8">
        <button onClick={() => onEditar(item)}
          className="opacity-0 group-hover:opacity-100 transition-opacity text-muted-foreground hover:text-foreground">
          <Pencil className="w-3.5 h-3.5" />
        </button>
      </td>
    </tr>
  )
}

function EtapaCard({ etapa, onAgregarItem, onEditarItem }: {
  etapa: EtapaInversion
  onAgregarItem: (etapaId: string) => void
  onEditarItem: (item: ItemInversion) => void
}) {
  const [expanded, setExpanded] = useState(false)
  const pendingItems = etapa.items.filter(i => i.estado === 'Pendiente')

  return (
    <Card>
      <CardHeader className="cursor-pointer select-none" onClick={() => setExpanded(!expanded)}>
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="w-8 h-8 rounded-lg bg-primary/10 flex items-center justify-center text-xs font-bold text-primary">{etapa.numero}</div>
            <div>
              <CardTitle className="text-sm">{etapa.nombre}</CardTitle>
              <p className="text-[10px] text-muted-foreground mt-0.5">
                {etapa.items.length} ítems · {pendingItems.length} pendientes
              </p>
            </div>
          </div>
          <div className="flex items-center gap-4">
            <div className="text-right">
              <p className="text-xs text-muted-foreground">Realizado</p>
              <p className="text-sm font-semibold text-emerald-400 tabular-nums">{fmt.cop(etapa.totalRealizadoMonto)}</p>
            </div>
            <div className="text-right">
              <p className="text-xs text-muted-foreground">Pendiente</p>
              <p className="text-sm font-semibold text-amber-400 tabular-nums">{fmt.cop(etapa.totalPendienteMonto)}</p>
            </div>
            {expanded ? <ChevronDown className="w-4 h-4 text-muted-foreground" /> : <ChevronRight className="w-4 h-4 text-muted-foreground" />}
          </div>
        </div>
      </CardHeader>
      {expanded && (
        <CardContent className="p-0 border-t border-border">
          <div className="overflow-x-auto">
            <table className="w-full text-xs">
              <thead>
                <tr className="border-b border-border bg-secondary/20">
                  {['', 'Producto', 'Costo', 'Avance', 'Estado', 'Observación', ''].map(h => (
                    <th key={h} className="text-left px-4 py-2.5 text-muted-foreground font-medium uppercase tracking-wide text-[9px]">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {etapa.items.map(item => <ItemRow key={item.id} item={item} onEditar={onEditarItem} />)}
              </tbody>
            </table>
          </div>
          <div className="px-4 py-2 border-t border-border flex gap-2">
            <Button size="sm" variant="ghost" onClick={() => onAgregarItem(etapa.id)}>
              <Plus className="w-3 h-3" />
              Agregar ítem
            </Button>
          </div>
        </CardContent>
      )}
    </Card>
  )
}

export default function InversionPage() {
  const [modalEtapa, setModalEtapa] = useState(false)
  const [modalItem, setModalItem] = useState(false)
  const [modalEditarItem, setModalEditarItem] = useState(false)
  const [itemEtapaId, setItemEtapaId] = useState<string | null>(null)
  const [itemEditando, setItemEditando] = useState<ItemInversion | null>(null)
  const [exito, setExito] = useState<string | null>(null)
  const [errorApi, setErrorApi] = useState<string>()

  const { data: etapas, isLoading } = useEtapasInversion()
  const etapasArray = (etapas as EtapaInversion[] | undefined) ?? []
  const crearEtapa = useCrearEtapaInversion()
  const agregarItem = useAgregarItemInversion()
  const actualizarItem = useActualizarItemInversion()

  const etapaForm = useForm<EtapaForm>({ resolver: zodResolver(etapaSchema), defaultValues: { numero: 1, nombre: '' } })
  const itemForm = useForm<ItemForm>({ resolver: zodResolver(itemSchema), defaultValues: { moneda: 'COP', estado: 'Pendiente', porcentajeAvance: 0 } })
  const editarForm = useForm<ItemForm>({ resolver: zodResolver(itemSchema), defaultValues: { moneda: 'COP', estado: 'Pendiente', porcentajeAvance: 0 } })

  const totalRealizado = etapasArray.reduce((s, e) => s + e.totalRealizadoMonto, 0)
  const totalPendiente = etapasArray.reduce((s, e) => s + e.totalPendienteMonto, 0)
  const totalItems = etapasArray.reduce((s, e) => s + e.items.length, 0)
  const totalPendientes = etapasArray.reduce((s, e) => s + e.items.filter(i => i.estado === 'Pendiente').length, 0)

  const onSubmitEtapa = async (data: EtapaForm) => {
    setErrorApi(undefined)
    try {
      await crearEtapa.mutateAsync(data)
      setExito(`Etapa "${data.nombre}" creada`)
      setModalEtapa(false)
      etapaForm.reset()
    } catch (err: any) {
      setErrorApi(err?.response?.data?.detail ?? 'Error al crear etapa')
    }
  }

  const onSubmitItem = async (data: ItemForm) => {
    if (!itemEtapaId) return
    setErrorApi(undefined)
    try {
      await agregarItem.mutateAsync({ ...data, etapaId: itemEtapaId })
      setExito(`Ítem "${data.producto}" agregado`)
      setModalItem(false)
      itemForm.reset()
    } catch (err: any) {
      setErrorApi(err?.response?.data?.detail ?? 'Error al agregar ítem')
    }
  }

  const abrirEditar = (item: ItemInversion) => {
    setItemEditando(item)
    editarForm.reset({
      producto: item.producto,
      monto: item.monto,
      moneda: item.moneda,
      observacion: item.observacion ?? '',
      estado: item.estado,
      porcentajeAvance: item.porcentajeAvance,
    })
    setModalEditarItem(true)
  }

  const onSubmitEditar = async (data: ItemForm) => {
    if (!itemEditando) return
    setErrorApi(undefined)
    try {
      await actualizarItem.mutateAsync({ itemId: itemEditando.id, ...data })
      setExito(`Ítem "${data.producto}" actualizado`)
      setModalEditarItem(false)
      setItemEditando(null)
    } catch (err: any) {
      setErrorApi(err?.response?.data?.detail ?? 'Error al actualizar ítem')
    }
  }

  return (
    <div className="flex flex-col h-full animate-fade-in">
      <PageHeader
        title="Planeación / Inversión"
        description="Etapas de inversión del proyecto, ítems con avance y aportes de socios"
        action={
          <Button size="sm" onClick={() => setModalEtapa(true)}>
            <Plus className="w-3.5 h-3.5" />
            Nueva etapa
          </Button>
        }
      />

      {/* KPIs */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 px-6 py-4 border-b border-border">
        <StatCard label="Etapas" value={etapasArray.length.toString()} icon={<Landmark className="w-4 h-4" />} />
        <StatCard label="Ítems totales" value={totalItems.toString()} icon={<DollarSign className="w-4 h-4" />} />
        <StatCard label="Realizado" value={fmt.cop(totalRealizado)} icon={<CheckCircle2 className="w-4 h-4 text-emerald-400" />} className="border-emerald-500/30" />
        <StatCard label="Pendiente" value={fmt.cop(totalPendiente)} sub={`${totalPendientes} ítems`} icon={<Clock className="w-4 h-4 text-amber-400" />} className="border-amber-500/30" />
      </div>

      {/* Lista de etapas */}
      <div className="flex-1 overflow-y-auto p-6">
        {isLoading ? (
          <div className="space-y-4">
            {[1, 2, 3].map(i => <Skeleton key={i} className="h-24 rounded-lg" />)}
          </div>
        ) : etapasArray.length === 0 ? (
          <EmptyState
            icon={<Landmark className="w-5 h-5" />}
            title="No hay etapas de inversión"
            description="Registra las etapas del proyecto con sus ítems y costos."
            action={
              <Button size="sm" onClick={() => setModalEtapa(true)}>
                <Plus className="w-3.5 h-3.5" />
                Crear primera etapa
              </Button>
            }
          />
        ) : (
          <div className="space-y-3">
            {etapasArray.map(etapa => (
              <EtapaCard
                key={etapa.id}
                etapa={etapa}
                onAgregarItem={(etapaId) => { setItemEtapaId(etapaId); setModalItem(true) }}
                onEditarItem={abrirEditar}
              />
            ))}
          </div>
        )}
      </div>

      {/* Modal crear etapa */}
      <Dialog open={modalEtapa} onClose={() => setModalEtapa(false)}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[420px] mx-4">
          <div className="flex items-center justify-between px-5 py-4 border-b border-border">
            <DialogHeader className="mb-0">
              <DialogTitle>Nueva etapa</DialogTitle>
              <DialogDescription>Define una etapa del plan de inversión</DialogDescription>
            </DialogHeader>
            <button onClick={() => setModalEtapa(false)} className="text-muted-foreground hover:text-foreground"><X className="w-4 h-4" /></button>
          </div>
          <form onSubmit={etapaForm.handleSubmit(onSubmitEtapa)} className="p-5 space-y-4">
            <FormField label="Número" error={etapaForm.formState.errors.numero?.message} required>
              <input type="number" min={1} max={5} {...etapaForm.register('numero')}
                className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
            </FormField>
            <FormField label="Nombre" error={etapaForm.formState.errors.nombre?.message} required>
              <input {...etapaForm.register('nombre')} placeholder="Ej: Adecuación potreros"
                className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
            </FormField>
            {errorApi && <Alert variant="destructive">{errorApi}</Alert>}
            <div className="flex gap-2 pt-1">
              <Button type="button" variant="outline" className="flex-1" onClick={() => setModalEtapa(false)}>Cancelar</Button>
              <Button type="submit" className="flex-1" loading={crearEtapa.isPending}>Crear etapa</Button>
            </div>
          </form>
        </div>
      </Dialog>

      {/* Modal agregar ítem */}
      <Dialog open={modalItem} onClose={() => setModalItem(false)}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[480px] mx-4">
          <div className="flex items-center justify-between px-5 py-4 border-b border-border">
            <DialogHeader className="mb-0">
              <DialogTitle>Nuevo ítem</DialogTitle>
              <DialogDescription>Agrega un producto/costo a la etapa</DialogDescription>
            </DialogHeader>
            <button onClick={() => setModalItem(false)} className="text-muted-foreground hover:text-foreground"><X className="w-4 h-4" /></button>
          </div>
          <form onSubmit={itemForm.handleSubmit(onSubmitItem)} className="p-5 space-y-4">
            <FormField label="Producto" error={itemForm.formState.errors.producto?.message} required>
              <input {...itemForm.register('producto')} placeholder="Ej: Cerca eléctrica"
                className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
            </FormField>
            <div className="grid grid-cols-2 gap-3">
              <FormField label="Costo" error={itemForm.formState.errors.monto?.message} required>
                <MoneyInput min={0} {...itemForm.register('monto')}
                  className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
              </FormField>
              <FormField label="Estado" error={itemForm.formState.errors.estado?.message} required>
                <select {...itemForm.register('estado')}
                  className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm">
                  <option value="Pendiente">Pendiente</option>
                  <option value="OK">OK</option>
                </select>
              </FormField>
            </div>
            <FormField label="Avance %" error={itemForm.formState.errors.porcentajeAvance?.message}>
              <input type="number" min={0} max={100} {...itemForm.register('porcentajeAvance')}
                className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
            </FormField>
            <FormField label="Observación" error={itemForm.formState.errors.observacion?.message}>
              <input {...itemForm.register('observacion')} placeholder="Detalles opcionales"
                className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
            </FormField>
            {errorApi && <Alert variant="destructive">{errorApi}</Alert>}
            <div className="flex gap-2 pt-1">
              <Button type="button" variant="outline" className="flex-1" onClick={() => setModalItem(false)}>Cancelar</Button>
              <Button type="submit" className="flex-1" loading={agregarItem.isPending}>Agregar ítem</Button>
            </div>
          </form>
        </div>
      </Dialog>

      {/* Modal editar ítem */}
      <Dialog open={modalEditarItem} onClose={() => setModalEditarItem(false)}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[480px] mx-4">
          <div className="flex items-center justify-between px-5 py-4 border-b border-border">
            <DialogHeader className="mb-0">
              <DialogTitle>Editar ítem</DialogTitle>
              <DialogDescription>Actualiza el estado, avance o costo del ítem</DialogDescription>
            </DialogHeader>
            <button onClick={() => setModalEditarItem(false)} className="text-muted-foreground hover:text-foreground"><X className="w-4 h-4" /></button>
          </div>
          <form onSubmit={editarForm.handleSubmit(onSubmitEditar)} className="p-5 space-y-4">
            <FormField label="Producto" error={editarForm.formState.errors.producto?.message} required>
              <input {...editarForm.register('producto')}
                className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
            </FormField>
            <div className="grid grid-cols-2 gap-3">
              <FormField label="Costo" error={editarForm.formState.errors.monto?.message} required>
                <MoneyInput min={0} {...editarForm.register('monto')}
                  className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
              </FormField>
              <FormField label="Estado" error={editarForm.formState.errors.estado?.message} required>
                <select {...editarForm.register('estado')}
                  className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm">
                  <option value="Pendiente">Pendiente</option>
                  <option value="OK">OK</option>
                </select>
              </FormField>
            </div>
            <FormField label="Avance %" error={editarForm.formState.errors.porcentajeAvance?.message}>
              <input type="number" min={0} max={100} {...editarForm.register('porcentajeAvance')}
                className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
            </FormField>
            <FormField label="Observación" error={editarForm.formState.errors.observacion?.message}>
              <input {...editarForm.register('observacion')}
                className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
            </FormField>
            {errorApi && <Alert variant="destructive">{errorApi}</Alert>}
            <div className="flex gap-2 pt-1">
              <Button type="button" variant="outline" className="flex-1" onClick={() => setModalEditarItem(false)}>Cancelar</Button>
              <Button type="submit" className="flex-1" loading={actualizarItem.isPending}>Guardar cambios</Button>
            </div>
          </form>
        </div>
      </Dialog>

      {/* Toast éxito */}
      {exito && (
        <div className="fixed bottom-4 right-4 z-50 bg-emerald-500/10 border border-emerald-500/30 text-emerald-400 px-4 py-3 rounded-lg text-sm animate-fade-in flex items-center gap-2 shadow-lg">
          <CheckCircle2 className="w-4 h-4" />
          {exito}
          <button onClick={() => setExito(null)} className="ml-2 hover:text-emerald-300"><X className="w-3.5 h-3.5" /></button>
        </div>
      )}
    </div>
  )
}
