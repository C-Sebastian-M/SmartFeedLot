import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Plus, X, PiggyBank, ChevronDown, ChevronRight, TrendingUp, Trash2, ArrowRight } from 'lucide-react'
import {
  useMarranas, useCrearMarrana, useRegistrarCamada,
  useLotesCerdos, useCrearLoteCerdos, useRegistrarVentaLoteCerdos,
  useEliminarMarrana, useAvanzarEstadoCamada,
} from '@/hooks/useFeedlot'
import {
  PageHeader, Card, CardHeader, CardTitle, CardContent,
  Skeleton, EmptyState, StatCard, Button,
  Dialog, DialogHeader, DialogTitle,
  FormField, MoneyInput, Alert, Badge,
} from '@/components/ui'
import { fmt } from '@/utils'
import type { Marrana, LoteCerdos } from '@/types'

type Tab = 'marranas' | 'lotes'
const tabs: { key: Tab; label: string }[] = [
  { key: 'marranas', label: 'Marranas' },
  { key: 'lotes', label: 'Lotes de cerdos' },
]

type ModalState =
  | { type: null }
  | { type: 'marrana' }
  | { type: 'camada'; marranaId: string }
  | { type: 'lote' }
  | { type: 'venta'; loteId: string }
  | { type: 'confirmarEliminar'; marranaId: string; nombre: string }
  | { type: 'avanzarEstado'; marranaId: string; camadaId: string; estadoActual: string }

const estadoColor: Record<string, string> = {
  Preceba: 'bg-amber-500/10 text-amber-400 border-amber-500/20',
  Ceba: 'bg-blue-500/10 text-blue-400 border-blue-500/20',
  Vendida: 'bg-emerald-500/10 text-emerald-400 border-emerald-500/20',
}

