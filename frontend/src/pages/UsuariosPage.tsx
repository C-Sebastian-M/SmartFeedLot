import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Plus, X, Users as UsersIcon, Check, Ban } from 'lucide-react'
import {
  useUsuarios, useCrearUsuario, useCambiarEstadoUsuario, useCambiarRolUsuario,
} from '@/hooks/useFeedlot'
import {
  PageHeader, Card, CardContent, Button, Skeleton, EmptyState,
  Dialog, DialogHeader, DialogTitle, FormField, Input, CustomSelect, Badge,
} from '@/components/ui'
import { fmt } from '@/utils'
import type { Usuario } from '@/types'

const roles = [
  { value: 'Admin', label: 'Admin' },
  { value: 'Supervisor', label: 'Supervisor' },
  { value: 'Operador', label: 'Operador' },
]

type CrearForm = { email: string; nombreCompleto: string; password: string; rol: string }

export default function UsuariosPage() {
  const { data: usuarios, isLoading } = useUsuarios()
  const crear = useCrearUsuario()
  const cambiarEstado = useCambiarEstadoUsuario()
  const cambiarRol = useCambiarRolUsuario()

  const [modalOpen, setModalOpen] = useState(false)
  const [errorApi, setErrorApi] = useState<string>()
  const form = useForm<CrearForm>({ defaultValues: { email: '', nombreCompleto: '', password: '', rol: 'Operador' } })

  const onSubmit = async (data: CrearForm) => {
    setErrorApi(undefined)
    try {
      await crear.mutateAsync(data)
      form.reset()
      setModalOpen(false)
    } catch (e: any) {
      setErrorApi(e?.response?.data?.error ?? 'No se pudo crear el usuario.')
    }
  }

  const arr = (usuarios as Usuario[] | undefined) ?? []

  return (
    <div className="flex flex-col h-full animate-fade-in">
      <PageHeader
        title="Usuarios"
        description="Crea usuarios, asígnales rol y actívalos o desactívalos."
        action={
          arr.length > 0 ? (
            <Button size="sm" onClick={() => setModalOpen(true)}>
              <Plus className="w-3.5 h-3.5" />Nuevo usuario
            </Button>
          ) : undefined
        }
      />
      <div className="flex-1 overflow-y-auto p-6">
        {isLoading ? (
          <div className="space-y-2">
            <Skeleton className="h-12 rounded-lg" />
            <Skeleton className="h-12 rounded-lg" />
          </div>
        ) : arr.length === 0 ? (
          <EmptyState
            icon={<UsersIcon className="w-5 h-5" />}
            title="Sin usuarios"
            description="Crea el primer usuario del sistema."
            action={<Button onClick={() => setModalOpen(true)}><Plus className="w-4 h-4" />Nuevo usuario</Button>}
          />
        ) : (
          <Card>
            <CardContent className="p-0 overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-border">
                    {['Email', 'Nombre', 'Rol', 'Estado', 'Último acceso', ''].map(h => (
                      <th key={h} className="text-left px-4 py-3 text-muted-foreground font-medium uppercase tracking-wide text-[10px] whitespace-nowrap last:text-right">{h}</th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {arr.map((u, i) => (
                    <tr key={u.id} className={`border-b border-border/40 hover:bg-muted/20 transition-colors ${i === arr.length - 1 ? 'border-b-0' : ''}`}>
                      <td className="px-4 py-2.5 font-medium">{u.email}</td>
                      <td className="px-4 py-2.5 text-muted-foreground">{u.nombreCompleto}</td>
                      <td className="px-4 py-2.5 w-40">
                        <CustomSelect
                          value={u.roles[0] ?? 'Operador'}
                          onChange={v => cambiarRol.mutate({ id: u.id, rol: v })}
                          options={roles}
                        />
                      </td>
                      <td className="px-4 py-2.5">
                        {u.activo
                          ? <Badge className="bg-emerald-500/10 text-emerald-400 border-emerald-500/20">Activo</Badge>
                          : <Badge className="bg-zinc-500/10 text-zinc-400 border-zinc-500/20">Inactivo</Badge>}
                      </td>
                      <td className="px-4 py-2.5 text-muted-foreground text-xs">
                        {u.ultimoAcceso ? fmt.fecha(u.ultimoAcceso) : 'Nunca'}
                      </td>
                      <td className="px-4 py-2.5 text-right">
                        <Button
                          size="sm"
                          variant={u.activo ? 'outline' : 'default'}
                          onClick={() => cambiarEstado.mutate({ id: u.id, activo: !u.activo })}
                          loading={cambiarEstado.isPending}
                        >
                          {u.activo ? <><Ban className="w-3.5 h-3.5" />Desactivar</> : <><Check className="w-3.5 h-3.5" />Activar</>}
                        </Button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </CardContent>
          </Card>
        )}
      </div>

      {/* Modal crear usuario */}
      <Dialog open={modalOpen} onClose={() => { setModalOpen(false); form.reset() }}>
        <div className="rounded-xl border border-border bg-card shadow-xl w-full max-w-[420px] mx-4">
          <div className="flex items-center justify-between px-5 py-4 border-b border-border">
            <DialogHeader className="mb-0"><DialogTitle>Nuevo usuario</DialogTitle></DialogHeader>
            <button onClick={() => { setModalOpen(false); form.reset() }} className="text-muted-foreground hover:text-foreground"><X className="w-4 h-4" /></button>
          </div>
          <form onSubmit={form.handleSubmit(onSubmit)} className="p-5 space-y-4">
            {errorApi && <p className="text-xs text-destructive">{errorApi}</p>}
            <FormField label="Email" required>
              <Input type="email" {...form.register('email', { required: true })} placeholder="usuario@finca.com" />
            </FormField>
            <FormField label="Nombre completo" required>
              <Input {...form.register('nombreCompleto', { required: true })} placeholder="Juan Pérez" />
            </FormField>
            <FormField label="Contraseña" required hint="Mínimo 8 caracteres, con mayúscula y número">
              <Input type="password" {...form.register('password', { required: true })} placeholder="••••••••" />
            </FormField>
            <FormField label="Rol" required>
              <CustomSelect
                value={form.watch('rol')}
                onChange={v => form.setValue('rol', v)}
                options={roles}
              />
            </FormField>
            <div className="flex gap-2 pt-1">
              <Button type="button" variant="outline" className="flex-1" onClick={() => { setModalOpen(false); form.reset() }}>Cancelar</Button>
              <Button type="submit" className="flex-1" loading={crear.isPending}>Crear usuario</Button>
            </div>
          </form>
        </div>
      </Dialog>
    </div>
  )
}
