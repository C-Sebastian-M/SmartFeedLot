import { useParams, useNavigate } from 'react-router-dom'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import {
  ArrowLeft, Beef, Scale, AlertTriangle,
  Calendar, TrendingUp, X, CheckCircle2, Plus, Pencil, Trash2
} from 'lucide-react'
import { useAnimal, useRegistrarPesaje, useRegistrarEventoSanitario, useActualizarAnimal, useEliminarPesaje, useEliminarAnimal } from '@/hooks/useFeedlot'
import {
  Button, Card, CardHeader, CardTitle, CardContent, Badge,
  Skeleton, EmptyState, Dialog, DialogHeader, DialogTitle,
  DialogDescription, FormField, Input, Alert,
} from '@/components/ui'
import {
  fmt, estadoProductivoColor, estadoSanitarioColor, severidadColor,
} from '@/utils'
import type { Animal, Pesaje, EventoSanitario, SeveridadEvento } from '@/types'
import {
  LineChart, Line, XAxis, YAxis, CartesianGrid,
  Tooltip, ResponsiveContainer, ReferenceLine
} from 'recharts'

// ─── Schema pesaje ────────────────────────────────────────────────────────────
const pesajeSchema = z.object({
  fechaPesaje: z.string().min(1, 'Requerida'),
  pesoKg: z.number({ invalid_type_error: 'Número requerido' }).positive('Mayor a 0').max(2000),
  observaciones: z.string().max(500).optional(),
})
type PesajeForm = z.infer<typeof pesajeSchema>

// ─── Schema evento sanitario ──────────────────────────────────────────────────
const eventoSchema = z.object({
  fechaEvento: z.string().min(1, 'Requerida'),
  diagnostico: z.string().min(3, 'Mínimo 3 caracteres').max(200),
  descripcion: z.string().min(5, 'Mínimo 5 caracteres').max(1000),
  severidad: z.enum(['Leve', 'Moderado', 'Grave', 'Critico'], { required_error: 'Requerida' }),
  tratamiento: z.string().max(500).optional(),
})
type EventoForm = z.infer<typeof eventoSchema>

// ─── Select simple ────────────────────────────────────────────────────────────
function Select({
  options, value, onChange, placeholder, error,
}: {
  options: { value: string; label: string }[]
  value?: string
  onChange: (v: string) => void
  placeholder?: string
  error?: boolean
}) {
  return (
    <select
      value={value ?? ''}
      onChange={e => onChange(e.target.value)}
      className={`flex h-9 w-full rounded-md border bg-transparent px-3 py-1 text-sm shadow-sm transition-colors
        focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring
        ${error ? 'border-destructive' : 'border-input'} [&>option]:bg-card`}
    >
      {placeholder && <option value="">{placeholder}</option>}
      {options.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
    </select>
  )
}

// ─── Modal registrar pesaje ───────────────────────────────────────────────────
function RegistrarPesajeModal({ animalId, open, onClose }: {
  animalId: string; open: boolean; onClose: () => void
}) {
  const [exito, setExito] = useState(false)
  const [errorApi, setErrorApi] = useState<string>()
  const mutation = useRegistrarPesaje()
  const today = new Date().toISOString().split('T')[0]

  const { register, handleSubmit, reset, formState: { errors, isSubmitting } } =
    useForm<PesajeForm>({
      resolver: zodResolver(pesajeSchema),
      defaultValues: { fechaPesaje: today },
    })

  const handleClose = () => { reset(); setExito(false); setErrorApi(undefined); onClose() }

  const onSubmit = async (data: PesajeForm) => {
    setErrorApi(undefined)
    try {
      await mutation.mutateAsync({
        animalId,
        fechaPesaje: data.fechaPesaje,
        pesoKg: data.pesoKg,
        observaciones: data.observaciones || undefined,
      })
      setExito(true)
      setTimeout(handleClose, 1500)
    } catch (err: any) {
      setErrorApi(err?.response?.data?.error ?? err?.response?.data?.detail ?? 'Error al registrar el pesaje.')
    }
  }

  return (
    <Dialog open={open} onClose={handleClose}>
      <div className="rounded-xl border border-border bg-card shadow-xl">
        <div className="flex items-center justify-between px-5 py-4 border-b border-border">
          <DialogHeader className="mb-0">
            <DialogTitle>Registrar pesaje</DialogTitle>
            <DialogDescription>Ingresa la fecha y el peso actual del animal.</DialogDescription>
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
              <p className="text-sm font-medium">Pesaje registrado</p>
            </div>
          ) : (
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              <FormField label="Fecha de pesaje" error={errors.fechaPesaje?.message} required>
                <Input {...register('fechaPesaje')} type="date" max={today}
                  className={errors.fechaPesaje ? 'border-destructive' : ''} />
              </FormField>
              <FormField label="Peso (kg)" error={errors.pesoKg?.message} required>
                <Input {...register('pesoKg', { valueAsNumber: true })}
                  type="number" step="0.1" min={1} max={2000} placeholder="325.5"
                  className={errors.pesoKg ? 'border-destructive' : ''} autoFocus />
              </FormField>
              <FormField label="Observaciones" error={errors.observaciones?.message}>
                <Input {...register('observaciones')} placeholder="Opcional" />
              </FormField>
              {errorApi && <Alert variant="destructive">{errorApi}</Alert>}
              <div className="flex gap-2">
                <Button type="button" variant="outline" className="flex-1" onClick={handleClose} disabled={isSubmitting}>Cancelar</Button>
                <Button type="submit" className="flex-1" loading={isSubmitting}>
                  <Scale className="w-3.5 h-3.5" />Registrar
                </Button>
              </div>
            </form>
          )}
        </div>
      </div>
    </Dialog>
  )
}

