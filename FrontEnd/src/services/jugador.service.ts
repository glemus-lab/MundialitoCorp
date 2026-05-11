import apiClient from '@/lib/api-client';
import axios from 'axios';
import { ApiResponse, JugadorReadModel, PagedList } from '@/types/api';

export const jugadorService = {
  getByEquipo: async (
    equipoId: string, 
    page: number = 1, 
    filter: string = ''
  ): Promise<ApiResponse<PagedList<JugadorReadModel>>> => {
    try {
      const res = await apiClient.get('/jugadores', {
        params: { 
          equipoId: equipoId,
          pageNumber: page, 
          pageSize: 5, 
          nombreFilter: filter 
        }
      });
      return res.data;
    } catch (err) {
      if (axios.isAxiosError(err) && err.response) return err.response.data;
      throw err;
    }
  },
  create: async (equipoId: string, nombre: string, idempotencyKey: string): Promise<ApiResponse<string>> => {
    try {
      const res = await apiClient.post('/jugadores', { nombre, equipoId }, {
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
  update: async (id: string, nombre: string): Promise<ApiResponse<void>> => {
    try {
      const res = await apiClient.put(`/jugadores/${id}`, { 
        id, 
        nombre
      });
      return res.data;
    } catch (err) {
      if (axios.isAxiosError(err) && err.response) return err.response.data;
      throw err;
    }
  },
  delete: async (id: string): Promise<ApiResponse<void>> => {
    try {
      const res = await apiClient.delete(`/jugadores/${id}`);
      return res.data;
    } catch (err) {
      if (axios.isAxiosError(err) && err.response) return err.response.data;
      throw err;
    }
  },
  getRanking: async (page: number = 1, pageSize: number = 5): Promise<ApiResponse<PagedList<JugadorReadModel>>> => {
    try {
      const res = await apiClient.get('/jugadores/ranking', {
        params: { 
          pageNumber: page, 
          pageSize: pageSize 
        }
      });
      
      return res.data;
    } catch (err) {
      if (axios.isAxiosError(err) && err.response) return err.response.data;
      throw err;
    }
  }
};