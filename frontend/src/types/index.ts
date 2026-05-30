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

// ─── Costos / Finanzas ────────────────────────────────────────────────────────
export interface CategoriaGasto {
  id: string
  nombre: string
  tipo: 'Directo' | 'Indirecto' | 'Operativo' | 'Inversion'
}

export interface Socio {
  id: string
  nombre: string
  participacion: number
}

export interface MovimientoFinanciero {
  id: string
  fecha: string
  periodoAnio: number
  periodoMes: number
  categoriaGastoId: string
  categoriaGastoNombre: string
  categoriaGastoTipo: string
  monto: number
  moneda: string
  origen: 'Bovino' | 'Porcino' | 'Agricola' | 'General'
  descripcion: string
  socioId?: string
  socioNombre?: string
}

export interface CuotaAmortizacion {
  id: string
  numeroCuota: number
  fechaVencimiento: string
  cuota: number
  interes: number
  abonoCapital: number
  saldoPendiente: number
  pagada: boolean
  fechaPago?: string
}

export interface Prestamo {
  id: string
  capital: number
  moneda: string
  tasaMensual: number
  nCuotas: number
  fechaInicio: string
  descripcion: string
  cuotas: CuotaAmortizacion[]
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
  categoria: string // Modified from CategoriaCosto to support dynamic category names
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

// ─── Reportes Financieros ─────────────────────────────────────────────────────

export interface LineaResultado {
  concepto: string
  monto: number
}

export interface EstadoResultados {
  anio: number
  mes?: number
  origen?: string
  moneda: string
  totalIngresos: number
  ingresos: LineaResultado[]
  totalCostosDirectos: number
  costosDirectos: LineaResultado[]
  utilidadBruta: number
  totalGastosIndirectos: number
  gastosIndirectos: LineaResultado[]
  totalGastosOperativos: number
  gastosOperativos: LineaResultado[]
  totalInteresesPrestamo: number
  utilidadOperativa: number
  totalInversiones: number
  inversiones: LineaResultado[]
  utilidadNeta: number
}

export interface FlujoCajaMes {
  mes: number
  nombreMes: string
  ingresos: number
  egresos: number
  saldoNeto: number
  saldoAcumulado: number
}

export interface FlujoCaja {
  anio: number
  origen?: string
  meses: FlujoCajaMes[]
  totalIngresos: number
  totalEgresos: number
  saldoNeto: number
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

// ─── Inversión / Planeación ────────────────────────────────────────────────────
export interface ItemInversion {
  id: string
  etapaId: string
  producto: string
  monto: number
  moneda: string
  observacion?: string
  estado: 'OK' | 'Pendiente'
  porcentajeAvance: number
}

export interface EtapaInversion {
  id: string
  numero: number
  nombre: string
  totalRealizadoMonto: number
  totalPendienteMonto: number
  moneda: string
  items: ItemInversion[]
}

export interface AporteSocio {
  id: string
  socioId: string
  socioNombre: string
  itemInversionId: string
  itemProducto: string
  monto: number
  moneda: string
}

// ─── Operación ─────────────────────────────────────────────────────────────────
export interface Potrero {
  id: string
  nombre: string
  capacidad: number
  animalesActuales: number
  estancias: EstanciaAnimal[]
}

export interface EstanciaAnimal {
  id: string
  animalId: string
  fechaEntrada: string
  salida: string | null
}

export interface Empleado {
  id: string
  nombre: string
  pagoMensualMonto: number
  pagoMensualMoneda: string
  actividades: ActividadManoObra[]
}

export interface ActividadManoObra {
  id: string
  tipo: string
  fecha: string
  costoMonto: number
  costoMoneda: string
}

export interface CultivoCania {
  id: string
  nombre: string
  callesTotales: number
  totalBolsasSilo: number
  totalHoras: number
  totalCortes: number
  cortes: CorteCania[]
}

export interface CorteCania {
  id: string
  fecha: string
  nCalles: number
  horas: number
  bolsasSilo: number
  melaza: number
  costoJornalMonto: number
  costoJornalMoneda: string
}

export interface LoteSilo {
  id: string
  corteCaniaId: string | null
  fechaProduccion: string
  bolsas: number
  costoUnitarioMonto: number
  costoUnitarioMoneda: string
  costoTotal: number
  observacion: string | null
}
