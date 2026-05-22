import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Plus, Package, Users, X, CheckCircle2 } from 'lucide-react'
import { useLotes, useCreateLote } from '@/hooks/useFeedlot'
import {
  PageHeader, Button, Card, Badge, Skeleton, EmptyState,
  Dialog, DialogHeader, DialogTitle, DialogDescription,
  FormField, Input, Alert,
} from '@/components/ui'
import { fmt } from '@/utils'
import type { LoteResumen } from '@/types'

// ─── Schema de validación ─────────────────────────────────────────────────────
const crearLoteSchema = z.object({
  codigo: z
    .string()
    .min(2, 'Mínimo 2 caracteres')
    .max(20, 'Máximo 20 caracteres')
    .regex(/^[A-Za-z0-9-]+$/, 'Solo letras, números y guiones'),
  nombre: z
    .string()
    .min(3, 'Mínimo 3 caracteres')
    .max(100, 'Máximo 100 caracteres'),
  capacidadMaxima: z
    .number({ invalid_type_error: 'Ingresa un número' })
    .int('Debe ser un número entero')
    .min(1, 'Mínimo 1 animal')
    .max(10000, 'Máximo 10.000 animales'),
})

type CrearLoteForm = z.infer<typeof crearLoteSchema>

// ─── Modal de creación ────────────────────────────────────────────────────────
function CrearLoteModal({
  open,
  onClose,
}: {
  open: boolean
  onClose: () => void
}) {
  const [exito, setExito] = useState(false)
  const [errorApi, setErrorApi] = useState<string>()
  const createLote = useCreateLote()

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<CrearLoteForm>({
    resolver: zodResolver(crearLoteSchema),
    defaultValues: {
      codigo: '',
      nombre: '',
      capacidadMaxima: undefined,
    },
  })

  const handleClose = () => {
    reset()
    setExito(false)
    setErrorApi(undefined)
    onClose()
  }

  const onSubmit = async (data: CrearLoteForm) => {
    setErrorApi(undefined)
    try {
      await createLote.mutateAsync({
        codigo: data.codigo.toUpperCase(),
        nombre: data.nombre,
        capacidadMaxima: data.capacidadMaxima,
      })
      setExito(true)
      // Cerrar automáticamente tras 1.5 segundos
      setTimeout(() => handleClose(), 1500)
    } catch (err: any) {
      const msg =
        err?.response?.data?.error ??
        err?.response?.data?.detail ??
        'Error al crear el lote. Intenta de nuevo.'
      setErrorApi(msg)
    }
  }

  return (
    <Dialog open={open} onClose={handleClose}>
      <div className="rounded-xl border border-border bg-card shadow-xl">
        {/* Header */}
        <div className="flex items-center justify-between px-5 py-4 border-b border-border">
          <DialogHeader className="mb-0">
            <DialogTitle>Crear nuevo lote</DialogTitle>
            <DialogDescription>
              Define el código, nombre y capacidad del lote de engorde.
            </DialogDescription>
          </DialogHeader>
          <button
            onClick={handleClose}
            className="text-muted-foreground hover:text-foreground transition-colors ml-4 flex-shrink-0"
          >
            <X className="w-4 h-4" />
          </button>
        </div>

        {/* Body */}
        <div className="p-5">
          {exito ? (
            <div className="flex flex-col items-center justify-center py-6 gap-3 animate-fade-in">
              <div className="w-12 h-12 rounded-full bg-emerald-500/10 flex items-center justify-center">
                <CheckCircle2 className="w-6 h-6 text-emerald-400" />
              </div>
              <p className="text-sm font-medium">¡Lote creado exitosamente!</p>
              <p className="text-xs text-muted-foreground">Cerrando...</p>
            </div>
          ) : (
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              {/* Código */}
              <FormField
                label="Código"
                error={errors.codigo?.message}
                required
                hint="Ej: LOT-001 — único, se guarda en mayúsculas"
              >
                <Input
                  {...register('codigo')}
                  placeholder="LOT-001"
                  className={errors.codigo ? 'border-destructive' : ''}
                  autoFocus
                  style={{ textTransform: 'uppercase' }}
                />
              </FormField>

              {/* Nombre */}
              <FormField
                label="Nombre"
                error={errors.nombre?.message}
                required
                hint="Nombre descriptivo del lote"
              >
                <Input
                  {...register('nombre')}
                  placeholder="Novillos Brahman 2024"
                  className={errors.nombre ? 'border-destructive' : ''}
                />
              </FormField>

              {/* Capacidad */}
              <FormField
                label="Capacidad máxima"
                error={errors.capacidadMaxima?.message}
                required
                hint="Número máximo de animales que puede contener este lote"
              >
                <Input
                  {...register('capacidadMaxima', { valueAsNumber: true })}
                  type="number"
                  min={1}
                  max={10000}
                  placeholder="50"
                  className={errors.capacidadMaxima ? 'border-destructive' : ''}
                />
              </FormField>

              {/* Error API */}
              {errorApi && (
                <Alert variant="destructive">
                  {errorApi}
                </Alert>
              )}

              {/* Acciones */}
              <div className="flex gap-2 pt-1">
                <Button
                  type="button"
                  variant="outline"
                  className="flex-1"
                  onClick={handleClose}
                  disabled={isSubmitting}
                >
                  Cancelar
                </Button>
                <Button
                  type="submit"
                  className="flex-1"
                  loading={isSubmitting}
                >
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

// ─── Card de lote ─────────────────────────────────────────────────────────────
function LoteCard({ lote }: { lote: LoteResumen }) {
  const pct = lote.porcentajeOcupacion

  const estadoStyle = {
    Activo: 'bg-emerald-500/10 text-emerald-400 border-emerald-500/20',
    EnPreparacion: 'bg-amber-500/10 text-amber-400 border-amber-500/20',
    Cerrado: 'bg-zinc-500/10 text-zinc-400 border-zinc-500/20',
  }[lote.estado] ?? 'bg-zinc-500/10 text-zinc-400 border-zinc-500/20'

  const barColor =
    pct >= 90 ? 'bg-rose-400' :
    pct >= 70 ? 'bg-amber-400' :
    'bg-primary'

  return (
    <Card className="p-5 hover:border-border/80 transition-all cursor-pointer group">
      {/* Header */}
      <div className="flex items-start justify-between mb-4">
        <div className="flex items-center gap-2.5">
          <div className="w-9 h-9 rounded-lg bg-primary/10 flex items-center justify-center flex-shrink-0">
            <Package className="w-4 h-4 text-primary" />
          </div>
          <div>
            <p className="text-sm font-semibold font-mono">{lote.codigo}</p>
            <p className="text-xs text-muted-foreground truncate max-w-[140px]">{lote.nombre}</p>
          </div>
        </div>
        <Badge className={estadoStyle}>
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
            <span className="text-muted-foreground font-normal">
              /{lote.capacidadMaxima}
            </span>
          </span>
        </div>

        {/* Barra de progreso */}
        <div className="h-1.5 rounded-full bg-border overflow-hidden">
          <div
            className={`h-full rounded-full transition-all ${barColor}`}
            style={{ width: `${Math.min(pct, 100)}%` }}
          />
        </div>

        <div className="flex items-center justify-between">
          <span className="text-[10px] text-muted-foreground">
            {lote.capacidadMaxima - lote.animalesActuales} disponibles
          </span>
          <span className="text-[10px] font-medium tabular-nums">
            {fmt.pct(pct)}
          </span>
        </div>
      </div>
    </Card>
  )
}

// ─── Página principal ─────────────────────────────────────────────────────────
export default function LotesPage() {
  const [modalAbierto, setModalAbierto] = useState(false)
  const [filtroEstado, setFiltroEstado] = useState<'todos' | 'Activo' | 'EnPreparacion' | 'Cerrado'>('todos')

  const { data: lotes, isLoading } = useLotes()

  const lotesArray = (lotes as LoteResumen[] | undefined) ?? []

  const lotesFiltrados = filtroEstado === 'todos'
    ? lotesArray
    : lotesArray.filter(l => l.estado === filtroEstado)

  const totalActivos = lotesArray.filter(l => l.estado === 'Activo').length
  const totalAnimales = lotesArray.reduce((s, l) => s + l.animalesActuales, 0)

  return (
    <div className="flex flex-col h-full animate-fade-in">
      <PageHeader
        title="Lotes"
        description={`${lotesArray.length} lotes · ${totalAnimales} animales`}
        action={
          <Button size="sm" onClick={() => setModalAbierto(true)}>
            <Plus className="w-3.5 h-3.5" />
            Crear lote
          </Button>
        }
      />

      {/* Filtros de estado */}
      <div className="flex items-center gap-2 px-6 py-3 border-b border-border">
        {(['todos', 'Activo', 'EnPreparacion', 'Cerrado'] as const).map((estado) => (
          <button
            key={estado}
            onClick={() => setFiltroEstado(estado)}
            className={`px-3 py-1 rounded-full text-xs font-medium transition-colors ${
              filtroEstado === estado
                ? 'bg-primary text-primary-foreground'
                : 'text-muted-foreground hover:text-foreground hover:bg-secondary'
            }`}
          >
            {estado === 'todos' ? 'Todos' :
             estado === 'EnPreparacion' ? 'En preparación' : estado}
            {estado !== 'todos' && (
              <span className="ml-1.5 opacity-70">
                {lotesArray.filter(l => l.estado === estado).length}
              </span>
            )}
          </button>
        ))}
      </div>

      {/* Contenido */}
      <div className="flex-1 overflow-y-auto p-6">
        {isLoading ? (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {Array.from({ length: 6 }).map((_, i) => (
              <Skeleton key={i} className="h-40 rounded-lg" />
            ))}
          </div>
        ) : !lotesFiltrados.length ? (
          <EmptyState
            icon={<Package className="w-5 h-5" />}
            title={filtroEstado === 'todos' ? 'Sin lotes' : `Sin lotes en estado "${filtroEstado}"`}
            description={
              filtroEstado === 'todos'
                ? 'Crea el primer lote para comenzar a organizar los animales.'
                : 'Prueba con otro filtro.'
            }
            action={
              filtroEstado === 'todos' ? (
                <Button size="sm" onClick={() => setModalAbierto(true)}>
                  <Plus className="w-3.5 h-3.5" />
                  Crear lote
                </Button>
              ) : undefined
            }
          />
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {lotesFiltrados.map((lote) => (
              <LoteCard key={lote.id} lote={lote} />
            ))}
          </div>
        )}
      </div>

      {/* Modal */}
      <CrearLoteModal
        open={modalAbierto}
        onClose={() => setModalAbierto(false)}
      />
    </div>
  )
}
