import { format, subDays } from 'date-fns'
import { es } from 'date-fns/locale'
import { AlertTriangle, Beef, Package, Activity, ArrowRight } from 'lucide-react'
import { Link } from 'react-router-dom'
import { useLotes, useAnimalesIneficientes, useVacunasProximas } from '@/hooks/useFeedlot'
import { Card, CardContent, Skeleton } from '@/components/ui'
import { GmdChart } from '@/components/charts/GmdChart'
import { fmt, gmdBadgeColor } from '@/utils'
import type { LoteResumen, AnimalIneficiente } from '@/types'

const hoy = format(new Date(), 'yyyy-MM-dd')
const hace30 = format(subDays(new Date(), 30), 'yyyy-MM-dd')

function KpiCard({ label, value, sub, loading, accent }: { label: string; value: string | number; sub?: string; loading?: boolean; accent?: string }) {
  return (
    <Card className={`p-4 ${accent || ''}`}>
      {loading ? (
        <><Skeleton className="h-3 w-20 mb-2.5" /><Skeleton className="h-7 w-16" /></>
      ) : (
        <>
          <p className="text-[10px] font-semibold uppercase tracking-widest text-muted-foreground/50 mb-1.5">{label}</p>
          <p className="text-2xl font-bold tabular-nums tracking-tight leading-none">{value}</p>
          {sub && <p className="text-[11px] text-muted-foreground mt-1.5">{sub}</p>}
        </>
      )}
    </Card>
  )
}

