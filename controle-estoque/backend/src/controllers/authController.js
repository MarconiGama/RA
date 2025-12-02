const bcrypt = require('bcrypt');
const jwt = require('jsonwebtoken');
const db = require('../db');

const JWT_SECRET = process.env.JWT_SECRET || 'dev_secret';
const JWT_EXPIRES_IN = process.env.JWT_EXPIRES_IN || '2h';

async function register(req, res) {
  try {
    const { username, password, role = 'user' } = req.body || {};
    if (!username || !password) {
      return res.status(400).json({ error: 'username e password são obrigatórios' });
    }

    const existing = await db.query('SELECT id FROM users WHERE username = $1', [username]);
    if (existing.rowCount > 0) {
      return res.status(409).json({ error: 'username já existe' });
    }

    const hash = await bcrypt.hash(password, 10);
    const result = await db.query(
      'INSERT INTO users (username, password, role) VALUES ($1,$2,$3) RETURNING id, username, role',
      [username, hash, role]
    );

    return res.status(201).json(result.rows[0]);
  } catch (err) {
    console.error('Erro ao registrar usuário', err);
    return res.status(500).json({ error: 'erro interno' });
  }
}

async function login(req, res) {
  try {
    const { username, password } = req.body || {};
    if (!username || !password) {
      return res.status(400).json({ error: 'username e password são obrigatórios' });
    }

    const userRes = await db.query('SELECT * FROM users WHERE username = $1', [username]);
    const user = userRes.rows[0];
    if (!user) return res.status(401).json({ error: 'credenciais inválidas' });

    const match = await bcrypt.compare(password, user.password);
    if (!match) return res.status(401).json({ error: 'credenciais inválidas' });

    const token = jwt.sign({ sub: user.id, username: user.username, role: user.role }, JWT_SECRET, {
      expiresIn: JWT_EXPIRES_IN,
    });

    return res.json({ token, user: { id: user.id, username: user.username, role: user.role } });
  } catch (err) {
    console.error('Erro ao realizar login', err);
    return res.status(500).json({ error: 'erro interno' });
  }
}

module.exports = { register, login };
