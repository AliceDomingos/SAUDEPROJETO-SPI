import { useEffect, useState } from 'react';
import { Eye, KeyRound, Pencil, Plus, UserX } from 'lucide-react';
import {
  createUser,
  deactivateUser,
  getUsers,
  sendUserPasswordInvite,
  updateUser,
  type CreateUserInput,
  type UpdateUserInput,
} from '../api';
import UserCreateDialog from '../components/dialogs/UserCreateDialog';
import UserDetailsDialog from '../components/dialogs/UserDetailsDialog';
import UserEditDialog from '../components/dialogs/UserEditDialog';
import UserDeactivateDialog from '../components/dialogs/UserDeactivateDialog';
import UserPasswordInviteDialog from '../components/dialogs/UserPasswordInviteDialog';
import type { User } from '@/types';
import DataTable, { type Column } from '@/shared/components/table/DataTable';
import { formatCreatedAt, formatRole, roleBadgeCls, statusBadgeCls } from '../components/utils/userUtils';
import type { Group } from '@/domains/groups/types';
import { getGroups } from '@/domains/groups/api';
import { useAuthStore } from '@/shared/store/authStore';
import { isAdminDefaultGroup } from '@/domains/groups/utils/systemGroupRules';

export default function UsersPage() {
  const isAdmin = useAuthStore((s) => s.isAdmin());
  const currentRole = useAuthStore((s) => s.user?.role);
  const isGestor = currentRole === 'gestor';
  const excludeRolesForCreate: import('@/types').UserRole[] = isGestor ? ['admin', 'analista'] : [];
  const [users, setUsers] = useState<User[]>([]);
  const [groups, setGroups] = useState<Group[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [createOpen, setCreateOpen] = useState(false);
  const [detailsUser, setDetailsUser] = useState<User | null>(null);
  const [editUser, setEditUser] = useState<User | null>(null);
  const [passwordInviteTarget, setPasswordInviteTarget] = useState<User | null>(null);
  const [deactivateTarget, setDeactivateTarget] = useState<User | null>(null);

  const refreshUsers = async () => {
    const data = await getUsers();
    setUsers(data);
    setError('');
  };

  useEffect(() => {
    const initialize = async () => {
      try {
        const [usersData, groupsData] = await Promise.all([getUsers(), getGroups()]);
        setUsers(usersData);
        setGroups(groupsData);
        setError('');
      } catch (err: unknown) {
        setError(err instanceof Error ? err.message : 'Erro ao carregar usuarios');
      } finally {
        setLoading(false);
      }
    };

    initialize();
  }, []);

  const handleCreate = async (data: CreateUserInput) => {
    await createUser(data);
    await refreshUsers();
  };

  const handleEdit = async (userId: number, data: UpdateUserInput) => {
    await updateUser(userId, data);
    await refreshUsers();
  };

  const handleDeactivate = async (userId: number) => {
    await deactivateUser(userId);
    await refreshUsers();
  };

  const handlePasswordInvite = async (userId: number) => {
    await sendUserPasswordInvite(userId);
  };

  const columns: Column<User>[] = [
    {
      header: 'Acoes',
      render: (u) => (
        <div className="flex flex-wrap gap-2">
          <button
            type="button"
            onClick={() => setDetailsUser(u)}
            className="inline-flex items-center gap-1 rounded-lg border border-gray-300 px-3 py-1.5 text-xs font-medium text-gray-700 transition hover:bg-gray-50"
          >
            <Eye className="h-3.5 w-3.5" />
            Visualizar
          </button>
          <button
            type="button"
            onClick={() => setEditUser(u)}
            className="inline-flex items-center gap-1 rounded-lg border border-blue-200 px-3 py-1.5 text-xs font-medium text-blue-700 transition hover:bg-blue-50"
          >
            <Pencil className="h-3.5 w-3.5" />
            Editar
          </button>
          {u.ativo && (
            <button
              type="button"
              onClick={() => setPasswordInviteTarget(u)}
              className="inline-flex items-center gap-1 rounded-lg border border-amber-200 px-3 py-1.5 text-xs font-medium text-amber-700 transition hover:bg-amber-50"
            >
              <KeyRound className="h-3.5 w-3.5" />
              Enviar senha
            </button>
          )}
          {u.ativo && (
            <button
              type="button"
              onClick={() => setDeactivateTarget(u)}
              className="inline-flex items-center gap-1 rounded-lg border border-red-200 px-3 py-1.5 text-xs font-medium text-red-700 transition hover:bg-red-50"
            >
              <UserX className="h-3.5 w-3.5" />
              Desativar
            </button>
          )}
        </div>
      ),
    },
    {
      header: 'Nome',
      render: (u) => <span className="font-medium text-gray-900">{u.nome}</span>,
    },
    {
      header: 'E-mail',
      render: (u) => <span className="text-gray-500">{u.email}</span>,
    },
    {
      header: 'Perfil',
      render: (u) => (
        <span className={`rounded-full px-2 py-1 text-xs font-bold ${roleBadgeCls(u.role)}`}>
          {formatRole(u.role)}
        </span>
      ),
    },
    {
      header: 'Status',
      render: (u) => (
        <span className={`rounded-full px-2 py-1 text-xs font-bold ${statusBadgeCls(u.ativo)}`}>
          {u.ativo ? 'Ativo' : 'Inativo'}
        </span>
      ),
    },
    {
      header: 'Cadastro',
      render: (u) => <span className="text-gray-500">{formatCreatedAt(u.criado_em)}</span>,
    },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-bold">Usuarios</h2>
          <p className="text-sm text-gray-500">Gerenciar avaliadores e administradores</p>
        </div>

        <button
          type="button"
          onClick={() => setCreateOpen(true)}
          className="flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-blue-700"
        >
          <Plus className="h-4 w-4" />
          Novo usuario
        </button>
      </div>

      {error && (
        <div className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {error}
        </div>
      )}

      {loading ? (
        <div className="rounded-xl border border-gray-200 bg-white px-6 py-10 text-center text-sm text-gray-500">
          Carregando usuarios...
        </div>
      ) : (
        <DataTable
          data={users}
          columns={columns}
          keyExtractor={(u) => u.id}
          emptyMessage="Nenhum usuario cadastrado ate o momento."
        />
      )}

      <UserCreateDialog
        open={createOpen}
        onClose={() => setCreateOpen(false)}
        groups={isAdmin ? groups.filter((g) => !isAdminDefaultGroup(g)) : groups}
        singleGroupSelect={true}
        excludeRoles={excludeRolesForCreate}
        onSubmit={handleCreate}
      />
      <UserDetailsDialog user={detailsUser} open={detailsUser !== null} onClose={() => setDetailsUser(null)} />
      <UserEditDialog
        user={editUser}
        open={editUser !== null}
        onClose={() => setEditUser(null)}
        onSubmit={handleEdit}
        groups={groups}
        isAdmin={isAdmin}
      />
      <UserPasswordInviteDialog
        user={passwordInviteTarget}
        open={passwordInviteTarget !== null}
        onClose={() => setPasswordInviteTarget(null)}
        onConfirm={handlePasswordInvite}
      />
      <UserDeactivateDialog
        user={deactivateTarget}
        open={deactivateTarget !== null}
        onClose={() => setDeactivateTarget(null)}
        onConfirm={handleDeactivate}
      />
    </div>
  );
}
