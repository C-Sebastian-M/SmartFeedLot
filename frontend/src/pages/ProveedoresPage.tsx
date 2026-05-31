import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Plus, X, Pencil, Trash2, Building2, CheckCircle2 } from 'lucide-react'
import { useProveedores, useCrearProveedor, useModificarProveedor, useEliminarProveedor } from '@/hooks/useFeedlot'
import {
  PageHeader, Card, CardContent,
  Skeleton, EmptyState, Button,
  Dialog, DialogHeader, DialogTitle, DialogDescription,
  FormField, Input, Alert,
} from '@/components/ui'
import type { Proveedor } from '@/types'

const proveedorSchema = z.object({
  nombre: z.string().min(1, 'El nombre es requerido').max(200, 'Máximo 200 caracteres'),
  contacto: z.string().max(200, 'Máximo 200 caracteres').optional().or(z.literal('')),
  telefono: z.string().max(50, 'Máximo 50 caracteres').optional().or(z.literal('')),
  email: z.string().email('Email inválido').max(200, 'Máximo 200 caracteres').optional().or(z.literal('')),
})
type ProveedorForm = z.infer<typeof proveedorSchema>

function ProveedorModal({
  open, onClose, proveedor,
}: {
  open: boolean
  onClose: () => void
  proveedor?: Proveedor
}) {
  const [exito, setExito] = useState(false)
  const [errorApi, setErrorApi] = useState<string>()
  const crear = useCrearProveedor()
  const modificar = useModificarProveedor()
  const esEdicion = !!proveedor

  const { register, handleSubmit, reset, formState: { errors, isSubmitting } } =
    useForm<ProveedorForm>({
      resolver: zodResolver(proveedorSchema),
      defaultValues: proveedor ?? { nombre: '', contacto: '', telefono: '', email: '' },
    })

  const handleClose = () => { reset(); setExito(false); setErrorApi(undefined); onClose() }

  const onSubmit = async (data: ProveedorForm) => {
    setErrorApi(undefined)
    try {
      const payload = {
        nombre: data.nombre,
        contacto: data.contacto || undefined,
        telefono: data.telefono || undefined,
        email: data.email || undefined,
      }
      if (esEdicion && proveedor) {
        await modificar.mutateAsync({ id: proveedor.id, ...payload })
      } else {
        await crear.mutateAsync(payload)
      }
      setExito(true)
      setTimeout(() => handleClose(), 1500)
    } catch (err: any) {
      setErrorApi(err?.response?.data?.error ?? err?.response?.data?.detail ?? 'Error al guardar.')
    }
  }

  return (
    <Dialog open={open} onClose={handleClose}>
      <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[480px] mx-4">
        <div className="flex items-center justify-between px-5 py-4 border-b border-border">
          <DialogHeader className="mb-0">
            <DialogTitle>{esEdicion ? 'Modificar proveedor' : 'Nuevo proveedor'}</DialogTitle>
            <DialogDescription>{esEdicion ? proveedor!.nombre : 'Registra un nuevo proveedor en el sistema'}</DialogDescription>
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
              <p className="text-sm font-medium">¡Proveedor {esEdicion ? 'actualizado' : 'registrado'}!</p>
            </div>
          ) : (
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              <FormField label="Nombre" error={errors.nombre?.message} required>
                <Input {...register('nombre')} placeholder="Nombre del proveedor"
                  className={errors.nombre ? 'border-destructive' : ''} />
              </FormField>

              <FormField label="Contacto" error={errors.contacto?.message}>
                <Input {...register('contacto')} placeholder="Persona de contacto"
                  className={errors.contacto ? 'border-destructive' : ''} />
              </FormField>

              <div className="grid grid-cols-2 gap-3">
                <FormField label="Teléfono" error={errors.telefono?.message}>
                  <Input {...register('telefono')} placeholder="Teléfono"
                    className={errors.telefono ? 'border-destructive' : ''} />
                </FormField>
                <FormField label="Email" error={errors.email?.message}>
                  <Input {...register('email')} placeholder="correo@ejemplo.com"
                    className={errors.email ? 'border-destructive' : ''} />
                </FormField>
              </div>

              {errorApi && <Alert variant="destructive">{errorApi}</Alert>}

              <div className="flex gap-2 pt-1">
                <Button type="button" variant="outline" className="flex-1" onClick={handleClose} disabled={isSubmitting}>
                  Cancelar
                </Button>
                <Button type="submit" className="flex-1" loading={isSubmitting}>
                  {esEdicion ? 'Guardar cambios' : 'Registrar proveedor'}
                </Button>
              </div>
            </form>
          )}
        </div>
      </div>
    </Dialog>
  )
}

