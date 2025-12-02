const db = require('../db');

async function listProducts(req, res) {
  try {
    const result = await db.query('SELECT * FROM products ORDER BY id');
    return res.json(result.rows);
  } catch (err) {
    console.error('Erro ao listar produtos', err);
    return res.status(500).json({ error: 'erro interno' });
  }
}

async function createProduct(req, res) {
  try {
    const { name, sku, quantity = 0, price = 0, description, barcode, min_quantity = 0 } = req.body || {};
    if (!name) return res.status(400).json({ error: 'name é obrigatório' });
    if (quantity < 0) return res.status(400).json({ error: 'quantity não pode ser negativo' });

    const result = await db.query(
      `INSERT INTO products (name, sku, barcode, quantity, price, description, min_quantity) VALUES ($1,$2,$3,$4,$5,$6,$7) RETURNING *`,
      [name, sku, barcode, quantity, price, description, min_quantity]
    );

    await db.query(
      'INSERT INTO stock_moves (product_id, change, reason, created_by) VALUES ($1,$2,$3,$4)',
      [result.rows[0].id, quantity, 'create', req.user?.sub || null]
    );

    return res.status(201).json(result.rows[0]);
  } catch (err) {
    if (err.code === '23505') {
      return res.status(409).json({ error: 'SKU duplicado' });
    }
    console.error('Erro ao criar produto', err);
    return res.status(500).json({ error: 'erro interno' });
  }
}

async function updateQuantity(req, res) {
  try {
    const { id } = req.params;
    const { quantity } = req.body || {};

    if (typeof quantity !== 'number') {
      return res.status(400).json({ error: 'quantity deve ser number' });
    }

    const current = await db.query('SELECT quantity FROM products WHERE id = $1', [id]);
    if (current.rowCount === 0) return res.status(404).json({ error: 'produto não encontrado' });
    const previousQty = current.rows[0].quantity;

    const result = await db.query('UPDATE products SET quantity = $1, updated_at = now() WHERE id = $2 RETURNING *', [quantity, id]);
    await db.query(
      'INSERT INTO stock_moves (product_id, change, reason, created_by) VALUES ($1,$2,$3,$4)',
      [id, quantity - previousQty, 'manual-update', req.user?.sub || null]
    );

    return res.json(result.rows[0]);
  } catch (err) {
    console.error('Erro ao atualizar quantidade', err);
    return res.status(500).json({ error: 'erro interno' });
  }
}

module.exports = { listProducts, createProduct, updateQuantity };
