import api from './api'
import type {
  Animal, AnimalResumen, PagedResult, VacunaProxima,
  IndicadorProductivo, ResumenLote, AnimalIneficiente,
  LoginResponse, Racion, Lote, LoteResumen,
  CosteoLote, Proveedor, Compra, Comprador, Venta,
  CategoriaGasto, Socio, MovimientoFinanciero, Prestamo,
  EtapaInversion, AporteSocio
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
    codigoIdentificacion: string
    numeroArete: string
    sexo: string
    raza: string
    fechaNacimiento: string
    pesoIngresoKg: number
    precioCompra: number
    moneda: string
    fechaIngreso: string
    loteInicialId?: string
  }): Promise<string> => {
    const { data } = await api.post('/animals', payload)
    return data
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
      tipoEvento?: string
      proximaDosis?: string
      responsable?: string
    }
  ): Promise<string> => {
    const { data } = await api.post(`/animals/${animalId}/eventos-sanitarios`, payload)
    return data
  },

  actualizar: async <T>(id: string, payload: T): Promise<void> => {
    await api.put(`/animals/${id}`, payload)
  },

  eliminarPesaje: async (animalId: string, pesajeId: string): Promise<void> => {
    await api.delete(`/animals/${animalId}/pesajes/${pesajeId}`)
  },

  eliminar: async (id: string): Promise<void> => {
    await api.delete(`/animals/${id}`)
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
    codigo: string
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

  getVacunasProximas: async (dias = 15): Promise<VacunaProxima[]> => {
    const { data } = await api.get('/analitica/vacunas-proximas', { params: { dias } })
    return data
  },
}

// ─── Ventas ───────────────────────────────────────────────────────────────────
export const ventasService = {
  getAll: async (): Promise<Venta[]> => {
    const { data } = await api.get('/ventas')
    return data
  },

  getById: async (id: string): Promise<Venta> => {
    const { data } = await api.get(`/ventas/${id}`)
    return data
  },

  create: async (payload: {
    compradorId: string
    fecha: string
    moneda: string
    descripcion?: string
    animales: { animalId: string; precioVenta: number; pesoVentaKg: number }[]
  }): Promise<string> => {
    const { data } = await api.post('/ventas', payload)
    return data
  },

  getCompradores: async (): Promise<Comprador[]> => {
    const { data } = await api.get('/ventas/compradores')
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

// ─── Costos operativos ────────────────────────────────────────────────────────
export const costosService = {
  getCosteoLote: async (params: {
    loteId: string
    desde: string
    hasta: string
  }): Promise<CosteoLote> => {
    const { loteId, ...rest } = params
    const { data } = await api.get(`/finanzas/lotes/${loteId}`, { params: rest })
    return data
  },
}

// ─── Finanzas (Categorías, Socios, Movimientos) ───────────────────────────────
export const finanzasService = {
  getCategorias: async (): Promise<CategoriaGasto[]> => {
    const { data } = await api.get('/finanzas/categorias')
    return data
  },

  crearCategoria: async (payload: { nombre: string; tipo: string }): Promise<string> => {
    const { data } = await api.post('/finanzas/categorias', payload)
    return data
  },

  getSocios: async (): Promise<Socio[]> => {
    const { data } = await api.get('/finanzas/socios')
    return data
  },

  crearSocio: async (payload: { nombre: string; participacion: number }): Promise<string> => {
    const { data } = await api.post('/finanzas/socios', payload)
    return data
  },

  getMovimientos: async (params?: {
    anio?: number
    mes?: number
    origen?: string
    categoriaGastoId?: string
    socioId?: string
  }): Promise<MovimientoFinanciero[]> => {
    const { data } = await api.get('/finanzas/movimientos', { params })
    return data
  },

  registrarMovimiento: async (payload: {
    fecha: string
    periodoAnio: number
    periodoMes: number
    categoriaGastoId: string
    monto: number
    moneda: string
    origen: string
    descripcion: string
    socioId?: string
    registradoPorId: string
  }): Promise<string> => {
    const { data } = await api.post('/finanzas/movimientos', payload)
    return data
  },
}

// ─── Préstamos ────────────────────────────────────────────────────────────────
export const prestamosService = {
  getAll: async (): Promise<Prestamo[]> => {
    const { data } = await api.get('/prestamos')
    return data
  },

  create: async (payload: {
    monto: number
    moneda: string
    tasaMensual: number
    nCuotas: number
    fechaInicio: string
    descripcion: string
  }): Promise<string> => {
    const { data } = await api.post('/prestamos', payload)
    return data
  },
}

// ─── Proveedores ──────────────────────────────────────────────────────────────
export const proveedoresService = {
  getAll: async (): Promise<Proveedor[]> => {
    const { data } = await api.get('/proveedores')
    return data
  },

  getById: async (id: string): Promise<Proveedor> => {
    const { data } = await api.get(`/proveedores/${id}`)
    return data
  },

  create: async (payload: {
    nombre: string
    contacto?: string
    telefono?: string
    email?: string
  }): Promise<string> => {
    const { data } = await api.post('/proveedores', payload)
    return data
  },

  update: async (id: string, payload: {
    id: string
    nombre: string
    contacto?: string
    telefono?: string
    email?: string
  }): Promise<void> => {
    await api.put(`/proveedores/${id}`, payload)
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/proveedores/${id}`)
  },
}

// ─── Compras ──────────────────────────────────────────────────────────────────
export const comprasService = {
  getAll: async (): Promise<Compra[]> => {
    const { data } = await api.get('/compras')
    return data
  },

  getByProveedor: async (proveedorId: string): Promise<Compra[]> => {
    const { data } = await api.get(`/compras/por-proveedor/${proveedorId}`)
    return data
  },

  create: async (payload: {
    proveedorId: string
    fecha: string
    tipoCompra: string
    costoTotal: number
    moneda: string
    descripcion?: string
    cantidadCabezas?: number
    precioPorCabeza?: number
    pesoPromedioKg?: number
    loteId?: string
    tipoInsumo?: string
    cantidadInsumo?: number
    unidadMedida?: string
  }): Promise<string> => {
    const { data } = await api.post('/compras', payload)
    return data
  },
}

// ─── Inversión ─────────────────────────────────────────────────────────────────
export const inversionService = {
  getEtapas: async (): Promise<EtapaInversion[]> => {
    const { data } = await api.get('/inversion/etapas')
    return data
  },

  crearEtapa: async (payload: { numero: number; nombre: string }): Promise<string> => {
    const { data } = await api.post('/inversion/etapas', payload)
    return data
  },

  agregarItem: async (payload: {
    etapaId: string
    producto: string
    monto: number
    moneda: string
    observacion?: string
    estado: string
    porcentajeAvance: number
  }): Promise<string> => {
    const { data } = await api.post('/inversion/items', payload)
    return data
  },

  actualizarItem: async (payload: {
    itemId: string
    producto: string
    monto: number
    moneda: string
    observacion?: string
    estado: string
    porcentajeAvance: number
  }): Promise<void> => {
    const { itemId, ...body } = payload
    await api.patch(`/inversion/items/${itemId}`, body)
  },

  getAportes: async (params?: { socioId?: string; itemInversionId?: string }): Promise<AporteSocio[]> => {
    const { data } = await api.get('/inversion/aportes', { params })
    return data
  },

  crearAporte: async (payload: {
    socioId: string
    itemInversionId: string
    monto: number
    moneda: string
  }): Promise<string> => {
    const { data } = await api.post('/inversion/aportes', payload)
    return data
  },
}
