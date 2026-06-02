import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Plus, X, Trees, Users, Sprout, ChevronDown, ChevronRight, Wheat } from 'lucide-react'
import { usePotreros, useCrearPotrero, useIngresarAnimalPotrero, useRetirarAnimalPotrero, useEliminarPotrero,
  useEmpleados, useCrearEmpleado, useModificarEmpleado, useEliminarEmpleado, useModificarActividad, useRegistrarActividadManoObra,
  useCultivosCania, useCrearCultivoCania, useRegistrarCorteCania,
  useLotesSilo, useCrearLoteSilo, useAnimals } from '@/hooks/useFeedlot'
import {
  PageHeader, Card, CardHeader, CardTitle, CardContent,
  Skeleton, EmptyState, Button,
  Dialog, DialogHeader, DialogTitle,
  FormField,
  MoneyInput,
} from '@/components/ui'
import { fmt } from '@/utils'
import type { Potrero, Empleado, CultivoCania, AnimalResumen, PagedResult } from '@/types'

type Tab = 'potreros' | 'empleados' | 'cania'

const tabs: { key: Tab; label: string; icon: typeof Trees }[] = [
  { key: 'potreros', label: 'Potreros', icon: Trees },
  { key: 'empleados', label: 'Mano de obra', icon: Users },
  { key: 'cania', label: 'Caña y Silo', icon: Sprout },
]

// ─── Modal helpers ──────────────────────────────────────────────────────────
type ModalState =
  | { type: null }
  | { type: 'potrero' }
  | { type: 'ingresar'; potreroId: string }
  | { type: 'empleado' }
  | { type: 'editarEmpleado'; empleadoId: string; nombre: string; pagoMensual: number }
  | { type: 'eliminarEmpleado'; empleadoId: string; nombre: string }
  | { type: 'editarActividad'; empleadoId: string; actividadId: string; tipo: string; fecha: string; costo: number }
  | { type: 'actividad'; empleadoId: string }
  | { type: 'cultivo' }
  | { type: 'corte'; cultivoId: string }
  | { type: 'silo' }
  | { type: 'retirar'; potreroId: string; estanciaId: string }
  | { type: 'eliminar'; potreroId: string; nombre: string; animalesActuales: number }

