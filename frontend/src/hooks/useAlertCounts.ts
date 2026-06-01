import { format, subDays } from 'date-fns'
import { useAnimalesIneficientes, usePrestamos, useEtapasInversion } from './useFeedlot'
import type { Prestamo, EtapaInversion } from '@/types'

/**
 * Agrega los conteos de alerta globales para mostrar badges en el sidebar.
 * Usa las mismas query keys que las páginas → React Query deduplica las peticiones.
 */
export function useAlertCounts() {
  const hoy = format(new Date(), 'yyyy-MM-dd')
  const hace30 = format(subDays(new Date(), 30), 'yyyy-MM-dd')
  const todayDate = new Date()
  todayDate.setHours(0, 0, 0, 0)

  const { data: ineficientes } = useAnimalesIneficientes({
    desde: hace30,
    hasta: hoy,
    gmdMinima: 0.8,
    icaMaxima: 8,
    precioVentaEstimadoPorKg: 5500,
  })

  const { data: prestamosData } = usePrestamos()
  const { data: etapasData } = useEtapasInversion()

  const animales = (ineficientes as any[] | undefined)?.length ?? 0

  const cuotasVencidas = ((prestamosData as Prestamo[] | undefined) ?? [])
    .flatMap(p => p.cuotas)
    .filter(c => {
      if (c.pagada) return false
      const [y, m, d] = c.fechaVencimiento.split('-').map(Number)
      return new Date(y, m - 1, d) < todayDate
    }).length

  const itemsPendientes = ((etapasData as EtapaInversion[] | undefined) ?? [])
    .flatMap(e => e.items)
    .filter(i => i.estado === 'Pendiente').length

  return {
    /** Animales con GMD o ICA fuera de umbral (últimos 30 días) */
    animales,
    /** Cuotas de préstamo con fecha de vencimiento pasada y sin pagar */
    cuotasVencidas,
    /** Ítems de inversión en estado Pendiente */
    itemsPendientes,
    /** Suma de las tres fuentes — para el badge principal de Alertas */
    total: animales + cuotasVencidas + itemsPendientes,
  }
}
