import { format, subDays } from 'date-fns'
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid,
  Tooltip, ResponsiveContainer, ReferenceLine, Cell,
} from 'recharts'
import { TrendingUp } from 'lucide-react'
import { useLotes, useResumenLote } from '@/hooks/useFeedlot'
import { Card, CardHeader, CardTitle, CardDescription, CardContent, Skeleton, EmptyState } from '@/components/ui'
import { CHART_COLORS } from '@/utils'
import type { LoteResumen } from '@/types'

const hoy = format(new Date(), 'yyyy-MM-dd')
const hace60 = format(subDays(new Date(), 60), 'yyyy-MM-dd')

const GMD_MIN = 0.8 // umbral mínimo productivo (kg/día)

const CustomTooltip = ({ active, payload, label }: any) => {
  if (!active || !payload?.length) return null
  const val: number = payload[0]?.value ?? 0
  return (
    <div className="rounded-lg border border-border bg-card px-3 py-2 shadow-lg text-xs">
      <p className="font-medium mb-1 truncate max-w-[160px]">{label}</p>
      <p style={{ color: val >= GMD_MIN ? CHART_COLORS.primary : CHART_COLORS.danger }}>
        GMD: {val.toFixed(3)} kg/día
      </p>
      {val < GMD_MIN && (
        <p className="text-rose-400 mt-0.5">⚠ Por debajo del umbral ({GMD_MIN} kg/día)</p>
      )}
    </div>
  )
}

function GmdChartInner({ lote }: { lote: LoteResumen }) {
  const { data: resumen, isLoading } = useResumenLote({
    loteId: lote.id,
    desde: hace60,
    hasta: hoy,
  })

  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Evolución GMD</CardTitle>
          <CardDescription className="mt-1">Cargando datos del lote {lote.codigo}…</CardDescription>
        </CardHeader>
        <CardContent>
          <Skeleton className="h-[220px] w-full" />
        </CardContent>
      </Card>
    )
  }

  const indicadores = resumen?.indicadores ?? []

  if (!indicadores.length) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>GMD por Animal</CardTitle>
          <CardDescription className="mt-1">Lote {lote.codigo} — últimos 60 días</CardDescription>
        </CardHeader>
        <CardContent>
          <EmptyState
            icon={<TrendingUp className="w-5 h-5" />}
            title="Sin datos de GMD"
            description="Registra pesajes en los animales del lote para ver métricas productivas."
          />
        </CardContent>
      </Card>
    )
  }

  const chartData = indicadores.map(ind => ({
    animal: ind.codigoAnimal,
    gmd: parseFloat(ind.gmd.toFixed(3)),
  }))

  const promedio = resumen!.gmdPromedioKgDia

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between flex-wrap gap-2">
          <div>
            <CardTitle>GMD por Animal</CardTitle>
            <CardDescription className="mt-1">
              Lote {lote.codigo} — {lote.animalesActuales} animales · Promedio {promedio.toFixed(3)} kg/día
            </CardDescription>
          </div>
          <div className="flex items-center gap-4 text-xs text-muted-foreground">
            <span className="flex items-center gap-1.5">
              <span className="w-3 h-3 rounded-sm inline-block" style={{ background: CHART_COLORS.primary }} />
              GMD real
            </span>
            <span className="flex items-center gap-1.5">
              <span className="w-3 h-0.5 bg-rose-400 inline-block rounded border-dashed" />
              Umbral mínimo
            </span>
          </div>
        </div>
      </CardHeader>
      <CardContent>
        <ResponsiveContainer width="100%" height={220}>
          <BarChart data={chartData} margin={{ top: 4, right: 4, bottom: 0, left: -20 }}>
            <CartesianGrid
              strokeDasharray="3 3"
              stroke="hsl(var(--border))"
              strokeOpacity={0.5}
            />
            <XAxis
              dataKey="animal"
              tick={{ fontSize: 9, fill: 'hsl(var(--muted-foreground))' }}
              tickLine={false}
              axisLine={false}
              interval={0}
              angle={chartData.length > 10 ? -45 : 0}
              textAnchor={chartData.length > 10 ? 'end' : 'middle'}
              height={chartData.length > 10 ? 40 : 20}
            />
            <YAxis
              tick={{ fontSize: 10, fill: 'hsl(var(--muted-foreground))' }}
              tickLine={false}
              axisLine={false}
              domain={[0, 'auto']}
              tickFormatter={(v) => `${v}`}
            />
            <Tooltip content={<CustomTooltip />} />
            <ReferenceLine
              y={GMD_MIN}
              stroke={CHART_COLORS.danger}
              strokeDasharray="4 2"
              strokeOpacity={0.7}
              label={{ value: 'Mín', position: 'right', fontSize: 9, fill: CHART_COLORS.danger }}
            />
            <Bar dataKey="gmd" radius={[3, 3, 0, 0]}>
              {chartData.map((entry, i) => (
                <Cell
                  key={i}
                  fill={entry.gmd >= GMD_MIN ? CHART_COLORS.primary : CHART_COLORS.danger}
                  fillOpacity={0.85}
                />
              ))}
            </Bar>
          </BarChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  )
}

export function GmdChart() {
  const { data: lotes, isLoading } = useLotes(true)
  const lotesArray = (lotes as LoteResumen[] | undefined) ?? []
  const primerLote = lotesArray[0]

  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>GMD por Animal</CardTitle>
          <CardDescription className="mt-1">Cargando lotes…</CardDescription>
        </CardHeader>
        <CardContent>
          <Skeleton className="h-[220px] w-full" />
        </CardContent>
      </Card>
    )
  }

  if (!primerLote) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>GMD por Animal</CardTitle>
          <CardDescription className="mt-1">Ganancia Media Diaria por animal</CardDescription>
        </CardHeader>
        <CardContent>
          <EmptyState
            icon={<TrendingUp className="w-5 h-5" />}
            title="Sin lotes activos"
            description="Crea y activa un lote para ver métricas de GMD."
          />
        </CardContent>
      </Card>
    )
  }

  return <GmdChartInner lote={primerLote} />
}
