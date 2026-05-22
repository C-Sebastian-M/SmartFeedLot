import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Plus, Beef, Search, X, CheckCircle2, Filter } from 'lucide-react'
import { useAnimals, useCreateAnimal, useLotes } from '@/hooks/useFeedlot'
import {
  PageHeader, Button, Card, CardContent, Badge, Skeleton, EmptyState,
  Dialog, DialogHeader, DialogTitle, DialogDescription,
  FormField, Input, Alert,
} from '@/components/ui'
import { fmt, estadoProductivoColor, estadoSanitarioColor } from '@/utils'
import type { AnimalResumen } from '@/types'

// ─── Schema ───────────────────────────────────────────────────────────────────
const schema = z.object({
  codigoIdentificacion: z
    .string().min(3, 'Mínimo 3 caracteres').max(20, 'Máximo 20 caracteres')
    .regex(/^[A-Za-z0-9-]+$/, 'Solo letras, números y guiones'),
  numeroArete: z.string().min(1, 'Requerido').max(50, 'Máximo 50 caracteres'),
  sexo: z.enum(['Macho', 'Hembra'], { required_error: 'Selecciona el sexo' }),
  raza: z.string().min(2, 'Requerido').max(100, 'Máximo 100 caracteres'),
  fechaNacimiento: z.string().min(1, 'Requerida'),
  pesoIngresoKg: z.number({ invalid_type_error: 'Ingresa un número' }).positive('Debe ser mayor a 0'),
  precioCompra: z.number({ invalid_type_error: 'Ingresa un número' }).min(0, 'No puede ser negativo'),
  moneda: z.enum(['COP', 'USD', 'EUR']).default('COP'),
  fechaIngreso: z.string().min(1, 'Requerida'),
  loteInicialId: z.string().optional(),
})
  .refine(
    (d) => new Date(d.fechaIngreso) > new Date(d.fechaNacimiento),
    { message: 'La fecha de ingreso debe ser posterior al nacimiento', path: ['fechaIngreso'] }
  )

type RegistrarAnimalForm = z.infer<typeof schema>

// ─── Select propio (sin dependencias externas) ────────────────────────────────
function Select({
  options, placeholder, value, onChange, error, disabled,
}: {
  options: { value: string; label: string }[]
  placeholder?: string
  value?: string
  onChange: (v: string) => void
  error?: boolean
  disabled?: boolean
}) {
  return (
    <select
      value={value ?? ''}
      onChange={(e) => onChange(e.target.value)}
      disabled={disabled}
      className={`flex h-9 w-full rounded-md border bg-transparent px-3 py-1 text-sm shadow-sm transition-colors
        focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring
        disabled:cursor-not-allowed disabled:opacity-50
        ${error ? 'border-destructive' : 'border-input'}
        [&>option]:bg-card`}
    >
      {placeholder && <option value="">{placeholder}</option>}
      {options.map((o) => (
        <option key={o.value} value={o.value}>{o.label}</option>
      ))}
    </select>
  )
}

