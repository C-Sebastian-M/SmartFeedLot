import { format, subDays } from 'date-fns'
import { AlertTriangle, Beef, Package, TrendingUp, Activity } from 'lucide-react'
import { useLotes, useAnimalesIneficientes } from '@/hooks/useFeedlot'
import {
  StatCard, Card, CardHeader, CardTitle, CardContent,
  Badge, Skeleton, EmptyState, PageHeader
} from '@/components/ui'
import { GmdChart } from '@/components/charts/GmdChart'
import { fmt, gmdBadgeColor } from '@/utils'
import type { LoteResumen, AnimalIneficiente } from '@/types'

const hoy = format(new Date(), 'yyyy-MM-dd')
const hace30 = format(subDays(new Date(), 30), 'yyyy-MM-dd')

export default function DashboardPage() {
  const { data: lotes, isLoading: loadingLotes } = useLotes(true)
  const { data: ineficientes, isLoading: loadingInef } = useAnimalesIneficientes({
    desde: hace30,
    hasta: hoy,
  })

  const lotesArray = (lotes as LoteResumen[] | undefined) ?? []
  const ineficientesArray = (ineficientes as AnimalIneficiente[] | undefined) ?? []

  const totalAnimales = lotesArray.reduce((s, l) => s + l.animalesActuales, 0)
  const totalLotesActivos = lotesArray.length

  return (
    <div className="flex flex-col h-full animate-fade-in">
      <PageHeader
        title="Dashboard"
        description="Resumen ejecutivo del sistema de feedlot"
      />

      <div className="flex-1 overflow-y-auto p-6 space-y-6">
        {/* KPI Cards */}
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
          <StatCard
            loading={loadingLotes}
            label="Animales en engorde"
            value={totalAnimales.toLocaleString('es-CO')}
            icon={<Beef className="w-4 h-4" />}
          />
          <StatCard
            loading={loadingLotes}
            label="Lotes activos"
            value={totalLotesActivos}
            icon={<Package className="w-4 h-4" />}
          />
          <StatCard
            loading={loadingInef}
            label="Alertas productivas"
            value={ineficientesArray.length}
            icon={<AlertTriangle className="w-4 h-4" />}
            className={ineficientesArray.length > 0 ? 'border-rose-500/30' : ''}
          />
          <StatCard
            loading={false}
            label="Período análisis"
            value="30 días"
            icon={<Activity className="w-4 h-4" />}
          />
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
          {/* Lotes activos */}
          <Card className="lg:col-span-2">
            <CardHeader>
              <CardTitle>Lotes activos</CardTitle>
            </CardHeader>
            <CardContent>
              {loadingLotes ? (
                <div className="space-y-3">
                  {[1, 2, 3].map(i => <Skeleton key={i} className="h-12 w-full" />)}
                </div>
              ) : !lotesArray.length ? (
                <EmptyState
                  icon={<Package className="w-5 h-5" />}
                  title="Sin lotes activos"
                  description="Crea un lote para comenzar a registrar animales."
                />
              ) : (
                <div className="space-y-2">
                  {lotesArray.map((lote) => (
                    <div
                      key={lote.id}
                      className="flex items-center justify-between p-3 rounded-lg bg-secondary/50 hover:bg-secondary transition-colors"
                    >
                      <div className="flex items-center gap-3">
                        <div className="w-8 h-8 rounded-md bg-primary/10 flex items-center justify-center">
                          <Package className="w-4 h-4 text-primary" />
                        </div>
                        <div>
                          <p className="text-sm font-medium">{lote.codigo}</p>
                          <p className="text-xs text-muted-foreground">{lote.nombre}</p>
                        </div>
                      </div>
                      <div className="flex items-center gap-4 text-right">
                        <div>
                          <p className="text-sm font-semibold tabular-nums">
                            {lote.animalesActuales}/{lote.capacidadMaxima}
                          </p>
                          <p className="text-xs text-muted-foreground">
                            {fmt.pct(lote.porcentajeOcupacion)} ocupado
                          </p>
                        </div>
                        <div className="w-16 h-1.5 rounded-full bg-border overflow-hidden">
                          <div
                            className="h-full rounded-full bg-primary transition-all"
                            style={{ width: `${Math.min(lote.porcentajeOcupacion, 100)}%` }}
                          />
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>

          {/* Panel de alertas */}
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle>Alertas productivas</CardTitle>
                {ineficientesArray.length > 0 && (
                  <Badge className="bg-rose-500/10 text-rose-400 border-rose-500/20">
                    {ineficientesArray.length}
                  </Badge>
                )}
              </div>
            </CardHeader>
            <CardContent>
              {loadingInef ? (
                <div className="space-y-2">
                  {[1, 2, 3].map(i => <Skeleton key={i} className="h-14 w-full" />)}
                </div>
              ) : !ineficientesArray.length ? (
                <EmptyState
                  icon={<TrendingUp className="w-5 h-5" />}
                  title="Sin alertas"
                  description="Todos los animales están dentro de los umbrales productivos."
                />
              ) : (
                <div className="space-y-2 max-h-72 overflow-y-auto">
                  {ineficientesArray.slice(0, 8).map((animal) => (
                    <div
                      key={animal.animalId}
                      className="p-2.5 rounded-lg border border-rose-500/20 bg-rose-500/5"
                    >
                      <div className="flex items-center justify-between mb-1">
                        <span className="text-xs font-mono font-medium">
                          {animal.codigoAnimal}
                        </span>
                        <Badge className="bg-rose-500/10 text-rose-400 border-rose-500/20 text-[10px]">
                          {animal.loteCodigo}
                        </Badge>
                      </div>
                      <p className="text-[10px] text-muted-foreground leading-relaxed">
                        GMD {fmt.kgDia(animal.gmd)} · ICA {fmt.decimal(animal.ica)}
                      </p>
                      <p className="text-[10px] text-rose-400/80 mt-0.5 leading-relaxed">
                        {animal.motivoAlerta}
                      </p>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </div>

        {/* GMD Chart */}
        <GmdChart />
      </div>
    </div>
  )
}
