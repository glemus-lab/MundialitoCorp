'use client';

import { useEffect, useState, useCallback } from 'react';
import { jugadorService } from '@/services/jugador.service';
import { ApiResponse, JugadorReadModel, PagedList } from '@/types/api';
import axios from 'axios';
import ErrorMessage from '@/components/ui/ErrorMessage';
import { useFormErrors } from '../hooks/use-form-errors';

export default function RankingGoleadoresPage() {
  const [pagedList, setPagedList] = useState<PagedList<JugadorReadModel> | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const { handleResultErrors, businessErrors } = useFormErrors();

  const [params, setParams] = useState({ page: 1, size: 5 });

  const cargarRanking = useCallback(async () => {
    setLoading(true);
    
    try {
      const res = await jugadorService.getRanking(params.page, params.size);
      if (res.isSuccess) {
        setPagedList(res.value);
      } else {
        handleResultErrors(res);
      }
    } catch (err: unknown) {
      let errorResponse: ApiResponse<null> = {
        isSuccess: false,
        isFailure: true,
        code: 500,
        errorMessage: "Error de comunicación con el servidor.",
        errors: [],
        value: null
      };

      if (axios.isAxiosError(err) && err.response) {
        errorResponse = err.response.data as ApiResponse<null>;
      }
      handleResultErrors(errorResponse);
    } finally {
      setLoading(false);
    }
  }, [params, handleResultErrors]);

  useEffect(() => {
    cargarRanking();
  }, [cargarRanking]);

  return (
    <div className="max-w-5xl mx-auto p-6 space-y-6">
      <header className="flex justify-between items-end border-b border-gray-100 pb-4">
        <div>
          <h1 className="text-2xl font-black text-gray-900 tracking-tighter uppercase italic">Ranking de Goleadores</h1>
          <p className="text-gray-400 text-[10px] font-bold uppercase tracking-widest">Máximos Artilleros del Torneo</p>
        </div>
      </header>

      <ErrorMessage messages={businessErrors} />

      <div className="bg-white rounded-[2rem] border border-gray-100 overflow-hidden shadow-sm">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="bg-gray-50/50 border-b border-gray-100 text-[10px] font-black text-gray-400 uppercase tracking-widest">
              <th className="p-5 w-20 text-center">Pos</th>
              <th className="p-5">Nombre del Jugador</th>
              <th className="p-5 text-center">Equipo</th>
              <th className="p-5 text-right">Goles</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-50">
            {loading ? (
              <tr>
                <td colSpan={4} className="p-16 text-center text-gray-400 animate-pulse text-xs font-bold uppercase">
                  Sincronizando Estadísticas...
                </td>
              </tr>
            ) : pagedList?.data.map((j, index) => {
              const posReal = ((params.page - 1) * params.size) + index + 1;
              return (
                <tr key={j.id} className="hover:bg-gray-50/50 transition-colors group">
                  <td className="p-5 text-center">
                    <span className="font-black text-gray-300 text-xs uppercase">
                      {posReal === 1 ? "🥇" : posReal === 2 ? "🥈" : posReal === 3 ? "🥉" : `#${posReal}`}
                    </span>
                  </td>
                  <td className="p-5">
                    <span className="font-bold text-gray-700 text-xs tracking-tight">
                      {j.nombre}
                    </span>
                  </td>
                  <td className="p-5 text-center text-gray-500 font-medium text-xs">
                    {j.equipoNombre}
                  </td>
                  <td className="p-5 text-right">
                    <span className="inline-flex items-center px-4 py-1 bg-indigo-600 text-white rounded-lg text-sm font-black shadow-sm tracking-tighter">
                      {j.golesAnotados}
                    </span>
                  </td>
                </tr>
              );
            })}
            {!loading && pagedList?.data.length === 0 && (
              <tr>
                <td colSpan={4} className="p-16 text-center text-gray-300 italic text-xs uppercase tracking-widest">
                  No hay goles registrados aún
                </td>
              </tr>
            )}
          </tbody>
        </table>

        <div className="p-4 bg-gray-50/50 border-t border-gray-100 flex justify-between items-center">
          <span className="text-[10px] font-black text-gray-400 uppercase tracking-widest">
            Pág {pagedList?.pageNumber} de {pagedList?.totalPages}<br></br>
            Total Registros: {pagedList?.totalRecords}
          </span>
          <div className="flex gap-2">
            <button 
              disabled={params.page <= 1 || loading} 
              onClick={() => {
                setParams({ ...params, page: params.page - 1 });
                window.scrollTo({ top: 0, behavior: 'smooth' });
              }}
              className="px-4 py-2 bg-white border border-gray-200 rounded-xl text-[10px] font-black uppercase hover:border-indigo-300 disabled:opacity-30 transition-all shadow-sm text-gray-600"
            >
              Anterior
            </button>
            <button 
              disabled={params.page >= (pagedList?.totalPages || 1) || loading} 
              onClick={() => {
                setParams({ ...params, page: params.page + 1 });
                window.scrollTo({ top: 0, behavior: 'smooth' });
              }}
              className="px-4 py-2 bg-white border border-gray-200 rounded-xl text-[10px] font-black uppercase hover:border-indigo-300 disabled:opacity-30 transition-all shadow-sm text-gray-600"
            >
              Siguiente
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}