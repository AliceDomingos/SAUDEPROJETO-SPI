import { create } from 'zustand';
import type { User } from '@/types';

interface AuthState {
  user: User | null;
  token: string | null;
  setAuth: (user: User, token: string) => void;
  logout: () => void;
  isAdmin: () => boolean;
  canViewDashboard: () => boolean;
  canViewPatients: () => boolean;
  canManagePatients: () => boolean;
  canViewEvaluations: () => boolean;
  canCreateEvaluations: () => boolean;
  canViewForms: () => boolean;
  canManageForms: () => boolean;
  canViewSpecialists: () => boolean;
  canManageSpecialists: () => boolean;
  canManageGroups: () => boolean;
  canManageUsers: () => boolean;
}

export const useAuthStore = create<AuthState>((set, get) => ({
  user: (() => {
    try {
      return JSON.parse(localStorage.getItem('user') || 'null');
    } catch {
      return null;
    }
  })(),
  token: localStorage.getItem('token'),
  setAuth: (user, token) => {
    localStorage.setItem('token', token);
    localStorage.setItem('user', JSON.stringify(user));
    set({ user, token });
  },
  logout: () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    set({ user: null, token: null });
  },
  isAdmin: () => get().user?.role === 'admin',
  canViewDashboard: () => {
    const role = get().user?.role;
    return role === 'admin' || role === 'analista' || role === 'gestor';
  },
  canViewPatients: () => {
    const role = get().user?.role;
    return role === 'admin' || role === 'gestor' || role === 'agente_saude';
  },
  canManagePatients: () => {
    const role = get().user?.role;
    return role === 'admin' || role === 'gestor' || role === 'agente_saude';
  },
  canViewEvaluations: () => {
    const role = get().user?.role;
    return role === 'admin' || role === 'gestor' || role === 'agente_saude';
  },
  canCreateEvaluations: () => {
    const role = get().user?.role;
    return role === 'admin' || role === 'gestor' || role === 'agente_saude';
  },
  canViewForms: () => {
    const role = get().user?.role;
    return role === 'admin' || role === 'analista' || role === 'gestor' || role === 'agente_saude';
  },
  canManageForms: () => {
    const role = get().user?.role;
    return role === 'admin' || role === 'gestor' || role === 'agente_saude';
  },
  canViewSpecialists: () => {
    const role = get().user?.role;
    return role === 'admin' || role === 'gestor' || role === 'agente_saude';
  },
  canManageSpecialists: () => {
    const role = get().user?.role;
    return role === 'admin' || role === 'gestor';
  },
  canManageGroups: () => {
    const role = get().user?.role;
    return role === 'admin' || role === 'gestor';
  },
  canManageUsers: () => {
    const role = get().user?.role;
    return role === 'admin' || role === 'gestor';
  },
}));
