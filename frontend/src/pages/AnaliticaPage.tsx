import { useState } from 'react'
import { format, subDays } from 'date-fns'
import {
  BarChart3, TrendingUp, Package, DollarSign,
  AlertTriangle, Beef, ChevronDown
} from 'lucide-react'
import { useLotes, useResumenLote } from '@/hooks/useFeedlot'
import {
  PageHeader, Card, CardHeader, CardTitle, CardContent,
  Badge, Skeleton, EmptyState, StatCard,
} from '@/components/ui'
import { fmt, gmdBadgeColor, CHART_COLORS } from '@/utils'
import type { LoteResumen, IndicadorProductivo } from '@/types'
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip,
  ResponsiveContainer, Cell, ScatterChart, Scatter,
  ReferenceLine, Legend,
} from 'recharts'

// ─── Selector de lote ─────────────────────────────────────────────────────────
function SelectorLote({
  lotes, value, onChange,
}: {
  lotes: LoteResumen[]
  value: string
  onChange: (id: string) => void
}) {
  return (
    <div className="relative">
      <select
        value={value}
        onChange={e => onChange(e.target.value)}
        className="h-9 pl-3 pr-8 rounded-md border border-input bg-card text-sm
          focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring
          appearance-none cursor-pointer [&>option]:bg-card"
      >
        <option value="">Seleccionar lote...</option>
        {lotes.map(l => (
          <option key={l.id} value={l.id}>
            {l.codigo} — {l.nombre} ({l.animalesActuales} animales)
          </option>
        ))}
      </select>
      <ChevronDown className="absolute right-2 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-muted-foreground pointer-events-none" />
    </div>
  )
}

// ─── Tooltips personalizados ──────────────────────────────────────────────────
const GmdTooltip = ({ active, payload }: any) => {
  if (!active || !payload?.length) return null
  const d = payload[0]?.payload as IndicadorProductivo
  return (
    <div className="rounded-lg border border-border bg-card px-3 py-2.5 shadow-lg text-xs space-y-1">
      <p className="font-semibold">{d.codigoAnimal}{d.nombreAnimal ? ` — ${d.nombreAnimal}` : ''}</p>
      <p className="text-muted-foreground">{d.raza}</p>
      <p className="text-emerald-400">GMD: {fmt.kgDia(d.gmd)}</p>
      <p className="text-blue-400">ICA: {fmt.decimal(d.ica)}</p>
      <p className="text-amber-400">Costo/kg: {fmt.cop(d.costoPorKgGanado)}</p>
    </div>
  )
}

const BarTooltip = ({ active, payload, label }: any) => {
  if (!active || !payload?.length) return null
  return (
    <div className="rounded-lg border border-border bg-card px-3 py-2 shadow-lg text-xs">
      <p className="font-mono font-medium mb-1">{label}</p>
      {payload.map((p: any) => (
        <p key={p.name} style={{ color: p.color }}>
          {p.name}: {p.name === 'GMD' ? fmt.kgDia(p.value) : fmt.decimal(p.value)}
        </p>
      ))}
    </div>
  )
}

