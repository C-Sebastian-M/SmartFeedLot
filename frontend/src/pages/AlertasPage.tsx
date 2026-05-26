import { useState } from 'react'
import { format, subDays } from 'date-fns'
import { AlertTriangle, TrendingDown, Beef, ChevronDown } from 'lucide-react'
import { useLotes, useAnimalesIneficientes } from '@/hooks/useFeedlot'
import {
  PageHeader, Card, CardContent, Badge, Skeleton, EmptyState,
} from '@/components/ui'
import { fmt } from '@/utils'
import type { AnimalIneficiente, LoteResumen } from '@/types'

export default function AlertasPage() {
  const hoy = format(new Date(), 'yyyy-MM-dd')
  const hace30 = format(subDays(new Date(), 30), 'yyyy-MM-dd')

  const [loteId, setLoteId] = useState<string>('')
  const [desde, setDesde] = useState(hace30)
  const [hasta, setHasta] = useState(hoy)
  const [gmdMinima, setGmdMinima] = useState(0.8)
  const [icaMaxima, setIcaMaxima] = useState(8.0)

  const { data: lotes } = useLotes(true)
  const lotesArray = (lotes as LoteResumen[] | undefined) ?? []

  const { data: ineficientes, isLoading } = useAnimalesIneficientes({
    loteId: loteId || undefined,
    desde,
    hasta,
    gmdMinima,
    icaMaxima,
    precioVentaEstimadoPorKg: 5500,
  })

  const alertas = (ineficientes as AnimalIneficiente[] | undefined) ?? []

  const porSeveridad = {
    critico: alertas.filter(a => a.gmd < 0.5),
    alto: alertas.filter(a => a.gmd >= 0.5 && a.gmd < gmdMinima),
    ica: alertas.filter(a => a.gmd >= gmdMinima && a.ica > icaMaxima),
  }

  return (
    <div className="flex flex-col h-full animate-fade-in">
      <PageHeader
        title="Panel de alertas"
        description="Animales por debajo de los umbrales productivos mínimos"
      />

      {/* Filtros */}
      <div className="flex items-center gap-3 px-6 py-3 border-b border-border flex-wrap">
        {/* Selector lote */}
        <div className="relative">
          <select
            value={loteId}
            onChange={e => setLoteId(e.target.value)}
            className="h-9 pl-3 pr-8 rounded-md border border-input bg-card text-sm
              focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring
              appearance-none [&>option]:bg-card"
          >
            <option value="">Todos los lotes</option>
            {lotesArray.map(l => (
              <option key={l.id} value={l.id}>{l.codigo} — {l.nombre}</option>
            ))}
          </select>
          <ChevronDown className="absolute right-2 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-muted-foreground pointer-events-none" />
        </div>

        <div className="flex items-center gap-2">
          <span className="text-xs text-muted-foreground">Desde</span>
          <input type="date" value={desde} max={hasta}
            onChange={e => setDesde(e.target.value)}
            className="h-9 px-3 rounded-md border border-input bg-card text-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring" />
          <span className="text-xs text-muted-foreground">hasta</span>
          <input type="date" value={hasta} min={desde} max={hoy}
            onChange={e => setHasta(e.target.value)}
            className="h-9 px-3 rounded-md border border-input bg-card text-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring" />
        </div>

        <div className="flex items-center gap-2">
          <span className="text-xs text-muted-foreground">GMD mín.</span>
          <input type="number" value={gmdMinima} step={0.1} min={0.1} max={2}
            onChange={e => setGmdMinima(Number(e.target.value))}
            className="h-9 w-20 px-3 rounded-md border border-input bg-card text-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring" />
          <span className="text-xs text-muted-foreground">ICA máx.</span>
          <input type="number" value={icaMaxima} step={0.5} min={1} max={20}
            onChange={e => setIcaMaxima(Number(e.target.value))}
            className="h-9 w-20 px-3 rounded-md border border-input bg-card text-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring" />
        </div>
      </div>

      <div className="flex-1 overflow-y-auto p-6">
        {isLoading ? (
          <div className="space-y-2">
            {Array.from({ length: 6 }).map((_, i) => <Skeleton key={i} className="h-20 rounded-lg" />)}
          </div>
        ) : !alertas.length ? (
          <EmptyState
            icon={<TrendingDown className="w-5 h-5" />}
            title="Sin alertas productivas"
            description={`Todos los animales están sobre GMD ≥ ${gmdMinima} kg/día e ICA ≤ ${icaMaxima} en el período seleccionado.`}
          />
        ) : (
          <div className="space-y-6">
            {/* Resumen de conteos */}
            <div className="grid grid-cols-3 gap-4">
              <Card className="p-4 border-rose-500/30">
                <p className="text-[10px] text-muted-foreground uppercase tracking-wide mb-1">Crítico (GMD &lt; 0.5)</p>
                <p className="text-2xl font-bold text-rose-400 tabular-nums">{porSeveridad.critico.length}</p>
              </Card>
              <Card className="p-4 border-amber-500/30">
                <p className="text-[10px] text-muted-foreground uppercase tracking-wide mb-1">GMD bajo umbral</p>
                <p className="text-2xl font-bold text-amber-400 tabular-nums">{porSeveridad.alto.length}</p>
              </Card>
              <Card className="p-4 border-blue-500/30">
                <p className="text-[10px] text-muted-foreground uppercase tracking-wide mb-1">ICA sobre máximo</p>
                <p className="text-2xl font-bold text-blue-400 tabular-nums">{porSeveridad.ica.length}</p>
              </Card>
            </div>

            {/* Tabla de alertas */}
            <Card>
              <CardContent className="p-0">
                <table className="w-full text-xs">
                  <thead>
                    <tr className="border-b border-border">
                      {['#', 'Código', 'Raza', 'Lote', 'GMD real', 'GMD mín.', 'ICA real', 'ICA máx.', 'Días', 'Motivo', 'Alerta'].map(h => (
                        <th key={h} className="text-left px-4 py-3 text-muted-foreground font-medium uppercase tracking-wide text-[10px] whitespace-nowrap">
                          {h}
                        </th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {alertas.map((animal, i) => {
                      const esCritico = animal.gmd < 0.5
                      const rowClass = esCritico
                        ? 'bg-rose-500/5 border-rose-500/20'
                        : 'hover:bg-secondary/30'

                      return (
                        <tr key={animal.animalId}
                          className={`border-b border-border/40 transition-colors ${rowClass} ${i === alertas.length - 1 ? 'border-b-0' : ''}`}
                        >
                          <td className="px-4 py-3 text-muted-foreground tabular-nums">{i + 1}</td>
                          <td className="px-4 py-3 font-mono font-semibold">{animal.codigoAnimal}</td>
                          <td className="px-4 py-3 text-muted-foreground">{animal.raza}</td>
                          <td className="px-4 py-3">
                            <Badge className="bg-secondary border-border text-foreground font-mono text-[10px]">
                              {animal.loteCodigo}
                            </Badge>
                          </td>
                          <td className={`px-4 py-3 tabular-nums font-medium ${esCritico ? 'text-rose-400' : 'text-amber-400'}`}>
                            {fmt.kgDia(animal.gmd)}
                          </td>
                          <td className="px-4 py-3 tabular-nums text-muted-foreground">
                            {fmt.kgDia(animal.gmdMinimaEsperada)}
                          </td>
                          <td className={`px-4 py-3 tabular-nums ${animal.ica > icaMaxima && animal.ica > 0 ? 'text-blue-400 font-medium' : 'text-muted-foreground'}`}>
                            {animal.ica > 0 ? fmt.decimal(animal.ica) : '—'}
                          </td>
                          <td className="px-4 py-3 tabular-nums text-muted-foreground">
                            {fmt.decimal(animal.icaMaximaEsperada)}
                          </td>
                          <td className="px-4 py-3 tabular-nums text-muted-foreground">{animal.diasEnEngorde}d</td>
                          <td className="px-4 py-3 text-muted-foreground max-w-xs">
                            <span className="truncate block">{animal.motivoAlerta}</span>
                          </td>
                          <td className="px-4 py-3">
                            <Badge className={esCritico
                              ? 'bg-rose-500/10 text-rose-400 border-rose-500/20'
                              : 'bg-amber-500/10 text-amber-400 border-amber-500/20'}>
                              {esCritico ? 'Crítico' : 'Alerta'}
                            </Badge>
                          </td>
                        </tr>
                      )
                    })}
                  </tbody>
                </table>
              </CardContent>
            </Card>

            {/* Pie de tabla */}
            <p className="text-xs text-muted-foreground text-center">
              {alertas.length} animal{alertas.length !== 1 ? 'es' : ''} por debajo de los umbrales · ordenados por GMD ascendente (más críticos primero)
            </p>
          </div>
        )}
      </div>
    </div>
  )
}
