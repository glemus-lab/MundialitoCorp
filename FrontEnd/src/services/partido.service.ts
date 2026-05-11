import apiClient from '@/lib/api-client';
import axios from 'axios';
import { ApiResponse, PagedList, PartidoDetalleReadModel, PartidoReadModel, RegistrarResultadoModel } from '@/types/api';

export const partidoService = {
  crear: async (datos: { equipoLocalId: string | null, equipoVisitanteId: string | null, fecha: string | null }, idempotencyKey: string): Promise<ApiResponse<string>> => {
    try {
      const res = await apiClient.post('/partidos', datos, {
        headers: {
          'Idempotency-Key': idempotencyKey
        }
      });
      return res.data;
    } catch (err) {
      if (axios.isAxiosError(err) && err.response) return err.response.data;
      throw err;
    }
  },
  delete: async(id: string) : Promise<ApiResponse<void>> => {
    try{
      const res = await apiClient.delete(`/partidos/${id}`);
      return res.data;
    } catch(err) {
      if (axios.isAxiosError(err) && err.response)
        return err.response.data as ApiResponse<void>;
      throw err;
    }
  },
  registrarResultado: async (command: RegistrarResultadoModel): Promise<ApiResponse<void>> => {
    try {
      const res = await apiClient.post('/partidos/registrar-resultado', command);
      return res.data;
    } catch (err) {
      if (axios.isAxiosError(err) && err.response) return err.response.data;
      throw err;
    }
  },
  updateFecha: async (id: string, nuevaFecha: string): Promise<ApiResponse<void>> => {
    try {
      const res = await apiClient.put(`/partidos/${id}/fecha`, { id, nuevaFecha });
      return res.data;
    } catch (err) {
      if (axios.isAxiosError(err) && err.response) return err.response.data;
      throw err;
    }
  },
  getPendientes: async (): Promise<ApiResponse<PartidoReadModel[]>> => {
    try {
      const res = await apiClient.get('/partidos/pendientes');
      return res.data;
    } catch (err) {
      if (axios.isAxiosError(err) && err.response) return err.response.data;
      throw err;
    }
  },
  getHistorial: async (page: number = 1, size: number = 10): Promise<ApiResponse<PagedList<PartidoReadModel>>> => {
    try {
      const res = await apiClient.get('/partidos/historial', {
        params: { pageNumber: page, pageSize: size }
      });
      return res.data;
    } catch (err) {
      if (axios.isAxiosError(err) && err.response) return err.response.data;
      throw err;
    }
  },
  getDetalle: async (id: string): Promise<ApiResponse<PartidoDetalleReadModel>> => {
    try {
      const res = await apiClient.get(`/partidos/${id}`);
      return res.data;
    } catch (err) {
      if (axios.isAxiosError(err) && err.response) return err.response.data;
      throw err;
    }
  }
};