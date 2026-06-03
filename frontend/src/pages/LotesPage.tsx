import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Plus, Package, Users, X, CheckCircle2, PlayCircle, XCircle, ChevronRight } from 'lucide-react'
import { useLotes, useCreateLote, useActivarLote, useCerrarLote } from '@/hooks/useFeedlot'
import {
  PageHeader, Button, Card, Badge, Skeleton, EmptyState,
  Dialog, DialogHeader, DialogTitle, DialogDescription,
  FormField, Input, Alert,
} from '@/components/ui'
import { fmt } from '@/utils'
import type { LoteResumen } from '@/types'

// ─── Schema ───────────────────────────────────────────────────────────────────
const crearLoteSchema = z.object({
  nombre: z.string().min(3, 'Mínimo 3 caracteres').max(100, 'Máximo 100 caracteres'),
  capacidadMaxima: z
    .number({ invalid_type_error: 'Ingresa un número' })
    .int('Debe ser entero').min(1, 'Mínimo 1').max(10000, 'Máximo 10.000'),
})
type CrearLoteForm = z.infer<typeof crearLoteSchema>

// ─── Modal crear lote ─────────────────────────────────────────────────────────
function CrearLoteModal({ open, onClose }: { open: boolean; onClose: () => void }) {
  const [exito, setExito] = useState(false)
  const [errorApi, setErrorApi] = useState<string>()
  const createLote = useCreateLote()

  const { register, handleSubmit, reset, formState: { errors, isSubmitting } } =
    useForm<CrearLoteForm>({ resolver: zodResolver(crearLoteSchema) })

  const handleClose = () => { reset(); setExito(false); setErrorApi(undefined); onClose() }

  const onSubmit = async (data: CrearLoteForm) => {
    setErrorApi(undefined)
    try {
      // Código autogenerado a partir del nombre (alfanumérico + guiones, único por timestamp).
      const slug = data.nombre.trim().toUpperCase()
        .normalize('NFD').replace(/[̀-ͯ]/g, '')
        .replace(/[^A-Z0-9]+/g, '-').replace(/^-+|-+$/g, '').slice(0, 12)
      const codigo = `L-${slug || 'LOTE'}-${Date.now().toString().slice(-5)}`
      await createLote.mutateAsync({
        codigo,
        nombre: data.nombre,
        capacidadMaxima: data.capacidadMaxima,
      })
      setExito(true)
      setTimeout(() => handleClose(), 1500)
    } catch (err: any) {
      setErrorApi(err?.response?.data?.error ?? err?.response?.data?.detail ?? 'Error al crear el lote.')
    }
  }

  return (
    <Dialog open={open} onClose={handleClose}>
      <div className="rounded-xl border border-border bg-card shadow-xl">
        <div className="flex items-center justify-between px-5 py-4 border-b border-border">
          <DialogHeader className="mb-0">
            <DialogTitle>Crear nuevo lote</DialogTitle>
            <DialogDescription>Define el código, nombre y capacidad del lote de engorde.</DialogDescription>
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
              <p className="text-sm font-medium">¡Lote creado!</p>
              <p className="text-xs text-muted-foreground">
                El lote inicia en estado <strong>En preparación</strong>. Actívalo para agregar animales.
              </p>
            </div>
          ) : (
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              <FormField label="Nombre" error={errors.nombre?.message} required>
                <Input {...register('nombre')} placeholder="Novillos Brahman 2024"
                  className={errors.nombre ? 'border-destructive' : ''} />
              </FormField>
              <FormField label="Capacidad máxima" error={errors.capacidadMaxima?.message} required
                hint="Número máximo de animales">
                <Input {...register('capacidadMaxima', { valueAsNumber: true })}
                  type="number" min={1} max={10000} placeholder="50"
                  className={errors.capacidadMaxima ? 'border-destructive' : ''} />
              </FormField>
              {errorApi && <Alert variant="destructive">{errorApi}</Alert>}
              <div className="flex gap-2 pt-1">
                <Button type="button" variant="outline" className="flex-1" onClick={handleClose} disabled={isSubmitting}>
                  Cancelar
                </Button>
                <Button type="submit" className="flex-1" loading={isSubmitting}>
                  <Package className="w-3.5 h-3.5" />
                  Crear lote
                </Button>
              </div>
            </form>
          )}
        </div>
      </div>
    </Dialog>
  )
}

