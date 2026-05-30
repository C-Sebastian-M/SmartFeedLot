import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Plus, X, CheckCircle2, DollarSign, Tag, Users, ChevronDown } from 'lucide-react'
import {
  useCategoriasGasto, useCrearCategoriaGasto,
  useSocios, useCrearSocio,
  useMovimientosFinancieros, useRegistrarMovimiento,
} from '@/hooks/useFeedlot'
import { useAuthStore } from '@/stores/auth.store'
import {
  PageHeader, Card, CardContent, Skeleton, EmptyState, Button,
  Dialog, DialogHeader, DialogTitle, DialogDescription,
  FormField, Input, Alert, Badge,
} from '@/components/ui'
import { fmt } from '@/utils'
import type { CategoriaGasto, Socio, MovimientoFinanciero } from '@/types'

type Tab = 'movimientos' | 'categorias' | 'socios'
const hoy = new Date().toISOString().slice(0, 10)

const movimientoSchema = z.object({
  fecha: z.string().min(1, 'Requerida'),
  periodoAnio: z.coerce.number().min(2000).max(2100),
  periodoMes: z.coerce.number().min(1).max(12),
  categoriaGastoId: z.string().min(1, 'Selecciona una categoría'),
  monto: z.coerce.number().positive('Debe ser mayor a cero'),
  moneda: z.string().default('COP'),
  origen: z.enum(['General', 'Bovino', 'Porcino', 'Agricola']),
  descripcion: z.string().min(3, 'Mínimo 3 caracteres').max(500),
  socioId: z.string().optional(),
})
type MovimientoForm = z.infer<typeof movimientoSchema>

const categoriaSchema = z.object({
  nombre: z.string().min(1, 'Requerido').max(100),
  tipo: z.enum(['Directo', 'Indirecto', 'Operativo', 'Inversion']),
})
type CategoriaForm = z.infer<typeof categoriaSchema>

