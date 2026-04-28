import { useEffect, useState } from 'react';
import { Eye, Pencil, Plus, Trash2 } from 'lucide-react';
import type { Patient } from '@/types';
import type { Group } from '@/domains/groups/types';
import { getGroups } from '@/domains/groups/api';
import { useAuthStore } from '@/shared/store/authStore';
import {
  createPatient,
  deletePatient,
  getPatients,
  updatePatient,
  type CreatePatientInput,
  type UpdatePatientInput,
} from '../api';
import PatientFiltersBar from '../components/filters/PatientFiltersBar';
import PatientCreateDialog from '../components/dialogs/PatientCreateDialog';
import PatientEditDialog from '../components/dialogs/PatientEditDialog';
import PatientDetailsDialog from '../components/dialogs/PatientDetailsDialog';
import PatientDeleteDialog from '../components/dialogs/PatientDeleteDialog';
import type { PatientSearchField } from '../types';
import { formatCpf, formatDate, formatPatientSex, formatPhone, getPatientAgeLabel, matchesPatientSearch } from '../components/utils/patientUtils';
import DataTable, { type Column } from '@/shared/components/table/DataTable';

export default function PatientsPage() {
  const user = useAuthStore((state) => state.user);
  const canManagePatients = useAuthStore((state) => state.canManagePatients);
  const isAdmin = user?.role === 'admin';
  const shouldLoadGroups = user?.role === 'admin' || user?.role === 'gestor';
  const [patients, setPatients] = useState<Patient[]>([]);
  const [groups, setGroups] = useState<Group[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');
  const [searchField, setSearchField] = useState<PatientSearchField>('all');
  const [createOpen, setCreateOpen] = useState(false);
  const [detailsPatient, setDetailsPatient] = useState<Patient | null>(null);
  const [editPatient, setEditPatient] = useState<Patient | null>(null);
  const [deletePatientTarget, setDeletePatientTarget] = useState<Patient | null>(null);

  const refreshPatients = async () => {
    const data = await getPatients();
    setPatients(data);
    setError('');
  };

  useEffect(() => {
    const initialize = async () => {
      try {
        const [patientsData, groupsData] = await Promise.all([
          getPatients(),
          shouldLoadGroups ? getGroups() : Promise.resolve([] as Group[]),
        ]);
        setPatients(patientsData);
        setGroups(groupsData);
        setError('');
      } catch (err: unknown) {
        setError(err instanceof Error ? err.message : 'Erro ao carregar pacientes');
      } finally {
        setLoading(false);
      }
    };

    initialize();
  }, [shouldLoadGroups]);

  const handleCreate = async (data: CreatePatientInput) => {
    await createPatient(data);
    await refreshPatients();
  };

  const handleEdit = async (patientId: number, data: UpdatePatientInput) => {
    await updatePatient(patientId, data);
    await refreshPatients();
  };

  const handleDelete = async (patientId: number) => {
    await deletePatient(patientId);
    await refreshPatients();
  };

  const normalizedSearch = search.trim().toLowerCase();
  const defaultCreateGroupId = (() => {
    if ((user?.groupIds?.length ?? 0) === 1) {
      const userGroupId = user!.groupIds![0];
      return groups.some((group) => group.id === userGroupId) ? String(userGroupId) : '';
    }

    if (groups.length === 1) {
      return String(groups[0].id);
    }

    return '';
  })();

  const requireGroupSelection = isAdmin || groups.length > 1;

  const filteredPatients = patients.filter((patient) => {
    if (!normalizedSearch) return true;
    return matchesPatientSearch(patient, normalizedSearch, searchField);
  });

  const columns: Column<Patient>[] = [
    {
      header: 'Acoes',
      render: (p) => (
        <div className="flex flex-wrap gap-2">
          <button
            type="button"
            onClick={() => setDetailsPatient(p)}
            className="inline-flex items-center gap-1 rounded-lg border border-gray-300 px-3 py-1.5 text-xs font-medium text-gray-700 transition hover:bg-gray-50"
          >
            <Eye className="h-3.5 w-3.5" />
            Visualizar
          </button>
          {canManagePatients() && (
            <>
              <button
                type="button"
                onClick={() => setEditPatient(p)}
                className="inline-flex items-center gap-1 rounded-lg border border-blue-200 px-3 py-1.5 text-xs font-medium text-blue-700 transition hover:bg-blue-50"
              >
                <Pencil className="h-3.5 w-3.5" />
                Editar
              </button>
              <button
                type="button"
                onClick={() => setDeletePatientTarget(p)}
                className="inline-flex items-center gap-1 rounded-lg border border-red-200 px-3 py-1.5 text-xs font-medium text-red-700 transition hover:bg-red-50"
              >
                <Trash2 className="h-3.5 w-3.5" />
                Excluir
              </button>
            </>
          )}
        </div>
      ),
    },
    {
      header: 'Paciente',
      render: (p) => (
        <>
          <div className="font-medium text-gray-900">{p.nome}</div>
          <div className="text-xs text-gray-500">{getPatientAgeLabel(p)}</div>
        </>
      ),
    },
    {
      header: 'CPF',
      render: (p) => <span className="text-gray-600">{formatCpf(p.cpf)}</span>,
    },
    {
      header: 'Nascimento',
      render: (p) => <span className="text-gray-600">{formatDate(p.data_nascimento)}</span>,
    },
    {
      header: 'Sexo',
      render: (p) => <span className="text-gray-600">{formatPatientSex(p.sexo)}</span>,
    },
    {
      header: 'Contato',
      render: (p) => (
        <>
          <div className="text-gray-600">{formatPhone(p.telefone)}</div>
          <div className="text-xs text-gray-500">{p.email || 'Sem e-mail'}</div>
        </>
      ),
    },
    {
      header: 'Cadastro',
      render: (p) => <span className="text-gray-500">{formatDate(p.criado_em)}</span>,
    },
  ];

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 md:flex-row md:items-end md:justify-between">
        <div>
          <h2 className="text-xl font-bold">Pacientes</h2>
          <p className="text-sm text-gray-500">
            {filteredPatients.length} paciente(s) exibido(s) de {patients.length} cadastrado(s)
          </p>
        </div>

        <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
          {canManagePatients() && (
            <button
              type="button"
              onClick={() => setCreateOpen(true)}
              className="inline-flex items-center justify-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-blue-700"
            >
              <Plus className="h-4 w-4" />
              Novo paciente
            </button>
          )}
        </div>
      </div>

      <PatientFiltersBar
        search={search}
        searchField={searchField}
        onSearchChange={setSearch}
        onSearchFieldChange={setSearchField}
        onClear={() => {
          setSearch('');
          setSearchField('all');
        }}
      />

      {error && (
        <div className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {error}
        </div>
      )}

      {loading ? (
        <div className="rounded-xl border border-gray-200 bg-white px-6 py-10 text-center text-sm text-gray-500">
          Carregando pacientes...
        </div>
      ) : (
        <DataTable
          data={filteredPatients}
          columns={columns}
          keyExtractor={(p) => p.id}
          emptyMessage="Nenhum paciente encontrado com os filtros atuais."
          rowClassName="align-top"
        />
      )}

      <PatientCreateDialog
        open={createOpen}
        onClose={() => setCreateOpen(false)}
        groups={groups}
        defaultGroupId={defaultCreateGroupId}
        requireGroupSelection={requireGroupSelection}
        onSubmit={handleCreate}
      />
      <PatientEditDialog
        patient={editPatient}
        open={editPatient !== null}
        onClose={() => setEditPatient(null)}
        groups={groups}
        requireGroupSelection={requireGroupSelection}
        onSubmit={handleEdit}
      />
      <PatientDetailsDialog
        patient={detailsPatient}
        open={detailsPatient !== null}
        onClose={() => setDetailsPatient(null)}
      />
      <PatientDeleteDialog
        patient={deletePatientTarget}
        open={deletePatientTarget !== null}
        onClose={() => setDeletePatientTarget(null)}
        onConfirm={handleDelete}
      />
    </div>
  );
}
