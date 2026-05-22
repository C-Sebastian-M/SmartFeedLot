import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { animalsService, lotesService, analiticaService } from '@/services/feedlot.service'

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
    },
  })
}

export function useRegistrarPesaje() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ animalId, ...payload }: { animalId: string } & Parameters<typeof animalsService.registrarPesaje>[1]) =>
      animalsService.registrarPesaje(animalId, payload),
    onSuccess: (_data, variables) => {
      qc.invalidateQueries({ queryKey: queryKeys.animals.detail(variables.animalId) })
    },
  })
}

export function useRegistrarEventoSanitario() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ animalId, ...payload }: { animalId: string } & Parameters<typeof animalsService.registrarEventoSanitario>[1]) =>
      animalsService.registrarEventoSanitario(animalId, payload),
    onSuccess: (_data, variables) => {
      qc.invalidateQueries({ queryKey: queryKeys.animals.detail(variables.animalId) })
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

// ─── Analítica ────────────────────────────────────────────────────────────────
export function useResumenLote(params: Parameters<typeof analiticaService.getResumenLote>[0]) {
  return useQuery({
    queryKey: queryKeys.analitica.resumenLote(params.loteId, params),
    queryFn: () => analiticaService.getResumenLote(params),
    enabled: Boolean(params.loteId && params.desde && params.hasta),
    staleTime: 60_000,
  })
}

export function useAnimalesIneficientes(params: Parameters<typeof analiticaService.getAnimalesIneficientes>[0]) {
  return useQuery({
    queryKey: queryKeys.analitica.ineficientes(params),
    queryFn: () => analiticaService.getAnimalesIneficientes(params),
    enabled: Boolean(params.desde && params.hasta),
    staleTime: 60_000,
  })
}