// ─── Potrero section ────────────────────────────────────────────────────────
function PotrerosSection() {
  const { data: potreros, isLoading } = usePotreros()
  const arr = (potreros as Potrero[] | undefined) ?? []
  const crearPotrero = useCrearPotrero()
  const ingresarAnimal = useIngresarAnimalPotrero()
  const retirarAnimal = useRetirarAnimalPotrero()
  const eliminarPotrero = useEliminarPotrero()
  const { data: animalesData } = useAnimals({ estadoProductivo: 'EnEngorde', pageSize: 500 })
  const animales = ((animalesData as PagedResult<AnimalResumen> | undefined)?.items) ?? []
  const [modal, setModal] = useState<ModalState>({ type: null })

  const potreroForm = useForm<{ nombre: string; capacidad: number }>({
    defaultValues: { nombre: '', capacidad: 50 },
  })

  const ingresarForm = useForm<{ fechaEntrada: string }>({
    defaultValues: { fechaEntrada: new Date().toISOString().split('T')[0] },
  })
  const [seleccionados, setSeleccionados] = useState<Set<string>>(new Set())
  const [busqueda, setBusqueda] = useState('')
  const [errorIngresar, setErrorIngresar] = useState<string>()

  const retirarForm = useForm<{ fechaSalida: string }>({
    defaultValues: { fechaSalida: new Date().toISOString().split('T')[0] },
  })

  const onSubmitPotrero = async (data: { nombre: string; capacidad: number }) => {
    await crearPotrero.mutateAsync(data)
    setModal({ type: null })
    potreroForm.reset()
  }

  const onSubmitIngresar = async (data: { fechaEntrada: string }) => {
    if (modal.type !== 'ingresar') return
    if (seleccionados.size === 0) return
    setErrorIngresar(undefined)
    try {
      for (const animalId of seleccionados) {
        await ingresarAnimal.mutateAsync({ potreroId: modal.potreroId, animalId, fechaEntrada: data.fechaEntrada })
      }
      setModal({ type: null })
      ingresarForm.reset()
      setSeleccionados(new Set())
      setBusqueda('')
    } catch (e: any) {
      const msg = e?.response?.data?.detail ?? e?.response?.data?.error ?? 'Error al ingresar el animal.'
      setErrorIngresar(msg)
    }
  }

  const onSubmitRetirar = async (data: { fechaSalida: string }) => {
    if (modal.type !== 'retirar') return
    await retirarAnimal.mutateAsync({ potreroId: modal.potreroId, estanciaId: modal.estanciaId, ...data })
    setModal({ type: null })
    retirarForm.reset()
  }

  if (isLoading) return <div className="space-y-4"><Skeleton className="h-20 rounded-lg" /><Skeleton className="h-20 rounded-lg" /></div>

  return (
    <>
      <div className="space-y-3">
        <div className="flex items-center justify-between">
          <p className="text-sm text-muted-foreground">{arr.length} potreros</p>
          <Button size="sm" variant="outline" onClick={() => setModal({ type: 'potrero' })}><Plus className="w-3 h-3" />Nuevo potrero</Button>
        </div>
        {arr.length === 0 && (
          <EmptyState icon={<Trees className="w-5 h-5" />} title="Sin potreros" description="Registra los potreros de la finca." />
        )}
        <div className="grid gap-3">
          {arr.map(p => (
            <Card key={p.id}>
              <CardHeader>
                <div className="flex items-center justify-between">
                  <div>
                    <CardTitle className="text-sm">{p.nombre}</CardTitle>
                    <p className="text-[10px] text-muted-foreground">{p.animalesActuales}/{p.capacidad} animales</p>
                  </div>
                  <div className="flex gap-2">
                    <Button size="sm" variant="ghost" onClick={() => setModal({ type: 'ingresar', potreroId: p.id })}>Ingresar</Button>
                    <Button size="sm" variant="ghost" className="text-rose-400 hover:text-rose-300"
                      onClick={() => setModal({ type: 'eliminar', potreroId: p.id, nombre: p.nombre, animalesActuales: p.animalesActuales })}>
                      Eliminar
                    </Button>
                  </div>
                </div>
              </CardHeader>
              {p.estancias.filter(e => !e.salida).length > 0 && (
                <CardContent className="p-0 border-t border-border">
                  <table className="w-full text-xs">
                    <thead><tr className="border-b bg-secondary/20"><th className="text-left px-4 py-2 text-muted-foreground">Animal</th><th className="text-left px-4 py-2 text-muted-foreground">Ingreso</th><th className="px-4 py-2" /></tr></thead>
                    <tbody>{p.estancias.filter(e => !e.salida).map(e => {
                      const animal = animales.find(a => a.id === e.animalId)
                      return (
                      <tr key={e.id} className="border-b border-border/30">
                        <td className="px-4 py-2">
                          <p className="font-semibold font-mono text-[11px]">{animal?.codigoIdentificacion ?? e.animalId.slice(0, 8)}</p>
                          {animal && <p className="text-[10px] text-muted-foreground">{[animal.nombre, animal.raza].filter(Boolean).join(' · ') || 'Sin nombre'}</p>}
                        </td>
                        <td className="px-4 py-2">{fmt.fecha(e.fechaEntrada)}</td>
                        <td className="px-4 py-2 text-right">
                          <Button size="sm" variant="ghost" className="text-rose-400 hover:text-rose-300 h-6 px-2 text-[10px]"
                            onClick={() => setModal({ type: 'retirar', potreroId: p.id, estanciaId: e.id })}>
                            Retirar
                          </Button>
                        </td>
                      </tr>
                    )})}</tbody>
                  </table>
                </CardContent>
              )}
            </Card>
          ))}
        </div>
      </div>

      <Dialog open={modal.type === 'potrero'} onClose={() => setModal({ type: null })}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[400px] mx-4">
          <div className="flex items-center justify-between px-5 py-4 border-b border-border">
            <DialogHeader className="mb-0"><DialogTitle>Nuevo potrero</DialogTitle></DialogHeader>
            <button onClick={() => setModal({ type: null })} className="text-muted-foreground hover:text-foreground"><X className="w-4 h-4" /></button>
          </div>
          <form onSubmit={potreroForm.handleSubmit(onSubmitPotrero)} className="p-5 space-y-4">
            <FormField label="Nombre" required><input {...potreroForm.register('nombre')} placeholder="Ej: Bajo" className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
            <FormField label="Capacidad" required><input type="number" min={1} {...potreroForm.register('capacidad', { valueAsNumber: true })} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
            <Button type="submit" className="w-full" loading={crearPotrero.isPending}>Crear potrero</Button>
          </form>
        </div>
      </Dialog>

      <Dialog open={modal.type === 'ingresar'} onClose={() => { setModal({ type: null }); setSeleccionados(new Set()); setBusqueda(''); setErrorIngresar(undefined) }}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[480px] mx-4">
          {/* Header */}
          <div className="flex items-center justify-between px-5 py-4 border-b border-border">
            <div>
              <DialogTitle className="text-sm font-semibold">Ingresar animales</DialogTitle>
              <p className="text-[11px] text-muted-foreground mt-0.5">
                Selecciona uno o más animales y define la fecha de entrada.
              </p>
            </div>
            <button onClick={() => { setModal({ type: null }); setSeleccionados(new Set()); setBusqueda(''); setErrorIngresar(undefined) }}
              className="text-muted-foreground hover:text-foreground ml-4 flex-shrink-0">
              <X className="w-4 h-4" />
            </button>
          </div>

          <form onSubmit={ingresarForm.handleSubmit(onSubmitIngresar)} className="p-5 space-y-4">

            {/* Fecha — primero para que sea lo más visible */}
            <div>
              <label className="text-xs font-medium text-foreground mb-1.5 block">
                Fecha de entrada <span className="text-destructive">*</span>
              </label>
              <input type="date" max={new Date().toISOString().split('T')[0]}
                {...ingresarForm.register('fechaEntrada')}
                className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
            </div>

            {/* Buscador */}
            <div>
              <label className="text-xs font-medium text-foreground mb-1.5 block">
                Animales en engorde
                {seleccionados.size > 0 && (
                  <span className="ml-2 text-primary font-semibold">
                    {seleccionados.size} seleccionado{seleccionados.size > 1 ? 's' : ''}
                  </span>
                )}
              </label>
              <input
                type="text"
                placeholder="Filtrar por código, nombre o raza..."
                value={busqueda}
                onChange={e => setBusqueda(e.target.value)}
                className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm mb-2"
              />

              {/* Lista */}
              <div className="border border-border rounded-md overflow-y-auto max-h-56">
                {(() => {
                  const q = busqueda.toLowerCase()
                  // IDs de animales ya activos en cualquier potrero (un animal no puede estar en dos a la vez)
                  const yaEnPotrero = new Set(
                    arr.flatMap(p => p.estancias.filter(e => !e.salida).map(e => e.animalId))
                  )

                  const filtrados = animales.filter(a =>
                    !q ||
                    a.codigoIdentificacion.toLowerCase().includes(q) ||
                    (a.nombre ?? '').toLowerCase().includes(q) ||
                    (a.raza ?? '').toLowerCase().includes(q)
                  )
                  if (animales.length === 0)
                    return <p className="text-xs text-muted-foreground text-center py-6">No hay animales en engorde disponibles.</p>
                  if (filtrados.length === 0)
                    return <p className="text-xs text-muted-foreground text-center py-6">Sin resultados para "{busqueda}".</p>
                  return filtrados.map(a => {
                    const enPotrero = yaEnPotrero.has(a.id)
                    const checked = seleccionados.has(a.id)
                    return (
                      <label key={a.id}
                        className={`flex items-center gap-3 px-3 py-2.5 border-b border-border/30 last:border-b-0 transition-colors
                          ${enPotrero ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer hover:bg-secondary/40'}
                          ${checked ? 'bg-primary/5' : ''}`}>
                        <input
                          type="checkbox"
                          checked={checked}
                          disabled={enPotrero}
                          onChange={() => {
                            if (enPotrero) return
                            setSeleccionados(prev => {
                              const next = new Set(prev)
                              next.has(a.id) ? next.delete(a.id) : next.add(a.id)
                              return next
                            })
                          }}
                          className="accent-primary w-3.5 h-3.5 flex-shrink-0"
                        />
                        <div className="flex-1 min-w-0">
                          <p className="text-xs font-semibold font-mono truncate">{a.codigoIdentificacion}</p>
                          <p className="text-[10px] text-muted-foreground truncate">
                            {[a.nombre, a.raza].filter(Boolean).join(' · ') || 'Sin nombre'}
                          </p>
                        </div>
                        {enPotrero
                          ? <span className="text-[9px] font-medium px-1.5 py-0.5 rounded bg-amber-500/10 text-amber-400 border border-amber-500/20 flex-shrink-0">Ya ingresado</span>
                          : <span className="text-[10px] tabular-nums text-muted-foreground flex-shrink-0 bg-secondary px-1.5 py-0.5 rounded">{a.pesoActualKg} kg</span>
                        }
                      </label>
                    )
                  })
                })()}
              </div>
            </div>

            {/* Alerta de error del servidor */}
            {errorIngresar && (
              <div className="flex items-start gap-2 rounded-md border border-destructive/40 bg-destructive/10 px-3 py-2.5 text-xs text-destructive">
                <span className="font-semibold flex-shrink-0">⚠</span>
                <span>{errorIngresar}</span>
              </div>
            )}

            {/* Botones */}
            <div className="flex gap-2 pt-1">
              <Button type="button" variant="outline" className="flex-1"
                onClick={() => { setModal({ type: null }); setSeleccionados(new Set()); setBusqueda(''); setErrorIngresar(undefined) }}>
                Cancelar
              </Button>
              <Button type="submit" className="flex-1" loading={ingresarAnimal.isPending} disabled={seleccionados.size === 0}>
                {seleccionados.size > 0
                  ? `Ingresar ${seleccionados.size} animal${seleccionados.size > 1 ? 'es' : ''}`
                  : 'Selecciona animales'}
              </Button>
            </div>
          </form>
        </div>
      </Dialog>

      <Dialog open={modal.type === 'eliminar'} onClose={() => setModal({ type: null })}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[400px] mx-4 p-5 space-y-4">
          <div>
            <h3 className="text-sm font-semibold">Eliminar potrero</h3>
            {modal.type === 'eliminar' && (
              <div className="mt-2 space-y-1.5">
                <p className="text-xs text-muted-foreground">
                  ¿Estás seguro de que quieres eliminar <strong className="text-foreground">{modal.nombre}</strong>?
                </p>
                {modal.animalesActuales > 0 && (
                  <div className="flex items-start gap-2 rounded-md border border-amber-500/30 bg-amber-500/10 px-3 py-2 text-xs text-amber-400">
                    <span className="font-semibold flex-shrink-0">⚠</span>
                    <span>
                      Este potrero tiene <strong>{modal.animalesActuales} animal{modal.animalesActuales > 1 ? 'es' : ''}</strong> registrado{modal.animalesActuales > 1 ? 's' : ''}.
                      Al eliminarlo también se borrarán sus registros de estancia.
                    </span>
                  </div>
                )}
                <p className="text-xs text-muted-foreground">Esta acción no se puede deshacer.</p>
              </div>
            )}
          </div>
          <div className="flex gap-2">
            <Button variant="outline" className="flex-1" onClick={() => setModal({ type: null })}>
              Cancelar
            </Button>
            <Button variant="destructive" className="flex-1" loading={eliminarPotrero.isPending}
              onClick={async () => {
                if (modal.type !== 'eliminar') return
                await eliminarPotrero.mutateAsync(modal.potreroId)
                setModal({ type: null })
              }}>
              Sí, eliminar
            </Button>
          </div>
        </div>
      </Dialog>

      <Dialog open={modal.type === 'retirar'} onClose={() => { setModal({ type: null }); retirarForm.reset() }}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[380px] mx-4">
          <div className="flex items-center justify-between px-5 py-4 border-b border-border">
            <DialogHeader className="mb-0"><DialogTitle>Retirar animal del potrero</DialogTitle></DialogHeader>
            <button onClick={() => setModal({ type: null })} className="text-muted-foreground hover:text-foreground"><X className="w-4 h-4" /></button>
          </div>
          <form onSubmit={retirarForm.handleSubmit(onSubmitRetirar)} className="p-5 space-y-4">
            <FormField label="Fecha de salida" required>
              <input type="date" max={new Date().toISOString().split('T')[0]}
                {...retirarForm.register('fechaSalida')}
                className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
            </FormField>
            <div className="flex gap-2">
              <Button type="button" variant="outline" className="flex-1" onClick={() => setModal({ type: null })}>Cancelar</Button>
              <Button type="submit" variant="destructive" className="flex-1" loading={retirarAnimal.isPending}>Retirar</Button>
            </div>
          </form>
        </div>
      </Dialog>
    </>
  )
}

