import './globals.css';
import Navbar from '@/components/layout/Navbar';

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="es">
      <body className="bg-gray-50 text-gray-900 min-h-screen">
        
        <Navbar />
        
        <main>
          {children}
        </main>
      </body>
    </html>
  );
}