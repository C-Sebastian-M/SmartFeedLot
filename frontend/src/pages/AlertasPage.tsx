import { useState } from 'react'
import { format, subDays } from 'date-fns'
import { AlertTriangle, TrendingDown, Beef, ChevronDown, Syringe, Landmark } from 'lucide-react'
import { useLotes, useAnimalesIneficientes, useVacunasProximas, useEtapasInversion } from '@/hooks/useFeedlot'
import {
  PageHeader, Card, CardContent, CardHeader, CardTitle, Badge, Skeleton, EmptyState,
} from '@/components/ui'
import { fmt } from '@/utils'
import type { AnimalIneficiente, LoteResumen, VacunaProxima, EtapaInversion } from '@/types'

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

  const { data: vacunasProximas, isLoading: loadingVacunas } = useVacunasProximas()
  const { data: etapas } = useEtapasInversion()
  const itemsPendientes = ((etapas as EtapaInversion[] | undefined) ?? [])
    .flatMap(e => e.items.filter(i => i.estado === 'Pendiente').map(i => ({ ...i, etapaNombre: e.nombre, etapaNumero: e.numero })))

  const alertas = (ineficientes as AnimalIneficiente[] | undefined) ?? []
  const vacunas = (vacunasProximas as VacunaProxima[] | undefined) ?? []

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
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
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
              <CardContent className="p-0 overflow-x-auto">
                <table className="w-full text-xs">
                  <thead>
                    <tr className="border-b border-border">
                      {['#', 'Código', 'Nombre', 'Raza', 'Lote', 'GMD real', 'GMD mín.', 'ICA real', 'ICA máx.', 'Días', 'Motivo', 'Alerta'].map(h => (
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
                          <td className="px-4 py-3 text-muted-foreground">{animal.nombreAnimal || '-'}</td>
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

        {/* Ítems de inversión pendientes (RF-051) */}
        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <Landmark className="w-4 h-4 text-amber-400" />
                <CardTitle>Ítems de inversión pendientes ({itemsPendientes.length})</CardTitle>
              </div>
            </div>
          </CardHeader>
          <CardContent>
            {!itemsPendientes.length ? (
              <EmptyState icon={<Landmark className="w-4 h-4" />} title="Sin ítems pendientes" description="Todos los ítems de inversión están completos." />
            ) : (
              <div className="overflow-x-auto max-h-64 overflow-y-auto">
                <table className="w-full text-xs">
                  <thead>
                    <tr className="border-b border-border">
                      {['Etapa', 'Producto', 'Costo', 'Avance'].map(h => (
                        <th key={h} className="text-left px-3 py-2 text-muted-foreground font-medium uppercase tracking-wide text-[9px]">{h}</th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {itemsPendientes.map(item => (
                      <tr key={item.id} className="border-b border-border/30 hover:bg-secondary/20">
                        <td className="px-3 py-2">
                          <Badge className="bg-amber-500/10 text-amber-400 border-amber-500/20 text-[9px]">
                            #{item.etapaNumero} {item.etapaNombre}
                          </Badge>
                        </td>
                        <td className="px-3 py-2 font-medium">{item.producto}</td>
                        <td className="px-3 py-2 tabular-nums text-muted-foreground">{fmt.cop(item.monto)}</td>
                        <td className="px-3 py-2">
                          <div className="flex items-center gap-2">
                            <div className="w-16 h-1.5 rounded-full bg-secondary">
                              <div className="h-full rounded-full bg-amber-400"
                                style={{ width: `${item.porcentajeAvance}%` }} />
                            </div>
                            <span className="tabular-nums text-muted-foreground">{item.porcentajeAvance}%</span>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Vacunas próximas */}
        <Card>
          <CardHeader>
            <div className="flex items-center gap-2">
              <Syringe className="w-4 h-4 text-blue-400" />
              <CardTitle>Vacunas próximas ({vacunas.length})</CardTitle>
            </div>
          </CardHeader>
          <CardContent>
            {loadingVacunas ? (
              <Skeleton className="h-20" />
            ) : !vacunas.length ? (
              <EmptyState icon={<Syringe className="w-4 h-4" />} title="Sin vacunas pendientes" description="No hay vacunas con próxima dosis en los próximos 15 días." />
            ) : (
              <div className="space-y-2 max-h-64 overflow-y-auto">
                {vacunas.map(v => (
                  <div key={`${v.animalId}-${v.proximaDosis}`}
                    className="p-2.5 rounded-lg border border-blue-500/20 bg-blue-500/5">
                    <div className="flex items-center justify-between mb-1">
                      <span className="text-xs font-mono font-medium">{v.codigoAnimal}</span>
                      <Badge className="bg-blue-500/10 text-blue-400 border-blue-500/20 text-[10px]">
                        {fmt.fecha(v.proximaDosis)}
                      </Badge>
                    </div>
                    <p className="text-[10px] text-muted-foreground leading-relaxed">
                      {v.nombreAnimal && <span className="mr-1">{v.nombreAnimal} · </span>}
                      {v.diagnostico}
                      {v.responsable && <span className="ml-1">· por {v.responsable}</span>}
                    </p>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
