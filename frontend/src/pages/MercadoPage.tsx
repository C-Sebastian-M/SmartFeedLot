import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Plus, X, TrendingUp, Pencil, Trash2, Download, ChevronDown, ChevronUp, RefreshCw } from 'lucide-react'
import {
  usePreciosMercado, useCrearPrecioMercado, useActualizarPrecioMercado, useEliminarPrecioMercado,
  useSubaganEventos, useSubaganLotes, useImportarSubasta, useEliminarSubaganEvento,
} from '@/hooks/useFeedlot'
import {
  PageHeader, Skeleton, EmptyState, Button, Card, CardContent,
  Dialog, DialogHeader, DialogTitle, DialogDescription,
  FormField, MoneyInput, CustomSelect,
} from '@/components/ui'
import { fmt } from '@/utils'
import type { PrecioMercado, SubaganEvento } from '@/types'

const especies = ['Bovino', 'Porcino']
const tipos = ['Novillo', 'Ternero', 'Vaca', 'Cerdo Preceba', 'Cerdo Ceba', 'Lechón']

type PrecioForm = { fecha: string; especie: string; tipo: string; precioPorKg: string; fuente: string }
const defaultValues: PrecioForm = {
  fecha: new Date().toISOString().split('T')[0],
  especie: 'Bovino', tipo: 'Novillo', precioPorKg: '', fuente: '',
}

// ── Componente fila de evento SUBAGAN ─────────────────────────────────────────
function SubaganEventoRow({ evento, onEliminar }: { evento: SubaganEvento; onEliminar: (e: SubaganEvento) => void }) {
  const [expanded, setExpanded] = useState(false)

  const tipoColors: Record<string, string> = {
    MC: 'bg-blue-500/10 text-blue-400',
    ML: 'bg-cyan-500/10 text-cyan-400',
    HV: 'bg-pink-500/10 text-pink-400',
    HL: 'bg-rose-500/10 text-rose-400',
    VE: 'bg-amber-500/10 text-amber-400',
    VC: 'bg-orange-500/10 text-orange-400',
    TO: 'bg-red-500/10 text-red-400',
    XX: 'bg-gray-500/10 text-gray-400',
  }

  return (
    <div className="border border-border/40 rounded-lg overflow-hidden">
      <div className="w-full flex items-center justify-between px-4 py-3 hover:bg-muted/20 transition-colors">
        <button
          onClick={() => setExpanded(!expanded)}
          className="flex items-center gap-4 text-left flex-1 min-w-0"
        >
          <div>
            <span className="font-medium text-sm">
              {evento.numeroSubasta ? `Subasta #${evento.numeroSubasta}` : `Evento ${evento.subaganEventoId}`}
            </span>
            <span className="text-xs text-muted-foreground ml-2">{fmt.fecha(evento.fecha)}</span>
          </div>
          <span className="text-xs text-muted-foreground">{evento.totalLotes} lotes</span>
          <span className="text-xs text-muted-foreground hidden sm:block">{evento.sede}</span>
        </button>
        <div className="flex items-center gap-2 shrink-0">
          <span className="text-xs text-muted-foreground hidden md:block">
            Importado {new Date(evento.importadoEn).toLocaleDateString('es-CO')}
          </span>
          <button
            onClick={() => onEliminar(evento)}
            className="p-1.5 rounded-md text-muted-foreground hover:text-destructive hover:bg-destructive/10 transition-colors"
            title="Eliminar evento"
          >
            <Trash2 className="w-3.5 h-3.5" />
          </button>
          <button
            onClick={() => setExpanded(!expanded)}
            className="p-1 text-muted-foreground hover:text-foreground transition-colors"
            title={expanded ? 'Contraer' : 'Expandir'}
          >
            {expanded ? <ChevronUp className="w-4 h-4" /> : <ChevronDown className="w-4 h-4" />}
          </button>
        </div>
      </div>

      {expanded && (
        <SubaganLotesTable eventoId={evento.id} tipoColors={tipoColors} />
      )}
    </div>
  )
}