// ─── Modal registro animal ────────────────────────────────────────────────────
function RegistrarAnimalModal({ open, onClose }: { open: boolean; onClose: () => void }) {
  const [exito, setExito] = useState(false)
  const [errorApi, setErrorApi] = useState<string>()
  const createAnimal = useCreateAnimal()
  const { data: lotes } = useLotes(true) // Solo lotes activos para asignar

  const today = new Date().toISOString().split('T')[0]

  const {
    register, handleSubmit, reset, setValue, watch,
    formState: { errors, isSubmitting },
  } = useForm<RegistrarAnimalForm>({
    resolver: zodResolver(schema),
    defaultValues: {
      moneda: 'COP',
      fechaIngreso: today,
      precioCompra: 0,
    },
  })

  const handleClose = () => {
    reset()
    setExito(false)
    setErrorApi(undefined)
    onClose()
  }

  const onSubmit = async (data: RegistrarAnimalForm) => {
    setErrorApi(undefined)
    try {
      await createAnimal.mutateAsync({
        codigoIdentificacion: data.codigoIdentificacion.toUpperCase(),
        numeroArete: data.numeroArete,
        sexo: data.sexo,
        raza: data.raza,
        fechaNacimiento: data.fechaNacimiento,
        pesoIngresoKg: data.pesoIngresoKg,
        precioCompra: data.precioCompra,
        moneda: data.moneda,
        fechaIngreso: data.fechaIngreso,
        loteInicialId: data.loteInicialId || undefined,
      })
      setExito(true)
      setTimeout(() => handleClose(), 1500)
    } catch (err: any) {
      setErrorApi(
        err?.response?.data?.error ??
        err?.response?.data?.detail ??
        'Error al registrar el animal.'
      )
    }
  }

  const lotesArray = (lotes as any[] | undefined) ?? []

  return (
    <Dialog open={open} onClose={handleClose}>
      <div className="rounded-xl border border-border bg-card shadow-xl max-h-[90vh] overflow-y-auto">
        {/* Header */}
        <div className="flex items-center justify-between px-5 py-4 border-b border-border sticky top-0 bg-card z-10">
          <DialogHeader className="mb-0">
            <DialogTitle>Registrar animal</DialogTitle>
            <DialogDescription>Ingresa los datos del nuevo bovino.</DialogDescription>
          </DialogHeader>
          <button onClick={handleClose} className="text-muted-foreground hover:text-foreground transition-colors ml-4">
            <X className="w-4 h-4" />
          </button>
        </div>

        <div className="p-5">
          {exito ? (
            <div className="flex flex-col items-center justify-center py-8 gap-3 animate-fade-in">
              <div className="w-12 h-12 rounded-full bg-emerald-500/10 flex items-center justify-center">
                <CheckCircle2 className="w-6 h-6 text-emerald-400" />
              </div>
              <p className="text-sm font-medium">¡Animal registrado exitosamente!</p>
            </div>
          ) : (
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">

              {/* Sección: Identificación */}
              <div>
                <p className="text-[10px] font-semibold text-muted-foreground uppercase tracking-widest mb-3">
                  Identificación
                </p>
                <div className="grid grid-cols-2 gap-3">
                  <FormField label="Código" error={errors.codigoIdentificacion?.message} required hint="Ej: ANI-001">
                    <Input
                      {...register('codigoIdentificacion')}
                      placeholder="ANI-001"
                      className={errors.codigoIdentificacion ? 'border-destructive' : ''}
                      style={{ textTransform: 'uppercase' }}
                      autoFocus
                    />
                  </FormField>
                  <FormField label="Número de arete" error={errors.numeroArete?.message} required>
                    <Input
                      {...register('numeroArete')}
                      placeholder="AR-0001"
                      className={errors.numeroArete ? 'border-destructive' : ''}
                    />
                  </FormField>
                </div>
              </div>

              {/* Sección: Características */}
              <div>
                <p className="text-[10px] font-semibold text-muted-foreground uppercase tracking-widest mb-3">
                  Características
                </p>
                <div className="grid grid-cols-2 gap-3">
                  <FormField label="Sexo" error={errors.sexo?.message} required>
                    <Select
                      placeholder="Seleccionar..."
                      options={[
                        { value: 'Macho', label: 'Macho' },
                        { value: 'Hembra', label: 'Hembra' },
                      ]}
                      value={watch('sexo')}
                      onChange={(v) => setValue('sexo', v as 'Macho' | 'Hembra', { shouldValidate: true })}
                      error={!!errors.sexo}
                    />
                  </FormField>
                  <FormField label="Raza" error={errors.raza?.message} required>
                    <Input
                      {...register('raza')}
                      placeholder="Brahman"
                      className={errors.raza ? 'border-destructive' : ''}
                    />
                  </FormField>
                  <FormField label="Fecha de nacimiento" error={errors.fechaNacimiento?.message} required>
                    <Input
                      {...register('fechaNacimiento')}
                      type="date"
                      max={today}
                      className={errors.fechaNacimiento ? 'border-destructive' : ''}
                    />
                  </FormField>
                  <FormField label="Fecha de ingreso" error={errors.fechaIngreso?.message} required>
                    <Input
                      {...register('fechaIngreso')}
                      type="date"
                      max={today}
                      className={errors.fechaIngreso ? 'border-destructive' : ''}
                    />
                  </FormField>
                </div>
              </div>

              {/* Sección: Datos productivos */}
              <div>
                <p className="text-[10px] font-semibold text-muted-foreground uppercase tracking-widest mb-3">
                  Datos productivos
                </p>
                <div className="grid grid-cols-2 gap-3">
                  <FormField label="Peso de ingreso (kg)" error={errors.pesoIngresoKg?.message} required>
                    <Input
                      {...register('pesoIngresoKg', { valueAsNumber: true })}
                      type="number"
                      min={1}
                      step="0.1"
                      placeholder="250.5"
                      className={errors.pesoIngresoKg ? 'border-destructive' : ''}
                    />
                  </FormField>
                  <FormField label="Precio de compra" error={errors.precioCompra?.message} required>
                    <Input
                      {...register('precioCompra', { valueAsNumber: true })}
                      type="number"
                      min={0}
                      step="1000"
                      placeholder="1500000"
                      className={errors.precioCompra ? 'border-destructive' : ''}
                    />
                  </FormField>
                  <FormField label="Moneda" error={errors.moneda?.message} required>
                    <Select
                      options={[
                        { value: 'COP', label: 'COP — Peso colombiano' },
                        { value: 'USD', label: 'USD — Dólar' },
                        { value: 'EUR', label: 'EUR — Euro' },
                      ]}
                      value={watch('moneda')}
                      onChange={(v) => setValue('moneda', v as 'COP' | 'USD' | 'EUR')}
                    />
                  </FormField>
                  <FormField label="Lote inicial (opcional)" hint="Asignar a un lote activo al registrar">
                    <Select
                      placeholder="Sin lote"
                      options={lotesArray.map((l: any) => ({
                        value: l.id,
                        label: `${l.codigo} — ${l.nombre}`,
                      }))}
                      value={watch('loteInicialId')}
                      onChange={(v) => setValue('loteInicialId', v || undefined)}
                      disabled={lotesArray.length === 0}
                    />
                  </FormField>
                </div>
              </div>

              {errorApi && <Alert variant="destructive">{errorApi}</Alert>}

              <div className="flex gap-2 pt-1">
                <Button type="button" variant="outline" className="flex-1" onClick={handleClose} disabled={isSubmitting}>
                  Cancelar
                </Button>
                <Button type="submit" className="flex-1" loading={isSubmitting}>
                  <Beef className="w-3.5 h-3.5" />
                  Registrar animal
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
export default function AnimalesPage() {
  const [modalAbierto, setModalAbierto] = useState(false)
  const [busqueda, setBusqueda] = useState('')
  const [estadoFiltro, setEstadoFiltro] = useState<string>()

  const { data, isLoading } = useAnimals({
    page: 1,
    pageSize: 100,
    busqueda: busqueda || undefined,
    estadoProductivo: estadoFiltro,
  })

  const animales = (data?.items as AnimalResumen[] | undefined) ?? []

  return (
    <div className="flex flex-col h-full animate-fade-in">
      <PageHeader
        title="Animales"
        description={`${data?.totalCount ?? 0} registrados`}
        action={
          <Button size="sm" onClick={() => setModalAbierto(true)}>
            <Plus className="w-3.5 h-3.5" />
            Registrar animal
          </Button>
        }
      />

      {/* Barra de filtros */}
      <div className="flex items-center gap-3 px-6 py-3 border-b border-border flex-wrap">
        <div className="relative">
          <Search className="absolute left-2.5 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-muted-foreground" />
          <Input
            placeholder="Buscar código o arete..."
            className="pl-8 h-8 text-xs w-56"
            value={busqueda}
            onChange={(e) => setBusqueda(e.target.value)}
          />
          {busqueda && (
            <button
              onClick={() => setBusqueda('')}
              className="absolute right-2 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
            >
              <X className="w-3 h-3" />
            </button>
          )}
        </div>

        <div className="flex items-center gap-1.5">
          <Filter className="w-3.5 h-3.5 text-muted-foreground" />
          {[
            { value: undefined, label: 'Todos' },
            { value: 'EnEngorde', label: 'En engorde' },
            { value: 'Vendido', label: 'Vendido' },
            { value: 'Muerto', label: 'Muerto' },
          ].map(({ value, label }) => (
            <button
              key={label}
              onClick={() => setEstadoFiltro(value)}
              className={`px-2.5 py-1 rounded-full text-xs font-medium transition-colors ${
                estadoFiltro === value
                  ? 'bg-primary text-primary-foreground'
                  : 'text-muted-foreground hover:text-foreground hover:bg-secondary'
              }`}
            >
              {label}
            </button>
          ))}
        </div>
      </div>

      {/* Tabla */}
      <div className="flex-1 overflow-y-auto p-6">
        {isLoading ? (
          <div className="space-y-2">
            {Array.from({ length: 8 }).map((_, i) => (
              <Skeleton key={i} className="h-12 w-full rounded-lg" />
            ))}
          </div>
        ) : !animales.length ? (
          <EmptyState
            icon={<Beef className="w-5 h-5" />}
            title={busqueda ? `Sin resultados para "${busqueda}"` : 'Sin animales'}
            description={
              busqueda
                ? 'Intenta con otro código o número de arete.'
                : 'Registra el primer animal para comenzar el seguimiento productivo.'
            }
            action={
              !busqueda ? (
                <Button size="sm" onClick={() => setModalAbierto(true)}>
                  <Plus className="w-3.5 h-3.5" />
                  Registrar animal
                </Button>
              ) : undefined
            }
          />
        ) : (
          <Card>
            <CardContent className="p-0">
              <table className="w-full text-xs">
                <thead>
                  <tr className="border-b border-border">
                    {['Código', 'Arete', 'Raza', 'Sexo', 'Peso actual', 'Días engorde', 'Estado prod.', 'Estado san.'].map((h) => (
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
                  {animales.map((animal, i) => (
                    <tr
                      key={animal.id}
                      className={`border-b border-border/40 hover:bg-secondary/30 transition-colors cursor-pointer ${
                        i === animales.length - 1 ? 'border-b-0' : ''
                      }`}
                    >
                      <td className="px-4 py-3 font-mono font-semibold">{animal.codigoIdentificacion}</td>
                      <td className="px-4 py-3 text-muted-foreground">{animal.numeroArete}</td>
                      <td className="px-4 py-3">{animal.raza}</td>
                      <td className="px-4 py-3 text-muted-foreground">{animal.sexo}</td>
                      <td className="px-4 py-3 tabular-nums font-medium">{fmt.kg(animal.pesoActualKg)}</td>
                      <td className="px-4 py-3 tabular-nums text-muted-foreground">
                        {animal.diasEnEngorde}d
                      </td>
                      <td className="px-4 py-3">
                        <Badge className={estadoProductivoColor[animal.estadoProductivo]}>
                          {animal.estadoProductivo === 'EnEngorde' ? 'En engorde' : animal.estadoProductivo}
                        </Badge>
                      </td>
                      <td className="px-4 py-3">
                        <Badge className={estadoSanitarioColor[animal.estadoSanitario]}>
                          {animal.estadoSanitario === 'EnTratamiento' ? 'En tratamiento' : animal.estadoSanitario}
                        </Badge>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>

              {/* Footer con conteo */}
              {(data?.totalCount ?? 0) > animales.length && (
                <div className="px-4 py-3 border-t border-border text-center">
                  <p className="text-xs text-muted-foreground">
                    Mostrando {animales.length} de {data?.totalCount} animales
                  </p>
                </div>
              )}
            </CardContent>
          </Card>
        )}
      </div>

      <RegistrarAnimalModal
        open={modalAbierto}
        onClose={() => setModalAbierto(false)}
      />
    </div>
  )
}