function RegistrarMovimientoModal({ open, onClose }: { open: boolean; onClose: () => void }) {
  const [exito, setExito] = useState(false)
  const [errorApi, setErrorApi] = useState<string>()
  const mutation = useRegistrarMovimiento()
  const { data: categorias } = useCategoriasGasto()
  const { data: socios } = useSocios()
  const currentUser = useAuthStore(s => s.user)

  const { register, handleSubmit, reset, formState: { errors, isSubmitting } } =
    useForm<MovimientoForm>({
      resolver: zodResolver(movimientoSchema),
      defaultValues: { moneda: 'COP', fecha: hoy, periodoAnio: new Date().getFullYear(), periodoMes: new Date().getMonth() + 1, origen: 'General' },
    })

  const handleClose = () => { reset(); setExito(false); setErrorApi(undefined); onClose() }

  const onSubmit = async (data: MovimientoForm) => {
    setErrorApi(undefined)
    try {
      if (!currentUser?.id) throw new Error('Usuario no autenticado')
      await mutation.mutateAsync({
        fecha: data.fecha,
        periodoAnio: data.periodoAnio,
        periodoMes: data.periodoMes,
        categoriaGastoId: data.categoriaGastoId,
        monto: data.monto,
        moneda: data.moneda,
        origen: data.origen,
        descripcion: data.descripcion,
        socioId: data.socioId || undefined,
        registradoPorId: currentUser.id,
      })
      setExito(true)
      setTimeout(() => handleClose(), 1500)
    } catch (err: any) {
      setErrorApi(err?.response?.data?.error ?? err?.response?.data?.detail ?? 'Error al registrar.')
    }
  }

  const tiposOrigen = ['General', 'Bovino', 'Porcino', 'Agricola'] as const

  return (
    <Dialog open={open} onClose={handleClose}>
      <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[520px] mx-4">
        <div className="flex items-center justify-between px-5 py-4 border-b border-border">
          <DialogHeader className="mb-0">
            <DialogTitle>Registrar movimiento financiero</DialogTitle>
            <DialogDescription>Ingreso o egreso del período</DialogDescription>
          </DialogHeader>
          <button onClick={handleClose} className="text-muted-foreground hover:text-foreground ml-4"><X className="w-4 h-4" /></button>
        </div>
        <div className="p-5 max-h-[70vh] overflow-y-auto">
          {exito ? (
            <div className="flex flex-col items-center py-6 gap-3 animate-fade-in">
              <div className="w-12 h-12 rounded-full bg-emerald-500/10 flex items-center justify-center"><CheckCircle2 className="w-6 h-6 text-emerald-400" /></div>
              <p className="text-sm font-medium">¡Movimiento registrado!</p>
            </div>
          ) : (
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              <div className="grid grid-cols-2 gap-3">
                <FormField label="Fecha" error={errors.fecha?.message} required>
                  <Input {...register('fecha')} type="date" className={errors.fecha ? 'border-destructive' : ''} />
                </FormField>
                <FormField label="Origen" error={errors.origen?.message} required>
                  <select {...register('origen')}
                    className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm [&>option]:bg-card">
                    {tiposOrigen.map(o => <option key={o} value={o}>{o}</option>)}
                  </select>
                </FormField>
              </div>
              <div className="grid grid-cols-2 gap-3">
                <FormField label="Año período" error={errors.periodoAnio?.message} required>
                  <Input {...register('periodoAnio')} type="number" min={2000} max={2100} />
                </FormField>
                <FormField label="Mes período" error={errors.periodoMes?.message} required>
                  <Input {...register('periodoMes')} type="number" min={1} max={12} />
                </FormField>
              </div>
              <FormField label="Categoría de gasto" error={errors.categoriaGastoId?.message} required>
                <div className="relative">
                  <select {...register('categoriaGastoId')}
                    className="h-9 pl-3 pr-8 rounded-md border border-input bg-card text-sm w-full appearance-none cursor-pointer [&>option]:bg-card">
                    <option value="">Seleccionar...</option>
                    {(categorias as CategoriaGasto[] | undefined)?.map(c => (
                      <option key={c.id} value={c.id}>{c.nombre} ({c.tipo})</option>
                    ))}
                  </select>
                  <ChevronDown className="absolute right-2 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-muted-foreground pointer-events-none" />
                </div>
              </FormField>
              <FormField label="Descripción" error={errors.descripcion?.message} required>
                <Input {...register('descripcion')} placeholder="Ej: Suministrar alimentación" />
              </FormField>
              <div className="grid grid-cols-2 gap-3">
                <FormField label="Monto" error={errors.monto?.message} required>
                  <Input {...register('monto')} type="number" min={1} step={100} placeholder="0" />
                </FormField>
                <FormField label="Moneda" error={errors.moneda?.message} required>
                  <Input {...register('moneda')} placeholder="COP" />
                </FormField>
              </div>
              <FormField label="Socio (opcional)" error={errors.socioId?.message} hint="Asignar a un socio">
                <div className="relative">
                  <select {...register('socioId')}
                    className="h-9 pl-3 pr-8 rounded-md border border-input bg-card text-sm w-full appearance-none cursor-pointer [&>option]:bg-card">
                    <option value="">— Sin asignar —</option>
                    {(socios as Socio[] | undefined)?.map(s => (
                      <option key={s.id} value={s.id}>{s.nombre} ({s.participacion}%)</option>
                    ))}
                  </select>
                  <ChevronDown className="absolute right-2 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-muted-foreground pointer-events-none" />
                </div>
              </FormField>
              {errorApi && <Alert variant="destructive">{errorApi}</Alert>}
              <div className="flex gap-2 pt-1">
                <Button type="button" variant="outline" className="flex-1" onClick={handleClose} disabled={isSubmitting}>Cancelar</Button>
                <Button type="submit" className="flex-1" loading={isSubmitting}><DollarSign className="w-3.5 h-3.5" /> Registrar</Button>
              </div>
            </form>
          )}
        </div>
      </div>
    </Dialog>
  )
}