function SubaganLotesTable({ eventoId, tipoColors }: { eventoId: string; tipoColors: Record<string, string> }) {
  const { data: lotes, isLoading } = useSubaganLotes(eventoId)

  if (isLoading) return (
    <div className="p-4 space-y-2 border-t border-border/40">
      <Skeleton className="h-8 rounded" />
      <Skeleton className="h-8 rounded" />
    </div>
  )

  if (!lotes?.length) return (
    <div className="p-4 border-t border-border/40 text-sm text-muted-foreground">Sin lotes.</div>
  )

  return (
    <div className="border-t border-border/40 overflow-x-auto">
      <table className="w-full text-xs">
        <thead>
          <tr className="border-b border-border/30 bg-muted/10">
            {['#', 'Tipo', 'Cant.', 'Peso Total', 'Peso Prom.', '$/kg', 'Procedencia'].map(h => (
              <th key={h} className="px-3 py-2 text-left text-muted-foreground font-medium whitespace-nowrap">{h}</th>
            ))}
          </tr>
        </thead>
        <tbody>
          {lotes.map((l: any) => (
            <tr key={l.id} className="border-b border-border/20 hover:bg-muted/10 transition-colors">
              <td className="px-3 py-2 tabular-nums">{l.numeroLote}</td>
              <td className="px-3 py-2">
                <span className={`px-1.5 py-0.5 rounded text-[10px] font-medium ${tipoColors[l.codigoTipo] ?? 'bg-gray-500/10 text-gray-400'}`}>
                  {l.codigoTipo}
                </span>
              </td>
              <td className="px-3 py-2 tabular-nums">{l.cantidad}</td>
              <td className="px-3 py-2 tabular-nums">{l.pesoTotal.toLocaleString('es-CO')} kg</td>
              <td className="px-3 py-2 tabular-nums">{l.pesoProm.toLocaleString('es-CO')} kg</td>
              <td className="px-3 py-2 tabular-nums font-medium text-emerald-400">
                ${l.precioPorKg.toLocaleString('es-CO')}
              </td>
              <td className="px-3 py-2 text-muted-foreground truncate max-w-[140px]">{l.procedencia}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

// ── Página principal ──────────────────────────────────────────────────────────
export default function MercadoPage() {
  const { data: precios, isLoading } = usePreciosMercado()
  const arr = (precios as PrecioMercado[] | undefined) ?? []
  const crearPrecio = useCrearPrecioMercado()
  const actualizarPrecio = useActualizarPrecioMercado()
  const eliminarPrecio = useEliminarPrecioMercado()

  const { data: subaganEventos, isLoading: loadingEventos } = useSubaganEventos()
  const importarSubasta = useImportarSubasta()
  const eliminarEvento = useEliminarSubaganEvento()
  const [confirmarEliminarEvento, setConfirmarEliminarEvento] = useState<SubaganEvento | undefined>(undefined)
  const [importando, setImportando] = useState(false)
  const [importEventId, setImportEventId] = useState('')
  const [importNumero, setImportNumero] = useState('')
  const [importResult, setImportResult] = useState<string | null>(null)

  const [modalOpen, setModalOpen] = useState(false)
  const [editando, setEditando] = useState<PrecioMercado | undefined>(undefined)
  const [confirmarEliminar, setConfirmarEliminar] = useState<PrecioMercado | undefined>(undefined)

  const [tab, setTab] = useState<'manuales' | 'subagan'>('subagan')

  const form = useForm<PrecioForm>({ defaultValues })

  const handleNuevo = () => { setEditando(undefined); form.reset(defaultValues); setModalOpen(true) }
  const handleEditar = (p: PrecioMercado) => {
    setEditando(p)
    form.reset({ fecha: p.fecha, especie: p.especie, tipo: p.tipo, precioPorKg: String(p.precioPorKg), fuente: p.fuente })
    setModalOpen(true)
  }
  const handleCerrar = () => { setModalOpen(false); setEditando(undefined); form.reset(defaultValues) }

  const onSubmit = async (data: PrecioForm) => {
    const precioPorKg = parseFloat(data.precioPorKg.replace(/[^0-9]/g, '')) || 0
    if (editando) await actualizarPrecio.mutateAsync({ id: editando.id, ...data, precioPorKg })
    else await crearPrecio.mutateAsync({ ...data, precioPorKg })
    handleCerrar()
  }

  const handleEliminar = async (p: PrecioMercado) => {
    try { await eliminarPrecio.mutateAsync(p.id) } catch { }
    setConfirmarEliminar(undefined)
  }

  const handleEliminarEvento = async (e: SubaganEvento) => {
    try { await eliminarEvento.mutateAsync(e.id) } catch { }
    setConfirmarEliminarEvento(undefined)
  }

  const handleImportar = async () => {
    const eventId = parseInt(importEventId)
    if (!eventId) return
    setImportando(true)
    setImportResult(null)
    try {
      const res = await importarSubasta.mutateAsync({
        eventId,
        numeroSubasta: importNumero ? parseInt(importNumero) : null,
      })
      setImportResult(res.yaExistia
        ? `✓ Este evento ya estaba importado (${res.totalLotes} lotes, ${fmt.fecha(res.fecha)})`
        : `✓ Importados ${res.totalLotes} lotes del ${fmt.fecha(res.fecha)}`)
      setImportEventId('')
      setImportNumero('')
    } catch (e: any) {
      setImportResult(`✗ Error: ${e?.response?.data?.error ?? 'No se pudo importar'}`)
    } finally {
      setImportando(false)
    }
  }

  const isPending = crearPrecio.isPending || actualizarPrecio.isPending
  const eventos = subaganEventos ?? []

  return (
    <div className="flex flex-col h-full animate-fade-in">
      <PageHeader
        title="Precios de Mercado"
        description="Referencias de precio por kg según canal y especie"
        action={
          tab === 'manuales' && arr.length > 0 ? (
            <Button size="sm" onClick={handleNuevo}>
              <Plus className="w-3.5 h-3.5" />Nuevo precio
            </Button>
          ) : undefined
        }
      />

      {/* Tabs */}
      <div className="px-6 pt-4 flex gap-1 border-b border-border">
        {(['subagan', 'manuales'] as const).map(t => (
          <button
            key={t}
            onClick={() => setTab(t)}
            className={`px-4 py-2 text-sm font-medium rounded-t-md transition-colors ${
              tab === t
                ? 'bg-background border border-b-background border-border text-foreground -mb-px'
                : 'text-muted-foreground hover:text-foreground'
            }`}
          >
            {t === 'subagan' ? `Subastas SUBAGAN${eventos.length ? ` (${eventos.length})` : ''}` : 'Precios manuales'}
          </button>
        ))}
      </div>

      <div className="flex-1 overflow-y-auto p-6 space-y-4">

        {/* ── Tab SUBAGAN ── */}
        {tab === 'subagan' && (
          <>
            {/* Panel de importación */}
            <Card>
              <CardContent className="p-4">
                <p className="text-sm font-medium mb-3 flex items-center gap-2">
                  <Download className="w-4 h-4 text-primary" />
                  Importar subasta desde SUBAGAN
                </p>
                <p className="text-xs text-muted-foreground mb-3">
                  Ingresa el <strong>eventId</strong> visible en la URL de SUBAGAN cuando abres una subasta:
                  <code className="ml-1 px-1 bg-muted rounded text-xs">showLots?eventId=<strong>1208</strong></code>
                </p>
                <div className="flex gap-2 flex-wrap">
                  <input
                    type="number"
                    placeholder="EventId (ej: 1208)"
                    value={importEventId}
                    onChange={e => setImportEventId(e.target.value)}
                    className="flex h-9 rounded-md border border-input bg-transparent px-3 py-1 text-sm w-44"
                  />
                  <input
                    type="number"
                    placeholder="# Subasta (opcional)"
                    value={importNumero}
                    onChange={e => setImportNumero(e.target.value)}
                    className="flex h-9 rounded-md border border-input bg-transparent px-3 py-1 text-sm w-44"
                  />
                  <Button
                    onClick={handleImportar}
                    loading={importando}
                    disabled={!importEventId}
                    size="sm"
                  >
                    <RefreshCw className="w-3.5 h-3.5" />
                    Importar
                  </Button>
                </div>
                {importResult && (
                  <p className={`mt-2 text-xs ${importResult.startsWith('✓') ? 'text-emerald-400' : 'text-destructive'}`}>
                    {importResult}
                  </p>
                )}
              </CardContent>
            </Card>

            {/* Lista de eventos importados */}
            {loadingEventos ? (
              <div className="space-y-2">
                <Skeleton className="h-12 rounded-lg" />
                <Skeleton className="h-12 rounded-lg" />
              </div>
            ) : eventos.length === 0 ? (
              <EmptyState
                icon={<TrendingUp className="w-5 h-5" />}
                title="Sin subastas importadas"
                description="Importa tu primera subasta usando el panel de arriba."
              />
            ) : (
              <div className="space-y-2">
                {eventos.map(e => <SubaganEventoRow key={e.id} evento={e} onEliminar={setConfirmarEliminarEvento} />)}
              </div>
            )}
          </>
        )}

        {/* ── Tab manuales ── */}
        {tab === 'manuales' && (
          isLoading ? (
            <div className="space-y-3">
              <Skeleton className="h-12 rounded-lg" />
              <Skeleton className="h-12 rounded-lg" />
            </div>
          ) : arr.length === 0 ? (
            <EmptyState
              icon={<TrendingUp className="w-5 h-5" />}
              title="Sin precios manuales"
              description="Registra precios de referencia del mercado."
              action={<Button onClick={handleNuevo}><Plus className="w-4 h-4" />Nuevo precio</Button>}
            />
          ) : (
            <Card>
              <CardContent className="p-0">
                <div className="overflow-x-auto">
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="border-b border-border">
                        {['Fecha', 'Especie', 'Tipo', 'Precio/kg', 'Fuente', ''].map(h => (
                          <th key={h} className="text-left px-4 py-3 text-muted-foreground font-medium uppercase tracking-wide text-[10px] whitespace-nowrap last:text-right">
                            {h}
                          </th>
                        ))}
                      </tr>
                    </thead>
                    <tbody>
                      {arr.map((p, i) => (
                        <tr key={p.id} className={`border-b border-border/40 hover:bg-muted/20 transition-colors ${i === arr.length - 1 ? 'border-b-0' : ''}`}>
                          <td className="px-4 py-2.5 text-muted-foreground">{fmt.fecha(p.fecha)}</td>
                          <td className="px-4 py-2.5">{p.especie}</td>
                          <td className="px-4 py-2.5">{p.tipo}</td>
                          <td className="px-4 py-2.5 tabular-nums font-medium">${p.precioPorKg.toLocaleString('es-CO')}/kg</td>
                          <td className="px-4 py-2.5 text-muted-foreground">{p.fuente}</td>
                          <td className="px-4 py-2.5">
                            <div className="flex gap-1 justify-end">
                              <button onClick={() => handleEditar(p)} className="p-1.5 rounded-md text-muted-foreground hover:text-foreground hover:bg-secondary transition-colors" title="Editar">
                                <Pencil className="w-3.5 h-3.5" />
                              </button>
                              <button onClick={() => setConfirmarEliminar(p)} className="p-1.5 rounded-md text-muted-foreground hover:text-destructive hover:bg-destructive/10 transition-colors" title="Eliminar">
                                <Trash2 className="w-3.5 h-3.5" />
                              </button>
                            </div>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </CardContent>
            </Card>
          )
        )}
      </div>

      {/* Modal crear / editar precio manual */}
      <Dialog open={modalOpen} onClose={handleCerrar}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[420px] mx-4">
          <div className="flex items-center justify-between px-5 py-4 border-b border-border">
            <DialogHeader className="mb-0">
              <DialogTitle>{editando ? 'Editar precio' : 'Nuevo precio de mercado'}</DialogTitle>
            </DialogHeader>
            <button onClick={handleCerrar} className="text-muted-foreground hover:text-foreground"><X className="w-4 h-4" /></button>
          </div>
          <form onSubmit={form.handleSubmit(onSubmit)} className="p-5 space-y-4">
            <FormField label="Fecha" required>
              <input type="date" {...form.register('fecha')} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
            </FormField>
            <div className="grid grid-cols-2 gap-3">
              <FormField label="Especie" required>
                <CustomSelect value={form.watch('especie') ?? ''} onChange={v => form.setValue('especie', v as string, { shouldValidate: true })} options={especies.map(e => ({ value: e, label: e }))} />
              </FormField>
              <FormField label="Tipo" required>
                <CustomSelect value={form.watch('tipo') ?? ''} onChange={v => form.setValue('tipo', v as string, { shouldValidate: true })} options={tipos.map(t => ({ value: t, label: t }))} />
              </FormField>
            </div>
            <FormField label="Precio por kg" required>
              <MoneyInput min={0} {...form.register('precioPorKg')} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
            </FormField>
            <FormField label="Fuente" required>
              <input {...form.register('fuente')} placeholder="Ej: SUBAGAN, Carnicería Local" className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
            </FormField>
            <div className="flex gap-2 pt-1">
              <Button type="button" variant="outline" className="flex-1" onClick={handleCerrar}>Cancelar</Button>
              <Button type="submit" className="flex-1" loading={isPending}>{editando ? 'Guardar cambios' : 'Crear precio'}</Button>
            </div>
          </form>
        </div>
      </Dialog>

      {/* Confirmar eliminación */}
      <Dialog open={!!confirmarEliminar} onClose={() => setConfirmarEliminar(undefined)}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-sm mx-4 p-5">
          <DialogHeader className="mb-4">
            <DialogTitle>¿Eliminar precio?</DialogTitle>
            <DialogDescription>
              El precio de <span className="font-medium text-foreground">{confirmarEliminar?.tipo}</span> ({confirmarEliminar?.especie}) del {confirmarEliminar?.fecha ? fmt.fecha(confirmarEliminar.fecha) : ''} se eliminará permanentemente.
            </DialogDescription>
          </DialogHeader>
          <div className="flex gap-2">
            <Button variant="outline" className="flex-1" onClick={() => setConfirmarEliminar(undefined)}>Cancelar</Button>
            <Button variant="destructive" className="flex-1" loading={eliminarPrecio.isPending} onClick={() => confirmarEliminar && handleEliminar(confirmarEliminar)}>
              <Trash2 className="w-3.5 h-3.5" />Eliminar
            </Button>
          </div>
        </div>
      </Dialog>

      {/* Confirmar eliminación de evento SUBAGAN */}
      <Dialog open={!!confirmarEliminarEvento} onClose={() => setConfirmarEliminarEvento(undefined)}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-sm mx-4 p-5">
          <DialogHeader className="mb-4">
            <DialogTitle>¿Eliminar evento?</DialogTitle>
            <DialogDescription>
              El evento{' '}
              <span className="font-medium text-foreground">
                {confirmarEliminarEvento?.numeroSubasta
                  ? `Subasta #${confirmarEliminarEvento.numeroSubasta}`
                  : `Evento ${confirmarEliminarEvento?.subaganEventoId}`}
              </span>{' '}
              y sus {confirmarEliminarEvento?.totalLotes ?? 0} lotes se eliminarán permanentemente.
            </DialogDescription>
          </DialogHeader>
          <div className="flex gap-2">
            <Button variant="outline" className="flex-1" onClick={() => setConfirmarEliminarEvento(undefined)}>Cancelar</Button>
            <Button variant="destructive" className="flex-1" loading={eliminarEvento.isPending} onClick={() => confirmarEliminarEvento && handleEliminarEvento(confirmarEliminarEvento)}>
              <Trash2 className="w-3.5 h-3.5" />Eliminar
            </Button>
          </div>
        </div>
      </Dialog>
    </div>
  )
}