// ─── Empleados section ──────────────────────────────────────────────────────
function EmpleadosSection() {
  const { data: empleados, isLoading } = useEmpleados()
  const arr = (empleados as Empleado[] | undefined) ?? []
  const crearEmpleado = useCrearEmpleado()
  const modificarEmpleado = useModificarEmpleado()
  const eliminarEmpleado = useEliminarEmpleado()
  const modificarActividad = useModificarActividad()
  const registrarActividad = useRegistrarActividadManoObra()
  const [modal, setModal] = useState<ModalState>({ type: null })
  const [expanded, setExpanded] = useState<string | null>(null)

  const empForm = useForm<{ nombre: string; pagoMensual: string }>({ defaultValues: { nombre: '', pagoMensual: '' } })
  const editForm = useForm<{ nombre: string; pagoMensual: string }>({ defaultValues: { nombre: '', pagoMensual: '' } })
  const actEditForm = useForm<{ tipo: string; fecha: string; costo: string }>({ defaultValues: { tipo: '', fecha: '', costo: '' } })
  const actForm = useForm<{ tipo: string; fecha: string; costo: string }>({ defaultValues: { tipo: '', fecha: new Date().toISOString().split('T')[0], costo: '' } })

  const onSubmitEditarActividad = async (data: { tipo: string; fecha: string; costo: string }) => {
    if (modal.type !== 'editarActividad') return
    const costo = parseFloat(data.costo.replace(/[^0-9]/g, '')) || 0
    await modificarActividad.mutateAsync({ empleadoId: modal.empleadoId, actividadId: modal.actividadId, tipo: data.tipo, fecha: data.fecha, costo, moneda: 'COP' })
    setModal({ type: null }); actEditForm.reset()
  }

  const onSubmitEditar = async (data: { nombre: string; pagoMensual: string }) => {
    if (modal.type !== 'editarEmpleado') return
    const pagoMensual = parseFloat(data.pagoMensual.replace(/[^0-9]/g, '')) || 0
    await modificarEmpleado.mutateAsync({ empleadoId: modal.empleadoId, nombre: data.nombre, pagoMensual, moneda: 'COP' })
    setModal({ type: null }); editForm.reset()
  }

  const onSubmitEmpleado = async (data: { nombre: string; pagoMensual: string }) => {
    const pagoMensual = parseFloat(data.pagoMensual.replace(/[^0-9]/g, '')) || 0
    await crearEmpleado.mutateAsync({ nombre: data.nombre, pagoMensual, moneda: 'COP' })
    setModal({ type: null }); empForm.reset()
  }

  const onSubmitActividad = async (data: { tipo: string; fecha: string; costo: string }) => {
    if (modal.type !== 'actividad') return
    const costo = parseFloat(data.costo.replace(/[^0-9]/g, '')) || 0
    await registrarActividad.mutateAsync({ empleadoId: modal.empleadoId, tipo: data.tipo, fecha: data.fecha, costo, moneda: 'COP' })
    setModal({ type: null }); actForm.reset()
  }

  if (isLoading) return <div className="space-y-4"><Skeleton className="h-20 rounded-lg" /><Skeleton className="h-20 rounded-lg" /></div>

  return (
    <>
      <div className="space-y-3">
        <div className="flex items-center justify-between">
          <p className="text-sm text-muted-foreground">{arr.length} empleados</p>
          <Button size="sm" variant="outline" onClick={() => setModal({ type: 'empleado' })}><Plus className="w-3 h-3" />Nuevo empleado</Button>
        </div>
        {arr.length === 0 && (
          <EmptyState icon={<Users className="w-5 h-5" />} title="Sin empleados" description="Registra los empleados de la finca." />
        )}
        {arr.map(e => (
          <Card key={e.id}>
            <CardHeader className="cursor-pointer" onClick={() => setExpanded(expanded === e.id ? null : e.id)}>
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle className="text-sm">{e.nombre}</CardTitle>
                  <p className="text-[10px] text-muted-foreground">Pago mensual: {fmt.cop(e.pagoMensualMonto)} · {e.actividades.length} actividades</p>
                </div>
                <div className="flex items-center gap-2">
                  <Button size="sm" variant="ghost" onClick={(ev) => { ev.stopPropagation(); setModal({ type: 'actividad', empleadoId: e.id }) }}>+ Actividad</Button>
                  <Button size="sm" variant="ghost" onClick={(ev) => {
                    ev.stopPropagation()
                    editForm.reset({ nombre: e.nombre, pagoMensual: String(e.pagoMensualMonto) })
                    setModal({ type: 'editarEmpleado', empleadoId: e.id, nombre: e.nombre, pagoMensual: e.pagoMensualMonto })
                  }}>Editar</Button>
                  <Button size="sm" variant="ghost" className="text-rose-400 hover:text-rose-300" onClick={(ev) => {
                    ev.stopPropagation()
                    setModal({ type: 'eliminarEmpleado', empleadoId: e.id, nombre: e.nombre })
                  }}>Eliminar</Button>
                  {expanded === e.id ? <ChevronDown className="w-4 h-4 text-muted-foreground" /> : <ChevronRight className="w-4 h-4 text-muted-foreground" />}
                </div>
              </div>
            </CardHeader>
            {expanded === e.id && e.actividades.length > 0 && (
              <CardContent className="p-0 border-t border-border">
                <table className="w-full text-xs">
                  <thead><tr className="border-b bg-secondary/20"><th className="text-left px-4 py-2 text-muted-foreground">Tipo</th><th className="text-left px-4 py-2 text-muted-foreground">Fecha</th><th className="text-right px-4 py-2 text-muted-foreground">Costo</th><th className="px-4 py-2" /></tr></thead>
                  <tbody>{e.actividades.map(a => (
                    <tr key={a.id} className="border-b border-border/30">
                      <td className="px-4 py-2">{a.tipo}</td>
                      <td className="px-4 py-2">{fmt.fecha(a.fecha)}</td>
                      <td className="px-4 py-2 text-right tabular-nums">{fmt.cop(a.costoMonto)}</td>
                      <td className="px-4 py-2 text-right">
                        <Button size="sm" variant="ghost" className="h-6 px-2 text-[10px]"
                          onClick={() => {
                            actEditForm.reset({
                              tipo: a.tipo,
                              fecha: typeof a.fecha === 'string' ? a.fecha : String(a.fecha),
                              costo: String(a.costoMonto),
                            })
                            setModal({ type: 'editarActividad', empleadoId: e.id, actividadId: a.id, tipo: a.tipo, fecha: typeof a.fecha === 'string' ? a.fecha : String(a.fecha), costo: a.costoMonto })
                          }}>
                          Editar
                        </Button>
                      </td>
                    </tr>
                  ))}</tbody>
                </table>
              </CardContent>
            )}
          </Card>
        ))}
      </div>

      <Dialog open={modal.type === 'empleado'} onClose={() => setModal({ type: null })}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[400px] mx-4">
          <div className="flex items-center justify-between px-5 py-4 border-b border-border">
            <DialogHeader className="mb-0"><DialogTitle>Nuevo empleado</DialogTitle></DialogHeader>
            <button onClick={() => setModal({ type: null })} className="text-muted-foreground hover:text-foreground"><X className="w-4 h-4" /></button>
          </div>
          <form onSubmit={empForm.handleSubmit(onSubmitEmpleado)} className="p-5 space-y-4">
            <FormField label="Nombre" required><input {...empForm.register('nombre')} placeholder="Ej: Juan Pérez" className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
            <FormField label="Pago mensual" required><MoneyInput min={0} {...empForm.register('pagoMensual')} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
            <Button type="submit" className="w-full" loading={crearEmpleado.isPending}>Crear empleado</Button>
          </form>
        </div>
      </Dialog>

      {/* Modal editar empleado */}
      <Dialog open={modal.type === 'editarEmpleado'} onClose={() => { setModal({ type: null }); editForm.reset() }}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[400px] mx-4">
          <div className="flex items-center justify-between px-5 py-4 border-b border-border">
            <DialogHeader className="mb-0"><DialogTitle>Editar empleado</DialogTitle></DialogHeader>
            <button onClick={() => { setModal({ type: null }); editForm.reset() }} className="text-muted-foreground hover:text-foreground"><X className="w-4 h-4" /></button>
          </div>
          <form onSubmit={editForm.handleSubmit(onSubmitEditar)} className="p-5 space-y-4">
            <FormField label="Nombre" required>
              <input {...editForm.register('nombre')} placeholder="Ej: Juan Pérez" className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
            </FormField>
            <FormField label="Pago mensual" required>
              <MoneyInput min={0} value={editForm.watch('pagoMensual')} {...editForm.register('pagoMensual')} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
            </FormField>
            <div className="flex gap-2">
              <Button type="button" variant="outline" className="flex-1" onClick={() => { setModal({ type: null }); editForm.reset() }}>Cancelar</Button>
              <Button type="submit" className="flex-1" loading={modificarEmpleado.isPending}>Guardar cambios</Button>
            </div>
          </form>
        </div>
      </Dialog>

      {/* Modal confirmar eliminar empleado */}
      <Dialog open={modal.type === 'eliminarEmpleado'} onClose={() => setModal({ type: null })}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[380px] mx-4 p-5 space-y-4">
          <div>
            <h3 className="text-sm font-semibold">Eliminar empleado</h3>
            {modal.type === 'eliminarEmpleado' && (
              <p className="text-xs text-muted-foreground mt-2">
                ¿Estás seguro de que quieres eliminar a <strong className="text-foreground">{modal.nombre}</strong>?
                Se eliminarán también sus actividades registradas. Esta acción no se puede deshacer.
              </p>
            )}
          </div>
          <div className="flex gap-2">
            <Button variant="outline" className="flex-1" onClick={() => setModal({ type: null })}>Cancelar</Button>
            <Button variant="destructive" className="flex-1" loading={eliminarEmpleado.isPending}
              onClick={async () => {
                if (modal.type !== 'eliminarEmpleado') return
                await eliminarEmpleado.mutateAsync(modal.empleadoId)
                setModal({ type: null })
              }}>
              Sí, eliminar
            </Button>
          </div>
        </div>
      </Dialog>

      {/* Modal editar actividad */}
      <Dialog open={modal.type === 'editarActividad'} onClose={() => { setModal({ type: null }); actEditForm.reset() }}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[400px] mx-4">
          <div className="flex items-center justify-between px-5 py-4 border-b border-border">
            <DialogHeader className="mb-0"><DialogTitle>Editar actividad</DialogTitle></DialogHeader>
            <button onClick={() => { setModal({ type: null }); actEditForm.reset() }} className="text-muted-foreground hover:text-foreground"><X className="w-4 h-4" /></button>
          </div>
          <form onSubmit={actEditForm.handleSubmit(onSubmitEditarActividad)} className="p-5 space-y-4">
            <FormField label="Tipo" required>
              <input {...actEditForm.register('tipo')} placeholder="Ej: Alimentación, Fumigación" className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
            </FormField>
            <FormField label="Fecha" required>
              <input type="date" {...actEditForm.register('fecha')} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
            </FormField>
            <FormField label="Costo" required>
              <MoneyInput min={0} value={actEditForm.watch('costo')} {...actEditForm.register('costo')} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
            </FormField>
            <div className="flex gap-2">
              <Button type="button" variant="outline" className="flex-1" onClick={() => { setModal({ type: null }); actEditForm.reset() }}>Cancelar</Button>
              <Button type="submit" className="flex-1" loading={modificarActividad.isPending}>Guardar cambios</Button>
            </div>
          </form>
        </div>
      </Dialog>

      <Dialog open={modal.type === 'actividad'} onClose={() => setModal({ type: null })}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[400px] mx-4">
          <div className="flex items-center justify-between px-5 py-4 border-b border-border">
            <DialogHeader className="mb-0"><DialogTitle>Registrar actividad</DialogTitle></DialogHeader>
            <button onClick={() => setModal({ type: null })} className="text-muted-foreground hover:text-foreground"><X className="w-4 h-4" /></button>
          </div>
          <form onSubmit={actForm.handleSubmit(onSubmitActividad)} className="p-5 space-y-4">
            <FormField label="Tipo" required><input {...actForm.register('tipo')} placeholder="Ej: Alimentación, Fumigación" className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
            <FormField label="Fecha" required><input type="date" {...actForm.register('fecha')} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
            <FormField label="Costo" required><MoneyInput min={0} {...actForm.register('costo')} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
            <Button type="submit" className="w-full" loading={registrarActividad.isPending}>Registrar</Button>
          </form>
        </div>
      </Dialog>
    </>
  )
}

