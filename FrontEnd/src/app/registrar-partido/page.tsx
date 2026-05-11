'use client';

import { useState, useEffect, useCallback } from 'react';
import { partidoService } from '@/services/partido.service';
import { jugadorService } from '@/services/jugador.service';
import { PartidoReadModel, RegistrarResultadoModel, ApiResponse, ValidationError, JugadorReadModel } from '@/types/api';
import SelectorGoleadores from '@/components/SelectorGoleadores';
import ConfirmModal from '@/components/ui/ConfirmModal';
import { useFormErrors } from '@/app/hooks/use-form-errors';
import ErrorMessage from '@/components/ui/ErrorMessage';
import axios from 'axios';
import SuccessMessage from '@/components/ui/SuccessMessage';

export default function ResultadosGestionPage() {  
  const [partidosPendientes, setPartidosPendientes] = useState<PartidoReadModel[]>([]);
  const [partidoSel, setPartidoSel] = useState<PartidoReadModel | null>(null);
  const [loading, setLoading] = useState(true);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  const [editandoFecha, setEditandoFecha] = useState(false);
  const [nuevaFecha, setNuevaFecha] = useState('');
  const [eliminarPartido, setEliminarPartido] = useState(false);

  const [jugadoresLocal, setJugadoresLocal] = useState<JugadorReadModel[]>([]);
  const [jugadoresVisitante, setJugadoresVisitante] = useState<JugadorReadModel[]>([]);
  const [golesL, setGolesL] = useState(0);
  const [golesV, setGolesV] = useState(0);
  const [idsL, setIdsL] = useState<string[]>([]);
  const [idsV, setIdsV] = useState<string[]>([]);

  const [showConfirm, setShowConfirm] = useState(false); 

  const { handleResultErrors, getMessageFor, businessErrors, clearErrors } = useFormErrors(['Fecha', 'GoleadoresLocalIds', 'GoleadoresVisitanteIds']);

  const cargarPendientes = useCallback(async () => {
    setLoading(true);
    try {
      const resP = await partidoService.getPendientes();
      if (resP.isSuccess) 
        setPartidosPendientes(resP.value || []);
      else
        handleResultErrors(resP);
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
    } finally {
      setLoading(false);
    }
  }, [handleResultErrors]);

  useEffect(() => { cargarPendientes(); }, [cargarPendientes]);

  const handleSelectPartido = async (id: string) => {
    const p = partidosPendientes.find(x => x.id === id) || null;
    setPartidoSel(p);
    setEditandoFecha(false);
    setEliminarPartido(false);
    clearErrors();
    setSuccessMsg(null);
    setGolesL(0); setGolesV(0); setIdsL([]); setIdsV([]);
    
    if (p) {
      setNuevaFecha(p.fecha.split('T')[0]);
      const [resL, resV] = await Promise.all([
        jugadorService.getByEquipo(p.equipoLocalId, 1, ''),
        jugadorService.getByEquipo(p.equipoVisitanteId, 1, '')
      ]);
      if (resL.isSuccess) setJugadoresLocal(resL.value?.data || []);
      if (resV.isSuccess) setJugadoresVisitante(resV.value?.data || []);
    }
  };

  const handleCambiarFecha = async () => {
    if (!partidoSel) return;
    clearErrors();

    if (!nuevaFecha) {
      handleResultErrors({
        isSuccess: false, isFailure: true, code: 422,
        errorMessage: "La fecha es requerida",
        errors: [{ propertyName: "Fecha", message: "Debe seleccionar una fecha válida." }],
        value: undefined
      } as ApiResponse<void>);
      return;
    }

    const isoFecha = new Date(`${nuevaFecha}T00:00:00`).toISOString();
    
    try
    {
      const res = await partidoService.updateFecha(partidoSel.id, isoFecha);
      if (res.isSuccess) {
        setEditandoFecha(false);
        setSuccessMsg("Fecha actualizada");
        cargarPendientes();
        setTimeout(() => setSuccessMsg(null), 3000);
      } 
      else
        handleResultErrors(res);
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

  const eliminarRegistro = async () => {
    if (!partidoSel || saving) return;
    
    try
    {
      const res = await partidoService.delete(partidoSel.id);
      debugger;
      if (res.isSuccess) {
        setShowConfirm(false);
        setEliminarPartido(false);
        setSuccessMsg("Partido eliminado correctamente.");
        setTimeout(() => window.location.reload(), 2000);
      } else {
        setEliminarPartido(false);
        setShowConfirm(false);
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
  }

  const ejecutarRegistro = async () => {
    if (!partidoSel || saving) return;

    const erroresManuales: ValidationError[] = [];
    for (let i = 0; i < golesL; i++) {
      if (!idsL[i]) erroresManuales.push({ propertyName: "GoleadoresLocalIds", message: `Falta autor gol ${i + 1} de ${partidoSel.local}` });
    }
    for (let i = 0; i < golesV; i++) {
      if (!idsV[i]) erroresManuales.push({ propertyName: "GoleadoresVisitanteIds", message: `Falta autor gol ${i + 1} de ${partidoSel.visitante}` });
    }

    if (erroresManuales.length > 0) {
      handleResultErrors({
        isSuccess: false, isFailure: true, code: 422,
        errorMessage: "Asignación de goles incompleta",
        errors: erroresManuales, value: undefined
      } as ApiResponse<void>);
      setShowConfirm(false);
      return;
    }

    setSaving(true);
    const command: RegistrarResultadoModel = {
      partidoId: partidoSel.id,
      golesLocal: golesL,
      golesVisitante: golesV,
      goleadoresLocalIds: idsL.filter(id => id),
      goleadoresVisitanteIds: idsV.filter(id => id)
    };

    try
    {
      const res = await partidoService.registrarResultado(command);
      if (res.isSuccess) {
        setShowConfirm(false);
        setSuccessMsg("Marcador oficial registrado");
        setTimeout(() => window.location.reload(), 2000);
      } else {
        setSaving(false);
        setShowConfirm(false);
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

  if (loading && partidosPendientes.length === 0) {
    return (
      <div className="p-20 text-center font-bold text-gray-300 animate-pulse uppercase tracking-widest text-xs">
        Sincronizando Registro de Partidos...
      </div>
    );
  }

  return (
    <div className="max-w-5xl mx-auto p-6 space-y-6">
      <header className="flex justify-between items-end border-b border-gray-100 pb-4">
        <div>
          <h1 className="text-2xl font-black text-gray-900 tracking-tighter uppercase italic">Registro de Partidos</h1>
        </div>
      </header>

      <ErrorMessage messages={businessErrors} />
      <SuccessMessage message={successMsg} />

      <section className="bg-white rounded-[2rem] border border-gray-100 shadow-sm p-6">
        <div className="mb-6">
          <label className="block text-[10px] font-black text-gray-400 uppercase mb-2 ml-1 tracking-widest">Encuentro en curso</label>
          <select 
            className="w-full p-3 bg-white border border-gray-200 rounded-xl text-xs font-bold outline-none focus:ring-4 focus:ring-indigo-50 transition-all"
            onChange={(e) => handleSelectPartido(e.target.value)}
            value={partidoSel?.id || ''}
          >
            <option value="">-- Seleccionar partido pendiente --</option>
            {partidosPendientes.map(p => (
              <option key={p.id} value={p.id}>{p.local} vs {p.visitante} — [{new Date(p.fecha).toLocaleDateString('es-ES')}]</option>
            ))}
          </select>
          
          {partidoSel && (
            <div className="mt-3 ml-1">
              {!editandoFecha ? (
                <button onClick={() => setEditandoFecha(true)} className="text-[10px] font-black text-indigo-500 uppercase tracking-tighter hover:underline">✏️ Reprogramar</button>
              ) : (
                <div className="flex items-center gap-2 animate-in fade-in">
                  <input type="date" className="p-2 bg-white border border-gray-200 rounded-xl text-[10px] font-bold outline-none" value={nuevaFecha} onChange={e => setNuevaFecha(e.target.value)} />
                  <button onClick={handleCambiarFecha} className="bg-indigo-600 text-white px-3 py-1.5 rounded-lg text-[10px] font-black uppercase">OK</button>
                  <button onClick={() => {setEditandoFecha(false); clearErrors();}} className="bg-gray-100 text-gray-400 px-3 py-1.5 rounded-lg text-[10px] font-black uppercase">X</button>
                </div>
              )}
              {getMessageFor('Fecha') && <p className="text-red-500 text-[9px] font-bold mt-1">{getMessageFor('Fecha')}</p>}
              &nbsp;&nbsp;
              <button onClick={() => setEliminarPartido(true)}  className="text-[10px] font-black text-indigo-500 uppercase tracking-tighter hover:underline">🗑️ Eliminar</button>
            </div>
          )}
        </div>

        {partidoSel ? (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 animate-in slide-in-from-bottom-2">
            <div className="bg-gray-50/50 p-5 rounded-[1.5rem] border border-gray-100">
              <div className="flex justify-between items-center mb-4">
                <span className="text-[10px] font-black text-indigo-600 tracking-tight truncate pr-2">{partidoSel.local}</span>
                <input 
                  type="number" min="0" 
                  className={`w-16 p-2 text-center text-3xl font-black bg-white border border-gray-200 rounded-xl shadow-sm focus:ring-4 focus:ring-indigo-50 outline-none transition-all ${
                      getMessageFor('GolesLocal') 
                        ? 'border-red-400 ring-2 ring-red-50' 
                        : 'border-gray-200 focus:border-indigo-500 focus:ring-4 focus:ring-indigo-50'
                  }`}
                  value={golesL} onChange={(e) => setGolesL(Number(e.target.value))}
                />
                {getMessageFor('GolesLocal') && <p className="text-red-500 text-[9px] font-black mt-2">{getMessageFor('GolesLocal')}</p>}
              </div>
              <SelectorGoleadores label="Anotadores Local" cantidadGoles={golesL} jugadores={jugadoresLocal} selectedIds={idsL} onChange={setIdsL} />
              {getMessageFor('GoleadoresLocalIds') && <p className="text-red-500 text-[9px] font-black mt-2">{getMessageFor('GoleadoresLocalIds')}</p>}
            </div>

            <div className="bg-gray-50/50 p-5 rounded-[1.5rem] border border-gray-100">
              <div className="flex justify-between items-center mb-4">
                <span className="text-[10px] font-black text-red-600 tracking-tight truncate pr-2">{partidoSel.visitante}</span>
                <input 
                  type="number" min="0" 
                  className={`w-16 p-2 text-center text-3xl font-black bg-white border border-gray-200 rounded-xl shadow-sm focus:ring-4 focus:ring-red-50 outline-none transition-all ${
                    getMessageFor('GolesLocal') 
                        ? 'border-red-400 ring-2 ring-red-50' 
                        : 'border-gray-200 focus:border-indigo-500 focus:ring-4 focus:ring-indigo-50'
                  }`}
                  value={golesV} onChange={(e) => setGolesV(Number(e.target.value))}
                />
                {getMessageFor('GolesVisitante') && <p className="text-red-500 text-[9px] font-black mt-2">{getMessageFor('GolesVisitante')}</p>}
              </div>
              <SelectorGoleadores label="Anotadores Visita" cantidadGoles={golesV} jugadores={jugadoresVisitante} selectedIds={idsV} onChange={setIdsV} />
              {getMessageFor('GoleadoresVisitanteIds') && <p className="text-red-500 text-[9px] font-black mt-2">{getMessageFor('GoleadoresVisitanteIds')}</p>}
            </div>

            <button 
              onClick={() => setShowConfirm(true)}
              disabled={saving}
              className="md:col-span-2 py-4 bg-gray-900 text-white rounded-2xl font-black uppercase text-[11px] tracking-[0.2em] shadow-lg hover:bg-black transition-all disabled:opacity-50 active:scale-[0.98]"
            >
              {saving ? 'PROCESANDO...' : 'CERRAR Y FINALIZAR PARTIDO'}
            </button>
          </div>
        ) : (
          <div className="p-20 text-center border-2 border-dashed rounded-[2rem] border-gray-100 text-gray-300">
            <p className="text-[10px] font-black uppercase tracking-widest italic">Seleccione encuentro para registrar el resultado</p>
          </div>
        )}
      </section>

      <ConfirmModal 
        isOpen={showConfirm || eliminarPartido }
        title= "Validación Final"
        confirmText='Confirmar'
        message={eliminarPartido 
                ? <span>¿Confirma la eliminación del partido entre <strong>{partidoSel?.local} vs {partidoSel?.visitante}</strong>?</span>
                : <span>¿Confirma el cierre del partido entre <strong>{partidoSel?.local} vs {partidoSel?.visitante}</strong>?</span>}
        onConfirm={eliminarPartido ? eliminarRegistro : ejecutarRegistro}
        onCancel={() => {
            setShowConfirm(false);
            setEliminarPartido(false);
          }
        }
      />
    </div>
  );
}