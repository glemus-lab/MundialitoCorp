'use client';

interface Props {
  cantidadGoles: number;
  jugadores: { id: string, nombre: string }[];
  selectedIds: string[];
  onChange: (ids: string[]) => void;
  label: string;
}

export default function SelectorGoleadores({ cantidadGoles, jugadores = [], selectedIds, onChange, label }: Props) {
  if (cantidadGoles <= 0) return null;

  const handleSelectChange = (index: number, value: string) => {
    const newIds = [...selectedIds];
    newIds[index] = value;
    onChange(newIds);
  };

  return (
    <div className="mt-2 space-y-1">
      <p className="text-[9px] font-black text-gray-400 uppercase tracking-tighter mb-1 ml-1">{label}</p>
      <div className={`grid ${cantidadGoles > 3 ? 'grid-cols-2' : 'grid-cols-1'} gap-1`}>
        {Array.from({ length: cantidadGoles }).map((_, i) => (
          <select
            key={i}
            className={`w-full p-1.5 text-[11px] border rounded-lg outline-none transition-all ${
              !selectedIds[i] ? 'border-amber-300 bg-amber-50' : 'border-gray-200 bg-white'
            }`}
            value={selectedIds[i] || ''}
            onChange={(e) => handleSelectChange(i, e.target.value)}
          >
            <option value="">¿Quién anotó?</option>
            {jugadores.map(j => (
              <option key={j.id} value={j.id}>{j.nombre}</option>
            ))}
          </select>
        ))}
      </div>
    </div>
  );
}