// ─── Tabla de indicadores ─────────────────────────────────────────────────────
function TablaIndicadores({ indicadores }: { indicadores: IndicadorProductivo[] }) {
  const ordenados = [...indicadores].sort((a, b) => b.gmd - a.gmd)

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle>Indicadores por animal</CardTitle>
          <span className="text-xs text-muted-foreground">{indicadores.length} animales</span>
        </div>
      </CardHeader>
      <CardContent className="p-0">
        <div className="overflow-x-auto">
          <table className="w-full text-xs">
            <thead>
              <tr className="border-b border-border">
                {['Código', 'Nombre', 'Raza', 'Peso inicial', 'Peso actual', 'Ganado', 'Días', 'GMD', 'ICA', 'Costo/kg', 'Rentabilidad', 'Estado'].map(h => (
                  <th key={h} className="text-left px-4 py-3 text-muted-foreground font-medium uppercase tracking-wide text-[10px] whitespace-nowrap">
                    {h}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {ordenados.map((ind, i) => (
                <tr key={ind.animalId}
                  className={`border-b border-border/40 hover:bg-secondary/30 transition-colors
                    ${i === ordenados.length - 1 ? 'border-b-0' : ''}
                    ${ind.esIneficiente ? 'bg-rose-500/3' : ''}`}
                >
                  <td className="px-4 py-2.5 font-mono font-semibold">{ind.codigoAnimal}</td>
                  <td className="px-4 py-2.5 text-muted-foreground">{ind.nombreAnimal || '-'}</td>
                  <td className="px-4 py-2.5 text-muted-foreground">{ind.raza}</td>
                  <td className="px-4 py-2.5 tabular-nums">{fmt.kg(ind.pesoInicialKg)}</td>
                  <td className="px-4 py-2.5 tabular-nums font-medium">{fmt.kg(ind.pesoActualKg)}</td>
                  <td className="px-4 py-2.5 tabular-nums text-emerald-400">+{fmt.kg(ind.pesoGanadoKg)}</td>
                  <td className="px-4 py-2.5 tabular-nums text-muted-foreground">{ind.diasEnEngorde}d</td>
                  <td className="px-4 py-2.5 tabular-nums font-medium">{fmt.kgDia(ind.gmd)}</td>
                  <td className="px-4 py-2.5 tabular-nums">{fmt.decimal(ind.ica)}</td>
                  <td className="px-4 py-2.5 tabular-nums">{fmt.cop(ind.costoPorKgGanado)}</td>
                  <td className={`px-4 py-2.5 tabular-nums font-medium ${ind.rentabilidadProyectada >= 0 ? 'text-emerald-400' : 'text-rose-400'}`}>
                    {fmt.cop(ind.rentabilidadProyectada)}
                  </td>
                  <td className="px-4 py-2.5">
                    <Badge className={gmdBadgeColor[ind.clasificacionGmd]}>
                      {ind.clasificacionGmd}
                    </Badge>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </CardContent>
    </Card>
  )
}

// ─── Página principal ─────────────────────────────────────────────────────────
export default function AnaliticaPage() {
  const hoy = format(new Date(), 'yyyy-MM-dd')
  const hace90 = format(subDays(new Date(), 90), 'yyyy-MM-dd')

  const [loteId, setLoteId] = useState('')
  const [desde, setDesde] = useState(hace90)
  const [hasta, setHasta] = useState(hoy)
  const [precioVenta, setPrecioVenta] = useState(5500)

  const { data: lotes, isLoading: loadingLotes } = useLotes(true)
  const lotesArray = (lotes as LoteResumen[] | undefined) ?? []

  const { data: resumen, isLoading: loadingResumen } = useResumenLote({
    loteId,
    desde,
    hasta,
    precioVentaEstimadoPorKg: precioVenta,
  })

  // Datos para el gráfico de barras de GMD por animal
  const datosGmd = resumen?.indicadores
    .sort((a, b) => b.gmd - a.gmd)
    .slice(0, 20)
    .map(ind => ({
      codigo: ind.codigoAnimal,
      GMD: ind.gmd,
      ICA: ind.ica,
      esIneficiente: ind.esIneficiente,
      ...ind,
    })) ?? []

  return (
    <div className="flex flex-col h-full animate-fade-in">
      <PageHeader
        title="Analítica productiva"
        description="GMD, ICA, eficiencia alimenticia y rentabilidad por lote"
      />

      {/* Controles */}
      <div className="flex items-center gap-3 px-6 py-3 border-b border-border flex-wrap">
        {loadingLotes ? (
          <Skeleton className="h-9 w-64" />
        ) : (
          <SelectorLote lotes={lotesArray} value={loteId} onChange={setLoteId} />
        )}

        <div className="flex items-center gap-2">
          <span className="text-xs text-muted-foreground">Desde</span>
          <input
            type="date"
            value={desde}
            max={hasta}
            onChange={e => setDesde(e.target.value)}
            className="h-9 px-3 rounded-md border border-input bg-card text-sm
              focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
          />
          <span className="text-xs text-muted-foreground">hasta</span>
          <input
            type="date"
            value={hasta}
            min={desde}
            max={hoy}
            onChange={e => setHasta(e.target.value)}
            className="h-9 px-3 rounded-md border border-input bg-card text-sm
              focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
          />
        </div>

        <div className="flex items-center gap-2">
          <span className="text-xs text-muted-foreground">Precio venta</span>
          <input
            type="number"
            value={precioVenta}
            min={1000}
            step={100}
            onChange={e => setPrecioVenta(Number(e.target.value))}
            className="h-9 w-28 px-3 rounded-md border border-input bg-card text-sm
              focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
          />
          <span className="text-xs text-muted-foreground">COP/kg</span>
        </div>
      </div>

      {/* Contenido */}
      <div className="flex-1 overflow-y-auto p-6">
        {!loteId ? (
          <EmptyState
            icon={<BarChart3 className="w-5 h-5" />}
            title="Selecciona un lote"
            description="Elige un lote activo para calcular sus indicadores productivos en el período seleccionado."
          />
        ) : loadingResumen ? (
          <div className="space-y-4">
            <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
              {Array.from({ length: 4 }).map((_, i) => <Skeleton key={i} className="h-24 rounded-lg" />)}
            </div>
            <Skeleton className="h-72 rounded-lg" />
            <Skeleton className="h-48 rounded-lg" />
          </div>
        ) : !resumen ? (
          <EmptyState
            icon={<BarChart3 className="w-5 h-5" />}
            title="Sin datos"
            description="No hay información para el período seleccionado."
          />
        ) : (
          <div className="space-y-6">
            {/* KPIs del lote */}
            <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
              <StatCard
                label="GMD promedio"
                value={fmt.kgDia(resumen.gmdPromedioKgDia)}
                icon={<TrendingUp className="w-4 h-4" />}
                delta={resumen.gmdPromedioKgDia >= 0.8 ? 'Sobre umbral mínimo' : 'Bajo umbral mínimo'}
                deltaPositive={resumen.gmdPromedioKgDia >= 0.8}
              />
              <StatCard
                label="ICA promedio"
                value={fmt.decimal(resumen.icaPromedio)}
                icon={<BarChart3 className="w-4 h-4" />}
                delta={resumen.icaPromedio <= 8 ? 'Dentro del rango' : 'Por encima del máximo'}
                deltaPositive={resumen.icaPromedio <= 8}
              />
              <StatCard
                label="Costo total alimento"
                value={fmt.cop(resumen.costoTotalAlimento)}
                icon={<DollarSign className="w-4 h-4" />}
              />
              <StatCard
                label="Rentabilidad proyectada"
                value={fmt.cop(resumen.rentabilidadProyectadaTotal)}
                icon={<TrendingUp className="w-4 h-4" />}
                delta={resumen.rentabilidadProyectadaTotal >= 0 ? 'Positiva' : 'Negativa'}
                deltaPositive={resumen.rentabilidadProyectadaTotal >= 0}
                className={resumen.rentabilidadProyectadaTotal < 0 ? 'border-rose-500/30' : ''}
              />
            </div>

            {/* Alerta de ineficientes */}
            {resumen.animalesIneficientes > 0 && (
              <div className="flex items-center gap-3 px-4 py-3 rounded-lg border border-rose-500/30 bg-rose-500/5">
                <AlertTriangle className="w-4 h-4 text-rose-400 flex-shrink-0" />
                <p className="text-xs text-rose-400">
                  <strong>{resumen.animalesIneficientes} animal{resumen.animalesIneficientes > 1 ? 'es' : ''}</strong>
                  {' '}por debajo de los umbrales productivos mínimos (GMD &lt; 0.8 kg/día o ICA &gt; 8).
                  Considera revisar la ración o reclasificar.
                </p>
              </div>
            )}

            {/* Gráfico GMD por animal */}
            {datosGmd.length > 0 && (
              <Card>
                <CardHeader>
                  <div className="flex items-center justify-between">
                    <CardTitle>GMD por animal</CardTitle>
                    <div className="flex items-center gap-3 text-[10px] text-muted-foreground">
                      <span className="flex items-center gap-1">
                        <span className="w-3 h-0.5 bg-emerald-400 inline-block rounded" />GMD eficiente
                      </span>
                      <span className="flex items-center gap-1">
                        <span className="w-3 h-0.5 bg-rose-400 inline-block rounded" />GMD bajo umbral
                      </span>
                    </div>
                  </div>
                </CardHeader>
                <CardContent>
                  <ResponsiveContainer width="100%" height={240}>
                    <BarChart data={datosGmd} margin={{ top: 4, right: 4, bottom: 0, left: -20 }}>
                      <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" strokeOpacity={0.5} vertical={false} />
                      <XAxis dataKey="codigo" tick={{ fontSize: 9, fill: 'hsl(var(--muted-foreground))' }} tickLine={false} axisLine={false} />
                      <YAxis tick={{ fontSize: 10, fill: 'hsl(var(--muted-foreground))' }} tickLine={false} axisLine={false}
                        tickFormatter={v => `${v.toFixed(2)}`} />
                      <Tooltip content={<GmdTooltip />} />
                      <ReferenceLine y={0.8} stroke={CHART_COLORS.danger} strokeDasharray="4 2" strokeOpacity={0.7}
                        label={{ value: 'Mín 0.8', position: 'right', fontSize: 9, fill: CHART_COLORS.danger }} />
                      <Bar dataKey="GMD" radius={[4, 4, 0, 0]}>
                        {datosGmd.map((entry, index) => (
                          <Cell
                            key={index}
                            fill={entry.esIneficiente ? CHART_COLORS.danger : CHART_COLORS.primary}
                            opacity={0.85}
                          />
                        ))}
                      </Bar>
                    </BarChart>
                  </ResponsiveContainer>
                </CardContent>
              </Card>
            )}

            {/* Gráfico dispersión GMD vs ICA */}
            {datosGmd.length > 0 && (
              <Card>
                <CardHeader>
                  <CardTitle>Eficiencia: GMD vs ICA</CardTitle>
                </CardHeader>
                <CardContent>
                  <p className="text-[10px] text-muted-foreground mb-3">
                    Cuadrante ideal: GMD alto (derecha) + ICA bajo (abajo). Los puntos rojos son animales ineficientes.
                  </p>
                  <ResponsiveContainer width="100%" height={220}>
                    <ScatterChart margin={{ top: 4, right: 4, bottom: 0, left: -20 }}>
                      <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" strokeOpacity={0.5} />
                      <XAxis dataKey="gmd" name="GMD" type="number"
                        tick={{ fontSize: 10, fill: 'hsl(var(--muted-foreground))' }} tickLine={false} axisLine={false}
                        label={{ value: 'GMD (kg/día)', position: 'insideBottom', offset: -2, fontSize: 9, fill: 'hsl(var(--muted-foreground))' }} />
                      <YAxis dataKey="ica" name="ICA" type="number"
                        tick={{ fontSize: 10, fill: 'hsl(var(--muted-foreground))' }} tickLine={false} axisLine={false} />
                      <Tooltip content={<GmdTooltip />} />
                      <ReferenceLine x={0.8} stroke={CHART_COLORS.danger} strokeDasharray="4 2" strokeOpacity={0.5} />
                      <ReferenceLine y={8} stroke={CHART_COLORS.danger} strokeDasharray="4 2" strokeOpacity={0.5} />
                      <Scatter
                        data={datosGmd}
                        fill={CHART_COLORS.primary}
                        shape={(props: any) => {
                          const { cx, cy, payload } = props
                          return (
                            <circle
                              cx={cx} cy={cy} r={5}
                              fill={payload.esIneficiente ? CHART_COLORS.danger : CHART_COLORS.primary}
                              opacity={0.8}
                            />
                          )
                        }}
                      />
                    </ScatterChart>
                  </ResponsiveContainer>
                </CardContent>
              </Card>
            )}

            {/* Tabla detallada */}
            {resumen.indicadores.length > 0 && (
              <TablaIndicadores indicadores={resumen.indicadores} />
            )}

            {/* Resumen de consumo */}
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
              <Card className="p-5">
                <p className="text-[10px] font-medium text-muted-foreground uppercase tracking-wide mb-1">Consumo total alimento</p>
                <p className="text-xl font-bold tabular-nums">{fmt.kg(resumen.consumoTotalKg)}</p>
              </Card>
              <Card className="p-5">
                <p className="text-[10px] font-medium text-muted-foreground uppercase tracking-wide mb-1">Costo/kg ganado promedio</p>
                <p className="text-xl font-bold tabular-nums">{fmt.cop(resumen.costoPorKgGanadoPromedio)}</p>
              </Card>
              <Card className="p-5">
                <p className="text-[10px] font-medium text-muted-foreground uppercase tracking-wide mb-1">Total animales analizados</p>
                <p className="text-xl font-bold tabular-nums">{resumen.totalAnimales}</p>
                <p className="text-xs text-muted-foreground mt-0.5">
                  {resumen.animalesIneficientes} ineficientes
                </p>
              </Card>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
