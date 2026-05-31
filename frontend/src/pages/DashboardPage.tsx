import { format, subDays } from 'date-fns'
import { es } from 'date-fns/locale'
import {
  AlertTriangle, Beef, Package, ArrowRight,
  TrendingUp, TrendingDown, Landmark, DollarSign,
  CheckCircle2, Clock, AlertCircle,
} from 'lucide-react'
import { Link } from 'react-router-dom'
import {
  useLotes, useAnimalesIneficientes, useVacunasProximas,
  useEstadoResultados, usePrestamos, useComparativoPresupuesto,
} from '@/hooks/useFeedlot'
import { Card, CardContent, Skeleton } from '@/components/ui'
import { GmdChart } from '@/components/charts/GmdChart'
import { fmt } from '@/utils'
import type { LoteResumen, AnimalIneficiente, Prestamo, EstadoResultados, ComparativoPresupuesto } from '@/types'

// ── Constants ─────────────────────────────────────────────────────────────────
const ahora = new Date()
const anioActual = ahora.getFullYear()
const mesActual = ahora.getMonth() + 1
const hoy = format(ahora, 'yyyy-MM-dd')
const hace30 = format(subDays(ahora, 30), 'yyyy-MM-dd')

function diasHasta(fechaIso: string): number {
  const [y, m, d] = fechaIso.split('-').map(Number)
  const fecha = new Date(y, m - 1, d)
  fecha.setHours(0, 0, 0, 0)
  const hoyD = new Date(); hoyD.setHours(0, 0, 0, 0)
  return Math.round((fecha.getTime() - hoyD.getTime()) / 86_400_000)
}

// ── Widgets ───────────────────────────────────────────────────────────────────

function KpiCard({ label, value, sub, accent, loading, to }: {
  label: string; value: string | number; sub?: string
  accent?: string; loading?: boolean; to?: string
}) {
  const inner = (
    <Card className={`p-4 h-full ${accent ?? ''} ${to ? 'hover:border-primary/30 cursor-pointer transition-colors' : ''}`}>
      {loading ? (
        <><Skeleton className="h-3 w-20 mb-2.5" /><Skeleton className="h-7 w-24" /></>
      ) : (
        <>
          <p className="text-[10px] font-semibold uppercase tracking-widest text-muted-foreground/50 mb-1.5">{label}</p>
          <p className="text-2xl font-bold tabular-nums tracking-tight leading-none">{value}</p>
          {sub && <p className="text-[11px] text-muted-foreground mt-1.5">{sub}</p>}
        </>
      )}
    </Card>
  )
  return to ? <Link to={to}>{inner}</Link> : inner
}

// ── P&L del mes ───────────────────────────────────────────────────────────────