// ─── Modal evento sanitario ───────────────────────────────────────────────────
function RegistrarEventoModal({ animalId, open, onClose }: {
  animalId: string; open: boolean; onClose: () => void
}) {
  const [exito, setExito] = useState(false)
  const [errorApi, setErrorApi] = useState<string>()
  const mutation = useRegistrarEventoSanitario()
  const today = new Date().toISOString().split('T')[0]

  const { register, handleSubmit, reset, setValue, watch, formState: { errors, isSubmitting } } =
    useForm<EventoForm>({ resolver: zodResolver(eventoSchema) })

  const handleClose = () => { reset(); setExito(false); setErrorApi(undefined); onClose() }

  const onSubmit = async (data: EventoForm) => {
    setErrorApi(undefined)
    try {
      await mutation.mutateAsync({
        animalId,
        fechaEvento: data.fechaEvento,
        diagnostico: data.diagnostico,
        descripcion: data.descripcion,
        severidad: data.severidad,
        tratamiento: data.tratamiento || undefined,
      })
      setExito(true)
      setTimeout(handleClose, 1500)
    } catch (err: any) {
      setErrorApi(err?.response?.data?.error ?? 'Error al registrar el evento.')
    }
  }

  return (
    <Dialog open={open} onClose={handleClose}>
      <div className="rounded-xl border border-border bg-card shadow-xl max-h-[90vh] overflow-y-auto">
        <div className="flex items-center justify-between px-5 py-4 border-b border-border sticky top-0 bg-card">
          <DialogHeader className="mb-0">
            <DialogTitle>Registrar evento sanitario</DialogTitle>
            <DialogDescription>Documenta el diagnóstico y tratamiento.</DialogDescription>
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
              <p className="text-sm font-medium">Evento registrado</p>
            </div>
          ) : (
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              <div className="grid grid-cols-2 gap-3">
                <FormField label="Fecha" error={errors.fechaEvento?.message} required>
                  <Input {...register('fechaEvento')} type="date" max={today}
                    className={errors.fechaEvento ? 'border-destructive' : ''} />
                </FormField>
                <FormField label="Severidad" error={errors.severidad?.message} required>
                  <Select
                    placeholder="Seleccionar..."
                    options={[
                      { value: 'Leve', label: 'Leve' },
                      { value: 'Moderado', label: 'Moderado' },
                      { value: 'Grave', label: 'Grave' },
                      { value: 'Critico', label: 'Crítico' },
                    ]}
                    value={watch('severidad')}
                    onChange={v => setValue('severidad', v as SeveridadEvento, { shouldValidate: true })}
                    error={!!errors.severidad}
                  />
                </FormField>
              </div>
              <FormField label="Diagnóstico" error={errors.diagnostico?.message} required>
                <Input {...register('diagnostico')} placeholder="Ej: Neumonía bovina"
                  className={errors.diagnostico ? 'border-destructive' : ''} autoFocus />
              </FormField>
              <FormField label="Descripción" error={errors.descripcion?.message} required>
                <Input {...register('descripcion')} placeholder="Síntomas observados..."
                  className={errors.descripcion ? 'border-destructive' : ''} />
              </FormField>
              <FormField label="Tratamiento" error={errors.tratamiento?.message}>
                <Input {...register('tratamiento')} placeholder="Medicamento, dosis, vía de administración..." />
              </FormField>
              {errorApi && <Alert variant="destructive">{errorApi}</Alert>}
              <div className="flex gap-2">
                <Button type="button" variant="outline" className="flex-1" onClick={handleClose} disabled={isSubmitting}>Cancelar</Button>
                <Button type="submit" className="flex-1" loading={isSubmitting}>
                  <AlertTriangle className="w-3.5 h-3.5" />Registrar
                </Button>
              </div>
            </form>
          )}
        </div>
      </div>
    </Dialog>
  )
}

