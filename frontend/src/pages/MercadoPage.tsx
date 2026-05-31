import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Plus, X, TrendingUp } from 'lucide-react'
import { usePreciosMercado, useCrearPrecioMercado } from '@/hooks/useFeedlot'
import {
  PageHeader, Skeleton, EmptyState, Button,
  Dialog, DialogHeader, DialogTitle,
  FormField,
  MoneyInput,
} from '@/components/ui'
import { fmt } from '@/utils'
import type { PrecioMercado } from '@/types'

const especies = ['Bovino', 'Porcino']
const tipos = ['Novillo', 'Ternero', 'Vaca', 'Cerdo Preceba', 'Cerdo Ceba', 'Lechón']

export default function MercadoPage() {
  const { data: precios, isLoading } = usePreciosMercado()
  const arr = (precios as PrecioMercado[] | undefined) ?? []
  const crearPrecio = useCrearPrecioMercado()
  const [modalOpen, setModalOpen] = useState(false)

  const form = useForm<{ fecha: string; especie: string; tipo: string; precioPorKg: number; fuente: string }>({
    defaultValues: { fecha: new Date().toISOString().split('T')[0], especie: 'Bovino', tipo: 'Novillo', precioPorKg: 0, fuente: '' },
  })

  const onSubmit = async (data: { fecha: string; especie: string; tipo: string; precioPorKg: number; fuente: string }) => {
    await crearPrecio.mutateAsync(data)
    setModalOpen(false)
    form.reset()
  }

  return (
    <div className="flex flex-col h-full animate-fade-in">
      <PageHeader
        title="Precios de Mercado"
        description="Referencias de precio por kg según canal y especie"
      />

      <div className="flex-1 overflow-y-auto p-6 space-y-4">
        <div className="flex items-center justify-between">
          <p className="text-sm text-muted-foreground">{arr.length} registros</p>
          <Button size="sm" variant="outline" onClick={() => setModalOpen(true)}>
            <Plus className="w-3 h-3" />Nuevo precio
          </Button>
        </div>

        {isLoading ? (
          <div className="space-y-3">
            <Skeleton className="h-12 rounded-lg" />
            <Skeleton className="h-12 rounded-lg" />
            <Skeleton className="h-12 rounded-lg" />
          </div>
        ) : arr.length === 0 ? (
          <EmptyState icon={<TrendingUp className="w-5 h-5" />} title="Sin precios" description="Registra precios de referencia del mercado." />
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-border">
                  <th className="text-left px-4 py-3 text-muted-foreground font-medium">Fecha</th>
                  <th className="text-left px-4 py-3 text-muted-foreground font-medium">Especie</th>
                  <th className="text-left px-4 py-3 text-muted-foreground font-medium">Tipo</th>
                  <th className="text-right px-4 py-3 text-muted-foreground font-medium">Precio/kg</th>
                  <th className="text-left px-4 py-3 text-muted-foreground font-medium">Fuente</th>
                </tr>
              </thead>
              <tbody>
                {arr.map(p => (
                  <tr key={p.id} className="border-b border-border/30 hover:bg-secondary/20 transition-colors">
                    <td className="px-4 py-3">{fmt.fecha(p.fecha)}</td>
                    <td className="px-4 py-3">{p.especie}</td>
                    <td className="px-4 py-3">{p.tipo}</td>
                    <td className="px-4 py-3 text-right tabular-nums font-medium">${p.precioPorKg.toLocaleString('es-CO')}/kg</td>
                    <td className="px-4 py-3">{p.fuente}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      <Dialog open={modalOpen} onClose={() => setModalOpen(false)}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[420px] mx-4">
          <div className="flex items-center justify-between px-5 py-4 border-b border-border">
            <DialogHeader className="mb-0"><DialogTitle>Nuevo precio de mercado</DialogTitle></DialogHeader>
            <button onClick={() => setModalOpen(false)} className="text-muted-foreground hover:text-foreground"><X className="w-4 h-4" /></button>
          </div>
          <form onSubmit={form.handleSubmit(onSubmit)} className="p-5 space-y-4">
            <FormField label="Fecha" required>
              <input type="date" {...form.register('fecha')} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
            </FormField>
            <div className="grid grid-cols-2 gap-3">
              <FormField label="Especie" required>
                <select {...form.register('especie')} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm">
                  {especies.map(e => <option key={e} value={e}>{e}</option>)}
                </select>
              </FormField>
              <FormField label="Tipo" required>
                <select {...form.register('tipo')} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm">
                  {tipos.map(t => <option key={t} value={t}>{t}</option>)}
                </select>
              </FormField>
            </div>
            <FormField label="Precio por kg" required>
              <MoneyInput min={0} {...form.register('precioPorKg', { })} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
            </FormField>
            <FormField label="Fuente" required>
              <input {...form.register('fuente')} placeholder="Ej: SUBAGAN, Carnicería Local" className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" />
            </FormField>
            <Button type="submit" className="w-full" loading={crearPrecio.isPending}>Crear precio</Button>
          </form>
        </div>
      </Dialog>
    </div>
  )
}