function CrearCategoriaModal({ open, onClose }: { open: boolean; onClose: () => void }) {
  const [exito, setExito] = useState(false)
  const [errorApi, setErrorApi] = useState<string>()
  const mutation = useCrearCategoriaGasto()
  const { register, handleSubmit, reset, formState: { errors, isSubmitting } } =
    useForm<CategoriaForm>({ resolver: zodResolver(categoriaSchema) })
  const handleClose = () => { reset(); setExito(false); setErrorApi(undefined); onClose() }
  const onSubmit = async (data: CategoriaForm) => {
    setErrorApi(undefined)
    try {
      await mutation.mutateAsync(data)
      setExito(true)
      setTimeout(() => handleClose(), 1500)
    } catch (err: any) {
      setErrorApi(err?.response?.data?.error ?? 'Error al crear categoría.')
    }
  }

  return (
    <Dialog open={open} onClose={handleClose}>
      <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[420px] mx-4">
        <div className="flex items-center justify-between px-5 py-4 border-b border-border">
          <DialogHeader className="mb-0"><DialogTitle>Nueva categoría</DialogTitle></DialogHeader>
          <button onClick={handleClose} className="text-muted-foreground hover:text-foreground ml-4"><X className="w-4 h-4" /></button>
        </div>
        <div className="p-5">
          {exito ? (
            <div className="flex flex-col items-center py-6 gap-3 animate-fade-in">
              <div className="w-12 h-12 rounded-full bg-emerald-500/10 flex items-center justify-center"><CheckCircle2 className="w-6 h-6 text-emerald-400" /></div>
              <p className="text-sm font-medium">¡Categoría creada!</p>
            </div>
          ) : (
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              <FormField label="Nombre" error={errors.nombre?.message} required>
                <Input {...register('nombre')} placeholder="Ej: Alimentación" />
              </FormField>
              <FormField label="Tipo" error={errors.tipo?.message} required>
                <select {...register('tipo')}
                  className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm [&>option]:bg-card">
                  <option value="Directo">Directo</option>
                  <option value="Indirecto">Indirecto</option>
                  <option value="Operativo">Operativo</option>
                  <option value="Inversion">Inversión</option>
                </select>
              </FormField>
              {errorApi && <Alert variant="destructive">{errorApi}</Alert>}
              <div className="flex gap-2 pt-1">
                <Button type="button" variant="outline" className="flex-1" onClick={handleClose} disabled={isSubmitting}>Cancelar</Button>
                <Button type="submit" className="flex-1" loading={isSubmitting}>Crear</Button>
              </div>
            </form>
          )}
        </div>
      </div>
    </Dialog>
  )
}

