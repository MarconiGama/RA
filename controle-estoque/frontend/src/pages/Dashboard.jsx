import { useEffect, useMemo, useState } from 'react';
import api from '../services/api';
import ProductForm from '../components/ProductForm';
import ProductList from '../components/ProductList';

export default function Dashboard() {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const totals = useMemo(() => {
    const totalItems = products.reduce((sum, p) => sum + Number(p.quantity || 0), 0);
    const lowStock = products.filter((p) => Number(p.quantity) <= Number(p.min_quantity || 0)).length;
    return { totalItems, lowStock };
  }, [products]);

  async function load() {
    setLoading(true);
    setError('');
    try {
      const res = await api.get('/products');
      setProducts(res.data);
    } catch (err) {
      setError(err.response?.data?.error || 'Falha ao carregar produtos');
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    load();
  }, []);

  function handleLogout() {
    localStorage.removeItem('token');
    window.location.href = '/login';
  }

  return (
    <div className="dashboard">
      <header className="topbar">
        <div>
          <h1>Controle de Estoque</h1>
          <p className="subtitle">Monitoramento rápido de produtos e quantidades.</p>
        </div>
        <button className="ghost" onClick={handleLogout}>
          Sair
        </button>
      </header>

      {error && <div className="alert">{error}</div>}

      <section className="cards">
        <div className="card">
          <p className="muted">Itens totais</p>
          <strong className="big">{totals.totalItems}</strong>
        </div>
        <div className="card">
          <p className="muted">Produtos em alerta</p>
          <strong className="big">{totals.lowStock}</strong>
        </div>
        <div className="card">
          <p className="muted">Produtos cadastrados</p>
          <strong className="big">{products.length}</strong>
        </div>
      </section>

      <section className="grid">
        <div className="panel">
          <div className="panel-header">
            <h2>Adicionar produto</h2>
            <p className="muted">Nome, SKU, quantidade e preço.</p>
          </div>
          <ProductForm onCreate={load} />
        </div>
        <div className="panel">
          <div className="panel-header">
            <h2>Produtos</h2>
            <p className="muted">Atualize rapidamente o estoque.</p>
          </div>
          {loading ? <p>Carregando...</p> : <ProductList products={products} onRefresh={load} />}
        </div>
      </section>
    </div>
  );
}