// ─── Caña section ────────────────────────────────────────────────────────────
function CaniaSection() {
  const { data: cultivos, isLoading } = useCultivosCania()
  const { data: lotesSilo } = useLotesSilo()
  const arr = (cultivos as CultivoCania[] | undefined) ?? []
  const siloArr = (lotesSilo as any[] | undefined) ?? []
  const crearCultivo = useCrearCultivoCania()
  const registrarCorte = useRegistrarCorteCania()
  const crearLoteSilo = useCrearLoteSilo()
  const [modal, setModal] = useState<ModalState>({ type: null })
  const [expanded, setExpanded] = useState<string | null>(null)

  const cultivoForm = useForm<{ nombre: string; callesTotales: number }>({ defaultValues: { nombre: '', callesTotales: 0 } })
  const corteForm = useForm<{ fecha: string; nCalles: number; horas: number; bolsasSilo: number; melaza: number; costoJornal: string }>({
    defaultValues: { fecha: new Date().toISOString().split('T')[0], nCalles: 0, horas: 0, bolsasSilo: 0, melaza: 0, costoJornal: '' },
  })
  const siloForm = useForm<{ fechaProduccion: string; bolsas: number; costoUnitario: string; observacion: string }>({
    defaultValues: { fechaProduccion: new Date().toISOString().split('T')[0], bolsas: 0, costoUnitario: '', observacion: '' },
  })

  const onSubmitCultivo = async (data: { nombre: string; callesTotales: number }) => {
    await crearCultivo.mutateAsync(data); setModal({ type: null }); cultivoForm.reset()
  }

  const onSubmitCorte = async (data: any) => {
    if (modal.type !== 'corte') return
    const costoJornal = parseFloat(String(data.costoJornal).replace(/[^0-9]/g, '')) || 0
    await registrarCorte.mutateAsync({ cultivoId: modal.cultivoId, ...data, costoJornal, moneda: 'COP' })
    setModal({ type: null }); corteForm.reset()
  }

  const onSubmitSilo = async (data: any) => {
    const costoUnitario = parseFloat(String(data.costoUnitario).replace(/[^0-9]/g, '')) || 0
    await crearLoteSilo.mutateAsync({ ...data, costoUnitario, moneda: 'COP' })
    setModal({ type: null }); siloForm.reset()
  }

  if (isLoading) return <div className="space-y-4"><Skeleton className="h-20 rounded-lg" /></div>

  return (
    <>
      {/* Cultivos */}
      <div className="flex items-center justify-between mb-3">
        <p className="text-sm text-muted-foreground">{arr.length} cultivos · {siloArr.length} lotes de silo</p>
        <div className="flex gap-2">
          <Button size="sm" variant="outline" onClick={() => setModal({ type: 'cultivo' })}><Sprout className="w-3 h-3" />Nuevo cultivo</Button>
          <Button size="sm" variant="outline" onClick={() => setModal({ type: 'silo' })}><Wheat className="w-3 h-3" />Nuevo lote silo</Button>
        </div>
      </div>

      {arr.length === 0 ? (
        <EmptyState icon={<Sprout className="w-5 h-5" />} title="Sin cultivos" description="Registra los cultivos de caña." />
      ) : (
        <div className="space-y-3">
          {arr.map(c => (
            <Card key={c.id}>
              <CardHeader className="cursor-pointer" onClick={() => setExpanded(expanded === c.id ? null : c.id)}>
                <div className="flex items-center justify-between">
                  <div>
                    <CardTitle className="text-sm">{c.nombre}</CardTitle>
                    <p className="text-[10px] text-muted-foreground">{c.callesTotales} calles · {c.totalCortes} cortes · {c.totalBolsasSilo} bolsas silo</p>
                  </div>
                  <div className="flex items-center gap-2">
                    <Button size="sm" variant="ghost" onClick={(ev) => { ev.stopPropagation(); setModal({ type: 'corte', cultivoId: c.id }) }}>+ Corte</Button>
                    {expanded === c.id ? <ChevronDown className="w-4 h-4" /> : <ChevronRight className="w-4 h-4" />}
                  </div>
                </div>
              </CardHeader>
              {expanded === c.id && c.cortes.length > 0 && (
                <CardContent className="p-0 border-t border-border">
                  <table className="w-full text-xs">
                    <thead>
                      <tr className="border-b bg-secondary/20">
                        <th className="text-left px-4 py-2 text-muted-foreground">Fecha</th>
                        <th className="text-center px-4 py-2 text-muted-foreground">Calles</th>
                        <th className="text-center px-4 py-2 text-muted-foreground">Horas</th>
                        <th className="text-center px-4 py-2 text-muted-foreground">Bolsas silo</th>
                        <th className="text-center px-4 py-2 text-muted-foreground">Melaza (kg)</th>
                        <th className="text-right px-4 py-2 text-muted-foreground">Costo jornal</th>
                      </tr>
                    </thead>
                    <tbody>{c.cortes.map(cc => (
                      <tr key={cc.id} className="border-b border-border/30">
                        <td className="px-4 py-2">{fmt.fecha(cc.fecha)}</td>
                        <td className="px-4 py-2 text-center tabular-nums">{cc.nCalles}</td>
                        <td className="px-4 py-2 text-center tabular-nums">{cc.horas}</td>
                        <td className="px-4 py-2 text-center tabular-nums">{cc.bolsasSilo}</td>
                        <td className="px-4 py-2 text-center tabular-nums">{cc.melaza}</td>
                        <td className="px-4 py-2 text-right tabular-nums">{fmt.cop(cc.costoJornalMonto)}</td>
                      </tr>
                    ))}</tbody>
                  </table>
                </CardContent>
              )}
            </Card>
          ))}
        </div>
      )}

      {/* Lotes de silo */}
      {siloArr.length > 0 && (
        <div className="mt-6">
          <p className="text-sm font-medium mb-2">Lotes de silo</p>
          <table className="w-full text-xs">
            <thead>
              <tr className="border-b">
                <th className="text-left px-4 py-2 text-muted-foreground">Fecha</th>
                <th className="text-center px-4 py-2 text-muted-foreground">Bolsas</th>
                <th className="text-right px-4 py-2 text-muted-foreground">Costo unitario</th>
                <th className="text-right px-4 py-2 text-muted-foreground">Total</th>
              </tr>
            </thead>
            <tbody>{siloArr.map((l: any) => (
              <tr key={l.id} className="border-b border-border/30">
                <td className="px-4 py-2">{fmt.fecha(l.fechaProduccion)}</td>
                <td className="px-4 py-2 text-center tabular-nums">{l.bolsas}</td>
                <td className="px-4 py-2 text-right tabular-nums">{fmt.cop(l.costoUnitarioMonto)}</td>
                <td className="px-4 py-2 text-right tabular-nums font-medium">{fmt.cop(l.costoTotal)}</td>
              </tr>
            ))}</tbody>
          </table>
        </div>
      )}

      {/* Modales */}
      <Dialog open={modal.type === 'cultivo'} onClose={() => setModal({ type: null })}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[400px] mx-4">
          <div className="flex items-center justify-between px-5 py-4 border-b border-border">
            <DialogHeader className="mb-0"><DialogTitle>Nuevo cultivo de caña</DialogTitle></DialogHeader>
            <button onClick={() => setModal({ type: null })} className="text-muted-foreground hover:text-foreground"><X className="w-4 h-4" /></button>
          </div>
          <form onSubmit={cultivoForm.handleSubmit(onSubmitCultivo)} className="p-5 space-y-4">
            <FormField label="Nombre" required><input {...cultivoForm.register('nombre')} placeholder="Ej: Caña panelera" className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
            <FormField label="Calles totales" required><input type="number" min={1} {...cultivoForm.register('callesTotales', { valueAsNumber: true })} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
            <Button type="submit" className="w-full" loading={crearCultivo.isPending}>Crear cultivo</Button>
          </form>
        </div>
      </Dialog>

      <Dialog open={modal.type === 'corte'} onClose={() => setModal({ type: null })}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[420px] mx-4">
          <div className="flex items-center justify-between px-5 py-4 border-b border-border">
            <DialogHeader className="mb-0"><DialogTitle>Registrar corte</DialogTitle></DialogHeader>
            <button onClick={() => setModal({ type: null })} className="text-muted-foreground hover:text-foreground"><X className="w-4 h-4" /></button>
          </div>
          <form onSubmit={corteForm.handleSubmit(onSubmitCorte)} className="p-5 space-y-4">
            <div className="grid grid-cols-2 gap-3">
              <FormField label="Fecha" required><input type="date" {...corteForm.register('fecha')} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
              <FormField label="Calles" required><input type="number" min={1} {...corteForm.register('nCalles', { valueAsNumber: true })} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
              <FormField label="Horas" required><input type="number" min={0} step={0.5} {...corteForm.register('horas', { valueAsNumber: true })} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
              <FormField label="Bolsas silo" required><input type="number" min={0} {...corteForm.register('bolsasSilo', { valueAsNumber: true })} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
              <FormField label="Melaza (kg)" required><input type="number" min={0} step={0.1} {...corteForm.register('melaza', { valueAsNumber: true })} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
              <FormField label="Costo jornal" required><MoneyInput min={0} {...corteForm.register('costoJornal')} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
            </div>
            <Button type="submit" className="w-full" loading={registrarCorte.isPending}>Registrar corte</Button>
          </form>
        </div>
      </Dialog>

      <Dialog open={modal.type === 'silo'} onClose={() => setModal({ type: null })}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[400px] mx-4">
          <div className="flex items-center justify-between px-5 py-4 border-b border-border">
            <DialogHeader className="mb-0"><DialogTitle>Nuevo lote de silo</DialogTitle></DialogHeader>
            <button onClick={() => setModal({ type: null })} className="text-muted-foreground hover:text-foreground"><X className="w-4 h-4" /></button>
          </div>
          <form onSubmit={siloForm.handleSubmit(onSubmitSilo)} className="p-5 space-y-4">
            <FormField label="Fecha producción" required><input type="date" {...siloForm.register('fechaProduccion')} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
            <FormField label="Bolsas" required><input type="number" min={1} {...siloForm.register('bolsas', { valueAsNumber: true })} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
            <FormField label="Costo unitario (por bolsa)" required><MoneyInput min={0} {...siloForm.register('costoUnitario')} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
            <FormField label="Observación"><input {...siloForm.register('observacion')} placeholder="Opcional" className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
            <Button type="submit" className="w-full" loading={crearLoteSilo.isPending}>Crear lote</Button>
          </form>
        </div>
      </Dialog>
    </>
  )
}

export default function OperacionPage() {
  const [tab, setTab] = useState<Tab>('potreros')

  return (
    <div className="flex flex-col h-full animate-fade-in">
      <PageHeader
        title="Operación"
        description="Potreros, mano de obra y producción agrícola"
      />

      {/* Tabs */}
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
        {tab === 'potreros' && <PotrerosSection />}
        {tab === 'empleados' && <EmpleadosSection />}
        {tab === 'cania' && <CaniaSection />}
      </div>
    </div>
  )
}
