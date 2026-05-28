import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Plus, X, Pencil, Trash2, Users, CheckCircle2 } from 'lucide-react'
import { useCompradores } from '@/hooks/useFeedlot'
import api from '@/services/api'
import {
  PageHeader, Card, CardContent,
  Skeleton, EmptyState, Button,
  Dialog, DialogHeader, DialogTitle, DialogDescription,
  FormField, Input, Alert,
} from '@/components/ui'
import type { Comprador } from '@/types'

const compradorSchema = z.object({
  nombre: z.string().min(1, 'El nombre es requerido').max(200, 'Máximo 200 caracteres'),
  contacto: z.string().max(200, 'Máximo 200 caracteres').optional().or(z.literal('')),
  telefono: z.string().max(50, 'Máximo 50 caracteres').optional().or(z.literal('')),
  email: z.string().email('Email inválido').max(200, 'Máximo 200 caracteres').optional().or(z.literal('')),
})
type CompradorForm = z.infer<typeof compradorSchema>

function CompradorModal({
  open, onClose, comprador, onSuccess,
}: {
  open: boolean
  onClose: () => void
  comprador?: Comprador
  onSuccess: () => void
}) {
  const [exito, setExito] = useState(false)
  const [errorApi, setErrorApi] = useState<string>()
  const esEdicion = !!comprador

  const { register, handleSubmit, reset, formState: { errors, isSubmitting } } =
    useForm<CompradorForm>({
      resolver: zodResolver(compradorSchema),
      defaultValues: comprador ?? { nombre: '', contacto: '', telefono: '', email: '' },
    })

  const handleClose = () => { reset(); setExito(false); setErrorApi(undefined); onClose() }

  const onSubmit = async (data: CompradorForm) => {
    setErrorApi(undefined)
    try {
      const payload = {
        nombre: data.nombre,
        contacto: data.contacto || undefined,
        telefono: data.telefono || undefined,
        email: data.email || undefined,
      }
      if (esEdicion && comprador) {
        await api.put(`/compradores/${comprador.id}`, { id: comprador.id, ...payload })
      } else {
        await api.post('/compradores', payload)
      }
      setExito(true)
      onSuccess()
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
            <DialogTitle>{esEdicion ? 'Modificar comprador' : 'Nuevo comprador'}</DialogTitle>
            <DialogDescription>{esEdicion ? comprador!.nombre : 'Registra un nuevo comprador'}</DialogDescription>
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
              <p className="text-sm font-medium">¡Comprador {esEdicion ? 'actualizado' : 'registrado'}!</p>
            </div>
          ) : (
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              <FormField label="Nombre" error={errors.nombre?.message} required>
                <Input {...register('nombre')} placeholder="Nombre del comprador"
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
                  {esEdicion ? 'Guardar cambios' : 'Registrar comprador'}
                </Button>
              </div>
            </form>
          )}
        </div>
      </div>
    </Dialog>
  )
}

export default function CompradoresPage() {
  const [modalAbierto, setModalAbierto] = useState(false)
  const [editando, setEditando] = useState<Comprador | undefined>(undefined)
  const [confirmarEliminar, setConfirmarEliminar] = useState<Comprador | undefined>(undefined)
  const [eliminando, setEliminando] = useState(false)

  const { data: compradores, isLoading, refetch } = useCompradores()
  const compradoresArray = (compradores as Comprador[] | undefined) ?? []

  const handleEditar = (c: Comprador) => {
    setEditando(c)
    setModalAbierto(true)
  }

  const handleNuevo = () => {
    setEditando(undefined)
    setModalAbierto(true)
  }

  const handleEliminar = async (c: Comprador) => {
    setEliminando(true)
    try {
      await api.delete(`/compradores/${c.id}`)
      refetch()
    } catch { }
    setConfirmarEliminar(undefined)
    setEliminando(false)
  }

  return (
    <div className="flex flex-col h-full animate-fade-in">
      <PageHeader
        title="Compradores"
        description="Gestiona los compradores de animales"
        action={
          <Button size="sm" onClick={handleNuevo}>
            <Plus className="w-3.5 h-3.5" />
            Nuevo comprador
          </Button>
        }
      />

      <div className="flex-1 overflow-y-auto p-6">
        {isLoading ? (
          <div className="space-y-3">
            {Array.from({ length: 4 }).map((_, i) => <Skeleton key={i} className="h-16 rounded-lg" />)}
          </div>
        ) : compradoresArray.length === 0 ? (
          <EmptyState
            icon={<Users className="w-5 h-5" />}
            title="Sin compradores"
            description="Aún no hay compradores registrados."
            action={
              <Button size="sm" onClick={handleNuevo}>
                <Plus className="w-3.5 h-3.5" />
                Registrar comprador
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
                    {compradoresArray.map((c, i) => (
                      <tr key={c.id}
                        className={`border-b border-border/40 hover:bg-secondary/30 transition-colors ${i === compradoresArray.length - 1 ? 'border-b-0' : ''}`}
                      >
                        <td className="px-4 py-3 font-medium">{c.nombre}</td>
                        <td className="px-4 py-3 text-muted-foreground">{c.contacto ?? '—'}</td>
                        <td className="px-4 py-3 text-muted-foreground">{c.telefono ?? '—'}</td>
                        <td className="px-4 py-3 text-muted-foreground">{c.email ?? '—'}</td>
                        <td className="px-4 py-3">
                          <div className="flex gap-1 justify-end">
                            <button onClick={() => handleEditar(c)}
                              className="p-1.5 rounded-md text-muted-foreground hover:text-foreground hover:bg-secondary transition-colors"
                              title="Editar">
                              <Pencil className="w-3.5 h-3.5" />
                            </button>
                            <button onClick={() => setConfirmarEliminar(c)}
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

      <CompradorModal
        open={modalAbierto}
        onClose={() => { setModalAbierto(false); setEditando(undefined) }}
        comprador={editando}
        onSuccess={() => refetch()}
      />

      <Dialog open={!!confirmarEliminar} onClose={() => setConfirmarEliminar(undefined)}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-sm mx-4 p-5">
          <DialogHeader className="mb-4">
            <DialogTitle>¿Eliminar comprador?</DialogTitle>
            <DialogDescription>
              {confirmarEliminar?.nombre} se eliminará permanentemente. Las ventas asociadas se conservarán.
            </DialogDescription>
          </DialogHeader>
          <div className="flex gap-2">
            <Button variant="outline" className="flex-1" onClick={() => setConfirmarEliminar(undefined)}>
              Cancelar
            </Button>
            <Button variant="destructive" className="flex-1" loading={eliminando}
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