export default function DashboardPage() {
  const { data: lotes, isLoading: loadingLotes } = useLotes(true)
  const { data: ineficientes, isLoading: loadingInef } = useAnimalesIneficientes({ desde: hace30, hasta: hoy })
  const { data: vacunasProx } = useVacunasProximas(7)

  const lotesArr = (lotes as LoteResumen[] | undefined) ?? []
  const alertas = (ineficientes as AnimalIneficiente[] | undefined) ?? []
  const vacunas = (vacunasProx as any[] | undefined) ?? []

  const totalAnimales = lotesArr.reduce((s, l) => s + l.animalesActuales, 0)
  const ocupacionMedia = lotesArr.length
    ? Math.round(lotesArr.reduce((s, l) => s + l.porcentajeOcupacion, 0) / lotesArr.length)
    : 0

  const fechaHoy = format(new Date(), "EEEE d 'de' MMMM", { locale: es })

  return (
    <div className="flex flex-col h-full">
      {/* Header */}
      <div className="px-6 py-4 border-b border-border/50">
        <p className="text-[11px] text-muted-foreground/60 uppercase tracking-widest capitalize">{fechaHoy}</p>
        <h1 className="text-base font-semibold tracking-tight mt-0.5">Panel de control</h1>
      </div>

      <div className="flex-1 overflow-y-auto p-6 space-y-5">
        {/* KPIs row */}
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-3">
          <KpiCard label="Animales en engorde" value={totalAnimales.toLocaleString('es-CO')} loading={loadingLotes} />
          <KpiCard label="Lotes activos" value={lotesArr.length} sub={`${ocupacionMedia}% ocupación media`} loading={loadingLotes} />
          <KpiCard label="Alertas productivas" value={alertas.length}
            accent={alertas.length > 0 ? 'border-rose-500/30' : ''}
            loading={loadingInef} />
          <KpiCard label="Vacunas próximas" value={vacunas.length}
            sub="en los próximos 7 días"
            accent={vacunas.length > 0 ? 'border-amber-500/30' : ''} />
        </div>

        {/* Main grid */}
        <div className="grid grid-cols-1 lg:grid-cols-5 gap-4">
          {/* Lotes — ocupa 3/5 */}
          <Card className="lg:col-span-3">
            <div className="flex items-center justify-between px-5 py-3 border-b border-border/40">
              <p className="text-xs font-semibold text-muted-foreground uppercase tracking-widest">Lotes activos</p>
              <Link to="/lotes" className="text-[11px] text-muted-foreground hover:text-foreground flex items-center gap-1 transition-colors">
                Ver todos <ArrowRight className="w-3 h-3" />
              </Link>
            </div>
            <CardContent className="p-0">
              {loadingLotes ? (
                <div className="p-5 space-y-3">
                  {[1,2,3].map(i => <Skeleton key={i} className="h-10" />)}
                </div>
              ) : lotesArr.length === 0 ? (
                <div className="flex flex-col items-center py-10 text-center">
                  <Package className="w-6 h-6 text-muted-foreground/20 mb-2" />
                  <p className="text-sm text-muted-foreground">Sin lotes activos</p>
                  <Link to="/lotes" className="text-xs text-primary mt-2 hover:underline">Crear lote →</Link>
                </div>
              ) : (
                <div>
                  {lotesArr.map((lote, i) => (
                    <Link key={lote.id} to={`/lotes/${lote.id}`}
                      className={`flex items-center gap-4 px-5 py-3 hover:bg-muted/30 transition-colors group ${i < lotesArr.length - 1 ? 'border-b border-border/30' : ''}`}>
                      <div className="w-7 h-7 rounded-md bg-primary/10 flex items-center justify-center flex-shrink-0">
                        <Package className="w-3.5 h-3.5 text-primary" />
                      </div>
                      <div className="flex-1 min-w-0">
                        <div className="flex items-baseline gap-2">
                          <span className="text-sm font-medium">{lote.codigo}</span>
                          {lote.nombre && <span className="text-xs text-muted-foreground truncate">{lote.nombre}</span>}
                        </div>
                        <div className="flex items-center gap-3 mt-0.5">
                          <span className="text-[11px] text-muted-foreground tabular-nums">
                            {lote.animalesActuales}/{lote.capacidadMaxima} animales
                          </span>
                          <div className="flex-1 h-1 bg-border rounded-full overflow-hidden max-w-[60px]">
                            <div className="h-full bg-primary rounded-full" style={{ width: `${Math.min(lote.porcentajeOcupacion, 100)}%` }} />
                          </div>
                          <span className="text-[11px] text-muted-foreground tabular-nums">{fmt.pct(lote.porcentajeOcupacion)}</span>
                        </div>
                      </div>
                      <ArrowRight className="w-3.5 h-3.5 text-muted-foreground/30 group-hover:text-muted-foreground transition-colors" />
                    </Link>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>

          {/* Panel derecho — 2/5 */}
          <div className="lg:col-span-2 space-y-4">
            {/* Alertas productivas */}
            <Card>
              <div className="flex items-center justify-between px-5 py-3 border-b border-border/40">
                <p className="text-xs font-semibold text-muted-foreground uppercase tracking-widest">Alertas</p>
                {alertas.length > 0 && (
                  <Link to="/alertas" className="text-[11px] text-rose-400 hover:text-rose-300 flex items-center gap-1 transition-colors">
                    {alertas.length} animales <ArrowRight className="w-3 h-3" />
                  </Link>
                )}
              </div>
              <CardContent className="p-0">
                {loadingInef ? (
                  <div className="p-4 space-y-2">{[1,2,3].map(i => <Skeleton key={i} className="h-9" />)}</div>
                ) : alertas.length === 0 ? (
                  <div className="flex items-center gap-2 px-5 py-4">
                    <div className="w-1.5 h-1.5 rounded-full bg-emerald-400 flex-shrink-0" />
                    <p className="text-xs text-muted-foreground">Sin alertas productivas</p>
                  </div>
                ) : (
                  <div className="max-h-48 overflow-y-auto">
                    {alertas.slice(0, 6).map(a => (
                      <div key={a.animalId} className="flex items-start gap-3 px-5 py-2.5 border-b border-border/20 last:border-0">
                        <div className="w-1.5 h-1.5 rounded-full bg-rose-400 flex-shrink-0 mt-1.5" />
                        <div className="min-w-0">
                          <div className="flex items-baseline gap-1.5">
                            <span className="text-xs font-mono font-medium">{a.codigoAnimal}</span>
                            <span className="text-[10px] text-muted-foreground">{a.loteCodigo}</span>
                          </div>
                          <p className="text-[10px] text-muted-foreground/70 mt-0.5 truncate">
                            GMD {fmt.kgDia(a.gmd)} · ICA {fmt.decimal(a.ica)}
                          </p>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </CardContent>
            </Card>

            {/* Accesos rápidos */}
            <Card>
              <div className="px-5 py-3 border-b border-border/40">
                <p className="text-xs font-semibold text-muted-foreground uppercase tracking-widest">Accesos rápidos</p>
              </div>
              <div className="p-2">
                {[
                  { to: '/animales', icon: Beef, label: 'Registrar animal', sub: 'Nuevo ingreso al feedlot' },
                  { to: '/finanzas', icon: Activity, label: 'Registrar movimiento', sub: 'Gasto o ingreso' },
                  { to: '/ventas', icon: AlertTriangle, label: 'Nueva venta', sub: 'Venta de animales' },
                ].map(item => (
                  <Link key={item.to} to={item.to}
                    className="flex items-center gap-3 px-3 py-2 rounded-md hover:bg-muted/40 transition-colors group">
                    <item.icon className="w-3.5 h-3.5 text-muted-foreground/60 group-hover:text-muted-foreground flex-shrink-0" />
                    <div className="min-w-0">
                      <p className="text-xs font-medium">{item.label}</p>
                      <p className="text-[10px] text-muted-foreground/60">{item.sub}</p>
                    </div>
                    <ArrowRight className="w-3 h-3 ml-auto text-muted-foreground/20 group-hover:text-muted-foreground/50 transition-colors" />
                  </Link>
                ))}
              </div>
            </Card>
          </div>
        </div>

        {/* GMD Chart */}
        <GmdChart />
      </div>
    </div>
  )
}
