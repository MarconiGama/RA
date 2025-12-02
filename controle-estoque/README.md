# Controle de Estoque

App fullstack com backend Express + PostgreSQL e frontend React/Vite para gestão de produtos.

## Requisitos
- Docker e Docker Compose
- Node 20+ (opcional para rodar sem containers)

## Configurar ambiente
1. Copie variáveis de ambiente:
   ```bash
   cp backend/.env.example backend/.env
   # Ajuste DATABASE_URL, JWT_SECRET e CORS_ORIGIN se necessário
   ```
2. Suba os containers:
   ```bash
   docker compose up --build
   ```
3. Aplique o schema inicial do banco:
   ```bash
   docker compose exec db psql -U postgres -d estoque -f /app/migrations.sql
   ```
4. Crie um usuário admin:
   ```bash
   docker compose exec backend node -e "(async()=>{const b=require('bcrypt');const db=require('./src/db');const hash=await b.hash('admin123',10);await db.query('INSERT INTO users (username,password,role) VALUES ($1,$2,$3)', ['admin',hash,'admin']);console.log('admin criado');process.exit();})();"
   ```
   Ou registre via API `POST /api/auth/register`.

## Endpoints principais
- `POST /api/auth/register` { username, password }
- `POST /api/auth/login` -> token JWT
- `GET /api/products` (autenticado)
- `POST /api/products` (autenticado) cria produto e registra movimento
- `PATCH /api/products/:id/quantity` (autenticado) ajusta quantidade e loga histórico

## Rodar local (sem Docker)
No backend:
```bash
cd backend
npm install
npm run dev
```
No frontend:
```bash
cd frontend
npm install
npm run dev -- --host
```

## Gerar ZIP para envio
```bash
make
# cria controle-estoque.zip com backend, frontend e configs
```