function PnLWidget({ data, loading }: { data?: EstadoResultados; loading: boolean }) {
  const nombreMes = format(ahora, 'MMMM yyyy', { locale: es })

  return (
    <Card className="flex flex-col h-full">
      <div className="flex items-center justify-between px-5 py-3.5 border-b border-border/40">
        <div>
          <p className="text-xs font-semibold text-muted-foreground uppercase tracking-widest">Estado de Resultados</p>
          <p className="text-[11px] text-muted-foreground/60 capitalize mt-0.5">{nombreMes}</p>
        </div>
        <Link to="/finanzas" className="text-[11px] text-muted-foreground hover:text-foreground flex items-center gap-1 transition-colors">
          Ver detalle <ArrowRight className="w-3 h-3" />
        </Link>
      </div>
      <CardContent className="p-5 flex-1">
        {loading ? (
          <div className="space-y-3">
            {[1, 2, 3, 4, 5].map(i => <Skeleton key={i} className="h-5" />)}
          </div>
        ) : !data ? (
          <div className="flex flex-col items-center justify-center h-full py-6 text-center">
            <DollarSign className="w-6 h-6 text-muted-foreground/20 mb-2" />
            <p className="text-sm text-muted-foreground">Sin movimientos este mes</p>
            <Link to="/finanzas" className="text-xs text-primary mt-2 hover:underline">Registrar movimiento →</Link>
          </div>
        ) : (
          <div className="space-y-0">
            {/* Ingresos */}
            <LineaPnL
              label="Ingresos"
              valor={data.totalIngresos}
              color="text-emerald-400"
              bg="bg-emerald-400"
              total={data.totalIngresos + data.totalCostosDirectos + data.totalGastosIndirectos + data.totalGastosOperativos}
              positivo
            />
            {/* Costos directos */}
            <LineaPnL
              label="Costos directos"
              valor={data.totalCostosDirectos}
              color="text-rose-400"
              bg="bg-rose-400"
              total={data.totalIngresos + data.totalCostosDirectos + data.totalGastosIndirectos + data.totalGastosOperativos}
            />
            {/* Gastos operativos */}
            {(data.totalGastosIndirectos + data.totalGastosOperativos) > 0 && (
              <LineaPnL
                label="Gastos op. e indirectos"
                valor={data.totalGastosIndirectos + data.totalGastosOperativos}
                color="text-amber-400"
                bg="bg-amber-400"
                total={data.totalIngresos + data.totalCostosDirectos + data.totalGastosIndirectos + data.totalGastosOperativos}
              />
            )}
            {/* Intereses */}
            {data.totalInteresesPrestamo > 0 && (
              <LineaPnL
                label="Intereses préstamo"
                valor={data.totalInteresesPrestamo}
                color="text-violet-400"
                bg="bg-violet-400"
                total={data.totalIngresos + data.totalCostosDirectos + data.totalGastosIndirectos + data.totalGastosOperativos}
              />
            )}

            {/* Separador y utilidad neta */}
            <div className="pt-3 mt-3 border-t border-border/40">
              <div className="flex items-baseline justify-between">
                <span className="text-sm font-semibold">Utilidad neta</span>
                <span className={`text-xl font-bold tabular-nums ${data.utilidadNeta >= 0 ? 'text-emerald-400' : 'text-rose-400'}`}>
                  {data.utilidadNeta < 0 ? `(${fmt.cop(Math.abs(data.utilidadNeta))})` : fmt.cop(data.utilidadNeta)}
                </span>
              </div>
              <div className="flex items-center gap-1.5 mt-1">
                {data.utilidadNeta >= 0
                  ? <TrendingUp className="w-3.5 h-3.5 text-emerald-400" />
                  : <TrendingDown className="w-3.5 h-3.5 text-rose-400" />
                }
                <span className={`text-[11px] ${data.utilidadNeta >= 0 ? 'text-emerald-400' : 'text-rose-400'}`}>
                  {data.utilidadNeta >= 0 ? 'Mes positivo' : 'Mes en negativo'}
                </span>
              </div>
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  )
}

function LineaPnL({ label, valor, color, bg, total, positivo }: {
  label: string; valor: number; color: string; bg: string; total: number; positivo?: boolean
}) {
  const pct = total > 0 ? Math.min((valor / total) * 100, 100) : 0
  return (
    <div className="py-2.5 border-b border-border/20 last:border-0">
      <div className="flex items-center justify-between mb-1.5">
        <span className="text-xs text-muted-foreground">{label}</span>
        <span className={`text-xs font-semibold tabular-nums ${color}`}>
          {positivo ? '' : '−'}{fmt.cop(valor)}
        </span>
      </div>
      <div className="h-1 bg-border/40 rounded-full overflow-hidden">
        <div className={`h-full ${bg} rounded-full transition-all duration-500`} style={{ width: `${pct}%` }} />
      </div>
    </div>
  )
}

// ── Próxima cuota ─────────────────────────────────────────────────────────────

function ProximaCuotaWidget({ prestamos, loading }: { prestamos: Prestamo[]; loading: boolean }) {
  // Encuentra la próxima cuota sin pagar más cercana a vencer
  const todasPendientes = prestamos
    .flatMap(p => p.cuotas
      .filter(c => !c.pagada)
      .map(c => ({ ...c, prestamo: p }))
    )
    .sort((a, b) => a.fechaVencimiento.localeCompare(b.fechaVencimiento))

  const proxima = todasPendientes[0] ?? null
  const dias = proxima ? diasHasta(proxima.fechaVencimiento) : null
  const vencida = dias !== null && dias < 0
  const urgent = dias !== null && dias <= 7

  // Progreso del préstamo de la próxima cuota
  const prestamoDeLaProxima = proxima ? prestamos.find(p => p.id === proxima.prestamo.id) : null
  const pagadasCount = prestamoDeLaProxima?.cuotas.filter(c => c.pagada).length ?? 0
  const totalCount = prestamoDeLaProxima?.nCuotas ?? 1
  const pctPagado = Math.round((pagadasCount / totalCount) * 100)

  return (
    <Card className={`${vencida ? 'border-rose-500/40' : urgent ? 'border-amber-500/30' : ''}`}>
      <div className="flex items-center justify-between px-5 py-3.5 border-b border-border/40">
        <p className="text-xs font-semibold text-muted-foreground uppercase tracking-widest">Próxima cuota</p>
        <Link to="/prestamos" className="text-[11px] text-muted-foreground hover:text-foreground flex items-center gap-1 transition-colors">
          Ver préstamos <ArrowRight className="w-3 h-3" />
        </Link>
      </div>
      <CardContent className="p-5">
        {loading ? (
          <div className="space-y-2"><Skeleton className="h-8 w-32" /><Skeleton className="h-4 w-48" /></div>
        ) : !proxima ? (
          <div className="flex items-center gap-2 text-emerald-400">
            <CheckCircle2 className="w-5 h-5" />
            <div>
              <p className="text-sm font-medium">Sin cuotas pendientes</p>
              <p className="text-[11px] text-muted-foreground mt-0.5">Todos los préstamos al día</p>
            </div>
          </div>
        ) : (
          <div className="space-y-3">
            {/* Countdown */}
            <div className="flex items-end gap-3">
              <div>
                <p className={`text-3xl font-bold tabular-nums tracking-tight ${vencida ? 'text-rose-400' : urgent ? 'text-amber-400' : 'text-foreground'}`}>
                  {fmt.cop(proxima.cuota)}
                </p>
                <p className="text-[11px] text-muted-foreground mt-0.5 truncate max-w-[200px]">{proxima.prestamo.descripcion}</p>
              </div>
              <div className={`ml-auto text-right flex-shrink-0`}>
                {vencida ? (
                  <div className="flex items-center gap-1.5 text-rose-400">
                    <AlertCircle className="w-4 h-4" />
                    <div>
                      <p className="text-sm font-bold">Vencida</p>
                      <p className="text-[10px]">hace {Math.abs(dias!)} días</p>
                    </div>
                  </div>
                ) : dias === 0 ? (
                  <div className="flex items-center gap-1.5 text-amber-400">
                    <Clock className="w-4 h-4" />
                    <div>
                      <p className="text-sm font-bold">Hoy</p>
                      <p className="text-[10px]">vence hoy</p>
                    </div>
                  </div>
                ) : (
                  <div className={urgent ? 'text-amber-400' : 'text-muted-foreground'}>
                    <p className={`text-2xl font-bold tabular-nums leading-none`}>{dias}</p>
                    <p className="text-[10px] mt-0.5">días</p>
                  </div>
                )}
              </div>
            </div>

            {/* Detalles de la cuota */}
            <div className="flex gap-4 text-[11px] text-muted-foreground">
              <span>Vence: <strong>{fmt.fecha(proxima.fechaVencimiento)}</strong></span>
              <span>Cuota #{proxima.numeroCuota}</span>
            </div>

            {/* Progreso del préstamo */}
            {prestamoDeLaProxima && (
              <div>
                <div className="flex items-center justify-between mb-1">
                  <span className="text-[10px] text-muted-foreground/60">{pagadasCount}/{totalCount} cuotas pagadas</span>
                  <span className="text-[10px] text-muted-foreground/60">{pctPagado}%</span>
                </div>
                <div className="h-1 bg-border/40 rounded-full overflow-hidden">
                  <div className="h-full bg-primary rounded-full" style={{ width: `${pctPagado}%` }} />
                </div>
              </div>
            )}
          </div>
        )}
      </CardContent>
    </Card>
  )
}

// ── Ejecución presupuesto ─────────────────────────────────────────────────────

function PresupuestoWidget({ data, loading }: { data?: ComparativoPresupuesto; loading: boolean }) {
  const nombreMes = format(ahora, 'MMMM', { locale: es })
  const pct = data?.porcentajeEjecucion ?? 0
  const color = pct > 110 ? 'bg-rose-400' : pct > 90 ? 'bg-amber-400' : 'bg-emerald-400'
  const textColor = pct > 110 ? 'text-rose-400' : pct > 90 ? 'text-amber-400' : 'text-emerald-400'

  return (
    <Card>
      <div className="flex items-center justify-between px-5 py-3.5 border-b border-border/40">
        <p className="text-xs font-semibold text-muted-foreground uppercase tracking-widest capitalize">Presupuesto · {nombreMes}</p>
        <Link to="/finanzas" className="text-[11px] text-muted-foreground hover:text-foreground flex items-center gap-1 transition-colors">
          Ver <ArrowRight className="w-3 h-3" />
        </Link>
      </div>
      <CardContent className="p-5">
        {loading ? (
          <div className="space-y-2"><Skeleton className="h-6 w-20" /><Skeleton className="h-2 w-full" /></div>
        ) : !data || data.totalPresupuestado === 0 ? (
          <div>
            <p className="text-sm text-muted-foreground">Sin presupuesto</p>
            <Link to="/finanzas" className="text-[11px] text-primary hover:underline">Configurar presupuesto →</Link>
          </div>
        ) : (
          <div className="space-y-3">
            <div className="flex items-end justify-between">
              <div>
                <p className={`text-2xl font-bold tabular-nums ${textColor}`}>{pct}%</p>
                <p className="text-[11px] text-muted-foreground mt-0.5">de ejecución</p>
              </div>
              <div className="text-right">
                <p className="text-xs font-semibold tabular-nums">{fmt.cop(data.totalReal)}</p>
                <p className="text-[11px] text-muted-foreground">de {fmt.cop(data.totalPresupuestado)}</p>
              </div>
            </div>
            <div className="h-2 bg-border/40 rounded-full overflow-hidden">
              <div className={`h-full ${color} rounded-full transition-all duration-500`}
                style={{ width: `${Math.min(pct, 100)}%` }} />
            </div>
            <div className="flex justify-between text-[10px] text-muted-foreground/60">
              <span>Desviación: {data.totalDesviacion >= 0 ? '+' : ''}{fmt.cop(data.totalDesviacion)}</span>
              <span>{pct > 110 ? '⚠ Sobre presupuesto' : pct > 90 ? 'Cerca del límite' : 'Dentro del presupuesto'}</span>
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  )
}

// ── Lotes activos ─────────────────────────────────────────────────────────────

function LotesWidget({ lotes, loading }: { lotes: LoteResumen[]; loading: boolean }) {
  return (
    <Card>
      <div className="flex items-center justify-between px-5 py-3.5 border-b border-border/40">
        <p className="text-xs font-semibold text-muted-foreground uppercase tracking-widest">Lotes activos</p>
        <Link to="/lotes" className="text-[11px] text-muted-foreground hover:text-foreground flex items-center gap-1 transition-colors">
          Ver todos <ArrowRight className="w-3 h-3" />
        </Link>
      </div>
      <CardContent className="p-0">
        {loading ? (
          <div className="p-5 space-y-3">{[1, 2, 3].map(i => <Skeleton key={i} className="h-10" />)}</div>
        ) : lotes.length === 0 ? (
          <div className="flex flex-col items-center py-8 text-center">
            <Package className="w-6 h-6 text-muted-foreground/20 mb-2" />
            <p className="text-sm text-muted-foreground">Sin lotes activos</p>
            <Link to="/lotes" className="text-xs text-primary mt-1.5 hover:underline">Crear lote →</Link>
          </div>
        ) : (
          lotes.map((lote, i) => (
            <Link key={lote.id} to={`/lotes/${lote.id}`}
              className={`flex items-center gap-3 px-5 py-3 hover:bg-muted/20 transition-colors group ${i < lotes.length - 1 ? 'border-b border-border/30' : ''}`}>
              <div className="w-7 h-7 rounded-md bg-primary/10 flex items-center justify-center flex-shrink-0">
                <Package className="w-3.5 h-3.5 text-primary" />
              </div>
              <div className="flex-1 min-w-0">
                <div className="flex items-baseline gap-2">
                  <span className="text-sm font-medium">{lote.codigo}</span>
                  {lote.nombre && <span className="text-xs text-muted-foreground truncate">{lote.nombre}</span>}
                </div>
                <div className="flex items-center gap-2 mt-0.5">
                  <span className="text-[11px] text-muted-foreground tabular-nums">
                    {lote.animalesActuales}/{lote.capacidadMaxima}
                  </span>
                  <div className="flex-1 h-1 bg-border/40 rounded-full overflow-hidden max-w-[48px]">
                    <div className="h-full bg-primary rounded-full" style={{ width: `${Math.min(lote.porcentajeOcupacion, 100)}%` }} />
                  </div>
                  <span className="text-[11px] text-muted-foreground">{Math.round(lote.porcentajeOcupacion)}%</span>
                </div>
              </div>
              <ArrowRight className="w-3.5 h-3.5 text-muted-foreground/20 group-hover:text-muted-foreground/60 transition-colors flex-shrink-0" />
            </Link>
          ))
        )}
      </CardContent>
    </Card>
  )
}

// ── Alertas productivas ───────────────────────────────────────────────────────

function AlertasWidget({ alertas, loading }: { alertas: AnimalIneficiente[]; loading: boolean }) {
  return (
    <Card className={alertas.length > 0 ? 'border-rose-500/20' : ''}>
      <div className="flex items-center justify-between px-5 py-3.5 border-b border-border/40">
        <p className="text-xs font-semibold text-muted-foreground uppercase tracking-widest">Alertas productivas</p>
        {alertas.length > 0 && (
          <Link to="/alertas" className="text-[11px] text-rose-400 hover:text-rose-300 flex items-center gap-1 transition-colors">
            {alertas.length} animales <ArrowRight className="w-3 h-3" />
          </Link>
        )}
      </div>
      <CardContent className="p-0">
        {loading ? (
          <div className="p-4 space-y-2">{[1, 2, 3].map(i => <Skeleton key={i} className="h-9" />)}</div>
        ) : alertas.length === 0 ? (
          <div className="flex items-center gap-3 px-5 py-4">
            <div className="w-1.5 h-1.5 rounded-full bg-emerald-400 flex-shrink-0" />
            <p className="text-xs text-muted-foreground">Sin alertas productivas</p>
          </div>
        ) : (
          <div className="max-h-52 overflow-y-auto">
            {alertas.slice(0, 8).map((a, i) => (
              <div key={a.animalId}
                className={`flex items-start gap-3 px-5 py-2.5 ${i < Math.min(alertas.length - 1, 7) ? 'border-b border-border/20' : ''}`}>
                <div className={`w-1.5 h-1.5 rounded-full flex-shrink-0 mt-1.5 ${a.gmd < 0.5 ? 'bg-rose-400' : 'bg-amber-400'}`} />
                <div className="min-w-0 flex-1">
                  <div className="flex items-baseline gap-1.5 justify-between">
                    <span className="text-xs font-mono font-medium">{a.codigoAnimal}</span>
                    <span className="text-[10px] text-muted-foreground tabular-nums">GMD {fmt.kgDia(a.gmd)}</span>
                  </div>
                  <p className="text-[10px] text-muted-foreground/70 truncate">{a.loteCodigo} · ICA {fmt.decimal(a.ica)}</p>
                </div>
              </div>
            ))}
            {alertas.length > 8 && (
              <Link to="/alertas" className="flex items-center justify-center py-2.5 text-[11px] text-muted-foreground hover:text-foreground border-t border-border/20 transition-colors">
                Ver {alertas.length - 8} más →
              </Link>
            )}
          </div>
        )}
      </CardContent>
    </Card>
  )
}

// ── Page ──────────────────────────────────────────────────────────────────────

export default function DashboardPage() {
  const { data: lotes, isLoading: loadingLotes } = useLotes(true)
  const { data: ineficientes, isLoading: loadingInef } = useAnimalesIneficientes({
    desde: hace30, hasta: hoy, gmdMinima: 0.8, icaMaxima: 8
  })
  const { data: vacunasData } = useVacunasProximas(7)
  const { data: pyg, isLoading: loadingPyg } = useEstadoResultados({ anio: anioActual, mes: mesActual })
  const { data: prestamosData, isLoading: loadingPrestamos } = usePrestamos()
  const { data: presupuesto, isLoading: loadingPresupuesto } = useComparativoPresupuesto({
    anio: anioActual, mes: mesActual
  })

  const lotesArr = (lotes as LoteResumen[] | undefined) ?? []
  const alertas = (ineficientes as AnimalIneficiente[] | undefined) ?? []
  const prestamos = (prestamosData as Prestamo[] | undefined) ?? []
  const vacunas = (vacunasData as any[] | undefined) ?? []
  const pygData = pyg as EstadoResultados | undefined
  const presupuestoData = presupuesto as ComparativoPresupuesto | undefined

  const totalAnimales = lotesArr.reduce((s, l) => s + l.animalesActuales, 0)
  const cuotasVencidas = prestamos.flatMap(p => p.cuotas.filter(c => !c.pagada)).filter(c => {
    const [y, m, d] = c.fechaVencimiento.split('-').map(Number)
    return new Date(y, m - 1, d) < new Date()
  }).length

  const fechaHoy = format(ahora, "EEEE d 'de' MMMM", { locale: es })

  return (
    <div className="flex flex-col h-full">
      {/* Header con fecha */}
      <div className="px-6 py-4 border-b border-border/50">
        <p className="text-[11px] text-muted-foreground/50 uppercase tracking-widest capitalize">{fechaHoy}</p>
        <h1 className="text-base font-semibold tracking-tight mt-0.5">Panel de control</h1>
      </div>

      <div className="flex-1 overflow-y-auto p-5 space-y-4">

        {/* ── Fila 1: KPIs compactos ── */}
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-3">
          <KpiCard
            label="Utilidad neta mes"
            value={loadingPyg ? '...' : pygData ? (pygData.utilidadNeta >= 0 ? fmt.cop(pygData.utilidadNeta) : `(${fmt.cop(Math.abs(pygData.utilidadNeta))})`) : '—'}
            accent={pygData ? (pygData.utilidadNeta < 0 ? 'border-rose-500/30' : 'border-emerald-500/20') : ''}
            loading={loadingPyg}
            to="/finanzas"
          />
          <KpiCard
            label="Animales en engorde"
            value={totalAnimales.toLocaleString('es-CO')}
            sub={`${lotesArr.length} lotes activos`}
            loading={loadingLotes}
            to="/animales"
          />
          <KpiCard
            label="Cuotas vencidas"
            value={cuotasVencidas}
            sub={cuotasVencidas > 0 ? 'Requieren atención' : 'Todo al día'}
            accent={cuotasVencidas > 0 ? 'border-rose-500/30' : ''}
            loading={loadingPrestamos}
            to="/prestamos"
          />
          <KpiCard
            label="Alertas (30 días)"
            value={alertas.length}
            sub={vacunas.length > 0 ? `+ ${vacunas.length} vacunas próximas` : undefined}
            accent={alertas.length > 0 ? 'border-amber-500/20' : ''}
            loading={loadingInef}
            to="/alertas"
          />
        </div>

        {/* ── Fila 2: P&L + columna derecha ── */}
        <div className="grid grid-cols-1 lg:grid-cols-5 gap-4">
          {/* P&L — 3/5 */}
          <div className="lg:col-span-3">
            <PnLWidget data={pygData} loading={loadingPyg} />
          </div>

          {/* Columna derecha — 2/5 */}
          <div className="lg:col-span-2 flex flex-col gap-4">
            <ProximaCuotaWidget prestamos={prestamos} loading={loadingPrestamos} />
            <PresupuestoWidget data={presupuestoData} loading={loadingPresupuesto} />
          </div>
        </div>

        {/* ── Fila 3: Lotes + Alertas ── */}
        <div className="grid grid-cols-1 lg:grid-cols-5 gap-4">
          <div className="lg:col-span-3">
            <LotesWidget lotes={lotesArr} loading={loadingLotes} />
          </div>
          <div className="lg:col-span-2">
            <AlertasWidget alertas={alertas} loading={loadingInef} />
          </div>
        </div>

        {/* ── GMD Chart ── */}
        <GmdChart />

      </div>
    </div>
  )
}