// ─── Tooltip del gráfico ──────────────────────────────────────────────────────
const PesajeTooltip = ({ active, payload, label }: any) => {
  if (!active || !payload?.length) return null
  return (
    <div className="rounded-lg border border-border bg-card px-3 py-2 shadow-lg text-xs">
      <p className="font-medium mb-1">{label}</p>
      <p className="text-emerald-400">Peso: {payload[0]?.value?.toFixed(1)} kg</p>
    </div>
  )
}

// ─── Schema modificar animal ───────────────────────────────────────────────────
const modificarSchema = z.object({
  nombre: z.string().max(100, 'Máximo 100 caracteres').optional(),
  numeroArete: z.string().min(1, 'Requerido').max(50, 'Máximo 50 caracteres'),
  sexo: z.enum(['Macho', 'Hembra'], { required_error: 'Requerido' }),
  raza: z.string().max(100, 'Máximo 100 caracteres').optional(),
  fechaNacimiento: z.string().optional(),
  fechaIngreso: z.string().min(1, 'Requerida'),
  pesoIngresoKg: z.number({ invalid_type_error: 'Número requerido' }).positive('Mayor a 0'),
  precioCompra: z.number({ invalid_type_error: 'Número requerido' }).min(0, 'No negativo'),
  moneda: z.enum(['COP', 'USD', 'EUR']).default('COP'),
})
type ModificarForm = z.infer<typeof modificarSchema>

// ─── Modal modificar animal ────────────────────────────────────────────────────
function formatPrecio(value: number) {
  return Math.floor(value).toLocaleString('es-CO')
}

