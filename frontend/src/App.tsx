import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { useAuthStore } from '@/shared/store/authStore';
import ProtectedRoute from '@/shared/components/ProtectedRoute';
import PageLayout from '@/shared/components/layout/PageLayout';
import LoginPage from '@/domains/auth/pages/LoginPage';
import SetPasswordPage from '@/domains/auth/pages/SetPasswordPage';
import DashboardPage from '@/domains/dashboard/pages/DashboardPage';
import PatientsPage from '@/domains/patients/pages/PatientsPage';
import UsersPage from '@/domains/users/pages/UsersPage';
import EvaluationFormPage from '@/domains/evaluations/pages/EvaluationFormPage';
import EvaluationsListPage from '@/domains/evaluations/pages/EvaluationsListPage';
import EvaluationResultPage from '@/domains/evaluations/pages/EvaluationResultPage';
import EvaluationDetailPage from '@/domains/evaluations/pages/EvaluationDetailPage';
import FormsListPage from '@/domains/forms/pages/FormsListPage';
import FormEditorPage from '@/domains/forms/pages/FormEditorPage';
import GroupsPage from '@/domains/groups/pages/GroupsPage';

function Private() {
  const canViewDashboard = useAuthStore((s) => s.canViewDashboard);
  const canViewPatients = useAuthStore((s) => s.canViewPatients);
  const canManageForms = useAuthStore((s) => s.canManageForms);
  const canViewEvaluations = useAuthStore((s) => s.canViewEvaluations);
  const canCreateEvaluations = useAuthStore((s) => s.canCreateEvaluations);
  const canViewForms = useAuthStore((s) => s.canViewForms);
  const canManageGroups = useAuthStore((s) => s.canManageGroups);
  const canManageUsers = useAuthStore((s) => s.canManageUsers);
  const defaultPath = canViewDashboard()
    ? '/'
    : canViewEvaluations()
      ? '/avaliacoes'
      : '/formularios';

  return (
    <Routes>
      <Route path="/" element={<PageLayout />}>
        <Route index element={canViewDashboard() ? <DashboardPage /> : <Navigate to={defaultPath} replace />} />
        <Route path="avaliacoes" element={canViewEvaluations() ? <EvaluationsListPage /> : <Navigate to={defaultPath} replace />} />
        <Route path="pacientes" element={canViewPatients() ? <PatientsPage /> : <Navigate to={defaultPath} replace />} />
        <Route path="nova-avaliacao" element={canCreateEvaluations() ? <EvaluationFormPage /> : <Navigate to={defaultPath} replace />} />
        <Route path="resultado" element={canViewEvaluations() ? <EvaluationResultPage /> : <Navigate to={defaultPath} replace />} />
        <Route path="avaliacoes/:id" element={canViewEvaluations() ? <EvaluationDetailPage /> : <Navigate to={defaultPath} replace />} />
        <Route path="formularios" element={canViewForms() ? <FormsListPage /> : <Navigate to={defaultPath} replace />} />
        <Route path="formularios/novo" element={canManageForms() ? <FormEditorPage /> : <Navigate to={defaultPath} replace />} />
        <Route path="formularios/:id" element={canManageForms() ? <FormEditorPage /> : <Navigate to={defaultPath} replace />} />
        <Route path="grupos" element={canManageGroups() ? <GroupsPage /> : <Navigate to={defaultPath} replace />} />
        <Route path="usuarios" element={canManageUsers() ? <UsersPage /> : <Navigate to={defaultPath} replace />} />
        <Route path="*" element={<Navigate to={defaultPath} replace />} />
      </Route>
    </Routes>
  );
}

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/definir-senha" element={<SetPasswordPage />} />
        <Route path="/*" element={<ProtectedRoute><Private /></ProtectedRoute>} />
      </Routes>
    </BrowserRouter>
  );
}
