import { useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { ArrowLeft, Package, Users, ArrowRightLeft, X } from 'lucide-react'
import { useLote, useLotes, useMoverAnimal } from '@/hooks/useFeedlot'
import {
  Button, Card, CardHeader, CardTitle, Badge,
  Skeleton, EmptyState,
  Dialog, DialogHeader, DialogTitle,
  FormField, Alert,
} from '@/components/ui'
import { fmt } from '@/utils'
import type { LoteResumen } from '@/types'

const estadoStyle: Record<string, string> = {
  Activo: 'bg-emerald-500/10 text-emerald-400 border-emerald-500/20',
  EnPreparacion: 'bg-amber-500/10 text-amber-400 border-amber-500/20',
  Cerrado: 'bg-zinc-500/10 text-zinc-400 border-zinc-500/20',
}

const estadoLabel: Record<string, string> = {
  Activo: 'Activo',
  EnPreparacion: 'En preparación',
  Cerrado: 'Cerrado',
}

function InfoRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex items-center justify-between py-2 border-b border-border/40 last:border-b-0">
      <span className="text-xs text-muted-foreground">{label}</span>
      <span className="text-xs font-semibold tabular-nums">{value}</span>
    </div>
  )
}

const MOTIVOS = ['IngresoInicial', 'Reclasificacion', 'Sanitario', 'Capacidad', 'Venta', 'Muerte', 'Otro'] as const

