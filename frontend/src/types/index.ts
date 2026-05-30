// ─── Enums ────────────────────────────────────────────────────────────────────
export type EstadoProductivo = 'EnEngorde' | 'Vendido' | 'Muerto' | 'Retirado'
export type EstadoSanitario = 'Sano' | 'EnTratamiento' | 'EnRetiro' | 'Restringido'
export type EstadoLote = 'Activo' | 'Cerrado' | 'EnPreparacion'
export type Sexo = 'Macho' | 'Hembra'
export type SeveridadEvento = 'Leve' | 'Moderado' | 'Grave' | 'Critico'
export type MotivoMovimiento =
  | 'IngresoInicial' | 'Reclasificacion' | 'Sanitario'
  | 'Capacidad' | 'Venta' | 'Muerte' | 'Otro' | 'CambioDeLote'
export type ClasificacionGmd = 'Excelente' | 'Bueno' | 'Regular' | 'Deficiente'

// ─── Animal ───────────────────────────────────────────────────────────────────
export interface Animal {
  id: string
  codigoIdentificacion: string
  nombre?: string
  numeroArete: string
  sexo: Sexo
  raza?: string
  fechaNacimiento?: string
  pesoIngresoKg: number
  precioCompra: number
  moneda: string
  fechaIngreso: string
  estadoProductivo: EstadoProductivo
  estadoSanitario: EstadoSanitario
  pesoActualKg: number
  diasEnEngorde: number
  totalPesajes: number
  pesajes: Pesaje[]
  eventosSanitarios: EventoSanitario[]
}

export interface AnimalResumen {
  id: string
  codigoIdentificacion: string
  nombre?: string
  numeroArete: string
  raza?: string
  sexo: Sexo
  pesoActualKg: number
  diasEnEngorde: number
  estadoProductivo: EstadoProductivo
  estadoSanitario: EstadoSanitario
}

export interface Pesaje {
  id: string
  animalId: string
  fechaPesaje: string
  pesoKg: number
  observaciones?: string
}

export interface EventoSanitario {
  id: string
  animalId: string
  fechaEvento: string
  diagnostico: string
  descripcion: string
  severidad: SeveridadEvento
  tratamiento?: string
  tipoEvento?: string
  proximaDosis?: string
  responsable?: string
}

// ─── Lote ─────────────────────────────────────────────────────────────────────
export interface Lote {
  id: string
  codigo: string
  nombre: string
  capacidadMaxima: number
  animalesActuales: number
  porcentajeOcupacion: number
  estado: EstadoLote
  animales: AnimalLote[]
}

export interface LoteResumen {
  id: string
  codigo: string
  nombre: string
  capacidadMaxima: number
  animalesActuales: number
  porcentajeOcupacion: number
  estado: EstadoLote
}

export interface AnimalLote {
  animalId: string
  codigoAnimal: string
  nombreAnimal?: string
  fechaIngreso: string
  fechaEgreso?: string
  motivoIngreso: MotivoMovimiento
  esActivo: boolean
  diasEnLote: number
}

// ─── Nutrición ────────────────────────────────────────────────────────────────
export interface Racion {
  id: string
  nombre: string
  costoKg: number
  proteinaPct: number
  energiaMcal: number
  activa: boolean
}

export interface ConsumoAlimenticio {
  id: string
  loteId: string
  racionId: string
  nombreRacion: string
  fecha: string
  cantidadKg: number
  costoTotal: number
  moneda: string
}

// ─── Analítica ────────────────────────────────────────────────────────────────
export interface IndicadorProductivo {
  animalId: string
  codigoAnimal: string
  nombreAnimal?: string
  raza: string
  pesoInicialKg: number
  pesoActualKg: number
  pesoGanadoKg: number
  diasEnEngorde: number
  gmd: number
  ica: number
  costoPorKgGanado: number
  rentabilidadProyectada: number
  esIneficiente: boolean
  clasificacionGmd: ClasificacionGmd
}

export interface ResumenLote {
  loteId: string
  codigoLote: string
  nombreLote: string
  totalAnimales: number
  gmdPromedioKgDia: number
  icaPromedio: number
  costoTotalAlimento: number
  costoPorKgGanadoPromedio: number
  rentabilidadProyectadaTotal: number
  animalesIneficientes: number
  consumoTotalKg: number
  indicadores: IndicadorProductivo[]
}

export interface AnimalIneficiente {
  animalId: string
  codigoAnimal: string
  nombreAnimal?: string
  raza: string
  loteCodigo: string
  gmd: number
  gmdMinimaEsperada: number
  ica: number
  icaMaximaEsperada: number
  diasEnEngorde: number
  motivoAlerta: string
}

// ─── Auth ─────────────────────────────────────────────────────────────────────
export interface LoginResponse {
  token: string
  usuario: {
    id: string
    email: string
    nombre: string
    roles: string[]
  }
  expiraEn: string
}

// ─── Paginación ───────────────────────────────────────────────────────────────
export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

// ─── Costos ────────────────────────────────────────────────────────────────────
export type CategoriaCosto = 'ManoDeObra' | 'CIF'

export interface CostoOperativo {
  id: string
  loteId: string
  categoria: CategoriaCosto
  concepto: string
  fecha: string
  monto: number
  moneda: string
  observaciones?: string
  registradoPorId: string
}

export interface CosteoLote {
  loteId: string
  codigoLote: string
  totalAnimales: number
  desde: string
  hasta: string
  costoTotalAlimento: number
  costoAlimentoPorAnimal: number
  consumoTotalKg: number
  costoTotalManoDeObra: number
  costoManoDeObraPorAnimal: number
  detallesManoDeObra: CostoDetalle[]
  costoTotalCif: number
  costoCifPorAnimal: number
  detallesCif: CostoDetalle[]
  costoOperativoTotal: number
  costoOperativoPorAnimal: number
}

export interface CostoDetalle {
  id: string
  categoria: CategoriaCosto
  concepto: string
  fecha: string
  monto: number
  moneda: string
  observaciones?: string
}

// ─── Ventas ───────────────────────────────────────────────────────────────────
export interface Comprador {
  id: string
  nombre: string
  contacto?: string
  telefono?: string
  email?: string
}

export interface VentaItem {
  id: string
  ventaId: string
  animalId: string
  codigoAnimal: string
  nombreAnimal?: string
  precioVenta: number
  pesoVentaKg: number
}

export interface Venta {
  id: string
  compradorId: string
  nombreComprador: string
  fecha: string
  montoTotal: number
  moneda: string
  descripcion?: string
  totalAnimales: number
  items: VentaItem[]
}

// ─── API Error ────────────────────────────────────────────────────────────────
export interface ApiError {
  status: number
  title: string
  detail: string
  errors?: Record<string, string[]>
}

export interface VacunaProxima {
  animalId: string
  codigoAnimal: string
  nombreAnimal?: string
  diagnostico: string
  proximaDosis: string
  responsable?: string
}

// ─── Proveedores ──────────────────────────────────────────────────────────────
export interface Proveedor {
  id: string
  nombre: string
  contacto?: string
  telefono?: string
  email?: string
}

export interface Compra {
  id: string
  proveedorId: string
  nombreProveedor: string
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
}
