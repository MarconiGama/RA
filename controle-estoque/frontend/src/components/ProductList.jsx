import api from '../services/api';

export default function ProductList({ products, onRefresh }) {
  async function setQty(id, current) {
    const newQty = Number(prompt('Nova quantidade', current));
    if (Number.isNaN(newQty)) return;
    await api.patch(`/products/${id}/quantity`, { quantity: newQty });
    onRefresh();
  }

  return (
    <div className="table-wrapper">
      <table>
        <thead>
          <tr>
            <th>ID</th>
            <th>Nome</th>
            <th>SKU</th>
            <th>Qtd</th>
            <th>Qtd mín.</th>
            <th>Preço</th>
            <th>Ações</th>
          </tr>
        </thead>
        <tbody>
          {products.map((p) => (
            <tr key={p.id} className={p.quantity <= (p.min_quantity || 0) ? 'warn' : ''}>
              <td>{p.id}</td>
              <td>{p.name}</td>
              <td>{p.sku || '-'}</td>
              <td>{p.quantity}</td>
              <td>{p.min_quantity}</td>
              <td>R$ {Number(p.price).toFixed(2)}</td>
              <td>
                <button onClick={() => setQty(p.id, p.quantity)}>Editar</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