function ModificarAnimalModal({ animal, open, onClose }: {
  animal: Animal
  open: boolean
  onClose: () => void
}) {
  const [exito, setExito] = useState(false)
  const [errorApi, setErrorApi] = useState<string>()
  const [precioDisplay, setPrecioDisplay] = useState(formatPrecio(animal.precioCompra))
  const mutation = useActualizarAnimal()

  const { register, handleSubmit, reset, setValue, formState: { errors, isSubmitting } } =
    useForm<ModificarForm>({
      resolver: zodResolver(modificarSchema),
      values: {
        nombre: animal.nombre,
        numeroArete: animal.numeroArete,
        sexo: animal.sexo,
        raza: animal.raza ?? '',
        fechaNacimiento: animal.fechaNacimiento ?? '',
        fechaIngreso: animal.fechaIngreso,
        pesoIngresoKg: animal.pesoIngresoKg,
        precioCompra: animal.precioCompra,
        moneda: animal.moneda as 'COP' | 'USD' | 'EUR',
      },
    })

  const handleClose = () => { reset(); setExito(false); setErrorApi(undefined); setPrecioDisplay(formatPrecio(animal.precioCompra)); onClose() }

  const onSubmit = async (data: ModificarForm) => {
    setErrorApi(undefined)
    try {
      await mutation.mutateAsync({
        id: animal.id,
        nombre: data.nombre || undefined,
        numeroArete: data.numeroArete,
        sexo: data.sexo,
        raza: data.raza || undefined,
        fechaNacimiento: data.fechaNacimiento || undefined,
        fechaIngreso: data.fechaIngreso,
        pesoIngresoKg: data.pesoIngresoKg,
        precioCompra: data.precioCompra,
        moneda: data.moneda,
      })
      setExito(true)
      setTimeout(handleClose, 1500)
    } catch (err: any) {
      setErrorApi(err?.response?.data?.error ?? err?.response?.data?.detail ?? 'Error al modificar el animal.')
    }
  }

  return (
    <Dialog open={open} onClose={handleClose}>
      <div className="rounded-xl border border-border bg-card shadow-xl max-h-[90vh] overflow-y-auto">
        <div className="flex items-center justify-between px-5 py-4 border-b border-border sticky top-0 bg-card z-10">
          <DialogHeader className="mb-0">
            <DialogTitle>Modificar animal</DialogTitle>
            <DialogDescription>{animal.codigoIdentificacion}</DialogDescription>
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
              <p className="text-sm font-medium">¡Animal modificado!</p>
            </div>
          ) : (
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              <div className="grid grid-cols-2 gap-3">
                <FormField label="Nombre" error={errors.nombre?.message} hint="Opcional">
                  <Input {...register('nombre')} placeholder="Ej: La Flaca"
                    className={errors.nombre ? 'border-destructive' : ''} />
                </FormField>
                <FormField label="Número de arete" error={errors.numeroArete?.message} required>
                  <Input {...register('numeroArete')} placeholder="AR-0001"
                    className={errors.numeroArete ? 'border-destructive' : ''} />
                </FormField>
                <FormField label="Sexo" error={errors.sexo?.message} required>
                  <select {...register('sexo')}
                    className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm [&>option]:bg-card">
                    <option value="Macho">Macho</option>
                    <option value="Hembra">Hembra</option>
                  </select>
                </FormField>
                <FormField label="Raza" error={errors.raza?.message}>
                  <Input {...register('raza')} placeholder="Brahman"
                    className={errors.raza ? 'border-destructive' : ''} />
                </FormField>
                <FormField label="Fecha de nacimiento" error={errors.fechaNacimiento?.message} hint="Opcional">
                  <Input {...register('fechaNacimiento')} type="date"
                    className={errors.fechaNacimiento ? 'border-destructive' : ''} />
                </FormField>
                <FormField label="Fecha de ingreso" error={errors.fechaIngreso?.message} required>
                  <Input {...register('fechaIngreso')} type="date"
                    className={errors.fechaIngreso ? 'border-destructive' : ''} />
                </FormField>
                <FormField label="Peso ingreso (kg)" error={errors.pesoIngresoKg?.message} required>
                  <Input {...register('pesoIngresoKg', { valueAsNumber: true })}
                    type="number" step="0.1" min={1}
                    className={errors.pesoIngresoKg ? 'border-destructive' : ''} />
                </FormField>
                <FormField label="Precio compra ($)" error={errors.precioCompra?.message} required>
                  <input
                    type="text" inputMode="numeric"
                    value={precioDisplay}
                    onChange={e => {
                      const raw = e.target.value.replace(/[^0-9]/g, '')
                      const num = raw ? parseInt(raw) : 0
                      setValue('precioCompra', num, { shouldValidate: true })
                      setPrecioDisplay(raw ? formatPrecio(num) : '')
                    }}
                    placeholder="1.000.000"
                    className={`flex h-9 w-full rounded-md border bg-transparent px-3 py-1 text-sm shadow-sm transition-colors
                      focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring
                      ${errors.precioCompra ? 'border-destructive' : 'border-input'}`}
                  />
                </FormField>
              </div>

              {errorApi && <Alert variant="destructive">{errorApi}</Alert>}

              <div className="flex gap-2 pt-1">
                <Button type="button" variant="outline" className="flex-1" onClick={handleClose} disabled={isSubmitting}>
                  Cancelar
                </Button>
                <Button type="submit" className="flex-1" loading={isSubmitting}>
                  <Beef className="w-3.5 h-3.5" />
                  Guardar cambios
                </Button>
              </div>
            </form>
          )}
        </div>
      </div>
    </Dialog>
  )
}

