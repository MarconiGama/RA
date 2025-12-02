const { Pool } = require('pg');

const connectionString = process.env.DATABASE_URL;
if (!connectionString) {
  throw new Error('DATABASE_URL não configurada. Defina no .env');
}

const pool = new Pool({ connectionString });

pool.on('error', (err) => {
  console.error('Erro inesperado no pool do PostgreSQL', err);
});

module.exports = {
  query: (text, params) => pool.query(text, params),
  pool,
};
