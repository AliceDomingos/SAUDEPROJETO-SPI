import { Outlet, useNavigate, Link, useLocation } from 'react-router-dom';
import { useAuthStore } from '@/shared/store/authStore';
import {
  LayoutDashboard,
  ClipboardList,
  ClipboardCheck,
  Layers3,
  Stethoscope,
  Users,
  UserCog,
  LogOut,
  Menu,
  X,
} from 'lucide-react';
import { useState } from 'react';

const navItems = [
  { to: '/', icon: LayoutDashboard, label: 'Dashboard', permission: 'canViewDashboard' },
  { to: '/avaliacoes', icon: ClipboardList, label: 'Avaliacoes', permission: 'canViewEvaluations' },
  { to: '/formularios', icon: ClipboardCheck, label: 'Formularios', permission: 'canViewForms' },
  { to: '/especialistas', icon: Stethoscope, label: 'Especialistas', permission: 'canViewSpecialists' },
  { to: '/pacientes', icon: Users, label: 'Pacientes', permission: 'canViewPatients' },
  { to: '/grupos', icon: Layers3, label: 'Grupos', permission: 'canManageGroups' },
  { to: '/usuarios', icon: UserCog, label: 'Usuarios', permission: 'canManageUsers' },
] as const;

export default function PageLayout() {
  const {
    user,
    logout,
    canManageGroups,
    canManageUsers,
    canViewDashboard,
    canViewEvaluations,
    canViewForms,
    canViewPatients,
    canViewSpecialists,
  } = useAuthStore();
  const navigate = useNavigate();
  const location = useLocation();
  const [sidebarOpen, setSidebarOpen] = useState(false);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const permissions = {
    canManageGroups,
    canManageUsers,
    canViewDashboard,
    canViewEvaluations,
    canViewForms,
    canViewPatients,
    canViewSpecialists,
  };
  const filteredNav = navItems.filter((item) => permissions[item.permission]());

  return (
    <div className="flex h-screen bg-gray-100">
      {sidebarOpen && (
        <div
          className="fixed inset-0 z-30 bg-black/40 lg:hidden"
          onClick={() => setSidebarOpen(false)}
        />
      )}

      <aside
        className={`fixed inset-y-0 left-0 z-40 flex w-64 flex-col border-r border-gray-200 bg-white transition-transform duration-200 lg:static ${sidebarOpen ? 'translate-x-0' : '-translate-x-full lg:translate-x-0'}`}
      >
        <div className="relative flex items-center gap-3 border-b border-gray-200 p-4">

          <img 
            src="/Persona.png" 
            alt="Persona" 
            className="w-30 h-20 -left-10 object-cover relative z-20"
          />

          <h1 className="font-sans absolute left-16 top-10 text-5xl font-bold text-blue-700 z-10">NEXOS</h1>
          <button className="p-1 lg:hidden" onClick={() => setSidebarOpen(false)}>
            <X className="h-5 w-5" />
          </button>
        </div>

        <nav className="flex-1 space-y-1 p-3">
          {filteredNav.map((item) => (
            <Link
              key={item.to}
              to={item.to}
              className={`flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors ${
                location.pathname === item.to
                  ? 'bg-blue-50 text-blue-700'
                  : 'text-gray-600 hover:bg-gray-50'
              }`}
              onClick={() => setSidebarOpen(false)}
            >
              <item.icon className="h-5 w-5" />
              {item.label}
            </Link>
          ))}
        </nav>

        <div className="border-t border-gray-200 p-3">
          <div className="px-3 py-2">
            <p className="text-sm font-medium text-gray-800">{user?.nome}</p>
            <p className="text-xs capitalize text-gray-500">{user?.role}</p>
          </div>
          <button
            onClick={handleLogout}
            className="flex w-full items-center gap-3 rounded-lg px-3 py-2 text-sm text-gray-600 transition-colors hover:bg-red-50 hover:text-red-600"
          >
            <LogOut className="h-5 w-5" />
            Sair
          </button>
        </div>
      </aside>

      <main className="flex-1 overflow-auto">
        <header className="flex items-center gap-3 border-b border-gray-200 bg-white px-4 py-3 lg:hidden">
          <button onClick={() => setSidebarOpen(true)}>
            <Menu className="h-6 w-6" />
          </button>
          <span className="font-semibold">SPI</span>
        </header>
        <div className="p-4 lg:p-6">
          <Outlet />
        </div>
      </main>
    </div>
  );
}



