import { Settings } from 'lucide-react'
import { useModulos, useCambiarEstadoModulo } from '@/hooks/useFeedlot'
import { PageHeader, Card, CardContent, Skeleton } from '@/components/ui'
import type { ModuloSistema } from '@/types'

function ToggleModulo({ modulo }: { modulo: ModuloSistema }) {
  const cambiar = useCambiarEstadoModulo()

  const onToggle = () => {
    cambiar.mutate({ clave: modulo.clave, activo: !modulo.activo })
  }

  return (
    <div className="flex items-center justify-between px-4 py-3 border-b border-border/40 last:border-b-0">
      <div className="min-w-0">
        <p className="text-sm font-medium">{modulo.nombre}</p>
        <p className="text-xs text-muted-foreground font-mono">{modulo.clave}</p>
      </div>
      <button
        type="button"
        role="switch"
        aria-checked={modulo.activo}
        onClick={onToggle}
        disabled={cambiar.isPending}
        className={`relative inline-flex h-6 w-11 shrink-0 items-center rounded-full transition-colors disabled:opacity-50 ${
          modulo.activo ? 'bg-emerald-500' : 'bg-muted'
        }`}
        title={modulo.activo ? 'Desactivar' : 'Activar'}
      >
        <span
          className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
            modulo.activo ? 'translate-x-6' : 'translate-x-1'
          }`}
        />
      </button>
    </div>
  )
}

export default function ConfiguracionPage() {
  const { data: modulos, isLoading } = useModulos()

  return (
    <div className="flex flex-col h-full animate-fade-in">
      <PageHeader
        title="Configuración"
        description="Activa o desactiva los módulos del sistema. Los desactivados se ocultan del menú para todos los usuarios."
      />
      <div className="flex-1 overflow-y-auto p-6">
        <Card className="max-w-2xl">
          <CardContent className="p-0">
            <div className="px-4 py-3 border-b border-border flex items-center gap-2">
              <Settings className="w-4 h-4 text-primary" />
              <span className="text-sm font-medium">Módulos del sistema</span>
            </div>
            {isLoading ? (
              <div className="p-4 space-y-2">
                <Skeleton className="h-10 rounded" />
                <Skeleton className="h-10 rounded" />
                <Skeleton className="h-10 rounded" />
              </div>
            ) : (
              (modulos ?? []).map(m => <ToggleModulo key={m.id} modulo={m} />)
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
