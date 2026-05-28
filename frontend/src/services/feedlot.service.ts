import api from './api'
import type {
  Animal, AnimalResumen, PagedResult,
  IndicadorProductivo, ResumenLote, AnimalIneficiente,
  LoginResponse, Racion, Lote, LoteResumen,
  CosteoLote,
} from '@/types'

// ─── Animals ──────────────────────────────────────────────────────────────────
export const animalsService = {
  getAll: async (params?: {
    page?: number
    pageSize?: number
    estadoProductivo?: string
    estadoSanitario?: string
    raza?: string
    busqueda?: string
  }): Promise<PagedResult<AnimalResumen>> => {
    const { data } = await api.get('/animals', { params })
    return data
  },

  getById: async (id: string): Promise<Animal> => {
    const { data } = await api.get(`/animals/${id}`)
    return data
  },

  create: async (payload: {
    nombre?: string
    sexo: string
    raza?: string
    fechaNacimiento?: string
    pesoIngresoKg: number
    precioCompraPorKg: number
    moneda: string
    fechaIngreso: string
    loteInicialId?: string
  }): Promise<string> => {
    const { data } = await api.post('/animals', payload)
    return data
  },

  eliminarPesaje: async (animalId: string, pesajeId: string): Promise<void> => {
    await api.delete(`/animals/${animalId}/pesajes/${pesajeId}`)
  },

  registrarPesaje: async (
    animalId: string,
    payload: { fechaPesaje: string; pesoKg: number; observaciones?: string }
  ): Promise<string> => {
    const { data } = await api.post(`/animals/${animalId}/pesajes`, payload)
    return data
  },

  registrarEventoSanitario: async (
    animalId: string,
    payload: {
      fechaEvento: string
      diagnostico: string
      descripcion: string
      severidad: string
      tratamiento?: string
    }
  ): Promise<string> => {
    const { data } = await api.post(`/animals/${animalId}/eventos-sanitarios`, payload)
    return data
  },

  actualizar: async (id: string, payload: {
    nombre?: string
    numeroArete: string
    sexo: string
    raza?: string
    fechaNacimiento?: string
    pesoIngresoKg: number
    precioCompra: number
    moneda: string
  }): Promise<void> => {
    await api.put(`/animals/${id}`, payload)
  },
}

// ─── Lotes ────────────────────────────────────────────────────────────────────
export const lotesService = {
  getAll: async (soloActivos = false): Promise<LoteResumen[]> => {
    const { data } = await api.get('/lotes', { params: { soloActivos } })
    return data
  },

  getById: async (id: string): Promise<Lote> => {
    const { data } = await api.get(`/lotes/${id}`)
    return data
  },

  create: async (payload: {
    nombre: string
    capacidadMaxima: number
  }): Promise<string> => {
    const { data } = await api.post('/lotes', payload)
    return data
  },

  activar: async (loteId: string): Promise<void> => {
    await api.put(`/lotes/${loteId}/activar`)
  },

  cerrar: async (loteId: string): Promise<void> => {
    await api.put(`/lotes/${loteId}/cerrar`)
  },

  moverAnimal: async (
    loteId: string,
    payload: { animalId: string; fechaMovimiento: string; motivo: string }
  ): Promise<void> => {
    await api.post(`/lotes/${loteId}/mover-animal`, payload)
  },
}

// ─── Analítica ────────────────────────────────────────────────────────────────
export const analiticaService = {
  getIndicadoresAnimal: async (params: {
    animalId: string
    loteId: string
    desde: string
    hasta: string
    precioVentaEstimadoPorKg?: number
  }): Promise<IndicadorProductivo> => {
    const { animalId, ...rest } = params
    const { data } = await api.get(`/analitica/animales/${animalId}/indicadores`, { params: rest })
    return data
  },

  getResumenLote: async (params: {
    loteId: string
    desde: string
    hasta: string
    precioVentaEstimadoPorKg?: number
  }): Promise<ResumenLote> => {
    const { loteId, ...rest } = params
    const { data } = await api.get(`/analitica/lotes/${loteId}/resumen`, { params: rest })
    return data
  },

  getAnimalesIneficientes: async (params: {
    loteId?: string
    desde: string
    hasta: string
    precioVentaEstimadoPorKg?: number
    gmdMinima?: number
    icaMaxima?: number
  }): Promise<AnimalIneficiente[]> => {
    const { data } = await api.get('/analitica/animales-ineficientes', { params })
    return data
  },
}

// ─── Auth ─────────────────────────────────────────────────────────────────────
export const authService = {
  login: async (email: string, password: string): Promise<LoginResponse> => {
    const { data } = await api.post('/auth/login', { email, password })
    return data
  },
}

// ─── Nutrición ────────────────────────────────────────────────────────────────
export const nutricionService = {
  getRaciones: async (): Promise<Racion[]> => {
    const { data } = await api.get('/nutricion/raciones')
    return data
  },

  crearRacion: async (payload: {
    nombre: string
    costoKg: number
    moneda: string
    proteinaPct: number
    energiaMcal: number
  }): Promise<string> => {
    const { data } = await api.post('/nutricion/raciones', payload)
    return data
  },

  registrarConsumo: async (payload: {
    loteId: string
    racionId: string
    fecha: string
    cantidadKg: number
    costoTotal: number
    moneda: string
    registradoPorId: string
  }): Promise<string> => {
    const { data } = await api.post('/nutricion/consumos', payload)
    return data
  },
}

// ─── Costos Operativos ─────────────────────────────────────────────────────────
export const costosService = {
  getCostosTotalesLote: async (params: {
    loteId: string
    desde: string
    hasta: string
  }): Promise<CosteoLote> => {
    const { loteId, ...rest } = params
    const { data } = await api.get(`/costos/lotes/${loteId}/costos-totales`, { params: rest })
    return data
  },

  registrarCostoOperativo: async (payload: {
    loteId: string
    categoria: string
    concepto: string
    fecha: string
    monto: number
    moneda: string
    observaciones?: string
    registradoPorId: string
  }): Promise<string> => {
    const { data } = await api.post('/costos/costo-operativo', payload)
    return data
  },
}
