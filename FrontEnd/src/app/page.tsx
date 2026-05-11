'use client';

import { useEffect, useState, useCallback } from 'react';
import { equipoService } from '@/services/equipo.service';
import { ApiResponse, EquipoTabla } from '@/types/api';
import axios from 'axios';
import ErrorMessage from '@/components/ui/ErrorMessage';
import { useFormErrors } from './hooks/use-form-errors';

export default function HomePage() {
  const [equipos, setEquipos] = useState<EquipoTabla[]>([]);
  const [loading, setLoading] = useState(true);
  const { handleResultErrors, businessErrors } = useFormErrors();
  
  const cargarTabla = useCallback(async () => {
    setLoading(true);
    try {
      const res = await equipoService.getTabla();
      
      if (res.isSuccess && res.value) {
        setEquipos(res.value);
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
  }, [handleResultErrors]);

  useEffect(() => {
    cargarTabla();
  }, [cargarTabla]);

  if (loading) {
    return (
      <div className="p-20 text-center font-bold text-gray-300 animate-pulse uppercase tracking-widest text-xs">
        Cargando Clasificación...
      </div>
    );
  }

  return (
    <main className="max-w-4xl mx-auto p-6 space-y-6">
      <header className="border-b border-gray-100 pb-4">
        <h1 className="text-2xl font-black text-gray-900 tracking-tighter uppercase italic">
          Tabla de Posiciones
        </h1>
      </header>
      
      <ErrorMessage messages={businessErrors} />

      <div className="bg-white rounded-[2rem] border border-gray-100 overflow-hidden shadow-sm">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="bg-gray-50/50 border-b border-gray-100 text-[10px] font-black text-gray-400 uppercase tracking-widest">
              <th className="p-5">Equipo</th>
              <th className="p-5 text-center">PJ</th>
              <th className="p-5 text-center">PG</th>
              <th className="p-5 text-center">PE</th>
              <th className="p-5 text-center">PP</th>
              <th className="p-5 text-center">GF</th>
              <th className="p-5 text-center">GC</th>
              <th className="p-5 text-center">DG</th>
              <th className="p-5 text-center">PTS</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-50">
            {equipos.length > 0 ? (
              equipos.map((e) => (
                <tr key={e.id} className="hover:bg-indigo-50/30 transition-colors group">
                  <td className="p-5 font-bold text-gray-700 text-xs tracking-tight">
                    {e.nombre}
                  </td>
                  <td className="p-5 text-center text-gray-500 font-medium text-sm">
                    {e.partidosJugados}
                  </td>
                  <td className="p-5 text-center text-gray-500 font-medium text-sm">
                    {e.partidosGanados}
                  </td>
                  <td className="p-5 text-center text-gray-500 font-medium text-sm">
                    {e.partidosPerdidos}
                  </td>
                  <td className="p-5 text-center text-gray-500 font-medium text-sm">
                    {e.partidosEmpatados}
                  </td>
                  <td className="p-5 text-center text-gray-500 font-medium text-sm">
                    {e.golesFavor}
                  </td>
                  <td className="p-5 text-center text-gray-500 font-medium text-sm">
                    {e.golesContra}
                  </td>
                  <td className="p-5 text-center text-gray-400 font-medium text-sm">
                    {e.diferenciaGoles}
                  </td>
                  <td className="p-5 text-center font-black text-indigo-600 text-lg">
                    {e.puntos}
                  </td>
                </tr>
              ))
            ) : (
              <tr>
                <td colSpan={6} className="p-16 text-center text-gray-300 italic text-sm">
                  No hay datos disponibles en este momento.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </main>
  );
}