// ─── Modal confirmación acción ────────────────────────────────────────────────
function ConfirmarAccionModal({
  open, onClose, onConfirm, titulo, descripcion, labelConfirmar, variante, loading,
}: {
  open: boolean
  onClose: () => void
  onConfirm: () => void
  titulo: string
  descripcion: string
  labelConfirmar: string
  variante: 'default' | 'destructive'
  loading?: boolean
}) {
  return (
    <Dialog open={open} onClose={onClose}>
      <div className="rounded-xl border border-border bg-card shadow-xl p-5 space-y-4">
        <DialogHeader className="mb-0">
          <DialogTitle>{titulo}</DialogTitle>
          <DialogDescription>{descripcion}</DialogDescription>
        </DialogHeader>
        <div className="flex gap-2">
          <Button variant="outline" className="flex-1" onClick={onClose} disabled={loading}>Cancelar</Button>
          <Button variant={variante} className="flex-1" onClick={onConfirm} loading={loading}>
            {labelConfirmar}
          </Button>
        </div>
      </div>
    </Dialog>
  )
}

// ─── Card de lote ─────────────────────────────────────────────────────────────
function LoteCard({ lote, onVerDetalle }: { lote: LoteResumen; onVerDetalle: (id: string) => void }) {
  const [confirmActivar, setConfirmActivar] = useState(false)
  const [confirmCerrar, setConfirmCerrar] = useState(false)
  const [errorAccion, setErrorAccion] = useState<string>()

  const activar = useActivarLote()
  const cerrar = useCerrarLote()

  const pct = lote.capacidadMaxima > 0
    ? (lote.animalesActuales / lote.capacidadMaxima) * 100
    : 0

  const barColor = pct >= 90 ? 'bg-rose-400' : pct >= 70 ? 'bg-amber-400' : 'bg-primary'

  const estadoStyle: Record<string, string> = {
    Activo: 'bg-emerald-500/10 text-emerald-400 border-emerald-500/20',
    EnPreparacion: 'bg-amber-500/10 text-amber-400 border-amber-500/20',
    Cerrado: 'bg-zinc-500/10 text-zinc-400 border-zinc-500/20',
  }

  const handleActivar = async () => {
    setErrorAccion(undefined)
    try {
      await activar.mutateAsync(lote.id)
      setConfirmActivar(false)
    } catch (err: any) {
      setErrorAccion(err?.response?.data?.error ?? 'Error al activar el lote.')
    }
  }

  const handleCerrar = async () => {
    setErrorAccion(undefined)
    try {
      await cerrar.mutateAsync(lote.id)
      setConfirmCerrar(false)
    } catch (err: any) {
      setErrorAccion(err?.response?.data?.error ?? 'Error al cerrar el lote.')
    }
  }

  return (
    <>
      <Card className="p-5 hover:border-border/80 transition-all group flex flex-col gap-4">
        {/* Header */}
        <div className="flex items-start justify-between">
          <div className="flex items-center gap-2.5">
            <div className="w-9 h-9 rounded-lg bg-primary/10 flex items-center justify-center flex-shrink-0">
              <Package className="w-4 h-4 text-primary" />
            </div>
            <div>
              <p className="text-sm font-semibold font-mono">{lote.codigo}</p>
              <p className="text-xs text-muted-foreground truncate max-w-[140px]">{lote.nombre}</p>
            </div>
          </div>
          <Badge className={estadoStyle[lote.estado] ?? estadoStyle.Cerrado}>
            {lote.estado === 'EnPreparacion' ? 'En preparación' : lote.estado}
          </Badge>
        </div>

        {/* Ocupación */}
        <div className="space-y-2">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-1.5 text-muted-foreground">
              <Users className="w-3.5 h-3.5" />
              <span className="text-xs">Ocupación</span>
            </div>
            <span className="text-xs font-semibold tabular-nums">
              {lote.animalesActuales}
              <span className="text-muted-foreground font-normal">/{lote.capacidadMaxima}</span>
            </span>
          </div>
          <div className="h-1.5 rounded-full bg-border overflow-hidden">
            <div className={`h-full rounded-full transition-all ${barColor}`}
              style={{ width: `${Math.min(pct, 100)}%` }} />
          </div>
          <div className="flex items-center justify-between">
            <span className="text-[10px] text-muted-foreground">
              {lote.capacidadMaxima - lote.animalesActuales} disponibles
            </span>
            <span className="text-[10px] font-medium tabular-nums">{fmt.pct(pct)}</span>
          </div>
        </div>

        {/* Error de acción */}
        {errorAccion && (
          <Alert variant="destructive" className="text-[10px] py-2">
            {errorAccion}
          </Alert>
        )}

        {/* Acciones según estado */}
        <div className="flex gap-2 pt-1 border-t border-border">
          {lote.estado === 'EnPreparacion' && (
            <Button
              size="sm" variant="default" className="flex-1"
              onClick={() => { setErrorAccion(undefined); setConfirmActivar(true) }}
            >
              <PlayCircle className="w-3.5 h-3.5" />
              Activar lote
            </Button>
          )}

          {lote.estado === 'Activo' && (
            <>
              <Button
                size="sm" variant="outline" className="flex-1 text-rose-400 border-rose-500/30 hover:bg-rose-500/10"
                onClick={() => { setErrorAccion(undefined); setConfirmCerrar(true) }}
              >
                <XCircle className="w-3.5 h-3.5" />
                Cerrar
              </Button>
              <Button size="sm" variant="secondary" className="flex-1" onClick={() => onVerDetalle(lote.id)}>
                <ChevronRight className="w-3.5 h-3.5" />
                Ver detalle
              </Button>
            </>
          )}

          {lote.estado === 'Cerrado' && (
            <p className="text-[10px] text-muted-foreground w-full text-center py-1">
              Lote cerrado — solo lectura
            </p>
          )}
        </div>
      </Card>

      {/* Modales de confirmación */}
      <ConfirmarAccionModal
        open={confirmActivar}
        onClose={() => setConfirmActivar(false)}
        onConfirm={handleActivar}
        titulo="Activar lote"
        descripcion={`¿Confirmas activar el lote "${lote.codigo}"? Una vez activo podrá recibir animales.`}
        labelConfirmar="Sí, activar"
        variante="default"
        loading={activar.isPending}
      />
      <ConfirmarAccionModal
        open={confirmCerrar}
        onClose={() => setConfirmCerrar(false)}
        onConfirm={handleCerrar}
        titulo="Cerrar lote"
        descripcion={`¿Confirmas cerrar el lote "${lote.codigo}"? Solo es posible si no tiene animales activos.`}
        labelConfirmar="Sí, cerrar"
        variante="destructive"
        loading={cerrar.isPending}
      />
    </>
  )
}

