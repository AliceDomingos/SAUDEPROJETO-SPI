import { useEffect, useMemo, useState } from 'react';
import { Eye, Pencil, Plus, Search, Trash2 } from 'lucide-react';
import SearchFiltersPanel from '@/shared/components/filters/SearchFiltersPanel';
import { useAuthStore } from '@/shared/store/authStore';
import type { User } from '@/types';
import { getUsers } from '@/domains/users/api';
import {
  createGroup,
  deleteGroup,
  getGroups,
  updateGroup,
  type CreateGroupInput,
  type UpdateGroupInput,
} from '../api';
import type { Group } from '../types';
import GroupCreateDialog from '../components/dialogs/GroupCreateDialog';
import GroupEditDialog from '../components/dialogs/GroupEditDialog';
import GroupDetailsDialog from '../components/dialogs/GroupDetailsDialog';
import GroupDeleteDialog from '../components/dialogs/GroupDeleteDialog';
import { formatGroupDate, getGroupSearchText, getGroupStatusBadgeClass, getGroupStatusLabel } from '../components/utils/groupUtils';
import DataTable, { type Column } from '@/shared/components/table/DataTable';

export default function GroupsPage() {
  const user = useAuthStore((state) => state.user);
  const isAdmin = user?.role === 'admin';

  const [groups, setGroups] = useState<Group[]>([]);
  const [managers, setManagers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');
  const [createOpen, setCreateOpen] = useState(false);
  const [detailsGroup, setDetailsGroup] = useState<Group | null>(null);
  const [editGroup, setEditGroup] = useState<Group | null>(null);
  const [deleteGroupTarget, setDeleteGroupTarget] = useState<Group | null>(null);

  const loadData = async () => {
    const [groupsData, usersData] = await Promise.all([getGroups(), getUsers()]);
    setGroups(groupsData);
    setManagers(usersData.filter((item: User) => item.role === 'gestor'));
    setError('');
  };

  useEffect(() => {
    const initialize = async () => {
      try {
        await loadData();
      } catch (err: unknown) {
        setError(err instanceof Error ? err.message : 'Erro ao carregar grupos');
      } finally {
        setLoading(false);
      }
    };

    initialize();
  }, []);

  const managersById = useMemo(
    () => new Map(managers.map((manager) => [manager.id, manager])),
    [managers],
  );

  const handleCreate = async (data: CreateGroupInput) => {
    const selectedManager = data.gestorId ? managersById.get(data.gestorId) ?? null : user ?? null;
    await createGroup(data, selectedManager);
    await loadData();
  };

  const handleEdit = async (groupId: string, data: UpdateGroupInput) => {
    const selectedManager = data.gestorId ? managersById.get(data.gestorId) ?? null : user ?? null;
    await updateGroup(groupId, data, selectedManager);
    await loadData();
  };

  const handleDelete = async (groupId: string) => {
    await deleteGroup(groupId);
    await loadData();
  };

  const normalizedSearch = search.trim().toLowerCase();
  const filteredGroups = groups.filter((group) => {
    if (!normalizedSearch) return true;
    return getGroupSearchText(group).includes(normalizedSearch);
  });

  const columns: Column<Group>[] = [
    {
      header: 'Acoes',
      sticky: true,
      render: (g) => (
        <div className="flex gap-2">
          <button
            type="button"
            title="Visualizar"
            onClick={() => setDetailsGroup(g)}
            className="rounded-lg border border-gray-300 p-2 text-gray-700 transition hover:bg-gray-50"
          >
            <Eye className="h-3.5 w-3.5" />
          </button>
          <button
            type="button"
            title="Editar"
            onClick={() => setEditGroup(g)}
            className="rounded-lg border border-blue-200 p-2 text-blue-700 transition hover:bg-blue-50"
          >
            <Pencil className="h-3.5 w-3.5" />
          </button>
          <button
            type="button"
            title="Excluir"
            onClick={() => setDeleteGroupTarget(g)}
            className="rounded-lg border border-red-200 p-2 text-red-700 transition hover:bg-red-50"
          >
            <Trash2 className="h-3.5 w-3.5" />
          </button>
        </div>
      ),
    },
    {
      header: 'Grupo',
      sortKey: (g) => g.nome,
      render: (g) => <span className="font-medium text-gray-900">{g.nome}</span>,
    },
    {
      header: 'Gestor',
      sortKey: (g) => g.gestor_nome ?? '',
      render: (g) => <span className="text-gray-600">{g.gestor_nome || 'Nao informado'}</span>,
    },
    {
      header: 'Membros',
      sortKey: (g) => g.quantidade_membros,
      render: (g) => <span className="text-gray-600">{g.quantidade_membros}</span>,
    },
    {
      header: 'Status',
      sortKey: (g) => (g.ativo ? 0 : 1),
      render: (g) => (
        <span className={`rounded-full px-2 py-1 text-xs font-bold ${getGroupStatusBadgeClass(g.ativo)}`}>
          {getGroupStatusLabel(g.ativo)}
        </span>
      ),
    },
    {
      header: 'Cadastro',
      sortKey: (g) => g.criado_em,
      render: (g) => <span className="text-gray-500">{formatGroupDate(g.criado_em)}</span>,
    },
  ];

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 md:flex-row md:items-end md:justify-between">
        <div>
          <h2 className="text-xl font-bold">Grupos</h2>
          <p className="text-sm text-gray-500">
            {filteredGroups.length} grupo(s) exibido(s) de {groups.length} cadastrado(s)
          </p>
        </div>

        <button
          type="button"
          onClick={() => setCreateOpen(true)}
          className="inline-flex items-center justify-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-blue-700"
        >
          <Plus className="h-4 w-4" />
          Novo grupo
        </button>
      </div>

      <SearchFiltersPanel
        title="Encontre grupos e gestores com mais rapidez"
        description="Busque por nome do grupo ou pelo gestor responsavel para localizar o que precisa."
        searchLabel="Buscar grupo"
        searchValue={search}
        searchPlaceholder="Buscar por nome do grupo ou gestor"
        onSearchChange={setSearch}
        hasActiveFilters={search.trim().length > 0}
        onClear={() => setSearch('')}
      />

      {error && (
        <div className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {error}
        </div>
      )}

      {loading ? (
        <div className="rounded-xl border border-gray-200 bg-white px-6 py-10 text-center text-sm text-gray-500">
          Carregando grupos...
        </div>
      ) : (
        <DataTable
          data={filteredGroups}
          columns={columns}
          keyExtractor={(g) => g.id}
          emptyMessage="Nenhum grupo encontrado com os filtros atuais."
        />
      )}

      <GroupCreateDialog
        open={createOpen}
        onClose={() => setCreateOpen(false)}
        onSubmit={handleCreate}
        managers={managers}
        requireManager={Boolean(isAdmin)}
      />
      <GroupEditDialog
        group={editGroup}
        open={editGroup !== null}
        onClose={() => setEditGroup(null)}
        onSubmit={handleEdit}
        managers={managers}
        requireManager={Boolean(isAdmin)}
      />
      <GroupDetailsDialog
        group={detailsGroup}
        open={detailsGroup !== null}
        onClose={() => setDetailsGroup(null)}
      />
      <GroupDeleteDialog
        group={deleteGroupTarget}
        open={deleteGroupTarget !== null}
        onClose={() => setDeleteGroupTarget(null)}
        onConfirm={handleDelete}
      />
    </div>
  );
}
