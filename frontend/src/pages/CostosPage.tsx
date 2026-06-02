import { useState } from 'react'
import { format, subDays } from 'date-fns'
import {
  DollarSign, Beef,
  Wrench, FileText, CalendarDays, Users
} from 'lucide-react'
import { useLotes, useCostosTotalesLote } from '@/hooks/useFeedlot'
import {
  PageHeader, Card, CardHeader, CardTitle, CardContent,
  Skeleton, EmptyState, StatCard, CustomSelect,
} from '@/components/ui'
import { fmt } from '@/utils'
import type { LoteResumen, CostoDetalle } from '@/types'

const hoy = format(new Date(), 'yyyy-MM-dd')
const hace30 = format(subDays(new Date(), 30), 'yyyy-MM-dd')

function SelectorLote({
  lotes, value, onChange,
}: {
  lotes: LoteResumen[]
  value: string
  onChange: (id: string) => void
}) {
  return (
    <CustomSelect
      value={value}
      onChange={onChange}
      placeholder="Seleccionar lote..."
      options={lotes.map(l => ({ value: l.id, label: `${l.codigo} — ${l.nombre} (${l.animalesActuales} animales)` }))}
    />
  )
}

function TablaDetalles({ detalles, titulo }: { detalles: CostoDetalle[]; titulo: string }) {
  if (!detalles.length) return null

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle>{titulo}</CardTitle>
          <span className="text-xs text-muted-foreground">{detalles.length} registro{detalles.length > 1 ? 's' : ''}</span>
        </div>
      </CardHeader>
      <CardContent className="p-0">
        <div className="overflow-x-auto">
          <table className="w-full text-xs">
            <thead>
              <tr className="border-b border-border">
                {['Concepto', 'Fecha', 'Monto', 'Observaciones'].map(h => (
                  <th key={h} className="text-left px-4 py-3 text-muted-foreground font-medium uppercase tracking-wide text-[10px] whitespace-nowrap">
                    {h}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {detalles.map((d, i) => (
                <tr key={d.id}
                  className={`border-b border-border/40 hover:bg-muted/20 transition-colors ${
                    i === detalles.length - 1 ? 'border-b-0' : ''
                  }`}
                >
                  <td className="px-4 py-2.5 font-medium">{d.concepto}</td>
                  <td className="px-4 py-2.5 text-muted-foreground tabular-nums">{fmt.fecha(d.fecha)}</td>
                  <td className="px-4 py-2.5 tabular-nums font-medium">{fmt.cop(d.monto)}</td>
                  <td className="px-4 py-2.5 text-muted-foreground">{d.observaciones ?? '—'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </CardContent>
    </Card>
  )
}

export default function CostosPage() {
  const [loteId, setLoteId] = useState('')
  const [desde, setDesde] = useState(hace30)
  const [hasta, setHasta] = useState(hoy)

  const { data: lotes, isLoading: loadingLotes } = useLotes()
  const lotesArray = (lotes as LoteResumen[] | undefined) ?? []

  const { data: costeo, isLoading: loadingCosteo } = useCostosTotalesLote({
    loteId,
    desde,
    hasta,
  })

  return (
    <div className="flex flex-col h-full animate-fade-in">
      <PageHeader
        title="Costos operativos"
        description="Registro y consulta de costos de mano de obra y CIF por lote"
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
      </div>

      {/* Contenido */}
      <div className="flex-1 overflow-y-auto p-6">
        {!loteId ? (
          <EmptyState
            icon={<DollarSign className="w-5 h-5" />}
            title="Selecciona un lote"
            description="Elige un lote para ver su desglose de costos operativos en el período seleccionado."
          />
        ) : loadingCosteo ? (
          <div className="space-y-4">
            <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
              {Array.from({ length: 4 }).map((_, i) => <Skeleton key={i} className="h-24 rounded-lg" />)}
            </div>
            <Skeleton className="h-48 rounded-lg" />
          </div>
        ) : !costeo ? (
          <EmptyState
            icon={<DollarSign className="w-5 h-5" />}
            title="Sin datos de costos"
            description="No hay costos registrados para este lote en el período seleccionado."

          />
        ) : (
          <div className="space-y-6">
            {/* KPIs */}
            <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
              <StatCard
                label="Alimento"
                value={fmt.cop(costeo.costoTotalAlimento)}
                sub={`${fmt.cop(costeo.costoAlimentoPorAnimal)}/animal`}
                icon={<Beef className="w-4 h-4" />}
              />
              <StatCard
                label="Mano de obra"
                value={fmt.cop(costeo.costoTotalManoDeObra)}
                sub={`${fmt.cop(costeo.costoManoDeObraPorAnimal)}/animal`}
                icon={<Wrench className="w-4 h-4" />}
              />
              <StatCard
                label="CIF"
                value={fmt.cop(costeo.costoTotalCif)}
                sub={`${fmt.cop(costeo.costoCifPorAnimal)}/animal`}
                icon={<FileText className="w-4 h-4" />}
              />
              <StatCard
                label="Costo total operativo"
                value={fmt.cop(costeo.costoOperativoTotal)}
                sub={`${fmt.cop(costeo.costoOperativoPorAnimal)}/animal · ${costeo.totalAnimales} animales`}
                icon={<DollarSign className="w-4 h-4" />}
                className="border-primary/30"
              />
            </div>

            {/* Consumo */}
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
              <Card className="p-5">
                <div className="flex items-center gap-2 text-muted-foreground mb-1">
                  <Users className="w-3.5 h-3.5" />
                  <p className="text-[10px] font-medium uppercase tracking-wide">Animales en lote</p>
                </div>
                <p className="text-xl font-bold tabular-nums">{costeo.totalAnimales}</p>
              </Card>
              <Card className="p-5">
                <div className="flex items-center gap-2 text-muted-foreground mb-1">
                  <CalendarDays className="w-3.5 h-3.5" />
                  <p className="text-[10px] font-medium uppercase tracking-wide">Período</p>
                </div>
                <p className="text-sm font-medium tabular-nums">
                  {fmt.fecha(costeo.desde)} — {fmt.fecha(costeo.hasta)}
                </p>
              </Card>
              <Card className="p-5">
                <div className="flex items-center gap-2 text-muted-foreground mb-1">
                  <Beef className="w-3.5 h-3.5" />
                  <p className="text-[10px] font-medium uppercase tracking-wide">Consumo total alimento</p>
                </div>
                <p className="text-xl font-bold tabular-nums">{fmt.kg(costeo.consumoTotalKg)}</p>
              </Card>
            </div>

            {/* Detalle Mano de Obra */}
            {costeo.detallesManoDeObra.length > 0 && (
              <TablaDetalles detalles={costeo.detallesManoDeObra} titulo="Detalle — Mano de obra" />
            )}

            {/* Detalle CIF */}
            {costeo.detallesCif.length > 0 && (
              <TablaDetalles detalles={costeo.detallesCif} titulo="Detalle — CIF" />
            )}
          </div>
        )}
      </div>

    </div>
  )
}
