'use client';

import { useEffect, useState, useCallback } from 'react';
import Link from 'next/link';
import { equipoService } from '@/services/equipo.service';
import { ApiResponse, EquipoPagedReadModel, PagedList } from '@/types/api';
import ConfirmModal from '@/components/ui/ConfirmModal';
import ErrorMessage from '@/components/ui/ErrorMessage';
import { useFormErrors } from '../hooks/use-form-errors';
import axios from 'axios';

export default function EquiposIndex() {
  const [pagedList, setPagedList] = useState<PagedList<EquipoPagedReadModel> | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const { handleResultErrors, businessErrors, clearErrors } = useFormErrors();

  const [showModal, setShowModal] = useState(false);
  const [equipoParaBorrar, setEquipoParaBorrar] = useState<{id: string, nombre: string} | null>(null);

  const [params, setParams] = useState({ page: 1, size: 5, sortBy: 'Nombre', dir: 'ASC', filter: '' });
  const [tempFilter, setTempFilter] = useState('');

  const cargarEquipos = useCallback(async () => {
    setLoading(true);
    clearErrors();
    
    try {
      const result = await equipoService.getPaged(params.page, params.size, params.sortBy, params.dir, params.filter);
      if (result.isSuccess)
        setPagedList(result.value);
      else
      {
          handleResultErrors(result);
      }
    }
    catch (err: unknown) {
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
    finally 
    { 
      setLoading(false); 
    }
  }, [params, clearErrors, handleResultErrors]);

  useEffect(() => { cargarEquipos(); }, [cargarEquipos]);

  const aplicarBusqueda = () => setParams({ ...params, filter: tempFilter, page: 1 });

  const ejecutarBorrado = async () => {
    if (!equipoParaBorrar) return;
    try {
      const res = await equipoService.delete(equipoParaBorrar.id);
      if (res.isSuccess) {
        setShowModal(false);
        await cargarEquipos();
      } else {
        setShowModal(false);
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

  return (
    <div className="max-w-5xl mx-auto p-6 space-y-6">
      
      <header className="flex justify-between items-end border-b border-gray-100 pb-4">
        <div>
          <h1 className="text-2xl font-black text-gray-900 tracking-tighter uppercase italic">eQUIPOS</h1>
        </div>
        <Link href="/equipos/nuevo" className="bg-indigo-600 text-white px-5 py-2.5 rounded-2xl text-[10px] font-black uppercase tracking-widest shadow-lg shadow-indigo-100 hover:bg-indigo-700 transition-all active:scale-95">
          + Nuevo Equipo
        </Link>
      </header>
      
      <ErrorMessage messages={businessErrors} />
      
      <div className="grid grid-cols-1 md:grid-cols-12 gap-3 items-center bg-white p-3 rounded-[1.5rem] border border-gray-100 shadow-sm">
        <div className="md:col-span-6 flex gap-2">
          <input 
            type="text" placeholder="Buscar por nombre..." 
            className="flex-grow p-2.5 bg-gray-50 border-none rounded-xl text-xs outline-none focus:ring-2 focus:ring-indigo-100 transition-all"
            value={tempFilter} onChange={(e) => setTempFilter(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && aplicarBusqueda()}
          />
          <button onClick={aplicarBusqueda} className="bg-gray-900 text-white px-5 py-2 rounded-xl text-[10px] font-black uppercase hover:bg-black transition-all">Filtrar</button>
        </div>
        <div className="md:col-span-3">
          <select 
            className="w-full p-2.5 bg-gray-50 border-none rounded-xl text-xs font-bold text-gray-500 outline-none focus:ring-2 focus:ring-indigo-100"
            value={params.sortBy} onChange={(e) => setParams({ ...params, sortBy: e.target.value })}
          >
            <option value="nombre">Orden: Nombre</option>
            <option value="puntos">Orden: Puntos</option>
            <option value="partidosjugados">Orden: Partidos</option>
          </select>
        </div>
        <div className="md:col-span-3">
          <select 
            className="w-full p-2.5 bg-gray-50 border-none rounded-xl text-xs font-bold text-gray-500 outline-none focus:ring-2 focus:ring-indigo-100"
            value={params.dir} onChange={(e) => setParams({ ...params, dir: e.target.value })}
          >
            <option value="ASC">Dirección: ASCENDENTE ↑</option>
            <option value="DESC">Dirección: DESCENDENTE ↓</option>
          </select>
        </div>
      </div>

      <div className="bg-white rounded-[2rem] border border-gray-100 overflow-hidden shadow-sm">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="bg-gray-50/50 border-b border-gray-100 text-[10px] font-black text-gray-400 uppercase tracking-widest">
              <th className="p-5">Nombre del Equipo</th>
              <th className="p-5 text-center">Puntos</th>
              <th className="p-5 text-center">Partidos Jugados</th>
              <th className="p-5 text-right">ACCIONES</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-50">
            {loading ? (
              <tr><td colSpan={4} className="p-16 text-center text-gray-400 animate-pulse text-xs font-bold uppercase">Actualizando listado...</td></tr>
            ) : pagedList?.data.map((e) => (
              <tr key={e.id} className="hover:bg-gray-50/50 transition-colors group">
                <td className="p-5"><span className="font-bold text-gray-700 text-xs tracking-tight">{e.nombre}</span></td>
                <td className="p-5 text-center"><span className="font-bold text-gray-700 text-xs tracking-tight">{e.puntos}</span></td>
                <td className="p-5 text-center"><span className="font-bold text-gray-700 text-xs tracking-tight">{e.partidosJugados}</span></td>
                <td className="p-5 text-right space-x-1">
                  <Link 
                    href={`/equipos/${e.id}`}
                    className="inline-flex px-3 py-1.5 bg-indigo-50 text-indigo-600 rounded-lg text-[9px] font-black uppercase hover:bg-indigo-600 hover:text-white transition-all shadow-sm"
                  >
                    Editar
                  </Link>
                  <button 
                    onClick={() => { setEquipoParaBorrar({id: e.id, nombre: e.nombre}); setShowModal(true); }}
                    className="px-3 py-1.5 bg-white border border-red-100 text-red-500 rounded-lg text-[9px] font-black uppercase hover:bg-red-500 hover:text-white transition-all"
                  >
                    Borrar
                  </button>
                </td>
              </tr>
            ))}
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
              onClick={() => setParams({ ...params, page: params.page - 1 })}
              className="px-4 py-2 bg-white border border-gray-200 rounded-xl text-[10px] font-black uppercase hover:border-indigo-300 disabled:opacity-30 transition-all shadow-sm"
            >
              Anterior
            </button>
            <button 
              disabled={params.page >= (pagedList?.totalPages || 1) || loading} 
              onClick={() => setParams({ ...params, page: params.page + 1 })}
              className="px-4 py-2 bg-white border border-gray-200 rounded-xl text-[10px] font-black uppercase hover:border-indigo-300 disabled:opacity-30 transition-all shadow-sm"
            >
              Siguiente
            </button>
          </div>
        </div>
      </div>

      <ConfirmModal 
        isOpen={showModal}
        title="Eliminar Registro"
        message={<span>¿Confirmas la eliminación del equipo <strong>{equipoParaBorrar?.nombre}</strong>? Esta acción no se puede deshacer.</span>}
        onConfirm={ejecutarBorrado}
        onCancel={() => setShowModal(false)}
      />
    </div>
  );
}