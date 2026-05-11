import apiClient from '@/lib/api-client';
import axios from 'axios';
import { ApiResponse, EquipoPagedReadModel, EquipoReadModel, EquipoTabla, PagedList } from '@/types/api';

export const equipoService = {
  getPaged: async (page: number, size: number, sortBy: string, direction: string, filter: string): Promise<ApiResponse<PagedList<EquipoPagedReadModel>>> => {
    try {
      const response = await apiClient.get('/equipos', {
        params: { pageNumber: page, pageSize: size, sortBy, sortDirection: direction, nombre: filter }
      });
      return response.data;
    } catch (err) {
      if (axios.isAxiosError(err) && err.response) {
        return err.response.data as ApiResponse<PagedList<EquipoPagedReadModel>>;
      }
      throw err;
    }
  },
  getCatalogo: async () : Promise<ApiResponse<EquipoReadModel[]>> => {
    try
    {
      const resposne = await apiClient.get('/equipos/catalogo');
      return resposne.data;
    } catch (err) {
      if (axios.isAxiosError(err) && err.response) {
        return err.response.data as ApiResponse<EquipoReadModel[]>;
      }
      throw err;
    }
  },
  getTabla: async (): Promise<ApiResponse<EquipoTabla[]>> => {
    try {
      const response = await apiClient.get('/torneo/tabla');
      return response.data;
    } catch (err) {
      if (axios.isAxiosError(err) && err.response) {
        return err.response.data as ApiResponse<EquipoTabla[]>;
      }
      throw err;
    }
  },
  getById: async (id: string): Promise<ApiResponse<EquipoReadModel>> => {
    try {
      const res = await apiClient.get(`/equipos/${id}`);
      return res.data;
    } catch (err) {
      if (axios.isAxiosError(err) && err.response) {
        return err.response.data as ApiResponse<EquipoReadModel>;
      }
      throw err;
    }
  },
  create: async (nombre: string, idempotencyKey: string): Promise<ApiResponse<string>> => {
    try {
      const response = await apiClient.post('/equipos', { nombre }, {
        headers: {
          'Idempotency-Key': idempotencyKey
        }
      });
      return response.data;
    } catch (err) {
      if (axios.isAxiosError(err) && err.response) {
        return err.response.data as ApiResponse<string>;
      }
      throw err;
    }
  },
  update: async (id: string, nombre: string): Promise<ApiResponse<void>> => {
    try {
      const response = await apiClient.put(`/equipos/${id}`, { 
        id: id,
        nombre: nombre 
      });
      return response.data;
    } catch (err) {
      if (axios.isAxiosError(err) && err.response) return err.response.data;
      throw err;
    }
  },
  delete: async (id: string): Promise<ApiResponse<void>> => {
    try {
      const response = await apiClient.delete(`/equipos/${id}`);
      return response.data;
    } catch (err) {
      if (axios.isAxiosError(err) && err.response) {
        return err.response.data as ApiResponse<void>;
      }
      throw err;
    }
  }
};