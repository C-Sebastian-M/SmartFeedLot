import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Plus, X, PiggyBank, ChevronDown, ChevronRight } from 'lucide-react'
import { useMarranas, useCrearMarrana, useRegistrarCamada,
  useLotesCerdos, useCrearLoteCerdos, useRegistrarVentaLoteCerdos } from '@/hooks/useFeedlot'
import {
  PageHeader, Card, CardHeader, CardTitle, CardContent,
  Skeleton, EmptyState, Button,
  Dialog, DialogHeader, DialogTitle,
  FormField,
} from '@/components/ui'
import { fmt } from '@/utils'
import type { Marrana, LoteCerdos } from '@/types'

type Tab = 'marranas' | 'lotes'

const tabs: { key: Tab; label: string; icon: typeof PiggyBank }[] = [
  { key: 'marranas', label: 'Marranas', icon: PiggyBank },
  { key: 'lotes', label: 'Lotes de cerdos', icon: PiggyBank },
]

type ModalState =
  | { type: null }
  | { type: 'marrana' }
  | { type: 'camada'; marranaId: string }
  | { type: 'lote' }
  | { type: 'venta'; loteId: string }

// ─── Marranas section ───────────────────────────────────────────────────────
function MarranasSection() {
  const { data: marranas, isLoading } = useMarranas()
  const arr = (marranas as Marrana[] | undefined) ?? []
  const crearMarrana = useCrearMarrana()
  const registrarCamada = useRegistrarCamada()
  const [modal, setModal] = useState<ModalState>({ type: null })
  const [expanded, setExpanded] = useState<string | null>(null)

  const marranaForm = useForm<{ identificacion: string; fechaCompra: string; costo: number }>({
    defaultValues: { identificacion: '', fechaCompra: new Date().toISOString().split('T')[0], costo: 0 },
  })

  const camadaForm = useForm<{ fechaNacimiento: string; nLechones: number }>({
    defaultValues: { fechaNacimiento: new Date().toISOString().split('T')[0], nLechones: 0 },
  })

  const onSubmitMarrana = async (data: { identificacion: string; fechaCompra: string; costo: number }) => {
    await crearMarrana.mutateAsync({ ...data, moneda: 'COP' })
    setModal({ type: null })
    marranaForm.reset()
  }

  const onSubmitCamada = async (data: { fechaNacimiento: string; nLechones: number }) => {
    if (modal.type !== 'camada') return
    await registrarCamada.mutateAsync({ marranaId: modal.marranaId, ...data })
    setModal({ type: null })
    camadaForm.reset()
  }

  if (isLoading) return <div className="space-y-4"><Skeleton className="h-20 rounded-lg" /><Skeleton className="h-20 rounded-lg" /></div>
  if (arr.length === 0) return <EmptyState icon={<PiggyBank className="w-5 h-5" />} title="Sin marranas" description="Registra las marranas reproductoras." action={<Button size="sm" onClick={() => setModal({ type: 'marrana' })}><Plus className="w-3.5 h-3.5" />Nueva marrana</Button>} />

  return (
    <>
      <div className="space-y-3">
        <div className="flex items-center justify-between">
          <p className="text-sm text-muted-foreground">{arr.length} marranas</p>
          <Button size="sm" variant="outline" onClick={() => setModal({ type: 'marrana' })}><Plus className="w-3 h-3" />Nueva marrana</Button>
        </div>
        {arr.map(m => (
          <Card key={m.id}>
            <CardHeader className="cursor-pointer" onClick={() => setExpanded(expanded === m.id ? null : m.id)}>
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle className="text-sm">{m.identificacion}</CardTitle>
                  <p className="text-[10px] text-muted-foreground">{fmt.fecha(m.fechaCompra)} · {fmt.cop(m.costoMonto)} · {m.camadas.length} camadas</p>
                </div>
                <div className="flex items-center gap-2">
                  <Button size="sm" variant="ghost" onClick={(ev) => { ev.stopPropagation(); setModal({ type: 'camada', marranaId: m.id }) }}>+ Camada</Button>
                  {expanded === m.id ? <ChevronDown className="w-4 h-4 text-muted-foreground" /> : <ChevronRight className="w-4 h-4 text-muted-foreground" />}
                </div>
              </div>
            </CardHeader>
            {expanded === m.id && m.camadas.length > 0 && (
              <CardContent className="p-0 border-t border-border">
                <table className="w-full text-xs">
                  <thead><tr className="border-b bg-secondary/20"><th className="text-left px-4 py-2 text-muted-foreground">Fecha nacimiento</th><th className="text-right px-4 py-2 text-muted-foreground">Lechones</th><th className="text-left px-4 py-2 text-muted-foreground">Estado</th></tr></thead>
                  <tbody>{m.camadas.map(c => (
                    <tr key={c.id} className="border-b border-border/30"><td className="px-4 py-2">{fmt.fecha(c.fechaNacimiento)}</td><td className="px-4 py-2 text-right tabular-nums">{c.nLechones}</td><td className="px-4 py-2">{c.estado}</td></tr>
                  ))}</tbody>
                </table>
              </CardContent>
            )}
          </Card>
        ))}
      </div>

      <Dialog open={modal.type === 'marrana'} onClose={() => setModal({ type: null })}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[400px] mx-4">
          <div className="flex items-center justify-between px-5 py-4 border-b border-border">
            <DialogHeader className="mb-0"><DialogTitle>Nueva marrana</DialogTitle></DialogHeader>
            <button onClick={() => setModal({ type: null })} className="text-muted-foreground hover:text-foreground"><X className="w-4 h-4" /></button>
          </div>
          <form onSubmit={marranaForm.handleSubmit(onSubmitMarrana)} className="p-5 space-y-4">
            <FormField label="Identificación" required><input {...marranaForm.register('identificacion')} placeholder="Ej: M-001" className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
            <FormField label="Fecha compra" required><input type="date" {...marranaForm.register('fechaCompra')} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
            <FormField label="Costo" required><input type="number" min={0} {...marranaForm.register('costo', { valueAsNumber: true })} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
            <Button type="submit" className="w-full" loading={crearMarrana.isPending}>Crear marrana</Button>
          </form>
        </div>
      </Dialog>

      <Dialog open={modal.type === 'camada'} onClose={() => setModal({ type: null })}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[400px] mx-4">
          <div className="flex items-center justify-between px-5 py-4 border-b border-border">
            <DialogHeader className="mb-0"><DialogTitle>Registrar camada</DialogTitle></DialogHeader>
            <button onClick={() => setModal({ type: null })} className="text-muted-foreground hover:text-foreground"><X className="w-4 h-4" /></button>
          </div>
          <form onSubmit={camadaForm.handleSubmit(onSubmitCamada)} className="p-5 space-y-4">
            <FormField label="Fecha nacimiento" required><input type="date" {...camadaForm.register('fechaNacimiento')} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
            <FormField label="N° lechones" required><input type="number" min={1} {...camadaForm.register('nLechones', { valueAsNumber: true })} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
            <Button type="submit" className="w-full" loading={registrarCamada.isPending}>Registrar camada</Button>
          </form>
        </div>
      </Dialog>
    </>
  )
}

