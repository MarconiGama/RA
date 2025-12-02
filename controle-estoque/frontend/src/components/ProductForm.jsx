import { useState } from 'react';
import api from '../services/api';

const initial = { name: '', sku: '', quantity: 0, price: 0, min_quantity: 0, barcode: '', description: '' };

export default function ProductForm({ onCreate }) {
  const [form, setForm] = useState(initial);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  function update(field, value) {
    setForm((prev) => ({ ...prev, [field]: value }));
  }

  async function handle(e) {
    e.preventDefault();
    setLoading(true);
    setError('');
    try {
      await api.post('/products', {
        ...form,
        quantity: Number(form.quantity),
        price: Number(form.price),
        min_quantity: Number(form.min_quantity),
      });
      setForm(initial);
      onCreate();
    } catch (err) {
      setError(err.response?.data?.error || 'Falha ao criar produto');
    } finally {
      setLoading(false);
    }
  }

  return (
    <form onSubmit={handle} className="form-grid">
      {error && <div className="alert">{error}</div>}
      <label>
        Nome
        <input value={form.name} onChange={(e) => update('name', e.target.value)} placeholder="Ex: Teclado Mecânico" required />
      </label>
      <label>
        SKU
        <input value={form.sku} onChange={(e) => update('sku', e.target.value)} placeholder="Ex: TEC-001" />
      </label>
      <label>
        Código de barras
        <input value={form.barcode} onChange={(e) => update('barcode', e.target.value)} placeholder="EAN" />
      </label>
      <label>
        Quantidade inicial
        <input type="number" value={form.quantity} onChange={(e) => update('quantity', e.target.value)} min="0" />
      </label>
      <label>
        Quantidade mínima
        <input type="number" value={form.min_quantity} onChange={(e) => update('min_quantity', e.target.value)} min="0" />
      </label>
      <label>
        Preço
        <input type="number" step="0.01" value={form.price} onChange={(e) => update('price', e.target.value)} min="0" />
      </label>
      <label className="full">
        Descrição
        <textarea value={form.description} onChange={(e) => update('description', e.target.value)} rows="2" />
      </label>
      <div className="actions">
        <button type="submit" disabled={loading}>
          {loading ? 'Salvando...' : 'Criar produto'}
        </button>
      </div>
    </form>
  );
}
