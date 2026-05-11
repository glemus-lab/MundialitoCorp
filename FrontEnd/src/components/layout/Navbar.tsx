'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';

export default function Navbar() {
  const pathname = usePathname();

  const links = [
    { name: 'TABLA DE POSICIONES', href: '/' },
    { name: 'EQUIPOS', href: '/equipos' },
    { name: 'PROGRAMACIÓN', href: '/programacion' },
    { name: 'ADMINISTRAR PARTIDOS', href: '/registrar-partido' },
    { name: 'RESULTADOS', href: '/resultados' },
    { name: 'GOLEADORES', href: '/goleadores' },
  ];

  return (
    <nav className="sticky top-0 z-50 w-full bg-white/90 backdrop-blur-md border-b border-gray-100 shadow-sm">
      <div className="max-w-6xl mx-auto px-6">
        <div className="flex h-14 items-center justify-between">
          <div className="flex items-center gap-2">
            <div className="w-7 h-7 bg-gray-900 rounded-lg flex items-center justify-center text-white text-xs shadow-md">
              ⚽
            </div>
            <div className="font-black text-gray-900 text-base tracking-tighter uppercase italic">
              Mundialito<span className="text-indigo-600 not-italic">CORP</span>
            </div>
          </div>

          <div className="flex items-center gap-0.5">
            {links.map((link) => {
              const isActive = pathname === link.href;
              return (
                <Link
                  key={link.href}
                  href={link.href}
                  className={`px-3 py-2 rounded-xl text-[10px] font-black tracking-widest transition-all duration-200 ${
                    isActive 
                      ? 'bg-indigo-600 text-white shadow-lg shadow-indigo-100' 
                      : 'text-gray-400 hover:bg-gray-50 hover:text-gray-900'
                  }`}
                >
                  {link.name}
                </Link>
              );
            })}
          </div>
        </div>
      </div>
    </nav>
  );
}