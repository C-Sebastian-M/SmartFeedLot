import {
  AreaChart, Area, XAxis, YAxis, CartesianGrid,
  Tooltip, ResponsiveContainer, ReferenceLine
} from 'recharts'
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from '@/components/ui'
import { CHART_COLORS } from '@/utils'

// Datos de ejemplo — en producción vienen de useResumenLote
const mockData = [
  { dia: 'Día 0', gmd: 0 },
  { dia: 'Día 7', gmd: 0.92 },
  { dia: 'Día 14', gmd: 1.05 },
  { dia: 'Día 21', gmd: 1.18 },
  { dia: 'Día 28', gmd: 1.31 },
  { dia: 'Día 35', gmd: 1.24 },
  { dia: 'Día 42', gmd: 1.38 },
  { dia: 'Día 49', gmd: 1.42 },
  { dia: 'Día 56', gmd: 1.35 },
  { dia: 'Día 63', gmd: 1.51 },
]

const CustomTooltip = ({ active, payload, label }: any) => {
  if (!active || !payload?.length) return null
  return (
    <div className="rounded-lg border border-border bg-card px-3 py-2 shadow-lg text-xs">
      <p className="font-medium mb-1">{label}</p>
      <p className="text-emerald-400">GMD: {payload[0]?.value?.toFixed(3)} kg/día</p>
    </div>
  )
}

export function GmdChart() {
  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <div>
            <CardTitle>Evolución GMD</CardTitle>
            <CardDescription className="mt-1">Ganancia Media Diaria — últimos 63 días</CardDescription>
          </div>
          <div className="flex items-center gap-4 text-xs text-muted-foreground">
            <span className="flex items-center gap-1.5">
              <span className="w-3 h-0.5 bg-emerald-400 inline-block rounded" />
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
          <AreaChart data={mockData} margin={{ top: 4, right: 4, bottom: 0, left: -20 }}>
            <defs>
              <linearGradient id="gmdGradient" x1="0" y1="0" x2="0" y2="1">
                <stop offset="5%" stopColor={CHART_COLORS.primary} stopOpacity={0.15} />
                <stop offset="95%" stopColor={CHART_COLORS.primary} stopOpacity={0} />
              </linearGradient>
            </defs>
            <CartesianGrid
              strokeDasharray="3 3"
              stroke="hsl(var(--border))"
              strokeOpacity={0.5}
            />
            <XAxis
              dataKey="dia"
              tick={{ fontSize: 10, fill: 'hsl(var(--muted-foreground))' }}
              tickLine={false}
              axisLine={false}
            />
            <YAxis
              tick={{ fontSize: 10, fill: 'hsl(var(--muted-foreground))' }}
              tickLine={false}
              axisLine={false}
              domain={[0, 1.8]}
              tickFormatter={(v) => `${v}`}
            />
            <Tooltip content={<CustomTooltip />} />
            {/* Línea de referencia del umbral mínimo productivo */}
            <ReferenceLine
              y={0.8}
              stroke={CHART_COLORS.danger}
              strokeDasharray="4 2"
              strokeOpacity={0.6}
              label={{ value: 'Mín', position: 'right', fontSize: 9, fill: CHART_COLORS.danger }}
            />
            <Area
              type="monotone"
              dataKey="gmd"
              stroke={CHART_COLORS.primary}
              strokeWidth={2}
              fill="url(#gmdGradient)"
              dot={{ r: 3, fill: CHART_COLORS.primary, strokeWidth: 0 }}
              activeDot={{ r: 5, fill: CHART_COLORS.primary, strokeWidth: 0 }}
            />
          </AreaChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  )
}
