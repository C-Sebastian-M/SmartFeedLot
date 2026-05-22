import { Plus, Package } from 'lucide-react'
import { useLotes } from '@/hooks/useFeedlot'
import { PageHeader, Button, Card, CardContent, Badge, Skeleton, EmptyState } from '@/components/ui'
import { fmt } from '@/utils'
import type { LoteResumen } from '@/types'

export default function LotesPage() {
  const { data: lotes, isLoading } = useLotes()

  return (
    <div className="flex flex-col h-full animate-fade-in">
      <PageHeader
        title="Lotes"
        description="Gestión de lotes de engorde"
        action={
          <Button size="sm">
            <Plus className="w-3.5 h-3.5" />
            Crear lote
          </Button>
        }
      />

      <div className="flex-1 overflow-y-auto p-6">
        {isLoading ? (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {Array.from({ length: 6 }).map((_, i) => (
              <Skeleton key={i} className="h-36 rounded-lg" />
            ))}
          </div>
        ) : !lotes?.length ? (
          <EmptyState
            icon={<Package className="w-5 h-5" />}
            title="Sin lotes"
            description="Crea el primer lote para comenzar a organizar los animales."
            action={<Button size="sm"><Plus className="w-3.5 h-3.5" />Crear lote</Button>}
          />
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {(lotes as LoteResumen[]).map((lote) => (
              <Card key={lote.id} className="p-5 hover:border-border/80 transition-all cursor-pointer">
                <div className="flex items-start justify-between mb-3">
                  <div>
                    <p className="text-sm font-semibold">{lote.codigo}</p>
                    <p className="text-xs text-muted-foreground">{lote.nombre}</p>
                  </div>
                  <Badge className={
                    lote.estado === 'Activo'
                      ? 'bg-emerald-500/10 text-emerald-400 border-emerald-500/20'
                      : lote.estado === 'EnPreparacion'
                        ? 'bg-amber-500/10 text-amber-400 border-amber-500/20'
                        : 'bg-zinc-500/10 text-zinc-400 border-zinc-500/20'
                  }>
                    {lote.estado}
                  </Badge>
                </div>

                {/* Barra de ocupación */}
                <div className="mb-3">
                  <div className="flex justify-between text-xs mb-1">
                    <span className="text-muted-foreground">Ocupación</span>
                    <span className="tabular-nums font-medium">
                      {lote.animalesActuales}/{lote.capacidadMaxima}
                    </span>
                  </div>
                  <div className="h-1.5 rounded-full bg-border overflow-hidden">
                    <div
                      className="h-full rounded-full bg-primary transition-all"
                      style={{ width: `${Math.min(lote.porcentajeOcupacion, 100)}%` }}
                    />
                  </div>
                  <p className="text-[10px] text-muted-foreground mt-1 text-right">
                    {fmt.pct(lote.porcentajeOcupacion)} ocupado
                  </p>
                </div>
              </Card>
            ))}
          </div>
        )}
      </div>
    </div>
  )
}