// ─── Marranas section ─────────────────────────────────────────────────────────
function MarranasSection() {
  const { data: marranas, isLoading } = useMarranas()
  const arr = (marranas as Marrana[] | undefined) ?? []
  const crearMarrana = useCrearMarrana()
  const registrarCamada = useRegistrarCamada()
  const eliminarMarrana = useEliminarMarrana()
  const avanzarEstadoCamada = useAvanzarEstadoCamada()
  const [modal, setModal] = useState<ModalState>({ type: null })
  const [expanded, setExpanded] = useState<string | null>(null)
  const [errorApi, setErrorApi] = useState<string>()

  const mForm = useForm<{ identificacion: string; fechaCompra: string; costo: string }>({
    defaultValues: { identificacion: '', fechaCompra: new Date().toISOString().split('T')[0], costo: '' },
  })
  const cForm = useForm<{ fechaNacimiento: string; nLechones: number }>({
    defaultValues: { fechaNacimiento: new Date().toISOString().split('T')[0], nLechones: 0 },
  })

  const totalCamadas = arr.reduce((s, m) => s + m.camadas.length, 0)
  const preceba = arr.reduce((s, m) => s + m.camadas.filter(c => c.estado === 'Preceba').reduce((a, c) => a + c.nLechones, 0), 0)
  const ceba = arr.reduce((s, m) => s + m.camadas.filter(c => c.estado === 'Ceba').reduce((a, c) => a + c.nLechones, 0), 0)

  if (isLoading) return <div className="space-y-3"><Skeleton className="h-20" /><Skeleton className="h-20" /></div>

  return (
    <>
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 mb-4">
        <StatCard label="Marranas" value={String(arr.length)} icon={<PiggyBank className="w-4 h-4" />} />
        <StatCard label="Camadas" value={String(totalCamadas)} icon={<PiggyBank className="w-4 h-4" />} />
        <StatCard label="Lechones preceba" value={String(preceba)} icon={<PiggyBank className="w-4 h-4 text-amber-400" />} className="border-amber-500/20" />
        <StatCard label="Lechones en ceba" value={String(ceba)} icon={<PiggyBank className="w-4 h-4 text-blue-400" />} className="border-blue-500/20" />
      </div>

      {arr.length === 0 ? (
        <EmptyState icon={<PiggyBank className="w-5 h-5" />} title="Sin marranas"
          description="Registra las marranas reproductoras."
          action={<Button size="sm" onClick={() => setModal({ type: 'marrana' })}><Plus className="w-3.5 h-3.5" />Nueva marrana</Button>} />
      ) : (
        <div className="space-y-3">
          <div className="flex justify-end">
            <Button size="sm" variant="outline" onClick={() => setModal({ type: 'marrana' })}><Plus className="w-3 h-3" />Nueva marrana</Button>
          </div>
          {arr.map(m => (
            <Card key={m.id}>
              <CardHeader className="cursor-pointer select-none" onClick={() => setExpanded(expanded === m.id ? null : m.id)}>
                <div className="flex items-center justify-between">
                  <div>
                    <CardTitle className="text-sm">{m.identificacion}</CardTitle>
                    <p className="text-[10px] text-muted-foreground mt-0.5">
                      {fmt.fecha(m.fechaCompra)} · {fmt.cop(m.costoMonto)} · {m.camadas.length} camadas
                    </p>
                  </div>
                  <div className="flex items-center gap-2">
                    <Button size="sm" variant="ghost" onClick={e => { e.stopPropagation(); setModal({ type: 'camada', marranaId: m.id }) }}>
                      <Plus className="w-3 h-3" /> Camada
                    </Button>
                    <Button size="sm" variant="ghost" className="text-rose-400 hover:text-rose-300" onClick={e => { e.stopPropagation(); setModal({ type: 'confirmarEliminar', marranaId: m.id, nombre: m.identificacion }) }}>
                      <Trash2 className="w-3 h-3" />
                    </Button>
                    {expanded === m.id ? <ChevronDown className="w-4 h-4 text-muted-foreground" /> : <ChevronRight className="w-4 h-4 text-muted-foreground" />}
                  </div>
                </div>
              </CardHeader>
              {expanded === m.id && (
                <CardContent className="p-0 border-t border-border">
                  {m.camadas.length === 0 ? (
                    <p className="text-xs text-muted-foreground px-4 py-3">Sin camadas registradas.</p>
                  ) : (
                    <table className="w-full text-xs">
                      <thead>
                        <tr className="border-b border-border bg-secondary/20">
                          <th className="text-left px-4 py-2 text-muted-foreground">Fecha nacimiento</th>
                          <th className="text-right px-4 py-2 text-muted-foreground">Lechones</th>
                          <th className="text-center px-4 py-2 text-muted-foreground">Estado</th>
                          <th className="px-4 py-2" />
                        </tr>
                      </thead>
                      <tbody>
                        {m.camadas.map(c => (
                          <tr key={c.id} className="border-b border-border/30 hover:bg-secondary/20">
                            <td className="px-4 py-2">{fmt.fecha(c.fechaNacimiento)}</td>
                            <td className="px-4 py-2 text-right tabular-nums">{c.nLechones}</td>
                            <td className="px-4 py-2 text-center">
                              <Badge className={`text-[9px] ${estadoColor[c.estado] ?? ''}`}>{c.estado}</Badge>
                            </td>
                            <td className="px-4 py-2 text-right">
                              {c.estado !== 'Vendida' && (
                                <Button size="sm" variant="ghost" className="h-6 px-2 text-[10px] gap-1"
                                  onClick={() => setModal({ type: 'avanzarEstado', marranaId: m.id, camadaId: c.id, estadoActual: c.estado })}>
                                  <ArrowRight className="w-3 h-3" />
                                  {c.estado === 'Preceba' ? 'Pasar a Ceba' : 'Marcar vendida'}
                                </Button>
                              )}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  )}
                </CardContent>
              )}
            </Card>
          ))}
        </div>
      )}

      {/* Modal nueva marrana */}
      <Dialog open={modal.type === 'marrana'} onClose={() => { setModal({ type: null }); mForm.reset(); setErrorApi(undefined) }}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[400px] mx-4">
          <div className="flex items-center justify-between px-5 py-4 border-b border-border">
            <DialogHeader className="mb-0"><DialogTitle>Nueva marrana</DialogTitle></DialogHeader>
            <button onClick={() => setModal({ type: null })} className="text-muted-foreground hover:text-foreground"><X className="w-4 h-4" /></button>
          </div>
          <form onSubmit={mForm.handleSubmit(async d => {
            setErrorApi(undefined)
            try {
              await crearMarrana.mutateAsync({ identificacion: d.identificacion, fechaCompra: d.fechaCompra, costo: parseFloat(d.costo) || 0, moneda: 'COP' })
              setModal({ type: null }); mForm.reset()
            } catch (e: any) { setErrorApi(e?.response?.data?.detail ?? 'Error') }
          })} className="p-5 space-y-4">
            <FormField label="Identificación" required>
              <input {...mForm.register('identificacion')} placeholder="Ej: M-001" className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
            </FormField>
            <FormField label="Fecha compra" required>
              <input type="date" {...mForm.register('fechaCompra')} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
            </FormField>
            <FormField label="Costo (COP)">
              <MoneyInput {...mForm.register('costo')} placeholder="0" />
            </FormField>
            {errorApi && <Alert variant="destructive">{errorApi}</Alert>}
            <Button type="submit" className="w-full" loading={crearMarrana.isPending}>Crear marrana</Button>
          </form>
        </div>
      </Dialog>

      {/* Modal confirmar eliminar marrana */}
      <Dialog open={modal.type === 'confirmarEliminar'} onClose={() => setModal({ type: null })}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[380px] mx-4 p-5 space-y-4">
          <DialogHeader className="mb-0">
            <DialogTitle>Eliminar marrana</DialogTitle>
            <p className="text-xs text-muted-foreground mt-1">
              ¿Confirmas eliminar <strong>{modal.type === 'confirmarEliminar' ? modal.nombre : ''}</strong>?
              Esta acción no se puede deshacer.
            </p>
          </DialogHeader>
          <div className="flex gap-2">
            <Button variant="outline" className="flex-1" onClick={() => setModal({ type: null })}>Cancelar</Button>
            <Button variant="destructive" className="flex-1" loading={eliminarMarrana.isPending}
              onClick={async () => {
                if (modal.type !== 'confirmarEliminar') return
                await eliminarMarrana.mutateAsync(modal.marranaId)
                setModal({ type: null })
              }}>
              Eliminar
            </Button>
          </div>
        </div>
      </Dialog>

      {/* Modal avanzar estado camada */}
      <Dialog open={modal.type === 'avanzarEstado'} onClose={() => setModal({ type: null })}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[380px] mx-4 p-5 space-y-4">
          <DialogHeader className="mb-0">
            <DialogTitle>
              {modal.type === 'avanzarEstado' && modal.estadoActual === 'Preceba' ? 'Pasar a Ceba' : 'Marcar como vendida'}
            </DialogTitle>
            <p className="text-xs text-muted-foreground mt-1">
              {modal.type === 'avanzarEstado' && modal.estadoActual === 'Preceba'
                ? 'La camada pasará del estado Preceba a Ceba.'
                : 'La camada quedará marcada como vendida y no podrá avanzar más.'}
            </p>
          </DialogHeader>
          <div className="flex gap-2">
            <Button variant="outline" className="flex-1" onClick={() => setModal({ type: null })}>Cancelar</Button>
            <Button className="flex-1" loading={avanzarEstadoCamada.isPending}
              onClick={async () => {
                if (modal.type !== 'avanzarEstado') return
                const accion = modal.estadoActual === 'Preceba' ? 'AvanzarCeba' : 'MarcarVendida'
                await avanzarEstadoCamada.mutateAsync({ marranaId: modal.marranaId, camadaId: modal.camadaId, accionEstado: accion })
                setModal({ type: null })
              }}>
              Confirmar
            </Button>
          </div>
        </div>
      </Dialog>

      {/* Modal registrar camada */}
      <Dialog open={modal.type === 'camada'} onClose={() => { setModal({ type: null }); cForm.reset(); setErrorApi(undefined) }}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[400px] mx-4">
          <div className="flex items-center justify-between px-5 py-4 border-b border-border">
            <DialogHeader className="mb-0"><DialogTitle>Registrar camada</DialogTitle></DialogHeader>
            <button onClick={() => setModal({ type: null })} className="text-muted-foreground hover:text-foreground"><X className="w-4 h-4" /></button>
          </div>
          <form onSubmit={cForm.handleSubmit(async d => {
            if (modal.type !== 'camada') return
            setErrorApi(undefined)
            try {
              await registrarCamada.mutateAsync({ marranaId: modal.marranaId, ...d })
              setModal({ type: null }); cForm.reset()
            } catch (e: any) { setErrorApi(e?.response?.data?.detail ?? 'Error') }
          })} className="p-5 space-y-4">
            <FormField label="Fecha nacimiento" required>
              <input type="date" {...cForm.register('fechaNacimiento')} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
            </FormField>
            <FormField label="N° lechones" required>
              <input type="number" min={1} {...cForm.register('nLechones', { valueAsNumber: true })} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
            </FormField>
            {errorApi && <Alert variant="destructive">{errorApi}</Alert>}
            <Button type="submit" className="w-full" loading={registrarCamada.isPending}>Registrar camada</Button>
          </form>
        </div>
      </Dialog>

    </>
  )
}

// ─── Lotes section ────────────────────────────────────────────────────────────
function LotesSection() {
  const { data: marranas } = useMarranas()
  const marranaArr = (marranas as Marrana[] | undefined) ?? []
  const { data: lotes, isLoading } = useLotesCerdos()
  const arr = (lotes as LoteCerdos[] | undefined) ?? []
  const crearLote = useCrearLoteCerdos()
  const registrarVenta = useRegistrarVentaLoteCerdos()
  const [modal, setModal] = useState<ModalState>({ type: null })
  const [errorApi, setErrorApi] = useState<string>()

  const lForm = useForm<{ codigo: string; fechaInicio: string; nAnimales: number; pesoPromedioKg: number; ciclo: string; camadaId: string; precioVentaKg: string }>({
    defaultValues: { codigo: '', fechaInicio: new Date().toISOString().split('T')[0], nAnimales: 0, pesoPromedioKg: 0, ciclo: '', camadaId: '', precioVentaKg: '' },
  })
  const vForm = useForm<{ fechaVenta: string; precioVentaKg: string }>({
    defaultValues: { fechaVenta: new Date().toISOString().split('T')[0], precioVentaKg: '' },
  })

  const lotesActivos = arr.filter(l => !l.vendido).length
  const lotesVendidos = arr.filter(l => l.vendido).length
  const ingresosTotales = arr.filter(l => l.vendido && l.precioVentaKgMonto).reduce((s, l) => s + l.precioVentaKgMonto! * l.pesoPromedioKg * l.nAnimales, 0)

  const todasCamadas = marranaArr.flatMap(m => m.camadas.map(c => ({
    id: c.id, label: `${m.identificacion} — ${fmt.fecha(c.fechaNacimiento)} (${c.nLechones} lech. · ${c.estado})`
  })))

  if (isLoading) return <div className="space-y-3"><Skeleton className="h-20" /><Skeleton className="h-20" /></div>

  return (
    <>
      <div className="grid grid-cols-3 gap-3 mb-4">
        <StatCard label="Lotes activos" value={String(lotesActivos)} icon={<PiggyBank className="w-4 h-4 text-emerald-400" />} className="border-emerald-500/20" />
        <StatCard label="Lotes vendidos" value={String(lotesVendidos)} icon={<TrendingUp className="w-4 h-4 text-blue-400" />} className="border-blue-500/20" />
        <StatCard label="Ingresos ventas" value={fmt.cop(ingresosTotales)} icon={<TrendingUp className="w-4 h-4" />} />
      </div>

      {arr.length === 0 ? (
        <EmptyState icon={<PiggyBank className="w-5 h-5" />} title="Sin lotes de cerdos"
          description="Registra los lotes de engorde porcino."
          action={<Button size="sm" onClick={() => setModal({ type: 'lote' })}><Plus className="w-3.5 h-3.5" />Nuevo lote</Button>} />
      ) : (
        <>
          <div className="flex justify-end mb-3">
            <Button size="sm" variant="outline" onClick={() => setModal({ type: 'lote' })}><Plus className="w-3 h-3" />Nuevo lote</Button>
          </div>
          <Card>
            <CardContent className="p-0">
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-border">
                      {['Código', 'Inicio', 'Animales', 'Peso prom.', 'Ciclo', 'Estado', 'Ingreso estimado', ''].map(h => (
                        <th key={h} className="text-left px-4 py-3 text-muted-foreground font-medium uppercase tracking-wide text-[10px] whitespace-nowrap">{h}</th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {arr.map((l, i) => {
                      const ingreso = l.precioVentaKgMonto ? l.precioVentaKgMonto * l.pesoPromedioKg * l.nAnimales : null
                      return (
                        <tr key={l.id} className={`border-b border-border/40 hover:bg-secondary/30 ${i === arr.length - 1 ? 'border-b-0' : ''}`}>
                          <td className="px-4 py-2.5 font-medium">{l.codigo}</td>
                          <td className="px-4 py-3 text-muted-foreground text-xs">{fmt.fecha(l.fechaInicio)}</td>
                          <td className="px-4 py-3 tabular-nums text-right">{l.nAnimales}</td>
                          <td className="px-4 py-3 tabular-nums text-right">{fmt.kg(l.pesoPromedioKg)}</td>
                          <td className="px-4 py-2.5 text-muted-foreground">{l.ciclo}</td>
                          <td className="px-4 py-3">
                            <Badge className={`text-[9px] ${l.vendido ? 'bg-blue-500/10 text-blue-400 border-blue-500/20' : 'bg-emerald-500/10 text-emerald-400 border-emerald-500/20'}`}>
                              {l.vendido ? 'Vendido' : 'Activo'}
                            </Badge>
                          </td>
                          <td className="px-4 py-3 tabular-nums text-right">
                            {ingreso !== null ? <span className="text-emerald-400">{fmt.cop(ingreso)}</span> : <span className="text-muted-foreground">—</span>}
                          </td>
                          <td className="px-4 py-3">
                            {!l.vendido && (
                              <Button size="sm" variant="ghost" onClick={() => {
                                setModal({ type: 'venta', loteId: l.id })
                                if (l.precioVentaKgMonto) vForm.setValue('precioVentaKg', String(l.precioVentaKgMonto))
                              }}>Vender</Button>
                            )}
                          </td>
                        </tr>
                      )
                    })}
                  </tbody>
                </table>
              </div>
            </CardContent>
          </Card>
        </>
      )}

      {/* Modal nuevo lote */}
      <Dialog open={modal.type === 'lote'} onClose={() => { setModal({ type: null }); lForm.reset(); setErrorApi(undefined) }}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[480px] mx-4">
          <div className="flex items-center justify-between px-5 py-4 border-b border-border">
            <DialogHeader className="mb-0"><DialogTitle>Nuevo lote de cerdos</DialogTitle></DialogHeader>
            <button onClick={() => setModal({ type: null })} className="text-muted-foreground hover:text-foreground"><X className="w-4 h-4" /></button>
          </div>
          <form onSubmit={lForm.handleSubmit(async d => {
            setErrorApi(undefined)
            try {
              await crearLote.mutateAsync({ codigo: d.codigo, fechaInicio: d.fechaInicio, nAnimales: d.nAnimales, pesoPromedioKg: d.pesoPromedioKg, ciclo: d.ciclo, camadaId: d.camadaId || undefined, precioVentaKg: parseFloat(d.precioVentaKg) || undefined, moneda: 'COP' })
              setModal({ type: null }); lForm.reset()
            } catch (e: any) { setErrorApi(e?.response?.data?.detail ?? 'Error') }
          })} className="p-5 space-y-4">
            <div className="grid grid-cols-2 gap-3">
              <FormField label="Código" required>
                <input {...lForm.register('codigo')} placeholder="Ej: LC-001" className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
              </FormField>
              <FormField label="Ciclo" required>
                <input {...lForm.register('ciclo')} placeholder="Ej: 1" className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
              </FormField>
              <FormField label="Fecha inicio" required>
                <input type="date" {...lForm.register('fechaInicio')} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
              </FormField>
              <FormField label="N° animales" required>
                <input type="number" min={1} {...lForm.register('nAnimales', { valueAsNumber: true })} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
              </FormField>
              <FormField label="Peso prom. (kg)" required>
                <input type="number" min={0} step={0.1} {...lForm.register('pesoPromedioKg', { valueAsNumber: true })} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
              </FormField>
              <FormField label="Precio venta/kg (COP)">
                <MoneyInput {...lForm.register('precioVentaKg')} placeholder="Opcional" />
              </FormField>
            </div>
            <FormField label="Camada origen" hint="Opcional">
              <select {...lForm.register('camadaId')} className="flex h-9 w-full rounded-md border border-input bg-card text-sm px-3 [&>option]:bg-card">
                <option value="">— Sin camada —</option>
                {todasCamadas.map(c => <option key={c.id} value={c.id}>{c.label}</option>)}
              </select>
            </FormField>
            {errorApi && <Alert variant="destructive">{errorApi}</Alert>}
            <Button type="submit" className="w-full" loading={crearLote.isPending}>Crear lote</Button>
          </form>
        </div>
      </Dialog>

      {/* Modal venta */}
      <Dialog open={modal.type === 'venta'} onClose={() => { setModal({ type: null }); vForm.reset(); setErrorApi(undefined) }}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[400px] mx-4">
          <div className="flex items-center justify-between px-5 py-4 border-b border-border">
            <DialogHeader className="mb-0"><DialogTitle>Registrar venta de lote</DialogTitle></DialogHeader>
            <button onClick={() => setModal({ type: null })} className="text-muted-foreground hover:text-foreground"><X className="w-4 h-4" /></button>
          </div>
          <form onSubmit={vForm.handleSubmit(async d => {
            if (modal.type !== 'venta') return
            setErrorApi(undefined)
            try {
              await registrarVenta.mutateAsync({ loteId: modal.loteId, fechaVenta: d.fechaVenta, precioVentaKg: parseFloat(d.precioVentaKg) || 0 })
              setModal({ type: null }); vForm.reset()
            } catch (e: any) { setErrorApi(e?.response?.data?.detail ?? 'Error') }
          })} className="p-5 space-y-4">
            <FormField label="Fecha venta" required>
              <input type="date" {...vForm.register('fechaVenta')} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
            </FormField>
            <FormField label="Precio venta/kg (COP)" required>
              <MoneyInput {...vForm.register('precioVentaKg')} placeholder="0" />
            </FormField>
            {errorApi && <Alert variant="destructive">{errorApi}</Alert>}
            <Button type="submit" className="w-full" loading={registrarVenta.isPending}>Registrar venta</Button>
          </form>
        </div>
      </Dialog>
    </>
  )
}

// ─── Page ─────────────────────────────────────────────────────────────────────
export default function PorcinoPage() {
  const [tab, setTab] = useState<Tab>('marranas')
  return (
    <div className="flex flex-col h-full animate-fade-in">
      <PageHeader title="Porcino" description="Marranas reproductoras y lotes de engorde" />
      <div className="flex border-b border-border px-6">
        {tabs.map(t => (
          <button key={t.key} onClick={() => setTab(t.key)}
            className={`flex items-center gap-2 px-4 py-3 text-sm font-medium border-b-2 transition-colors ${
              tab === t.key ? 'border-primary text-foreground' : 'border-transparent text-muted-foreground hover:text-foreground'
            }`}>
            <PiggyBank className="w-4 h-4" />{t.label}
          </button>
        ))}
      </div>
      <div className="flex-1 overflow-y-auto p-6">
        {tab === 'marranas' && <MarranasSection />}
        {tab === 'lotes' && <LotesSection />}
      </div>
    </div>
  )
}
