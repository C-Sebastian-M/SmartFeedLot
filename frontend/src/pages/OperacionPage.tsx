import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Plus, X, Trees, Users, Sprout, ChevronDown, ChevronRight, Wheat } from 'lucide-react'
import { usePotreros, useCrearPotrero, useIngresarAnimalPotrero, useRetirarAnimalPotrero,
  useEmpleados, useCrearEmpleado, useRegistrarActividadManoObra,
  useCultivosCania, useCrearCultivoCania, useRegistrarCorteCania,
  useLotesSilo, useCrearLoteSilo } from '@/hooks/useFeedlot'
import {
  PageHeader, Card, CardHeader, CardTitle, CardContent,
  Skeleton, EmptyState, Button,
  Dialog, DialogHeader, DialogTitle,
  FormField,
  MoneyInput,
} from '@/components/ui'
import { fmt } from '@/utils'
import type { Potrero, Empleado, CultivoCania } from '@/types'

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
  | { type: 'actividad'; empleadoId: string }
  | { type: 'cultivo' }
  | { type: 'corte'; cultivoId: string }
  | { type: 'silo' }
  | { type: 'retirar'; potreroId: string; estanciaId: string }

// ─── Potrero section ────────────────────────────────────────────────────────
function PotrerosSection() {
  const { data: potreros, isLoading } = usePotreros()
  const arr = (potreros as Potrero[] | undefined) ?? []
  const crearPotrero = useCrearPotrero()
  const ingresarAnimal = useIngresarAnimalPotrero()
  const retirarAnimal = useRetirarAnimalPotrero()
  const [modal, setModal] = useState<ModalState>({ type: null })

  const potreroForm = useForm<{ nombre: string; capacidad: number }>({
    defaultValues: { nombre: '', capacidad: 50 },
  })

  const ingresarForm = useForm<{ animalId: string; fechaEntrada: string }>({
    defaultValues: { animalId: '', fechaEntrada: new Date().toISOString().split('T')[0] },
  })

  const retirarForm = useForm<{ fechaSalida: string }>({
    defaultValues: { fechaSalida: new Date().toISOString().split('T')[0] },
  })

  const onSubmitPotrero = async (data: { nombre: string; capacidad: number }) => {
    await crearPotrero.mutateAsync(data)
    setModal({ type: null })
    potreroForm.reset()
  }

  const onSubmitIngresar = async (data: { animalId: string; fechaEntrada: string }) => {
    if (modal.type !== 'ingresar') return
    await ingresarAnimal.mutateAsync({ potreroId: modal.potreroId, ...data })
    setModal({ type: null })
    ingresarForm.reset()
  }

  const onSubmitRetirar = async (data: { fechaSalida: string }) => {
    if (modal.type !== 'retirar') return
    await retirarAnimal.mutateAsync({ potreroId: modal.potreroId, estanciaId: modal.estanciaId, ...data })
    setModal({ type: null })
    retirarForm.reset()
  }

  if (isLoading) return <div className="space-y-4"><Skeleton className="h-20 rounded-lg" /><Skeleton className="h-20 rounded-lg" /></div>
  if (arr.length === 0) return <EmptyState icon={<Trees className="w-5 h-5" />} title="Sin potreros" description="Registra los potreros de la finca." action={<Button size="sm" onClick={() => setModal({ type: 'potrero' })}><Plus className="w-3.5 h-3.5" />Crear potrero</Button>} />

  return (
    <>
      <div className="space-y-3">
        <div className="flex items-center justify-between">
          <p className="text-sm text-muted-foreground">{arr.length} potreros</p>
          <Button size="sm" variant="outline" onClick={() => setModal({ type: 'potrero' })}><Plus className="w-3 h-3" />Nuevo potrero</Button>
        </div>
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
                  </div>
                </div>
              </CardHeader>
              {p.estancias.filter(e => !e.salida).length > 0 && (
                <CardContent className="p-0 border-t border-border">
                  <table className="w-full text-xs">
                    <thead><tr className="border-b bg-secondary/20"><th className="text-left px-4 py-2 text-muted-foreground">Animal ID</th><th className="text-left px-4 py-2 text-muted-foreground">Ingreso</th><th className="px-4 py-2" /></tr></thead>
                    <tbody>{p.estancias.filter(e => !e.salida).map(e => (
                      <tr key={e.id} className="border-b border-border/30">
                        <td className="px-4 py-2 font-mono">{e.animalId.slice(0, 8)}…</td>
                        <td className="px-4 py-2">{fmt.fecha(e.fechaEntrada)}</td>
                        <td className="px-4 py-2 text-right">
                          <Button size="sm" variant="ghost" className="text-rose-400 hover:text-rose-300 h-6 px-2 text-[10px]"
                            onClick={() => setModal({ type: 'retirar', potreroId: p.id, estanciaId: e.id })}>
                            Retirar
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

      <Dialog open={modal.type === 'ingresar'} onClose={() => setModal({ type: null })}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[400px] mx-4">
          <div className="flex items-center justify-between px-5 py-4 border-b border-border">
            <DialogHeader className="mb-0"><DialogTitle>Ingresar animal</DialogTitle></DialogHeader>
            <button onClick={() => setModal({ type: null })} className="text-muted-foreground hover:text-foreground"><X className="w-4 h-4" /></button>
          </div>
          <form onSubmit={ingresarForm.handleSubmit(onSubmitIngresar)} className="p-5 space-y-4">
            <FormField label="Animal ID" required><input {...ingresarForm.register('animalId')} placeholder="GUID del animal" className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
            <FormField label="Fecha entrada" required><input type="date" {...ingresarForm.register('fechaEntrada')} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
            <Button type="submit" className="w-full" loading={ingresarAnimal.isPending}>Ingresar</Button>
          </form>
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
  const registrarActividad = useRegistrarActividadManoObra()
  const [modal, setModal] = useState<ModalState>({ type: null })
  const [expanded, setExpanded] = useState<string | null>(null)

  const empForm = useForm<{ nombre: string; pagoMensual: number }>({ defaultValues: { nombre: '', pagoMensual: 200000 } })
  const actForm = useForm<{ tipo: string; fecha: string; costo: number }>({ defaultValues: { tipo: '', fecha: new Date().toISOString().split('T')[0], costo: 0 } })

  const onSubmitEmpleado = async (data: { nombre: string; pagoMensual: number }) => {
    await crearEmpleado.mutateAsync({ ...data, moneda: 'COP' })
    setModal({ type: null }); empForm.reset()
  }

  const onSubmitActividad = async (data: { tipo: string; fecha: string; costo: number }) => {
    if (modal.type !== 'actividad') return
    await registrarActividad.mutateAsync({ empleadoId: modal.empleadoId, ...data, moneda: 'COP' })
    setModal({ type: null }); actForm.reset()
  }

  if (isLoading) return <div className="space-y-4"><Skeleton className="h-20 rounded-lg" /><Skeleton className="h-20 rounded-lg" /></div>
  if (arr.length === 0) return <EmptyState icon={<Users className="w-5 h-5" />} title="Sin empleados" description="Registra los empleados de la finca." action={<Button size="sm" onClick={() => setModal({ type: 'empleado' })}><Plus className="w-3.5 h-3.5" />Crear empleado</Button>} />

  return (
    <>
      <div className="space-y-3">
        <div className="flex items-center justify-between">
          <p className="text-sm text-muted-foreground">{arr.length} empleados</p>
          <Button size="sm" variant="outline" onClick={() => setModal({ type: 'empleado' })}><Plus className="w-3 h-3" />Nuevo empleado</Button>
        </div>
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
                  {expanded === e.id ? <ChevronDown className="w-4 h-4 text-muted-foreground" /> : <ChevronRight className="w-4 h-4 text-muted-foreground" />}
                </div>
              </div>
            </CardHeader>
            {expanded === e.id && e.actividades.length > 0 && (
              <CardContent className="p-0 border-t border-border">
                <table className="w-full text-xs">
                  <thead><tr className="border-b bg-secondary/20"><th className="text-left px-4 py-2 text-muted-foreground">Tipo</th><th className="text-left px-4 py-2 text-muted-foreground">Fecha</th><th className="text-right px-4 py-2 text-muted-foreground">Costo</th></tr></thead>
                  <tbody>{e.actividades.map(a => (
                    <tr key={a.id} className="border-b border-border/30"><td className="px-4 py-2">{a.tipo}</td><td className="px-4 py-2">{fmt.fecha(a.fecha)}</td><td className="px-4 py-2 text-right tabular-nums">{fmt.cop(a.costoMonto)}</td></tr>
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
            <FormField label="Pago mensual" required><MoneyInput min={0} {...empForm.register('pagoMensual', { })} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
            <Button type="submit" className="w-full" loading={crearEmpleado.isPending}>Crear empleado</Button>
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
            <FormField label="Costo" required><MoneyInput min={0} {...actForm.register('costo', { })} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
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
  const corteForm = useForm<{ fecha: string; nCalles: number; horas: number; bolsasSilo: number; melaza: number; costoJornal: number }>({
    defaultValues: { fecha: new Date().toISOString().split('T')[0], nCalles: 0, horas: 0, bolsasSilo: 0, melaza: 0, costoJornal: 0 },
  })
  const siloForm = useForm<{ fechaProduccion: string; bolsas: number; costoUnitario: number; observacion: string }>({
    defaultValues: { fechaProduccion: new Date().toISOString().split('T')[0], bolsas: 0, costoUnitario: 0, observacion: '' },
  })

  const onSubmitCultivo = async (data: { nombre: string; callesTotales: number }) => {
    await crearCultivo.mutateAsync(data); setModal({ type: null }); cultivoForm.reset()
  }

  const onSubmitCorte = async (data: any) => {
    if (modal.type !== 'corte') return
    await registrarCorte.mutateAsync({ cultivoId: modal.cultivoId, ...data, moneda: 'COP' })
    setModal({ type: null }); corteForm.reset()
  }

  const onSubmitSilo = async (data: any) => {
    await crearLoteSilo.mutateAsync({ ...data, moneda: 'COP' })
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
                    <thead><tr className="border-b bg-secondary/20"><th className="text-left px-4 py-2">Fecha</th><th className="text-right px-4 py-2">Calles</th><th className="text-right px-4 py-2">Horas</th><th className="text-right px-4 py-2">Bolsas</th><th className="text-right px-4 py-2">Melaza</th><th className="text-right px-4 py-2">Jornal</th></tr></thead>
                    <tbody>{c.cortes.map(cc => (
                      <tr key={cc.id} className="border-b border-border/30"><td className="px-4 py-2">{fmt.fecha(cc.fecha)}</td><td className="px-4 py-2 text-right">{cc.nCalles}</td><td className="px-4 py-2 text-right">{cc.horas}</td><td className="px-4 py-2 text-right">{cc.bolsasSilo}</td><td className="px-4 py-2 text-right">{cc.melaza}</td><td className="px-4 py-2 text-right tabular-nums">{fmt.cop(cc.costoJornalMonto)}</td></tr>
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
            <thead><tr className="border-b"><th className="text-left px-4 py-2 text-muted-foreground">Fecha</th><th className="text-right px-4 py-2 text-muted-foreground">Bolsas</th><th className="text-right px-4 py-2 text-muted-foreground">Costo unit</th><th className="text-right px-4 py-2 text-muted-foreground">Total</th></tr></thead>
            <tbody>{siloArr.map((l: any) => (
              <tr key={l.id} className="border-b border-border/30"><td className="px-4 py-2">{fmt.fecha(l.fechaProduccion)}</td><td className="px-4 py-2 text-right">{l.bolsas}</td><td className="px-4 py-2 text-right tabular-nums">{fmt.cop(l.costoUnitarioMonto)}</td><td className="px-4 py-2 text-right tabular-nums font-medium">{fmt.cop(l.costoTotal)}</td></tr>
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
              <FormField label="Melaza" required><input type="number" min={0} step={0.1} {...corteForm.register('melaza', { valueAsNumber: true })} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
              <FormField label="Costo jornal" required><MoneyInput min={0} {...corteForm.register('costoJornal', { })} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
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
            <FormField label="Costo unitario" required><MoneyInput min={0} {...siloForm.register('costoUnitario', { })} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" /></FormField>
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
