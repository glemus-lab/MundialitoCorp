'use client';

import { useEffect, useState, useCallback } from 'react';
import { partidoService } from '@/services/partido.service';
import { PartidoReadModel, PagedList, ApiResponse } from '@/types/api';
import axios from 'axios';
import { useFormErrors } from '../hooks/use-form-errors';
import ErrorMessage from '@/components/ui/ErrorMessage';
import Link from 'next/link';

export default function HistorialResultadosPage() {
  const [pagedList, setPagedList] = useState<PagedList<PartidoReadModel> | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const { handleResultErrors, businessErrors } = useFormErrors();
  const [params, setParams] = useState({ page: 1, size: 5 });

  const cargarHistorial = useCallback(async () => {
    setLoading(true);
    try {
      const result = await partidoService.getHistorial(params.page, params.size);
      if (result.isSuccess) {
        setPagedList(result.value);
      } else {
        handleResultErrors(result);
      }
    } catch (err: unknown) {
      let errorResponse: ApiResponse<null> = {
        isSuccess: false, isFailure: true, code: 500,
        errorMessage: "No se pudo conectar con el servidor.",
        errors: [], value: null
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
    cargarHistorial();
  }, [cargarHistorial]);

  return (
    <div className="max-w-5xl mx-auto p-6 space-y-6">
      <header className="flex justify-between items-end border-b border-gray-100 pb-4">
        <div>
          <h1 className="text-2xl font-black text-gray-900 tracking-tighter uppercase italic">Resultados de Partidos</h1>
        </div>
      </header>

      <ErrorMessage messages={businessErrors} />

      <div className="bg-white rounded-[2rem] border border-gray-100 overflow-hidden shadow-sm">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="bg-gray-50/50 border-b border-gray-100 text-[10px] font-black text-gray-400 uppercase tracking-widest">
              <th className="p-5">Fecha</th>
              <th className="p-5 text-center">Local</th>
              <th className="p-5 text-center">Visita</th>
              <th className="p-5 text-center">Resultado</th>
              <th className="p-5 text-right">Acción</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-50">
            {loading ? (
              <tr><td colSpan={5} className="p-16 text-center text-gray-400 animate-pulse text-xs font-bold uppercase">Actualizando resultados...</td></tr>
            ) : (
              pagedList?.data.map((p) => (
                <tr key={p.id} className="hover:bg-gray-50/50 transition-colors group">
                  <td className="p-5"><span className="font-bold text-gray-700 uppercase text-xs tracking-tight">{new Date(p.fecha).toLocaleDateString('es-ES', { day: '2-digit', month: '2-digit', year: 'numeric' })}</span></td>
                  <td className="p-5 text-center text-gray-500 font-medium text-xs">{p.local}</td>
                  <td className="p-5 text-center text-gray-500 font-medium text-xs">{p.visitante}</td>
                  <td className="p-5 text-center"><span className="inline-flex items-center px-3 py-1 bg-gray-900 text-white rounded-lg text-sm font-black shadow-sm tracking-tighter">{p.golesL} — {p.golesV}</span></td>
                  <td className="p-5 text-right"><Link href={`/resultados/${p.id}`} className="text-[10px] font-black uppercase text-indigo-600 hover:text-indigo-900 tracking-widest border-b-2 border-transparent hover:border-indigo-900 transition-all">Ver Detalles</Link></td>
                </tr>
              ))
            )}
            {!loading && pagedList?.data.length === 0 && (
              <tr><td colSpan={5} className="p-16 text-center text-gray-300 italic text-xs uppercase tracking-widest">No hay partidos registrados</td></tr>
            )}
          </tbody>
        </table>

        <div className="p-4 bg-gray-50/50 border-t border-gray-100 flex justify-between items-center">
          <span className="text-[10px] font-black text-gray-400 uppercase tracking-widest">
            Pág {pagedList?.pageNumber} de {pagedList?.totalPages}<br/>
            Total Registros: {pagedList?.totalRecords}
          </span>
          <div className="flex gap-2">
            <button 
              disabled={params.page <= 1 || loading} 
              onClick={() => setParams({ ...params, page: params.page - 1 })}
              className="px-4 py-2 bg-white border border-gray-200 rounded-xl text-[10px] font-black uppercase hover:border-indigo-300 disabled:opacity-30 transition-all shadow-sm"
            >Anterior</button>
            <button 
              disabled={params.page >= (pagedList?.totalPages || 1) || loading} 
              onClick={() => setParams({ ...params, page: params.page + 1 })}
              className="px-4 py-2 bg-white border border-gray-200 rounded-xl text-[10px] font-black uppercase hover:border-indigo-300 disabled:opacity-30 transition-all shadow-sm"
            >Siguiente</button>
          </div>
        </div>
      </div>
    </div>
  );
}