// ─── Página principal ─────────────────────────────────────────────────────────
export default function LotesPage() {
  const navigate = useNavigate()
  const [modalAbierto, setModalAbierto] = useState(false)
  const [filtro, setFiltro] = useState<'todos' | 'Activo' | 'EnPreparacion' | 'Cerrado'>('todos')

  const { data: lotes, isLoading } = useLotes()
  const lotesArray = (lotes as LoteResumen[] | undefined) ?? []
  const lotesFiltrados = filtro === 'todos' ? lotesArray : lotesArray.filter(l => l.estado === filtro)

  const counts = {
    todos: lotesArray.length,
    Activo: lotesArray.filter(l => l.estado === 'Activo').length,
    EnPreparacion: lotesArray.filter(l => l.estado === 'EnPreparacion').length,
    Cerrado: lotesArray.filter(l => l.estado === 'Cerrado').length,
  }

  return (
    <div className="flex flex-col h-full animate-fade-in">
      <PageHeader
        title="Lotes"
        description={`${lotesArray.length} lotes · ${lotesArray.reduce((s, l) => s + l.animalesActuales, 0)} animales`}
        action={
          <Button size="sm" onClick={() => setModalAbierto(true)}>
            <Plus className="w-3.5 h-3.5" />
            Crear lote
          </Button>
        }
      />

      {/* Filtros */}
      <div className="flex items-center gap-2 px-6 py-3 border-b border-border">
        {([
          { key: 'todos', label: 'Todos' },
          { key: 'Activo', label: 'Activos' },
          { key: 'EnPreparacion', label: 'En preparación' },
          { key: 'Cerrado', label: 'Cerrados' },
        ] as const).map(({ key, label }) => (
          <button
            key={key}
            onClick={() => setFiltro(key)}
            className={`flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-medium transition-colors ${
              filtro === key
                ? 'bg-primary text-primary-foreground'
                : 'text-muted-foreground hover:text-foreground hover:bg-secondary'
            }`}
          >
            {label}
            <span className={`text-[10px] tabular-nums ${filtro === key ? 'opacity-80' : 'opacity-60'}`}>
              {counts[key]}
            </span>
          </button>
        ))}
      </div>

      {/* Grid */}
      <div className="flex-1 overflow-y-auto p-6">
        {isLoading ? (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {Array.from({ length: 6 }).map((_, i) => <Skeleton key={i} className="h-48 rounded-lg" />)}
          </div>
        ) : !lotesFiltrados.length ? (
          <EmptyState
            icon={<Package className="w-5 h-5" />}
            title={filtro === 'todos' ? 'Sin lotes' : `Sin lotes en "${filtro === 'EnPreparacion' ? 'En preparación' : filtro}"`}
            description={filtro === 'todos' ? 'Crea el primer lote para empezar.' : 'Prueba con otro filtro.'}
            action={filtro === 'todos'
              ? <Button size="sm" onClick={() => setModalAbierto(true)}><Plus className="w-3.5 h-3.5" />Crear lote</Button>
              : undefined}
          />
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {lotesFiltrados.map(lote => <LoteCard key={lote.id} lote={lote} onVerDetalle={id => navigate(`/lotes/${id}`)} />)}
          </div>
        )}
      </div>

      <CrearLoteModal open={modalAbierto} onClose={() => setModalAbierto(false)} />
    </div>
  )
}