export default function ProveedoresPage() {
  const [modalAbierto, setModalAbierto] = useState(false)
  const [editando, setEditando] = useState<Proveedor | undefined>(undefined)
  const [confirmarEliminar, setConfirmarEliminar] = useState<Proveedor | undefined>(undefined)
  const eliminarProveedor = useEliminarProveedor()

  const { data: proveedores, isLoading } = useProveedores()
  const proveedoresArray = (proveedores as Proveedor[] | undefined) ?? []

  const handleEditar = (p: Proveedor) => {
    setEditando(p)
    setModalAbierto(true)
  }

  const handleNuevo = () => {
    setEditando(undefined)
    setModalAbierto(true)
  }

  const handleEliminar = async (p: Proveedor) => {
    try {
      await eliminarProveedor.mutateAsync(p.id)
    } catch { }
    setConfirmarEliminar(undefined)
  }

  return (
    <div className="flex flex-col h-full animate-fade-in">
      <PageHeader
        title="Proveedores"
        description="Gestiona los proveedores de ganado e insumos"
        action={
          <Button size="sm" onClick={handleNuevo}>
            <Plus className="w-3.5 h-3.5" />
            Nuevo proveedor
          </Button>
        }
      />

      <div className="flex-1 overflow-y-auto p-6">
        {isLoading ? (
          <div className="space-y-3">
            {Array.from({ length: 4 }).map((_, i) => <Skeleton key={i} className="h-16 rounded-lg" />)}
          </div>
        ) : proveedoresArray.length === 0 ? (
          <EmptyState
            icon={<Building2 className="w-5 h-5" />}
            title="Sin proveedores"
            description="Aún no hay proveedores registrados."
            action={
              <Button size="sm" onClick={handleNuevo}>
                <Plus className="w-3.5 h-3.5" />
                Registrar proveedor
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
                      {['Nombre', 'Contacto', 'Teléfono', 'Email', ''].map(h => (
                        <th key={h} className="text-left px-4 py-3 text-muted-foreground font-medium uppercase tracking-wide text-[10px] whitespace-nowrap">
                          {h}
                        </th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {proveedoresArray.map((p, i) => (
                      <tr key={p.id}
                        className={`border-b border-border/40 hover:bg-muted/20 transition-colors ${i === proveedoresArray.length - 1 ? 'border-b-0' : ''}`}
                      >
                        <td className="px-4 py-2.5 font-medium">{p.nombre}</td>
                        <td className="px-4 py-2.5 text-muted-foreground">{p.contacto ?? '—'}</td>
                        <td className="px-4 py-2.5 text-muted-foreground">{p.telefono ?? '—'}</td>
                        <td className="px-4 py-2.5 text-muted-foreground">{p.email ?? '—'}</td>
                        <td className="px-4 py-3">
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

      <ProveedorModal
        open={modalAbierto}
        onClose={() => { setModalAbierto(false); setEditando(undefined) }}
        proveedor={editando}
      />

      {/* Confirmar eliminación */}
      <Dialog open={!!confirmarEliminar} onClose={() => setConfirmarEliminar(undefined)}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-sm mx-4 p-5">
          <DialogHeader className="mb-4">
            <DialogTitle>¿Eliminar proveedor?</DialogTitle>
            <DialogDescription>
              {confirmarEliminar?.nombre} se eliminará permanentemente. Las compras asociadas se conservarán.
            </DialogDescription>
          </DialogHeader>
          <div className="flex gap-2">
            <Button variant="outline" className="flex-1" onClick={() => setConfirmarEliminar(undefined)}>
              Cancelar
            </Button>
            <Button variant="destructive" className="flex-1" loading={eliminarProveedor.isPending}
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
