'use client';

import { useState, useEffect } from 'react';
import { equipoService } from '@/services/equipo.service';
import { partidoService } from '@/services/partido.service';
import { EquipoReadModel, ApiResponse, ValidationError } from '@/types/api';
import { useFormErrors } from '@/app/hooks/use-form-errors';
import ErrorMessage from '@/components/ui/ErrorMessage';
import axios from 'axios';
import SuccessMessage from '@/components/ui/SuccessMessage';

export default function ProgramarPartidoPage() {
  const [equipos, setEquipos] = useState<EquipoReadModel[]>([]);
  const [prog, setProg] = useState({ localId: '', visitanteId: '', fecha: '' });
  const [successMessage, setSuccessMessage] = useState('');
  const [idempotencyKey, setIdempotencyKey] = useState('');
  
  const { handleResultErrors, getMessageFor, businessErrors, clearErrors } = useFormErrors(['EquipoLocalId', 'EquipoVisitanteId', 'Fecha']);

  useEffect(() => {
    equipoService.getCatalogo().then(res => {
      if (res.isSuccess){
         setIdempotencyKey(crypto.randomUUID());
         setEquipos(res.value || []);
      }
      else
        handleResultErrors(res);
    });
  }, [handleResultErrors]);

  const handleProgramar = async () => {
    clearErrors();
    setSuccessMessage('');
    const errores: ValidationError[] = [];
    if (!prog.localId) errores.push({ propertyName: "EquipoLocalId", message: "Seleccione equipo local" });
    if (!prog.visitanteId) errores.push({ propertyName: "EquipoVisitanteId", message: "Seleccione equipo visitante" });
    if (!prog.fecha) errores.push({ propertyName: "Fecha", message: "La fecha es obligatoria" });

    if (errores.length > 0) {
        const validationResult: ApiResponse<null> = {
            isSuccess: false,
            isFailure: true,
            code: 422,
            errors: errores,
            errorMessage: "Complete los campos requeridos",
            value: null
        };
      handleResultErrors(validationResult);
      return;
    }

    const fechaISO = prog.fecha ?  new Date(`${prog.fecha}T00:00:00`).toISOString() : null;
    
    try
    {
      const res = await partidoService.crear({ equipoLocalId: prog.localId || null, equipoVisitanteId: prog.visitanteId || null, fecha: fechaISO || null }, idempotencyKey);

      if (res.isSuccess) {
        setSuccessMessage('Partido creado correctamente.');
        setIdempotencyKey(crypto.randomUUID());
      } else {
        handleResultErrors(res);
      }
    }
    catch (err: unknown) {
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
    }
  };

  return (
    <div className="max-w-md mx-auto p-6 space-y-6">
      <header className="border-b border-gray-100 pb-4 text-center">
        <h1 className="text-2xl font-black text-gray-900 tracking-tighter uppercase italic">
          Programar Partido
        </h1>
      </header>

      <div className="bg-white p-6 rounded-[2rem] shadow-sm border border-gray-100 space-y-6">
        <SuccessMessage message={successMessage} />
        <ErrorMessage messages={businessErrors} />

        <div className="space-y-4">
          <div>
            <label className="block text-[10px] font-black text-gray-500 uppercase mb-1.5 ml-1 tracking-widest">Equipo Local</label>
            <select 
              className={`w-full p-3 bg-white border rounded-xl text-xs font-bold outline-none transition-all shadow-sm ${
                getMessageFor('EquipoLocalId') ? 'border-red-400 ring-2 ring-red-50' : 'border-gray-200 focus:border-indigo-500 focus:ring-4 focus:ring-indigo-50'
              }`} 
              value={prog.localId} 
              onChange={e => setProg({...prog, localId: e.target.value})}
            >
              <option value="">Seleccionar...</option>
              {equipos.map(e => <option key={e.id} value={e.id}>{e.nombre}</option>)}
            </select>
            {getMessageFor('EquipoLocalId') && <p className="text-red-500 text-[9px] font-black mt-1.5 ml-1">{getMessageFor('EquipoLocalId')}</p>}
          </div>

          <div>
            <label className="block text-[10px] font-black text-gray-500 uppercase mb-1.5 ml-1 tracking-widest">Equipo Visitante</label>
            <select 
              className={`w-full p-3 bg-white border rounded-xl text-xs font-bold outline-none transition-all shadow-sm ${
                getMessageFor('EquipoVisitanteId') ? 'border-red-400 ring-2 ring-red-50' : 'border-gray-200 focus:border-indigo-500 focus:ring-4 focus:ring-indigo-50'
              }`} 
              value={prog.visitanteId} 
              onChange={e => setProg({...prog, visitanteId: e.target.value})}
            >
              <option value="">Seleccionar...</option>
              {equipos.map(e => e.id !== prog.localId && <option key={e.id} value={e.id}>{e.nombre}</option>)}
            </select>
            {getMessageFor('EquipoVisitanteId') && <p className="text-red-500 text-[9px] font-black mt-1.5 ml-1">{getMessageFor('EquipoVisitanteId')}</p>}
          </div>

          <div>
            <label className="block text-[10px] font-black text-gray-500 uppercase mb-1.5 ml-1 tracking-widest">Fecha del Partido</label>
            <input 
              type="date" 
              className={`w-full p-3 bg-white border rounded-xl text-xs font-bold outline-none transition-all shadow-sm ${
                getMessageFor('Fecha') ? 'border-red-400 ring-2 ring-red-50' : 'border-gray-200 focus:border-indigo-500 focus:ring-4 focus:ring-indigo-50'
              }`} 
              value={prog.fecha} 
              onChange={e => setProg({...prog, fecha: e.target.value})} 
            />
            {getMessageFor('Fecha') && <p className="text-red-500 text-[9px] font-black mt-1.5 ml-1">{getMessageFor('Fecha')}</p>}
          </div>

          <button 
            onClick={handleProgramar} 
            className="w-full py-4 bg-indigo-600 text-white rounded-2xl text-[11px] font-black uppercase tracking-widest shadow-lg shadow-indigo-100 hover:bg-indigo-700 transition-all active:scale-95"
          >
            Confirmar Registro
          </button>
        </div>
      </div>
    </div>
  );
}