function CrearSocioModal({ open, onClose }: { open: boolean; onClose: () => void }) {
  const [exito, setExito] = useState(false)
  const [errorApi, setErrorApi] = useState<string>()
  const mutation = useCrearSocio()
  const [nombre, setNombre] = useState('')
  const [participacion, setParticipacion] = useState(50)
  const [reparto, setReparto] = useState<{ socio: string; pct: number }[]>([])

  const handleClose = () => { setNombre(''); setParticipacion(50); setReparto([]); setExito(false); setErrorApi(undefined); onClose() }

  const agregarAReparto = () => {
    if (!nombre.trim()) return
    const totalActual = reparto.reduce((s, r) => s + r.pct, 0)
    const resto = 100 - totalActual
    const pct = Math.min(participacion, resto)
    setReparto(prev => [...prev, { socio: nombre.trim(), pct }])
    setNombre('')
    setParticipacion(50)
  }

  const onSubmit = async () => {
    setErrorApi(undefined)
    if (reparto.length === 0) { setErrorApi('Agrega al menos un socio'); return }
    const total = reparto.reduce((s, r) => s + r.pct, 0)
    if (total !== 100) { setErrorApi(`La participación total debe ser 100% (actual: ${total}%)`); return }
    try {
      for (const r of reparto) {
        await mutation.mutateAsync({ nombre: r.socio, participacion: r.pct })
      }
      setExito(true)
      setTimeout(() => handleClose(), 1500)
    } catch (err: any) {
      setErrorApi(err?.response?.data?.error ?? 'Error al crear socios.')
    }
  }

  return (
    <Dialog open={open} onClose={handleClose}>
      <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[420px] mx-4">
        <div className="flex items-center justify-between px-5 py-4 border-b border-border">
          <DialogHeader className="mb-0"><DialogTitle>Crear socios</DialogTitle></DialogHeader>
          <button onClick={handleClose} className="text-muted-foreground hover:text-foreground ml-4"><X className="w-4 h-4" /></button>
        </div>
        <div className="p-5">
          {exito ? (
            <div className="flex flex-col items-center py-6 gap-3 animate-fade-in">
              <div className="w-12 h-12 rounded-full bg-emerald-500/10 flex items-center justify-center"><CheckCircle2 className="w-6 h-6 text-emerald-400" /></div>
              <p className="text-sm font-medium">¡Socios creados!</p>
            </div>
          ) : (
            <div className="space-y-4">
              <div className="flex gap-2 items-end">
                <div className="flex-1">
                  <label className="text-xs font-medium text-muted-foreground mb-1 block">Nombre</label>
                  <input value={nombre} onChange={e => setNombre(e.target.value)}
                    className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" placeholder="Ej: Estefania" />
                </div>
                <FormField label="Participación %">
                  <input type="number" min={1} max={100} value={participacion}
                    onChange={e => setParticipacion(Number(e.target.value))}
                    className="flex h-9 w-20 rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
                </FormField>
                <Button type="button" size="sm" onClick={agregarAReparto} disabled={!nombre.trim()}>
                  <Plus className="w-3 h-3" />
                </Button>
              </div>

              {reparto.length > 0 && (
                <div className="space-y-1">
                  {reparto.map((r, i) => (
                    <div key={i} className="flex items-center justify-between px-3 py-2 rounded-md bg-secondary/30 text-sm">
                      <span>{r.socio}</span>
                      <span className="font-medium">{r.pct}%</span>
                    </div>
                  ))}
                  <p className="text-xs text-muted-foreground text-right">
                    Total: {reparto.reduce((s, r) => s + r.pct, 0)}% / 100%
                  </p>
                </div>
              )}

              {errorApi && <Alert variant="destructive">{errorApi}</Alert>}

              <div className="flex gap-2 pt-1">
                <Button type="button" variant="outline" className="flex-1" onClick={handleClose}>Cancelar</Button>
                <Button type="button" className="flex-1" onClick={onSubmit} disabled={reparto.length === 0}>
                  <Users className="w-3.5 h-3.5" /> Guardar socios
                </Button>
              </div>
            </div>
          )}
        </div>
      </div>
    </Dialog>
  )
}

