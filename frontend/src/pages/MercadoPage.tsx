import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Plus, X, TrendingUp, Pencil, Trash2 } from 'lucide-react'
import { usePreciosMercado, useCrearPrecioMercado, useActualizarPrecioMercado, useEliminarPrecioMercado } from '@/hooks/useFeedlot'
import {
  PageHeader, Skeleton, EmptyState, Button, Card, CardContent,
  Dialog, DialogHeader, DialogTitle, DialogDescription,
  FormField,
  MoneyInput, CustomSelect,
} from '@/components/ui'
import { fmt } from '@/utils'
import type { PrecioMercado } from '@/types'

const especies = ['Bovino', 'Porcino']
const tipos = ['Novillo', 'Ternero', 'Vaca', 'Cerdo Preceba', 'Cerdo Ceba', 'Lechón']

type PrecioForm = {
  fecha: string
  especie: string
  tipo: string
  precioPorKg: string
  fuente: string
}

const defaultValues: PrecioForm = {
  fecha: new Date().toISOString().split('T')[0],
  especie: 'Bovino',
  tipo: 'Novillo',
  precioPorKg: '',
  fuente: '',
}

export default function MercadoPage() {
  const { data: precios, isLoading } = usePreciosMercado()
  const arr = (precios as PrecioMercado[] | undefined) ?? []

  const crearPrecio = useCrearPrecioMercado()
  const actualizarPrecio = useActualizarPrecioMercado()
  const eliminarPrecio = useEliminarPrecioMercado()

  const [modalOpen, setModalOpen] = useState(false)
  const [editando, setEditando] = useState<PrecioMercado | undefined>(undefined)
  const [confirmarEliminar, setConfirmarEliminar] = useState<PrecioMercado | undefined>(undefined)

  const form = useForm<PrecioForm>({ defaultValues })

  const handleNuevo = () => {
    setEditando(undefined)
    form.reset(defaultValues)
    setModalOpen(true)
  }

  const handleEditar = (p: PrecioMercado) => {
    setEditando(p)
    form.reset({
      fecha: p.fecha,
      especie: p.especie,
      tipo: p.tipo,
      precioPorKg: String(p.precioPorKg),
      fuente: p.fuente,
    })
    setModalOpen(true)
  }

  const handleCerrar = () => {
    setModalOpen(false)
    setEditando(undefined)
    form.reset(defaultValues)
  }

  const onSubmit = async (data: PrecioForm) => {
    const precioPorKg = parseFloat(data.precioPorKg.replace(/[^0-9]/g, '')) || 0
    if (editando) {
      await actualizarPrecio.mutateAsync({ id: editando.id, ...data, precioPorKg })
    } else {
      await crearPrecio.mutateAsync({ ...data, precioPorKg })
    }
    handleCerrar()
  }

  const handleEliminar = async (p: PrecioMercado) => {
    try {
      await eliminarPrecio.mutateAsync(p.id)
    } catch { }
    setConfirmarEliminar(undefined)
  }

  const isPending = crearPrecio.isPending || actualizarPrecio.isPending

  return (
    <div className="flex flex-col h-full animate-fade-in">
      <PageHeader
        title="Precios de Mercado"
        description="Referencias de precio por kg según canal y especie"
        action={
          arr.length > 0 ? (
            <Button size="sm" onClick={handleNuevo}>
              <Plus className="w-3.5 h-3.5" />Nuevo precio
            </Button>
          ) : undefined
        }
      />

      <div className="flex-1 overflow-y-auto p-6 space-y-4">
        {isLoading ? (
          <div className="space-y-3">
            <Skeleton className="h-12 rounded-lg" />
            <Skeleton className="h-12 rounded-lg" />
            <Skeleton className="h-12 rounded-lg" />
          </div>
        ) : arr.length === 0 ? (
          <EmptyState
            icon={<TrendingUp className="w-5 h-5" />}
            title="Sin precios"
            description="Registra precios de referencia del mercado."
            action={
              <Button onClick={handleNuevo}>
                <Plus className="w-4 h-4" />Nuevo precio
              </Button>
            }
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
                      <tr key={p.id}
                        className={`border-b border-border/40 hover:bg-muted/20 transition-colors ${i === arr.length - 1 ? 'border-b-0' : ''}`}
                      >
                        <td className="px-4 py-2.5 text-muted-foreground">{fmt.fecha(p.fecha)}</td>
                        <td className="px-4 py-2.5">{p.especie}</td>
                        <td className="px-4 py-2.5">{p.tipo}</td>
                        <td className="px-4 py-2.5 tabular-nums font-medium">${p.precioPorKg.toLocaleString('es-CO')}/kg</td>
                        <td className="px-4 py-2.5 text-muted-foreground">{p.fuente}</td>
                        <td className="px-4 py-2.5">
                          <div className="flex gap-1 justify-end">
                            <button onClick={() => handleEditar(p)}
                              className="p-1.5 rounded-md text-muted-foreground hover:text-foreground hover:bg-secondary transition-colors"
                              title="Editar">
                              <Pencil className="w-3.5 h-3.5" />
                            </button>
                            <button onClick={() => setConfirmarEliminar(p)}
                              className="p-1.5 rounded-md text-muted-foreground hover:text-destructive hover:bg-destructive/10 transition-colors"
                              title="Eliminar">
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
        )}
      </div>

      {/* Modal crear / editar */}
      <Dialog open={modalOpen} onClose={handleCerrar}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[420px] mx-4">
          <div className="flex items-center justify-between px-5 py-4 border-b border-border">
            <DialogHeader className="mb-0">
              <DialogTitle>{editando ? 'Editar precio' : 'Nuevo precio de mercado'}</DialogTitle>
            </DialogHeader>
            <button onClick={handleCerrar} className="text-muted-foreground hover:text-foreground">
              <X className="w-4 h-4" />
            </button>
          </div>
          <form onSubmit={form.handleSubmit(onSubmit)} className="p-5 space-y-4">
            <FormField label="Fecha" required>
              <input type="date" {...form.register('fecha')}
                className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
            </FormField>
            <div className="grid grid-cols-2 gap-3">
              <FormField label="Especie" required>
                <CustomSelect
                  value={form.watch('especie') ?? ''}
                  onChange={v => form.setValue('especie', v as string, { shouldValidate: true })}
                  options={especies.map(e => ({ value: e, label: e }))}
                />
              </FormField>
              <FormField label="Tipo" required>
                <CustomSelect
                  value={form.watch('tipo') ?? ''}
                  onChange={v => form.setValue('tipo', v as string, { shouldValidate: true })}
                  options={tipos.map(t => ({ value: t, label: t }))}
                />
              </FormField>
            </div>
            <FormField label="Precio por kg" required>
              <MoneyInput min={0} {...form.register('precioPorKg')}
                className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
            </FormField>
            <FormField label="Fuente" required>
              <input {...form.register('fuente')} placeholder="Ej: SUBAGAN, Carnicería Local"
                className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
            </FormField>
            <div className="flex gap-2 pt-1">
              <Button type="button" variant="outline" className="flex-1" onClick={handleCerrar}>
                Cancelar
              </Button>
              <Button type="submit" className="flex-1" loading={isPending}>
                {editando ? 'Guardar cambios' : 'Crear precio'}
              </Button>
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
            <Button variant="outline" className="flex-1" onClick={() => setConfirmarEliminar(undefined)}>
              Cancelar
            </Button>
            <Button variant="destructive" className="flex-1" loading={eliminarPrecio.isPending}
              onClick={() => confirmarEliminar && handleEliminar(confirmarEliminar)}>
              <Trash2 className="w-3.5 h-3.5" />
              Eliminar
            </Button>
          </div>
        </div>
      </Dialog>
    </div>
  )
}
