import { useState } from 'react'
import { Search, Plus, Beef } from 'lucide-react'
import { useAnimals } from '@/hooks/useFeedlot'
import {
  PageHeader, Button, Input, Badge, Card, CardContent,
  Skeleton, EmptyState
} from '@/components/ui'
import { fmt, estadoProductivoColor, estadoSanitarioColor } from '@/utils'
import type { AnimalResumen } from '@/types'

export default function AnimalesPage() {
  const [busqueda, setBusqueda] = useState('')
  const [estadoProductivo, setEstadoProductivo] = useState<string>()

  const { data, isLoading } = useAnimals({
    page: 1,
    pageSize: 50,
    busqueda: busqueda || undefined,
    estadoProductivo,
  })

  return (
    <div className="flex flex-col h-full animate-fade-in">
      <PageHeader
        title="Animales"
        description={`${data?.totalCount ?? 0} animales registrados`}
        action={
          <Button size="sm">
            <Plus className="w-3.5 h-3.5" />
            Registrar animal
          </Button>
        }
      />

      {/* Filtros */}
      <div className="flex items-center gap-3 px-6 py-3 border-b border-border">
        <div className="relative flex-1 max-w-xs">
          <Search className="absolute left-2.5 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-muted-foreground" />
          <Input
            placeholder="Buscar por código o arete..."
            className="pl-8 h-8 text-xs"
            value={busqueda}
            onChange={(e) => setBusqueda(e.target.value)}
          />
        </div>
        {['EnEngorde', 'Vendido', 'Muerto', 'Retirado'].map((estado) => (
          <Button
            key={estado}
            variant={estadoProductivo === estado ? 'default' : 'outline'}
            size="sm"
            onClick={() => setEstadoProductivo(estadoProductivo === estado ? undefined : estado)}
          >
            {estado}
          </Button>
        ))}
      </div>

      {/* Tabla */}
      <div className="flex-1 overflow-y-auto p-6">
        {isLoading ? (
          <div className="space-y-2">
            {Array.from({ length: 8 }).map((_, i) => (
              <Skeleton key={i} className="h-14 w-full rounded-lg" />
            ))}
          </div>
        ) : !data?.items.length ? (
          <EmptyState
            icon={<Beef className="w-5 h-5" />}
            title="Sin animales"
            description="Registra el primer animal para comenzar el seguimiento productivo."
            action={
              <Button size="sm">
                <Plus className="w-3.5 h-3.5" />
                Registrar animal
              </Button>
            }
          />
        ) : (
          <Card>
            <CardContent className="p-0">
              <div className="overflow-x-auto">
                <table className="w-full text-xs">
                  <thead>
                    <tr className="border-b border-border">
                      {['Código', 'Arete', 'Raza', 'Sexo', 'Peso actual', 'Días engorde', 'Estado prod.', 'Estado san.'].map((h) => (
                        <th key={h} className="text-left px-4 py-3 text-muted-foreground font-medium uppercase tracking-wide text-[10px]">
                          {h}
                        </th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {(data.items as AnimalResumen[]).map((animal) => (
                      <tr
                        key={animal.id}
                        className="border-b border-border/50 hover:bg-secondary/30 transition-colors cursor-pointer"
                      >
                        <td className="px-4 py-3 font-mono font-medium">{animal.codigoIdentificacion}</td>
                        <td className="px-4 py-3 text-muted-foreground">{animal.numeroArete}</td>
                        <td className="px-4 py-3">{animal.raza}</td>
                        <td className="px-4 py-3 text-muted-foreground">{animal.sexo}</td>
                        <td className="px-4 py-3 tabular-nums font-medium">{fmt.kg(animal.pesoActualKg)}</td>
                        <td className="px-4 py-3 tabular-nums text-muted-foreground">{animal.diasEnEngorde}d</td>
                        <td className="px-4 py-3">
                          <Badge className={estadoProductivoColor[animal.estadoProductivo]}>
                            {animal.estadoProductivo}
                          </Badge>
                        </td>
                        <td className="px-4 py-3">
                          <Badge className={estadoSanitarioColor[animal.estadoSanitario]}>
                            {animal.estadoSanitario}
                          </Badge>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  )
}