export default function FinanzasPage() {
  const [tab, setTab] = useState<Tab>('movimientos')
  const [modalMovimiento, setModalMovimiento] = useState(false)
  const [modalCategoria, setModalCategoria] = useState(false)
  const [modalSocios, setModalSocios] = useState(false)

  const [filtroAnio, setFiltroAnio] = useState(new Date().getFullYear())
  const [filtroMes, setFiltroMes] = useState<number | undefined>(undefined)
  const [filtroOrigen, setFiltroOrigen] = useState<string | undefined>(undefined)

  const { data: categorias } = useCategoriasGasto()
  const { data: socios } = useSocios()
  const { data: movimientos, isLoading } = useMovimientosFinancieros({
    anio: filtroAnio, mes: filtroMes, origen: filtroOrigen,
  })

  const tabs: { key: Tab; label: string; icon: typeof DollarSign }[] = [
    { key: 'movimientos', icon: DollarSign, label: 'Movimientos' },
    { key: 'categorias', icon: Tag, label: 'Categorías' },
    { key: 'socios', icon: Users, label: 'Socios' },
  ]

  const meses = [
    { value: 1, label: 'Enero' }, { value: 2, label: 'Febrero' }, { value: 3, label: 'Marzo' },
    { value: 4, label: 'Abril' }, { value: 5, label: 'Mayo' }, { value: 6, label: 'Junio' },
    { value: 7, label: 'Julio' }, { value: 8, label: 'Agosto' }, { value: 9, label: 'Septiembre' },
    { value: 10, label: 'Octubre' }, { value: 11, label: 'Noviembre' }, { value: 12, label: 'Diciembre' },
  ]

  const origenTagColor: Record<string, string> = {
    Bovino: 'bg-emerald-500/10 text-emerald-400 border-emerald-500/20',
    Porcino: 'bg-violet-500/10 text-violet-400 border-violet-500/20',
    Agricola: 'bg-amber-500/10 text-amber-400 border-amber-500/20',
    General: 'bg-zinc-500/10 text-zinc-400 border-zinc-500/20',
  }

  const movsArray = (movimientos as MovimientoFinanciero[] | undefined) ?? []
  const categoriasArray = (categorias as CategoriaGasto[] | undefined) ?? []
  const sociosArray = (socios as Socio[] | undefined) ?? []

  return (
    <div className="flex flex-col h-full animate-fade-in">
      <PageHeader
        title="Finanzas"
        description="Movimientos financieros, categorías de gasto y socios"
        action={
          tab === 'movimientos' ? (
            <Button size="sm" onClick={() => setModalMovimiento(true)}>
              <Plus className="w-3.5 h-3.5" /> Nuevo movimiento
            </Button>
          ) : tab === 'categorias' ? (
            <Button size="sm" onClick={() => setModalCategoria(true)}>
              <Plus className="w-3.5 h-3.5" /> Nueva categoría
            </Button>
          ) : (
            <Button size="sm" onClick={() => setModalSocios(true)}>
              <Plus className="w-3.5 h-3.5" /> Crear socios
            </Button>
          )
        }
      />

      {/* Tabs */}
      <div className="flex border-b border-border px-6 gap-0">
        {tabs.map(t => (
          <button key={t.key} onClick={() => setTab(t.key)}
            className={`flex items-center gap-2 px-4 py-3 text-sm font-medium border-b-2 transition-colors ${
              tab === t.key ? 'border-primary text-primary' : 'border-transparent text-muted-foreground hover:text-foreground'
            }`}>
            <t.icon className="w-3.5 h-3.5" />
            {t.label}
          </button>
        ))}
      </div>

      <div className="flex-1 overflow-y-auto p-6">
        {/* ── Tab: Movimientos ── */}
        {tab === 'movimientos' && (
          <div className="space-y-4">
            <div className="flex items-center gap-3 flex-wrap">
              <select value={filtroAnio} onChange={e => setFiltroAnio(Number(e.target.value))}
                className="h-9 px-3 rounded-md border border-input bg-card text-sm [&>option]:bg-card">
                {[2024, 2025, 2026, 2027].map(a => <option key={a} value={a}>{a}</option>)}
              </select>
              <select value={filtroMes ?? ''} onChange={e => setFiltroMes(e.target.value ? Number(e.target.value) : undefined)}
                className="h-9 px-3 rounded-md border border-input bg-card text-sm [&>option]:bg-card">
                <option value="">Todos los meses</option>
                {meses.map(m => <option key={m.value} value={m.value}>{m.label}</option>)}
              </select>
              <select value={filtroOrigen ?? ''} onChange={e => setFiltroOrigen(e.target.value || undefined)}
                className="h-9 px-3 rounded-md border border-input bg-card text-sm [&>option]:bg-card">
                <option value="">Todos los orígenes</option>
                <option value="Bovino">Bovino</option>
                <option value="Porcino">Porcino</option>
                <option value="Agricola">Agrícola</option>
                <option value="General">General</option>
              </select>
            </div>

            {isLoading ? (
              <div className="space-y-2">{Array.from({ length: 5 }).map((_, i) => <Skeleton key={i} className="h-12 rounded-lg" />)}</div>
            ) : movsArray.length === 0 ? (
              <EmptyState icon={<DollarSign className="w-5 h-5" />} title="Sin movimientos"
                description="No hay movimientos financieros para los filtros seleccionados."
                action={<Button size="sm" onClick={() => setModalMovimiento(true)}><Plus className="w-3.5 h-3.5" /> Registrar primero</Button>} />
            ) : (
              <Card>
                <CardContent className="p-0">
                  <div className="overflow-x-auto">
                    <table className="w-full text-sm">
                      <thead>
                        <tr className="border-b border-border">
                          {['Fecha', 'Descripción', 'Categoría', 'Origen', 'Socio', 'Monto'].map(h => (
                            <th key={h} className="text-left px-4 py-3 text-muted-foreground font-medium uppercase tracking-wide text-[10px] whitespace-nowrap">{h}</th>
                          ))}
                        </tr>
                      </thead>
                      <tbody>
                        {movsArray.map((m, i) => (
                          <tr key={m.id} className={`border-b border-border/40 hover:bg-secondary/30 transition-colors ${i === movsArray.length - 1 ? 'border-b-0' : ''}`}>
                            <td className="px-4 py-3 text-muted-foreground tabular-nums">{fmt.fecha(m.fecha)}</td>
                            <td className="px-4 py-3 font-medium">{m.descripcion}</td>
                            <td className="px-4 py-3"><Badge className="bg-primary/10 text-primary border-primary/20 text-[10px]">{m.categoriaGastoNombre}</Badge></td>
                            <td className="px-4 py-3"><Badge className={origenTagColor[m.origen] ?? ''}>{m.origen}</Badge></td>
                            <td className="px-4 py-3 text-muted-foreground">{m.socioNombre ?? '—'}</td>
                            <td className="px-4 py-3 tabular-nums font-medium text-right">{fmt.cop(m.monto)}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </CardContent>
              </Card>
            )}
          </div>
        )}

        {/* ── Tab: Categorías ── */}
        {tab === 'categorias' && (
          <div className="space-y-3">
            {categoriasArray.length === 0 ? (
              <EmptyState icon={<Tag className="w-5 h-5" />} title="Sin categorías" description="Aún no hay categorías de gasto creadas."
                action={<Button size="sm" onClick={() => setModalCategoria(true)}><Plus className="w-3.5 h-3.5" /> Crear primera</Button>} />
            ) : (
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
                {categoriasArray.map(c => {
                  const tipoColor = {
                    Directo: 'bg-emerald-500/10 text-emerald-400',
                    Indirecto: 'bg-amber-500/10 text-amber-400',
                    Operativo: 'bg-blue-500/10 text-blue-400',
                    Inversion: 'bg-violet-500/10 text-violet-400',
                  }[c.tipo] ?? ''
                  return (
                    <Card key={c.id} className="p-4">
                      <div className="flex items-start justify-between">
                        <div>
                          <p className="text-sm font-medium">{c.nombre}</p>
                          <Badge className={`mt-1 text-[9px] ${tipoColor} border-0`}>{c.tipo}</Badge>
                        </div>
                      </div>
                    </Card>
                  )
                })}
              </div>
            )}
          </div>
        )}

        {/* ── Tab: Socios ── */}
        {tab === 'socios' && (
          <div className="space-y-3">
            {sociosArray.length === 0 ? (
              <EmptyState icon={<Users className="w-5 h-5" />} title="Sin socios" description="Aún no hay socios registrados."
                action={<Button size="sm" onClick={() => setModalSocios(true)}><Plus className="w-3.5 h-3.5" /> Crear socios</Button>} />
            ) : (
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
                {sociosArray.map(s => (
                  <Card key={s.id} className="p-4">
                    <div className="flex items-center gap-3">
                      <div className="w-10 h-10 rounded-full bg-primary/10 flex items-center justify-center">
                        <Users className="w-4 h-4 text-primary" />
                      </div>
                      <div>
                        <p className="text-sm font-medium">{s.nombre}</p>
                        <p className="text-xs text-muted-foreground">{s.participacion}% de participación</p>
                      </div>
                    </div>
                  </Card>
                ))}
              </div>
            )}
          </div>
        )}
      </div>

      <RegistrarMovimientoModal open={modalMovimiento} onClose={() => setModalMovimiento(false)} />
      <CrearCategoriaModal open={modalCategoria} onClose={() => setModalCategoria(false)} />
      <CrearSocioModal open={modalSocios} onClose={() => setModalSocios(false)} />
    </div>
  )
}