// ─── Página principal ─────────────────────────────────────────────────────────
export default function AnimalDetallePage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [modalPesaje, setModalPesaje] = useState(false)
  const [modalEvento, setModalEvento] = useState(false)
  const [modalEditar, setModalEditar] = useState(false)

  const [eliminandoPesajeId, setEliminandoPesajeId] = useState<string | null>(null)
  const eliminarPesaje = useEliminarPesaje()
  const eliminarAnimal = useEliminarAnimal()

  const handleEliminarPesaje = async (pesajeId: string) => {
    setEliminandoPesajeId(pesajeId)
    try {
      await eliminarPesaje.mutateAsync({ animalId: id!, pesajeId })
    } finally {
      setEliminandoPesajeId(null)
    }
  }

  const { data: animal, isLoading, error } = useAnimal(id ?? '')

  if (isLoading) {
    return (
      <div className="p-6 space-y-4">
        <Skeleton className="h-8 w-48" />
        <div className="grid grid-cols-4 gap-4">
          {Array.from({ length: 4 }).map((_, i) => <Skeleton key={i} className="h-24 rounded-lg" />)}
        </div>
        <Skeleton className="h-64 rounded-lg" />
      </div>
    )
  }

  if (error || !animal) {
    return (
      <div className="p-6">
        <EmptyState
          icon={<Beef className="w-5 h-5" />}
          title="Animal no encontrado"
          description="El animal que buscas no existe o fue eliminado."
          action={<Button size="sm" onClick={() => navigate('/animales')}>Volver a animales</Button>}
        />
      </div>
    )
  }

  // Datos para el gráfico de evolución de peso
  const datosGrafico = [
    { fecha: fmt.fecha(animal.fechaIngreso), peso: animal.pesoIngresoKg, label: 'Ingreso' },
    ...animal.pesajes
      ?.sort((a: Pesaje, b: Pesaje) => a.fechaPesaje.localeCompare(b.fechaPesaje))
      .map((p: Pesaje) => ({
        fecha: fmt.fecha(p.fechaPesaje),
        peso: p.pesoKg,
        label: p.observaciones,
      })) ?? [],
  ]

  const pesoGanado = animal.pesoActualKg - animal.pesoIngresoKg
  const gmd = animal.diasEnEngorde > 0 ? pesoGanado / animal.diasEnEngorde : 0

  const pesajes: Pesaje[] = animal.pesajes
    ?.sort((a: Pesaje, b: Pesaje) => b.fechaPesaje.localeCompare(a.fechaPesaje)) ?? []

  const eventos: EventoSanitario[] = animal.eventosSanitarios
    ?.sort((a: EventoSanitario, b: EventoSanitario) =>
      b.fechaEvento.localeCompare(a.fechaEvento)) ?? []

  return (
    <div className="flex flex-col h-full animate-fade-in">
      {/* Header */}
      <div className="flex items-center justify-between px-6 py-4 border-b border-border">
        <div className="flex items-center gap-3">
          <button
            onClick={() => navigate('/animales')}
            className="text-muted-foreground hover:text-foreground transition-colors"
          >
            <ArrowLeft className="w-4 h-4" />
          </button>
          <div>
            <div className="flex items-center gap-2">
              <h1 className="text-base font-semibold font-mono">{animal.codigoIdentificacion}</h1>
              <Badge className={estadoProductivoColor[animal.estadoProductivo]}>
                {animal.estadoProductivo === 'EnEngorde' ? 'En engorde' : animal.estadoProductivo}
              </Badge>
              <Badge className={estadoSanitarioColor[animal.estadoSanitario]}>
                {animal.estadoSanitario}
              </Badge>
            </div>
              <p className="text-xs text-muted-foreground">
                {animal.raza} · {animal.sexo}{animal.nombre ? ` · "${animal.nombre}"` : ''} · Arete {animal.numeroArete}
              </p>
          </div>
        </div>
        <div className="flex gap-2">
          <Button size="sm" variant="outline" className="text-destructive border-destructive/30 hover:bg-destructive/10" onClick={() => {
            if (window.confirm(`¿Eliminar ${animal.codigoIdentificacion}? Esta acción no se puede deshacer.`))
              eliminarAnimal.mutate(id!)
          }}>
            <Trash2 className="w-3.5 h-3.5" />
            Eliminar
          </Button>
          <Button size="sm" variant="outline" onClick={() => setModalEditar(true)}>
            <Pencil className="w-3.5 h-3.5" />
            Editar
          </Button>
          {animal.estadoProductivo === 'EnEngorde' && (
            <>
              <Button size="sm" variant="outline" onClick={() => setModalEvento(true)}>
                <AlertTriangle className="w-3.5 h-3.5" />
                Evento sanitario
              </Button>
              <Button size="sm" onClick={() => setModalPesaje(true)}>
                <Scale className="w-3.5 h-3.5" />
                Registrar pesaje
              </Button>
            </>
          )}
        </div>
      </div>

      <div className="flex-1 overflow-y-auto p-6 space-y-6">
        {/* KPIs */}
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
          {[
            { label: 'Peso actual', value: fmt.kg(animal.pesoActualKg), icon: <Scale className="w-4 h-4" /> },
            { label: 'Peso ganado', value: fmt.kg(pesoGanado), icon: <TrendingUp className="w-4 h-4" /> },
            { label: 'GMD', value: fmt.kgDia(gmd), icon: <TrendingUp className="w-4 h-4" /> },
            { label: 'Días en engorde', value: `${animal.diasEnEngorde}d`, icon: <Calendar className="w-4 h-4" /> },
          ].map(({ label, value, icon }) => (
            <Card key={label} className="p-5">
              <div className="flex items-start justify-between">
                <div>
                  <p className="text-[10px] font-medium text-muted-foreground uppercase tracking-wide mb-1">{label}</p>
                  <p className="text-xl font-bold tabular-nums">{value}</p>
                </div>
                <div className="w-8 h-8 rounded-lg bg-primary/10 flex items-center justify-center text-primary">
                  {icon}
                </div>
              </div>
            </Card>
          ))}
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
          {/* Gráfico evolución peso */}
          <Card className="lg:col-span-2">
            <CardHeader>
              <CardTitle>Evolución de peso</CardTitle>
            </CardHeader>
            <CardContent>
              {datosGrafico.length < 2 ? (
                <EmptyState
                  icon={<Scale className="w-4 h-4" />}
                  title="Sin pesajes suficientes"
                  description="Registra al menos un pesaje para ver la evolución."
                />
              ) : (
                <ResponsiveContainer width="100%" height={200}>
                  <LineChart data={datosGrafico} margin={{ top: 4, right: 4, bottom: 0, left: -20 }}>
                    <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" strokeOpacity={0.5} />
                    <XAxis dataKey="fecha" tick={{ fontSize: 10, fill: 'hsl(var(--muted-foreground))' }} tickLine={false} axisLine={false} />
                    <YAxis tick={{ fontSize: 10, fill: 'hsl(var(--muted-foreground))' }} tickLine={false} axisLine={false} domain={['dataMin - 20', 'dataMax + 20']} />
                    <Tooltip content={<PesajeTooltip />} />
                    <ReferenceLine y={animal.pesoIngresoKg} stroke="hsl(var(--muted-foreground))" strokeDasharray="4 2" strokeOpacity={0.5}
                      label={{ value: 'Ingreso', position: 'right', fontSize: 9, fill: 'hsl(var(--muted-foreground))' }} />
                    <Line type="monotone" dataKey="peso" stroke="#4ade80" strokeWidth={2}
                      dot={{ r: 3, fill: '#4ade80', strokeWidth: 0 }}
                      activeDot={{ r: 5, fill: '#4ade80', strokeWidth: 0 }} />
                  </LineChart>
                </ResponsiveContainer>
              )}
            </CardContent>
          </Card>

          {/* Info del animal */}
          <Card>
            <CardHeader><CardTitle>Información</CardTitle></CardHeader>
            <CardContent>
              <dl className="space-y-2.5">
                {[
                  { label: 'Código', value: animal.codigoIdentificacion },
                  { label: 'Nombre', value: animal.nombre ?? '—' },
                  { label: 'Arete', value: animal.numeroArete },
                  { label: 'Raza', value: animal.raza },
                  { label: 'Sexo', value: animal.sexo },
                  { label: 'Nacimiento', value: animal.fechaNacimiento ? fmt.fecha(animal.fechaNacimiento) : 'No registrada' },
                  { label: 'Ingreso', value: fmt.fecha(animal.fechaIngreso) },
                  { label: 'Peso ingreso', value: fmt.kg(animal.pesoIngresoKg) },
                  { label: 'Precio compra', value: fmt.cop(animal.precioCompra) },
                ].map(({ label, value }) => (
                  <div key={label} className="flex items-center justify-between">
                    <dt className="text-xs text-muted-foreground">{label}</dt>
                    <dd className="text-xs font-medium">{value}</dd>
                  </div>
                ))}
              </dl>
            </CardContent>
          </Card>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
          {/* Historial de pesajes */}
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle>Pesajes ({pesajes.length})</CardTitle>
                {animal.estadoProductivo === 'EnEngorde' && (
                  <Button size="sm" variant="ghost" onClick={() => setModalPesaje(true)}>
                    <Plus className="w-3.5 h-3.5" />
                  </Button>
                )}
              </div>
            </CardHeader>
            <CardContent>
              {!pesajes.length ? (
                <EmptyState icon={<Scale className="w-4 h-4" />} title="Sin pesajes" description="Registra el primer pesaje." />
              ) : (
                <div className="space-y-2 max-h-64 overflow-y-auto">
                  {pesajes.map((p, i) => (
                    <div key={p.id} className="flex items-center justify-between p-2.5 rounded-lg bg-secondary/40 group">
                      <div>
                        <p className="text-xs font-medium">{fmt.fecha(p.fechaPesaje)}</p>
                        {p.observaciones && <p className="text-[10px] text-muted-foreground">{p.observaciones}</p>}
                      </div>
                      <div className="flex items-center gap-2">
                        <div className="text-right">
                          <p className="text-sm font-bold tabular-nums">{fmt.kg(p.pesoKg)}</p>
                          {i < pesajes.length - 1 && (
                            <p className={`text-[10px] tabular-nums ${p.pesoKg > pesajes[i + 1].pesoKg ? 'text-emerald-400' : 'text-rose-400'}`}>
                              {p.pesoKg > pesajes[i + 1].pesoKg ? '+' : ''}{fmt.kg(p.pesoKg - pesajes[i + 1].pesoKg)}
                            </p>
                          )}
                        </div>
                        <button
                          onClick={(e) => { e.stopPropagation(); handleEliminarPesaje(p.id) }}
                          disabled={eliminandoPesajeId === p.id}
                          className="opacity-0 group-hover:opacity-100 transition-opacity p-1 rounded hover:bg-destructive/10 text-muted-foreground hover:text-destructive"
                          title="Eliminar pesaje"
                        >
                          <Trash2 className="w-3.5 h-3.5" />
                        </button>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>

          {/* Eventos sanitarios */}
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle>Eventos sanitarios ({eventos.length})</CardTitle>
                {animal.estadoProductivo === 'EnEngorde' && (
                  <Button size="sm" variant="ghost" onClick={() => setModalEvento(true)}>
                    <Plus className="w-3.5 h-3.5" />
                  </Button>
                )}
              </div>
            </CardHeader>
            <CardContent>
              {!eventos.length ? (
                <EmptyState icon={<AlertTriangle className="w-4 h-4" />} title="Sin eventos" description="Sin eventos sanitarios registrados." />
              ) : (
                <div className="space-y-2 max-h-64 overflow-y-auto">
                  {eventos.map(e => (
                    <div key={e.id} className="p-2.5 rounded-lg border border-border/50 bg-secondary/20">
                      <div className="flex items-center justify-between mb-1">
                        <p className="text-xs font-medium">{e.diagnostico}</p>
                        <Badge className={`${severidadColor[e.severidad]} text-[10px]`}>{e.severidad}</Badge>
                      </div>
                      <p className="text-[10px] text-muted-foreground">{fmt.fecha(e.fechaEvento)}</p>
                      {e.tratamiento && (
                        <p className="text-[10px] text-muted-foreground mt-1 leading-relaxed">{e.tratamiento}</p>
                      )}
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </div>
      </div>

      <ModificarAnimalModal animal={animal} open={modalEditar} onClose={() => setModalEditar(false)} />
      <RegistrarPesajeModal animalId={id!} open={modalPesaje} onClose={() => setModalPesaje(false)} />
      <RegistrarEventoModal animalId={id!} open={modalEvento} onClose={() => setModalEvento(false)} />
    </div>
  )
}