export default function LoteDetallePage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { data: lote, isLoading } = useLote(id!)
  const { data: todosLotes } = useLotes()
  const moverAnimal = useMoverAnimal()

  const [moverModal, setMoverModal] = useState<{ open: false } | { open: true; animalId: string; codigoAnimal: string }>({ open: false })
  const [errorApi, setErrorApi] = useState<string>()

  const moverForm = useForm<{ loteDestinoId: string; fechaMovimiento: string; motivo: string }>({
    defaultValues: { loteDestinoId: '', fechaMovimiento: new Date().toISOString().split('T')[0], motivo: 'Reclasificacion' },
  })

  const lotesDestino = ((todosLotes as LoteResumen[] | undefined) ?? []).filter(l => l.id !== id && l.estado === 'Activo')

  const onSubmitMover = async (data: { loteDestinoId: string; fechaMovimiento: string; motivo: string }) => {
    if (!moverModal.open) return
    setErrorApi(undefined)
    try {
      await moverAnimal.mutateAsync({ loteId: data.loteDestinoId, animalId: moverModal.animalId, fechaMovimiento: data.fechaMovimiento, motivo: data.motivo })
      setMoverModal({ open: false })
      moverForm.reset()
    } catch (e: any) {
      setErrorApi(e?.response?.data?.detail ?? 'Error al mover el animal.')
    }
  }

  if (isLoading) {
    return (
      <div className="flex flex-col h-full animate-fade-in">
        <div className="flex items-center gap-3 px-6 py-4 border-b border-border">
          <Skeleton className="h-8 w-8 rounded-lg" />
          <div className="space-y-1">
            <Skeleton className="h-4 w-32" />
            <Skeleton className="h-3 w-20" />
          </div>
        </div>
        <div className="flex-1 p-6 space-y-4">
          <Skeleton className="h-32 rounded-lg" />
          <Skeleton className="h-48 rounded-lg" />
        </div>
      </div>
    )
  }

  if (!lote) {
    return (
      <div className="flex flex-col h-full">
        <div className="flex items-center gap-3 px-6 py-4 border-b border-border">
          <Button variant="ghost" size="sm" onClick={() => navigate('/lotes')}>
            <ArrowLeft className="w-4 h-4" />
          </Button>
          <p className="text-sm font-medium">Lote no encontrado</p>
        </div>
        <div className="flex-1 flex items-center justify-center">
          <EmptyState icon={<Package className="w-5 h-5" />}
            title="Lote no encontrado"
            description="El lote que buscas no existe o fue eliminado."
            action={<Button size="sm" onClick={() => navigate('/lotes')}>Volver a lotes</Button>} />
        </div>
      </div>
    )
  }

  const pct = lote.capacidadMaxima > 0
    ? (lote.animalesActuales / lote.capacidadMaxima) * 100
    : 0

  const barColor = pct >= 90 ? 'bg-rose-400' : pct >= 70 ? 'bg-amber-400' : 'bg-primary'

  const animalesActivos = lote.animales.filter(a => a.esActivo)
  const animalesInactivos = lote.animales.filter(a => !a.esActivo)

  return (
    <div className="flex flex-col h-full animate-fade-in">
      {/* Header */}
      <div className="flex items-center gap-3 px-6 py-4 border-b border-border">
        <Button variant="ghost" size="sm" onClick={() => navigate('/lotes')}>
          <ArrowLeft className="w-4 h-4" />
        </Button>
        <div className="w-9 h-9 rounded-lg bg-primary/10 flex items-center justify-center flex-shrink-0">
          <Package className="w-4 h-4 text-primary" />
        </div>
        <div className="flex-1">
          <div className="flex items-center gap-2">
            <h1 className="text-sm font-semibold font-mono">{lote.codigo}</h1>
            <Badge className={estadoStyle[lote.estado] ?? estadoStyle.Cerrado}>
              {estadoLabel[lote.estado] ?? lote.estado}
            </Badge>
          </div>
          <p className="text-xs text-muted-foreground">{lote.nombre}</p>
        </div>
      </div>

      <div className="flex-1 overflow-y-auto p-6 space-y-5">
        {/* Ocupación */}
        <Card className="p-5">
          <CardHeader className="p-0 mb-4">
            <CardTitle className="text-xs font-semibold uppercase tracking-widest text-muted-foreground">
              Ocupación
            </CardTitle>
          </CardHeader>
          <div className="space-y-3">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-1.5 text-muted-foreground">
                <Users className="w-3.5 h-3.5" />
                <span className="text-xs">Animales</span>
              </div>
              <span className="text-xs font-semibold tabular-nums">
                {lote.animalesActuales}
                <span className="text-muted-foreground font-normal">/{lote.capacidadMaxima}</span>
              </span>
            </div>
            <div className="h-2 rounded-full bg-border overflow-hidden">
              <div className={`h-full rounded-full transition-all ${barColor}`}
                style={{ width: `${Math.min(pct, 100)}%` }} />
            </div>
            <div className="flex items-center justify-between text-[10px] text-muted-foreground">
              <span>{lote.capacidadMaxima - lote.animalesActuales} disponibles</span>
              <span className="font-medium tabular-nums">{fmt.pct(pct)}</span>
            </div>
          </div>
        </Card>

        {/* Información general */}
        <Card className="p-5">
          <CardHeader className="p-0 mb-3">
            <CardTitle className="text-xs font-semibold uppercase tracking-widest text-muted-foreground">
              Información general
            </CardTitle>
          </CardHeader>
          <InfoRow label="Código" value={lote.codigo} />
          <InfoRow label="Nombre" value={lote.nombre} />
          <InfoRow label="Capacidad máxima" value={String(lote.capacidadMaxima)} />
          <InfoRow label="Animales actuales" value={String(lote.animalesActuales)} />
          <InfoRow label="Ocupación" value={fmt.pct(pct)} />
        </Card>

        {/* Animales activos */}
        <Card className="p-5">
          <CardHeader className="p-0 mb-3">
            <CardTitle className="text-xs font-semibold uppercase tracking-widest text-muted-foreground">
              Animales en el lote ({animalesActivos.length})
            </CardTitle>
          </CardHeader>
          {animalesActivos.length === 0 ? (
            <p className="text-xs text-muted-foreground py-4 text-center">No hay animales activos en este lote.</p>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-xs">
                <thead>
                  <tr className="border-b border-border">
                    <th className="text-left pb-2 text-muted-foreground font-medium whitespace-nowrap">Código</th>
                    <th className="text-left pb-2 text-muted-foreground font-medium whitespace-nowrap">Nombre</th>
                    <th className="text-left pb-2 text-muted-foreground font-medium whitespace-nowrap">Ingreso</th>
                    <th className="text-left pb-2 text-muted-foreground font-medium whitespace-nowrap">Días</th>
                    <th className="text-left pb-2 text-muted-foreground font-medium whitespace-nowrap">Motivo</th>
                    <th className="text-right pb-2 text-muted-foreground font-medium whitespace-nowrap">Estado</th>
                    <th className="pb-2" />
                  </tr>
                </thead>
                <tbody>
                  {animalesActivos.map(al => (
                    <tr key={al.animalId}
                      className="border-b border-border/40 hover:bg-muted/20 transition-colors cursor-pointer"
                      onClick={() => navigate(`/animales/${al.animalId}`)}>
                      <td className="py-2.5 font-mono font-semibold whitespace-nowrap">{al.codigoAnimal}</td>
                      <td className="py-2.5 text-muted-foreground whitespace-nowrap">{al.nombreAnimal || '-'}</td>
                      <td className="py-2.5 text-muted-foreground whitespace-nowrap">{fmt.fecha(al.fechaIngreso as string)}</td>
                      <td className="py-2.5 tabular-nums whitespace-nowrap">{al.diasEnLote}d</td>
                      <td className="py-2.5 text-muted-foreground capitalize whitespace-nowrap">{al.motivoIngreso}</td>
                      <td className="py-2.5 text-right whitespace-nowrap">
                        <Badge className="bg-emerald-500/10 text-emerald-400 border-emerald-500/20">Activo</Badge>
                      </td>
                      <td className="py-2.5 text-right whitespace-nowrap">
                        <Button size="sm" variant="ghost" className="h-6 px-2 text-[10px] gap-1"
                          onClick={e => { e.stopPropagation(); setMoverModal({ open: true, animalId: al.animalId, codigoAnimal: al.codigoAnimal }) }}>
                          <ArrowRightLeft className="w-3 h-3" /> Mover
                        </Button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </Card>

        {/* Historial de movimientos */}
        {animalesInactivos.length > 0 && (
          <Card className="p-5">
            <CardHeader className="p-0 mb-3">
              <CardTitle className="text-xs font-semibold uppercase tracking-widest text-muted-foreground">
                Historial ({animalesInactivos.length})
              </CardTitle>
            </CardHeader>
            <div className="overflow-x-auto">
              <table className="w-full text-xs">
                <thead>
                  <tr className="border-b border-border">
                    <th className="text-left pb-2 text-muted-foreground font-medium whitespace-nowrap">Código</th>
                    <th className="text-left pb-2 text-muted-foreground font-medium whitespace-nowrap">Nombre</th>
                    <th className="text-left pb-2 text-muted-foreground font-medium whitespace-nowrap">Ingreso</th>
                    <th className="text-left pb-2 text-muted-foreground font-medium whitespace-nowrap">Egreso</th>
                    <th className="text-left pb-2 text-muted-foreground font-medium whitespace-nowrap">Días</th>
                    <th className="text-right pb-2 text-muted-foreground font-medium whitespace-nowrap">Estado</th>
                  </tr>
                </thead>
                <tbody>
                  {animalesInactivos.map(al => (
                    <tr key={al.animalId}
                      className="border-b border-border/40 hover:bg-muted/20 transition-colors cursor-pointer"
                      onClick={() => navigate(`/animales/${al.animalId}`)}>
                      <td className="py-2.5 font-mono font-semibold whitespace-nowrap">{al.codigoAnimal}</td>
                      <td className="py-2.5 text-muted-foreground whitespace-nowrap">{al.nombreAnimal || '-'}</td>
                      <td className="py-2.5 text-muted-foreground whitespace-nowrap">{fmt.fecha(al.fechaIngreso as string)}</td>
                      <td className="py-2.5 text-muted-foreground whitespace-nowrap">{al.fechaEgreso ? fmt.fecha(al.fechaEgreso) : '-'}</td>
                      <td className="py-2.5 tabular-nums whitespace-nowrap">{al.diasEnLote}d</td>
                      <td className="py-2.5 text-right whitespace-nowrap">
                        <Badge className="bg-zinc-500/10 text-zinc-400 border-zinc-500/20">Inactivo</Badge>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </Card>
        )}
      </div>

      {/* Modal mover animal */}
      <Dialog open={moverModal.open} onClose={() => { setMoverModal({ open: false }); moverForm.reset(); setErrorApi(undefined) }}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[420px] mx-4">
          <div className="flex items-center justify-between px-5 py-4 border-b border-border">
            <DialogHeader className="mb-0">
              <DialogTitle>Mover animal</DialogTitle>
              {moverModal.open && <p className="text-xs text-muted-foreground mt-0.5">Animal: <span className="font-mono font-semibold">{moverModal.codigoAnimal}</span></p>}
            </DialogHeader>
            <button onClick={() => setMoverModal({ open: false })} className="text-muted-foreground hover:text-foreground">
              <X className="w-4 h-4" />
            </button>
          </div>
          <form onSubmit={moverForm.handleSubmit(onSubmitMover)} className="p-5 space-y-4">
            <FormField label="Lote destino" required>
              <select {...moverForm.register('loteDestinoId')} className="flex h-9 w-full rounded-md border border-input bg-card text-sm px-3 [&>option]:bg-card">
                <option value="">— Selecciona un lote —</option>
                {lotesDestino.map(l => (
                  <option key={l.id} value={l.id}>{l.codigo} — {l.nombre} ({l.animalesActuales}/{l.capacidadMaxima})</option>
                ))}
              </select>
            </FormField>
            <FormField label="Fecha movimiento" required>
              <input type="date" max={new Date().toISOString().split('T')[0]}
                {...moverForm.register('fechaMovimiento')}
                className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
            </FormField>
            <FormField label="Motivo" required>
              <select {...moverForm.register('motivo')} className="flex h-9 w-full rounded-md border border-input bg-card text-sm px-3 [&>option]:bg-card">
                {MOTIVOS.map(m => <option key={m} value={m}>{m}</option>)}
              </select>
            </FormField>
            {errorApi && <Alert variant="destructive">{errorApi}</Alert>}
            <div className="flex gap-2">
              <Button type="button" variant="outline" className="flex-1" onClick={() => setMoverModal({ open: false })}>Cancelar</Button>
              <Button type="submit" className="flex-1" loading={moverAnimal.isPending}>
                <ArrowRightLeft className="w-3.5 h-3.5" /> Mover
              </Button>
            </div>
          </form>
        </div>
      </Dialog>
    </div>
  )
}
