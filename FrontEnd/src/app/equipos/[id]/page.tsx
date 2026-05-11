'use client';

import { useEffect, useState, use, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import axios from 'axios';
import { equipoService } from '@/services/equipo.service';
import { ApiResponse } from '@/types/api';
import { useFormErrors } from '@/app/hooks/use-form-errors';
import ErrorMessage from '@/components/ui/ErrorMessage';
import SeccionJugadores from '@/components/SeccionJugadores';
import SuccessMessage from '@/components/ui/SuccessMessage';

export default function EquipoFormPage({ params }: { params: Promise<{ id: string }> }) {
  const router = useRouter();
  const { id } = use(params);
  const isEdit = id !== 'nuevo';

  const [nombre, setNombre] = useState('');
  const [loading, setLoading] = useState(isEdit);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);
  const [idempotencyKey, setIdempotencyKey] = useState("");
  
  const { handleResultErrors, getMessageFor, businessErrors, clearErrors } = useFormErrors(['Nombre']);
  
  const cargarDatos = useCallback(async () => {
    if (!isEdit){
      setIdempotencyKey(crypto.randomUUID());
      return;
    } 
    setLoading(true);
    clearErrors();

    try {
      const res = await equipoService.getById(id);
      if (res.isSuccess && res.value) {
        setNombre(res.value.nombre);
      } else {
        handleResultErrors(res);
      }
    } catch (err: unknown) {
      let errorResponse: ApiResponse<null> = {
        isSuccess: false,
        isFailure: true,
        code: 500,
        errorMessage: "No se pudo conectar con el servidor. Verifique que el servicio esté activo.",
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
  }, [id, isEdit, handleResultErrors, clearErrors]);

  useEffect(() => {
    cargarDatos();
  }, [cargarDatos]);

  const handleGuardar = async () => {
    clearErrors();
    setSuccessMsg(null);

    try {
      const res = isEdit 
        ? await equipoService.update(id, nombre)
        : await equipoService.create(nombre, idempotencyKey);

      if (res.isSuccess) {
        setSuccessMsg(isEdit ? "Cambios guardados correctamente" : "Equipo registrado");
        if (!isEdit && res.value) {
          setTimeout(() => router.push(`/equipos/${res.value}`), 1500);
        }
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
    }
  };

  if (loading) return (
    <div className="p-20 text-center font-black text-gray-300 animate-pulse text-xs uppercase tracking-widest">
      Sincronizando...
    </div>
  );

  return (
    <div className="max-w-6xl mx-auto p-6 space-y-6">
      <header className="flex items-center gap-4 border-b border-gray-100 pb-4">
        <button 
          onClick={() => router.push('/equipos')} 
          className="w-8 h-8 flex items-center justify-center bg-white border border-gray-200 hover:bg-gray-50 rounded-xl transition-all text-gray-400 font-bold shadow-sm"
        >
          ←
        </button>
        <div>
          <h1 className="text-2xl font-black text-gray-900 tracking-tighter uppercase italic">
            {isEdit ? 'Edición de Equipo' : 'Registro de Equipo'}
          </h1>
        </div>
      </header>

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        <div className="lg:col-span-4 space-y-4">
          <div className="bg-white p-6 rounded-[2rem] shadow-sm border border-gray-100 sticky top-20">
            <h2 className="text-[10px] font-black text-indigo-600 uppercase mb-6 tracking-widest border-b border-gray-50 pb-2">
              Datos Maestros
            </h2>
            
            <ErrorMessage messages={businessErrors} />
            <SuccessMessage message={successMsg} />

            <div className="space-y-4">
              <div>
                <label className="block text-[10px] font-black text-gray-500 uppercase mb-1.5 ml-1 tracking-widest">
                  Nombre
                </label>
                <input 
                  type="text" 
                  disabled={!!businessErrors && isEdit}
                  className={`w-full p-3 bg-white border rounded-xl text-xs font-bold outline-none transition-all shadow-sm ${
                    getMessageFor('Nombre') 
                      ? 'border-red-400 ring-2 ring-red-50' 
                      : 'border-gray-200 focus:border-indigo-500 focus:ring-4 focus:ring-indigo-50'
                  }`}
                  value={nombre}
                  onChange={(e) => setNombre(e.target.value)}
                />
                {getMessageFor('Nombre') && (
                  <p className="text-red-500 text-[9px] mt-1.5 font-bold ml-1">⚠️ {getMessageFor('Nombre')}</p>
                )}
              </div>

              <button 
                onClick={handleGuardar}
                disabled={!!businessErrors && isEdit}
                className="w-full py-3 bg-indigo-600 text-white rounded-xl text-[10px] font-black uppercase tracking-widest shadow-lg shadow-indigo-100 hover:bg-indigo-700 transition-all active:scale-95 disabled:opacity-50"
              >
                {isEdit ? 'Actualizar Datos' : 'Registrar Equipo'}
              </button>
            </div>
          </div>
        </div>

        <div className="lg:col-span-8">
          {isEdit && !businessErrors ? (
            <div className="bg-white p-6 rounded-[2rem] shadow-sm border border-gray-100">
              <SeccionJugadores equipoId={id} />
            </div>
          ) : (
            <div className="h-full min-h-[300px] flex flex-col items-center justify-center bg-gray-50 rounded-[2rem] border-2 border-dashed border-gray-200 p-8 text-center text-gray-400 uppercase font-black text-[10px]">
              {businessErrors ? "Error de conexión con el servicio" : "Registro inicial requerido"}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}