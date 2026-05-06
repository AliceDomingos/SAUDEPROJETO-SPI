import { useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { KeyRound } from 'lucide-react';
import { setPasswordFromInvite } from '../api';

export default function SetPasswordPage() {
  const [searchParams] = useSearchParams();
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(false);

  const token = searchParams.get('token') ?? '';
  const hasToken = token.trim().length > 0;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    if (!hasToken) {
      setError('Convite invalido ou ausente.');
      return;
    }

    if (password.trim().length < 6) {
      setError('A senha precisa ter pelo menos 6 caracteres.');
      return;
    }

    if (password !== confirmPassword) {
      setError('A confirmacao da senha nao confere.');
      return;
    }

    setLoading(true);

    try {
      await setPasswordFromInvite({ token, password });
      setSuccess('Senha definida com sucesso. Agora voce ja pode entrar no sistema.');
      setPassword('');
      setConfirmPassword('');
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Erro ao definir senha');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-600 to-indigo-700 p-4">
      <div className="w-full max-w-md">
        <div className="bg-white rounded-2xl shadow-xl p-8">
          <div className="flex flex-col items-center mb-6">
            <div className="w-14 h-14 bg-amber-100 rounded-full flex items-center justify-center mb-3">
              <KeyRound className="w-7 h-7 text-amber-600" />
            </div>
            <h1 className="text-2xl font-bold text-gray-900">Definir senha</h1>
            <p className="text-sm text-gray-500 mt-1 text-center">
              Crie sua senha de acesso ao SPI usando o convite enviado por e-mail.
            </p>
          </div>

          {!hasToken && (
            <div className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-600 mb-4">
              O link de convite esta incompleto. Solicite um novo convite ao administrador.
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Nova senha
              </label>
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                minLength={6}
                disabled={!hasToken || loading}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition disabled:bg-gray-100 disabled:text-gray-500"
                placeholder="Digite sua senha"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Confirmar senha
              </label>
              <input
                type="password"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                required
                minLength={6}
                disabled={!hasToken || loading}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition disabled:bg-gray-100 disabled:text-gray-500"
                placeholder="Repita a senha"
              />
            </div>

            {error && (
              <p className="text-sm text-red-600 bg-red-50 px-3 py-2 rounded-lg">
                {error}
              </p>
            )}

            {success && (
              <p className="text-sm text-green-700 bg-green-50 px-3 py-2 rounded-lg">
                {success}
              </p>
            )}

            <button
              type="submit"
              disabled={!hasToken || loading}
              className="w-full bg-blue-600 text-white py-2.5 rounded-lg font-semibold hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition"
            >
              {loading ? 'Salvando...' : 'Definir senha'}
            </button>
          </form>

          <div className="mt-4 text-center">
            <Link to="/login" className="text-sm font-medium text-blue-600 hover:text-blue-700">
              Voltar para o login
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}