// ─── Lotes de cerdos section ────────────────────────────────────────────────
function LotesSection() {
  const { data: lotes, isLoading } = useLotesCerdos()
  const arr = (lotes as LoteCerdos[] | undefined) ?? []
  const crearLote = useCrearLoteCerdos()
  const registrarVenta = useRegistrarVentaLoteCerdos()
  const [modal, setModal] = useState<ModalState>({ type: null })

  const loteForm = useForm<{ codigo: string; fechaInicio: string; nAnimales: number; pesoPromedioKg: number; ciclo: string; camadaId?: string; precioVentaKg?: number }>({
    defaultValues: { codigo: '', fechaInicio: new Date().toISOString().split('T')[0], nAnimales: 0, pesoPromedioKg: 0, ciclo: '', camadaId: '', precioVentaKg: undefined },
  })

  const ventaForm = useForm<{ fechaVenta: string; precioVentaKg: number }>({
    defaultValues: { fechaVenta: new Date().toISOString().split('T')[0], precioVentaKg: 0 },
  })

  const onSubmitLote = async (data: { codigo: string; fechaInicio: string; nAnimales: number; pesoPromedioKg: number; ciclo: string; camadaId?: string; precioVentaKg?: number }) => {
    await crearLote.mutateAsync({ ...data, moneda: 'COP' })
    setModal({ type: null })
    loteForm.reset()
  }

  const onSubmitVenta = async (data: { fechaVenta: string; precioVentaKg: number }) => {
    if (modal.type !== 'venta') return
    await registrarVenta.mutateAsync({ loteId: modal.loteId, ...data })
    setModal({ type: null })
    ventaForm.reset()
  }

  if (isLoading) return <div className="space-y-4"><Skeleton className="h-20 rounded-lg" /><Skeleton className="h-20 rounded-lg" /></div>
  if (arr.length === 0) return <EmptyState icon={<PiggyBank className="w-5 h-5" />} title="Sin lotes de cerdos" description="Registra los lotes de engorde porcino." action={<Button size="sm" onClick={() => setModal({ type: 'lote' })}><Plus className="w-3.5 h-3.5" />Nuevo lote</Button>} />

  return (
    <>
      <div className="flex items-center justify-between mb-3">
        <p className="text-sm text-muted-foreground">{arr.length} lotes</p>
        <Button size="sm" variant="outline" onClick={() => setModal({ type: 'lote' })}><Plus className="w-3 h-3" />Nuevo lote</Button>
      </div>
      <table className="w-full text-xs">
        <thead><tr className="border-b"><th className="text-left px-4 py-2 text-muted-foreground">Código</th><th className="text-left px-4 py-2 text-muted-foreground">Inicio</th><th className="text-right px-4 py-2 text-muted-foreground">Animales</th><th className="text-right px-4 py-2 text-muted-foreground">Peso prom.</th><th className="text-left px-4 py-2 text-muted-foreground">Ciclo</th><th className="text-center px-4 py-2 text-muted-foreground">Estado</th><th className="text-right px-4 py-2 text-muted-foreground"></th></tr></thead>
        <tbody>{arr.map(l => (
          <tr key={l.id} className="border-b border-border/30">
            <td className="px-4 py-2 font-medium">{l.codigo}</td>
            <td className="px-4 py-2">{fmt.fecha(l.fechaInicio)}</td>
            <td className="px-4 py-2 text-right tabular-nums">{l.nAnimales}</td>
            <td className="px-4 py-2 text-right tabular-nums">{fmt.kg(l.pesoPromedioKg)}</td>
            <td className="px-4 py-2">{l.ciclo}</td>
            <td className="px-4 py-2 text-center">
              {l.vendido ? (
                <span className="inline-flex items-center px-2 py-0.5 rounded text-[10px] font-medium bg-blue-500/10 text-blue-400 border border-blue-500/20">Vendido</span>
              ) : (
                <span className="inline-flex items-center px-2 py-0.5 rounded text-[10px] font-medium bg-emerald-500/10 text-emerald-400 border border-emerald-500/20">Activo</span>
              )}
            </td>
            <td className="px-4 py-2 text-right">
              {!l.vendido && (
                <Button size="sm" variant="ghost" onClick={() => setModal({ type: 'venta', loteId: l.id })}>Vender</Button>
              )}
            </td>
          </tr>
        ))}</tbody>
      </table>

      <Dialog open={modal.type === 'lote'} onClose={() => setModal({ type: null })}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[420px] mx-4">
          <div className="flex items-center justify-between px-5 py-4 border-b border-border">
            <DialogHeader className="mb-0"><DialogTitle>Nuevo lote de cerdos</DialogTitle></DialogHeader>
            <button onClick={() => setModal({ type: null })} className="text-muted-foreground hover:text-foreground"><X className="w-4 h-4" /></button>
          </div>
          <form onSubmit={loteForm.handleSubmit(onSubmitLote)} className="p-5 space-y-4">
            <div className="grid grid-cols-2 gap-3">
              <FormField label="Código" required><input {...loteForm.register('codigo')} placeholder="Ej: LC-001" className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
              <FormField label="Ciclo" required><input {...loteForm.register('ciclo')} placeholder="Ej: 1" className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
              <FormField label="Fecha inicio" required><input type="date" {...loteForm.register('fechaInicio')} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
              <FormField label="N° animales" required><input type="number" min={1} {...loteForm.register('nAnimales', { valueAsNumber: true })} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
              <FormField label="Peso promedio kg" required><input type="number" min={0} step={0.1} {...loteForm.register('pesoPromedioKg', { valueAsNumber: true })} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
              <FormField label="Precio venta kg"><input type="number" min={0} {...loteForm.register('precioVentaKg', { valueAsNumber: true })} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
            </div>
            <FormField label="Camada ID"><input {...loteForm.register('camadaId')} placeholder="Opcional" className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
            <Button type="submit" className="w-full" loading={crearLote.isPending}>Crear lote</Button>
          </form>
        </div>
      </Dialog>

      <Dialog open={modal.type === 'venta'} onClose={() => setModal({ type: null })}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[400px] mx-4">
          <div className="flex items-center justify-between px-5 py-4 border-b border-border">
            <DialogHeader className="mb-0"><DialogTitle>Registrar venta</DialogTitle></DialogHeader>
            <button onClick={() => setModal({ type: null })} className="text-muted-foreground hover:text-foreground"><X className="w-4 h-4" /></button>
          </div>
          <form onSubmit={ventaForm.handleSubmit(onSubmitVenta)} className="p-5 space-y-4">
            <FormField label="Fecha venta" required><input type="date" {...ventaForm.register('fechaVenta')} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
            <FormField label="Precio venta kg" required><input type="number" min={0} step={100} {...ventaForm.register('precioVentaKg', { valueAsNumber: true })} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
            <Button type="submit" className="w-full" loading={registrarVenta.isPending}>Registrar venta</Button>
          </form>
        </div>
      </Dialog>
    </>
  )
}

export default function PorcinoPage() {
  const [tab, setTab] = useState<Tab>('marranas')

  return (
    <div className="flex flex-col h-full animate-fade-in">
      <PageHeader
        title="Porcino"
        description="Marranas reproductoras y lotes de engorde"
      />

      <div className="flex border-b border-border px-6">
        {tabs.map(t => (
          <button key={t.key} onClick={() => setTab(t.key)}
            className={`flex items-center gap-2 px-4 py-3 text-sm font-medium border-b-2 transition-colors ${
              tab === t.key ? 'border-primary text-foreground' : 'border-transparent text-muted-foreground hover:text-foreground'
            }`}>
            <t.icon className="w-4 h-4" />
            {t.label}
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
