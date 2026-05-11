export interface ValidationError {
  propertyName: string;
  message: string;
}

export interface PagedList<T> {
  data: T[];
  pageNumber: number;
  pageSize: number;
  totalRecords: number;
  totalPages: number;
}

export interface ApiResponse<T> {
  value: T | null;
  isSuccess: boolean;
  isFailure: boolean;
  errorMessage: string | null;
  code: number | null;
  errors: ValidationError[];
}

export interface EquipoReadModel {
  id: string;
  nombre: string;
}

export interface EquipoPagedReadModel {
  id: string;
  nombre: string;
  puntos: number;
  partidosJugados: number;
}

export interface EquipoTabla {
  id: string;
  nombre: string;
  puntos: number;
  partidosJugados: number;
  partidosGanados: number;
  partidosPerdidos: number;
  partidosEmpatados: number;
  golesFavor: number;
  golesContra: number;
  diferenciaGoles: number;
}

export interface PartidoReadModel {
  id: string;
  local: string;
  visitante: string;
  equipoLocalId: string;
  equipoVisitanteId: string;
  golesL: number | null;
  golesV: number | null;
  fecha: string;
}

export interface GoleadorReadModel {
  jugadorId: string;
  nombreJugador: string;
  equipoId: string;
}

export interface PartidoDetalleReadModel {
  equipoLocalId: string;
  equipoLocal: string;
  equipoVisitanteId: string;
  equipoVisitante: string;
  golesLocal: number;
  golesVisitante: number;
  fecha: string;
  goleadores: GoleadorReadModel[];
}

export interface JugadorReadModel {
  id: string;
  nombre: string;
  equipoId: string;
  equipoNombre: string;
  golesAnotados: number;
}

export interface RegistrarResultadoModel {
  partidoId: string;
  golesLocal: number;
  golesVisitante: number;
  goleadoresLocalIds: string[];
  goleadoresVisitanteIds: string[];
}