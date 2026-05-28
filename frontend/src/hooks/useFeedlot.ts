import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { animalsService, lotesService, analiticaService, costosService } from '@/services/feedlot.service'

// ─── Query Keys ───────────────────────────────────────────────────────────────
export const queryKeys = {
  animals: {
    all: ['animals'] as const,
    list: (params?: object) => ['animals', 'list', params] as const,
    detail: (id: string) => ['animals', 'detail', id] as const,
  },
  lotes: {
    all: ['lotes'] as const,
    list: (soloActivos?: boolean) => ['lotes', 'list', soloActivos] as const,
    detail: (id: string) => ['lotes', 'detail', id] as const,
  },
  analitica: {
    indicadores: (animalId: string, params: object) =>
      ['analitica', 'indicadores', animalId, params] as const,
    resumenLote: (loteId: string, params: object) =>
      ['analitica', 'resumen', loteId, params] as const,
    ineficientes: (params: object) => ['analitica', 'ineficientes', params] as const,
  },
  costos: {
    totalesLote: (loteId: string, params: object) =>
      ['costos', 'totales', loteId, params] as const,
    detalleLote: (loteId: string, params: object) =>
      ['costos', 'detalle', loteId, params] as const,
  },
}

// ─── Animals ──────────────────────────────────────────────────────────────────
export function useAnimals(params?: Parameters<typeof animalsService.getAll>[0]) {
  return useQuery({
    queryKey: queryKeys.animals.list(params),
    queryFn: () => animalsService.getAll(params),
    staleTime: 30_000,
  })
}

export function useAnimal(id: string) {
  return useQuery({
    queryKey: queryKeys.animals.detail(id),
    queryFn: () => animalsService.getById(id),
    enabled: Boolean(id),
    staleTime: 30_000,
  })
}

export function useCreateAnimal() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: animalsService.create,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.animals.all })
      qc.invalidateQueries({ queryKey: queryKeys.lotes.all })
    },
  })
}

export function useEliminarAnimal() {
  const qc = useQueryClient()
  const navigate = useNavigate()
  return useMutation({
    mutationFn: (id: string) => animalsService.eliminar(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.animals.all })
      qc.invalidateQueries({ queryKey: queryKeys.lotes.all })
      navigate('/animales')
    },
  })
}

export function useActualizarAnimal() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (input: { id: string; nombre?: string; numeroArete: string; sexo: string; raza?: string; fechaNacimiento?: string; fechaIngreso: string; pesoIngresoKg: number; precioCompra: number; moneda: string }) =>
      animalsService.actualizar(input.id, input),
    onSuccess: (_data, variables) => {
      qc.invalidateQueries({ queryKey: queryKeys.animals.detail(variables.id) })
      qc.invalidateQueries({ queryKey: queryKeys.animals.all })
      qc.invalidateQueries({ queryKey: queryKeys.lotes.all })
    },
  })
}

export function useRegistrarPesaje() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({
      animalId,
      ...payload
    }: { animalId: string } & Parameters<typeof animalsService.registrarPesaje>[1]) =>
      animalsService.registrarPesaje(animalId, payload),
    onSuccess: (_data, variables) => {
      qc.invalidateQueries({ queryKey: queryKeys.animals.detail(variables.animalId) })
      qc.invalidateQueries({ queryKey: queryKeys.animals.all })
    },
  })
}

export function useRegistrarEventoSanitario() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({
      animalId,
      ...payload
    }: { animalId: string } & Parameters<typeof animalsService.registrarEventoSanitario>[1]) =>
      animalsService.registrarEventoSanitario(animalId, payload),
    onSuccess: (_data, variables) => {
      qc.invalidateQueries({ queryKey: queryKeys.animals.detail(variables.animalId) })
    },
  })
}

export function useEliminarPesaje() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ animalId, pesajeId }: { animalId: string; pesajeId: string }) =>
      animalsService.eliminarPesaje(animalId, pesajeId),
    onSuccess: (_data, variables) => {
      qc.invalidateQueries({ queryKey: queryKeys.animals.detail(variables.animalId) })
      qc.invalidateQueries({ queryKey: queryKeys.animals.all })
    },
  })
}

