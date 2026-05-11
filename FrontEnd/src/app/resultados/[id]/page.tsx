'use client';

import { useEffect, useState, useCallback } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { partidoService } from '@/services/partido.service';
import { PartidoDetalleReadModel, ApiResponse } from '@/types/api';
import { useFormErrors } from '../../hooks/use-form-errors';
import ErrorMessage from '@/components/ui/ErrorMessage';
import axios from 'axios';

export default function DetallePartidoPage() {
  const { id } = useParams();
  const router = useRouter();
  const [partido, setPartido] = useState<PartidoDetalleReadModel | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const { handleResultErrors, businessErrors } = useFormErrors();

  const cargarDetalle = useCallback(async () => {
    if (!id) return;
    setLoading(true);
    try {
      const result = await partidoService.getDetalle(id as string);
      if (result.isSuccess) {
        setPartido(result.value);
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
  }, [id, handleResultErrors]);

  useEffect(() => {
    cargarDetalle();
  }, [cargarDetalle]);

  if (loading) return (
    <div className="max-w-3xl mx-auto p-12 text-center animate-pulse font-black uppercase text-gray-400">
      Cargando ficha técnica...
    </div>
  );

  if (!partido) return <ErrorMessage messages={businessErrors} />;

  const golesLocal = partido.goleadores.filter(g => g.equipoId === partido.equipoLocalId);
  const golesVisitante = partido.goleadores.filter(g => g.equipoId === partido.equipoVisitanteId);

  return (
    <div className="max-w-3xl mx-auto p-6 space-y-6">
      <button 
        onClick={() => router.back()}
        className="text-[10px] font-black uppercase tracking-widest text-gray-400 hover:text-indigo-600 transition-colors"
      >
        ← Volver a Resultados
      </button>

      <div className="bg-white rounded-[2rem] border border-gray-100 shadow-sm overflow-hidden">
        <div className="bg-gray-900 p-8 text-white text-center">
          <p className="text-[10px] font-bold uppercase tracking-[0.3em] text-indigo-400 mb-4">
            {new Date(partido.fecha).toLocaleDateString('es-ES', { dateStyle: 'full' })}
          </p>
          
          <div className="flex items-center justify-around gap-4">
            <div className="flex-1 text-right">
              <h2 className="text-xl font-black italic uppercase tracking-tighter">{partido.equipoLocal}</h2>
            </div>
            
            <div className="px-6 py-2 bg-white/10 rounded-2xl border border-white/10">
              <span className="text-4xl font-black tracking-tighter italic">
                {partido.golesLocal} — {partido.golesVisitante}
              </span>
            </div>

            <div className="flex-1 text-left">
              <h2 className="text-xl font-black italic uppercase tracking-tighter">{partido.equipoVisitante}</h2>
            </div>
          </div>
        </div>

        <div className="p-8 grid grid-cols-2 gap-8 divide-x divide-gray-100">
          <div className="space-y-4">
            <p className="text-[10px] font-black uppercase tracking-widest text-gray-400 border-b border-gray-50 pb-2">Goleadores Local</p>
            <ul className="space-y-2">
              {golesLocal.length > 0 ? golesLocal.map((g, i) => (
                <li key={i} className="text-xs font-bold text-gray-700 uppercase flex items-center gap-2">
                  <span className="w-1.5 h-1.5 bg-indigo-500 rounded-full"></span>
                  {g.nombreJugador}
                </li>
              )) : <li className="text-[10px] uppercase text-gray-300 italic">Sin goles registrados</li>}
            </ul>
          </div>

          <div className="pl-8 space-y-4">
            <p className="text-[10px] font-black uppercase tracking-widest text-gray-400 border-b border-gray-50 pb-2 text-right">Goleadores Visita</p>
            <ul className="space-y-2 text-right">
              {golesVisitante.length > 0 ? golesVisitante.map((g, i) => (
                <li key={i} className="text-xs font-bold text-gray-700 uppercase flex items-center justify-end gap-2">
                  {g.nombreJugador}
                  <span className="w-1.5 h-1.5 bg-rose-500 rounded-full"></span>
                </li>
              )) : <li className="text-[10px] uppercase text-gray-300 italic">Sin goles registrados</li>}
            </ul>
          </div>
        </div>
      </div>
    </div>
  );
}