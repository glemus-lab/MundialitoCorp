'use client';

import { useEffect, useState, useCallback } from 'react';
import { jugadorService } from '@/services/jugador.service';
import { JugadorReadModel, PagedList, ApiResponse } from '@/types/api';

import ErrorMessage from '@/components/ui/ErrorMessage';
import ConfirmModal from './ui/ConfirmModal';
import { useFormErrors } from '@/app/hooks/use-form-errors';
import SuccessMessage from './ui/SuccessMessage';

export default function SeccionJugadores({ equipoId }: { equipoId: string }) {
  const [paged, setPaged] = useState<PagedList<JugadorReadModel> | null>(null);
  const [nombreNuevo, setNombreNuevo] = useState('');
  const [jugadorParaBorrar, setJugadorParaBorrar] = useState<{id: string, nombre: string} | null>(null);

  const [editandoId, setEditandoId] = useState<string | null>(null);
  const [nombreEditado, setNombreEditado] = useState('');
  const [idempotencyKey, setIdempotencyKey] = useState("");
  
  const [tempFilter, setTempFilter] = useState(''); 
  const [filter, setFilter] = useState('');
  const [page, setPage] = useState(1);
  
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [showModal, setShowModal] = useState(false);
  const [successMessage, setsuccessMessage] = useState('');

  const { handleResultErrors, getMessageFor, businessErrors, clearErrors } = useFormErrors(['Nombre']);

  const cargar = useCallback(async () => {
    setLoading(true);
    try {
      const res = await jugadorService.getByEquipo(equipoId, page, filter);
      if (res.isSuccess) {
        setIdempotencyKey(crypto.randomUUID());
        setPaged(res.value);
      }
      else{
        handleResultErrors(res);
      }
    } finally {
      setLoading(false);
    }
  }, [equipoId, page, filter, handleResultErrors]);

  useEffect(() => {
    cargar();
  }, [cargar]);

  const aplicarBusqueda = () => {
    setPage(1);
    setFilter(tempFilter);
  };

  const agregar = async () => {
    clearErrors();
    setsuccessMessage('');
    if (!nombreNuevo.trim()) {
      const errorManual: ApiResponse<unknown> = {
        isSuccess: false,
        isFailure: true,
        code: 422,
        errorMessage: "Error de validación",
        errors: [{ propertyName: "Nombre", message: "Escriba el nombre del integrante." }],
        value: null
      };
      handleResultErrors(errorManual);
      return;
    }

    setSaving(true);
    const res = await jugadorService.create(equipoId, nombreNuevo, idempotencyKey);
    if (res.isSuccess) {
      setNombreNuevo('');
      setTempFilter('');
      setFilter('');
      setPage(1);
      cargar();
      setsuccessMessage('Jugador creado correctamente.');
      setIdempotencyKey(crypto.randomUUID());
    } else {
      handleResultErrors(res);
    }
    setSaving(false);
  };

const iniciarEdicion = (jugador: JugadorReadModel) => {
  clearErrors();
  setEditandoId(jugador.id);
  setNombreEditado(jugador.nombre);
};

const cancelarEdicion = () => {
  setEditandoId(null);
  setNombreEditado('');
  clearErrors();
};

const guardarEdicion = async (id: string) => {
  setsuccessMessage('');
  if (!nombreEditado.trim()) return;
  
  setSaving(true);
  const res = await jugadorService.update(id, nombreEditado);
  if (res.isSuccess) {
    setEditandoId(null);
    setsuccessMessage('Jugador actualizado correctamente.')
    await cargar();
  } else {
    handleResultErrors(res);
  }
  setSaving(false);
};

  const borrar = async () => {
    setsuccessMessage('');
    if (!jugadorParaBorrar) return;
    try {
      const res = await jugadorService.delete(jugadorParaBorrar.id);
      if (res.isSuccess) {
        setShowModal(false);
        setPage(1);
        setsuccessMessage('Jugador eliminado correctamente.')
        await cargar();
      } else {
        setShowModal(false);
      }
    } catch {  }
  };

  return (
    <div className="space-y-4">
      <div className="bg-gray-50 p-3 rounded-2xl border border-gray-100">
        <SuccessMessage message={successMessage} />
        <ErrorMessage messages={businessErrors} />
        <div className="flex gap-2">
          <input 
            type="text" 
            placeholder="Nombre del nuevo integrante..."
            className={`flex-1 p-2.5 bg-white border rounded-xl text-xs font-bold outline-none transition-all shadow-sm ${
              getMessageFor('Nombre') ? 'border-red-400 ring-2 ring-red-50' : 'border-gray-300 focus:border-indigo-500 focus:ring-4 focus:ring-indigo-50'
            }`}
            value={nombreNuevo}
            onChange={(e) => setNombreNuevo(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && agregar()}
          />
          <button 
            onClick={agregar}
            disabled={saving}
            className="px-6 bg-indigo-600 text-white rounded-xl text-[10px] font-black uppercase tracking-widest hover:bg-indigo-700 transition-all shadow-md active:scale-95"
          >
            Inscribir
          </button>
        </div>
        {getMessageFor('Nombre') && (
            <p className="text-red-500 text-[9px] font-black mt-2 ml-1 animate-in slide-in-from-top-1">
                {getMessageFor('Nombre')}
            </p>
        )}
      </div>

      <div className="flex gap-2 bg-white p-3 rounded-[1.5rem] border border-gray-100 shadow-sm">
        <input 
          type="text" 
          placeholder="Filtrar por nombre..." 
          className="flex-grow p-2.5 bg-gray-50 border-none rounded-xl text-xs outline-none focus:ring-2 focus:ring-indigo-100 transition-all"
          value={tempFilter} 
          onChange={(e) => setTempFilter(e.target.value)}
          onKeyDown={(e) => e.key === 'Enter' && aplicarBusqueda()}
        />
        <button 
          onClick={aplicarBusqueda} 
          className="bg-gray-900 text-white px-5 py-2 rounded-xl text-[10px] font-black uppercase hover:bg-black transition-all"
        >
          Filtrar
        </button>
      </div>

      <div className="bg-white rounded-[2rem] border border-gray-100 overflow-hidden shadow-sm">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="bg-gray-50/50 border-b border-gray-100 text-[10px] font-black text-gray-400 uppercase tracking-widest">
              <th className="p-5">Integrante</th>
              <th className="p-5 text-center">Goles</th>
              <th className="p-5 text-right">Acciones</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {loading ? (
              <tr><td colSpan={3} className="p-10 text-center text-[10px] font-bold text-gray-400 animate-pulse uppercase tracking-widest">Actualizando...</td></tr>
            ) : (
              paged?.data.map((j) => (
                <tr key={j.id} className="hover:bg-gray-50/50 transition-colors group">
                  <td className="p-5">
                    {editandoId === j.id ? (
                      <div className="flex flex-col">
                        <input
                          type="text"
                          className={`p-1.5 border rounded-lg text-xs font-bold outline-none ${
                            getMessageFor('Nombre') ? 'border-red-400' : 'border-indigo-300'
                          }`}
                          value={nombreEditado}
                          onChange={(e) => setNombreEditado(e.target.value)}
                          autoFocus
                        />
                        {getMessageFor('Nombre') && (
                          <span className="text-red-500 text-[8px] mt-1">{getMessageFor('Nombre')}</span>
                        )}
                      </div>
                    ) : (
                      <span className="font-bold text-gray-700 text-xs tracking-tight">{j.nombre}</span>
                    )}
                  </td>
                  <td className="p-5 text-center">
                    <span className="text-xs font-black text-indigo-600 bg-indigo-50 px-2 py-1 rounded-md">{j.golesAnotados}</span>
                  </td>
                  <td className="p-5 text-right">
                    <div className="flex justify-end gap-2">
                      {editandoId === j.id ? (
                        <>
                          <button
                            onClick={() => guardarEdicion(j.id)}
                            disabled={saving}
                            className="px-3 py-1.5 bg-green-600 text-white rounded-lg text-[9px] font-black uppercase hover:bg-green-700 shadow-sm"
                          >
                            √
                          </button>
                          <button
                            onClick={cancelarEdicion}
                            className="px-3 py-1.5 bg-gray-200 text-gray-600 rounded-lg text-[9px] font-black uppercase hover:bg-gray-300 shadow-sm"
                          >
                            X
                          </button>
                        </>
                      ) : (
                        <>
                          <button
                            onClick={() => iniciarEdicion(j)}
                            className="inline-flex px-3 py-1.5 bg-indigo-50 text-indigo-600 rounded-lg text-[9px] font-black uppercase hover:bg-indigo-600 hover:text-white transition-all shadow-sm"
                          >
                            Editar
                          </button>
                          <button
                            onClick={() => { setJugadorParaBorrar({id: j.id, nombre: j.nombre}); setShowModal(true); }}
                            className="px-3 py-1.5 bg-white border border-red-100 text-red-500 rounded-lg text-[9px] font-black uppercase hover:bg-red-500 hover:text-white transition-all shadow-sm"
                          >
                            Borrar
                          </button>
                        </>
                      )}
                    </div>
                  </td>
                </tr>
              ))

            )}
            {!loading && (!paged || paged.data.length === 0) && (
              <tr><td colSpan={3} className="p-10 text-center text-[10px] text-gray-300 font-bold uppercase italic tracking-widest">Sin integrantes</td></tr>
            )}
          </tbody>
        </table>

        <div className="p-4 bg-gray-50/50 border-t border-gray-100 flex justify-between items-center">
          <span className="text-[10px] font-black text-gray-400 uppercase tracking-widest">
            Pág {paged?.pageNumber || 0} de {paged?.totalPages || 0}<br></br>
            Total Registros: {paged?.totalRecords}
          </span>
          <div className="flex gap-2">
            <button 
              disabled={page <= 1 || loading} 
              onClick={() => setPage(page - 1)}
              className="px-4 py-2 bg-white border border-gray-300 rounded-xl text-[10px] font-black uppercase hover:border-indigo-300 disabled:opacity-30 transition-all shadow-sm"
            >
              Anterior
            </button>
            <button 
              disabled={page >= (paged?.totalPages || 1) || loading} 
              onClick={() => setPage(page + 1)}
              className="px-4 py-2 bg-white border border-gray-300 rounded-xl text-[10px] font-black uppercase hover:border-indigo-300 disabled:opacity-30 transition-all shadow-sm"
            >
              Siguiente
            </button>
          </div>
        </div>
      </div>

      <ConfirmModal 
              isOpen={showModal}
              title="Eliminar Registro"
              message={<span>¿Confirmas la eliminación del jugador <strong>{jugadorParaBorrar?.nombre}</strong>? Esta acción no se puede deshacer.</span>}
              onConfirm={borrar}
              onCancel={() => setShowModal(false)}
            />
            
    </div>
  );
}