// ─── Lotes ────────────────────────────────────────────────────────────────────
export function useLotes(soloActivos = false) {
  return useQuery({
    queryKey: queryKeys.lotes.list(soloActivos),
    queryFn: () => lotesService.getAll(soloActivos),
    staleTime: 30_000,
  })
}

export function useLote(id: string) {
  return useQuery({
    queryKey: queryKeys.lotes.detail(id),
    queryFn: () => lotesService.getById(id),
    enabled: Boolean(id),
    staleTime: 30_000,
  })
}

export function useCreateLote() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: lotesService.create,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.lotes.all })
    },
  })
}

export function useActivarLote() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (loteId: string) => lotesService.activar(loteId),
    onSuccess: (_data, loteId) => {
      qc.invalidateQueries({ queryKey: queryKeys.lotes.all })
      qc.invalidateQueries({ queryKey: queryKeys.lotes.detail(loteId) })
    },
  })
}

export function useCerrarLote() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (loteId: string) => lotesService.cerrar(loteId),
    onSuccess: (_data, loteId) => {
      qc.invalidateQueries({ queryKey: queryKeys.lotes.all })
      qc.invalidateQueries({ queryKey: queryKeys.lotes.detail(loteId) })
    },
  })
}

export function useMoverAnimal() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({
      loteId,
      ...payload
    }: { loteId: string } & Parameters<typeof lotesService.moverAnimal>[1]) =>
      lotesService.moverAnimal(loteId, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.lotes.all })
      qc.invalidateQueries({ queryKey: queryKeys.animals.all })
    },
  })
}

// ─── Analítica ────────────────────────────────────────────────────────────────
export function useResumenLote(
  params: Parameters<typeof analiticaService.getResumenLote>[0]
) {
  return useQuery({
    queryKey: queryKeys.analitica.resumenLote(params.loteId, params),
    queryFn: () => analiticaService.getResumenLote(params),
    enabled: Boolean(params.loteId && params.desde && params.hasta),
    staleTime: 60_000,
  })
}

export function useIndicadoresAnimal(
  params: Parameters<typeof analiticaService.getIndicadoresAnimal>[0]
) {
  return useQuery({
    queryKey: queryKeys.analitica.indicadores(params.animalId, params),
    queryFn: () => analiticaService.getIndicadoresAnimal(params),
    enabled: Boolean(params.animalId && params.loteId && params.desde && params.hasta),
    staleTime: 60_000,
  })
}

export function useAnimalesIneficientes(
  params: Parameters<typeof analiticaService.getAnimalesIneficientes>[0]
) {
  return useQuery({
    queryKey: queryKeys.analitica.ineficientes(params),
    queryFn: () => analiticaService.getAnimalesIneficientes(params),
    enabled: Boolean(params.desde && params.hasta),
    staleTime: 60_000,
  })
}

// ─── Costos ───────────────────────────────────────────────────────────────────
export function useCostosTotalesLote(params: {
  loteId: string
  desde: string
  hasta: string
}) {
  return useQuery({
    queryKey: queryKeys.costos.totalesLote(params.loteId, params),
    queryFn: () => costosService.getCosteoLote(params),
    enabled: Boolean(params.loteId && params.desde && params.hasta),
    staleTime: 60_000,
  })
}

export function useRegistrarCostoOperativo() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: costosService.registrarCosto,
    onSuccess: () => {
      // Invalidar todos los costos para refrescar el desglose
      qc.invalidateQueries({ queryKey: ['costos'] })
      // Invalidar analítica porque el costo afecta la rentabilidad
      qc.invalidateQueries({ queryKey: ['analitica'] })
    },
  })
}

export function useVacunasProximas(dias = 15) {
  return useQuery({
    queryKey: ['analitica', 'vacunas-proximas', dias],
    queryFn: () => analiticaService.getVacunasProximas(dias),
    staleTime: 60_000,
  })
}
