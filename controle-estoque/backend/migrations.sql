-- users
CREATE TABLE IF NOT EXISTS users (
  id SERIAL PRIMARY KEY,
  username VARCHAR(100) UNIQUE NOT NULL,
  password VARCHAR(200) NOT NULL,
  role VARCHAR(50) DEFAULT 'user',
  created_at TIMESTAMP DEFAULT now()
);

-- products
CREATE TABLE IF NOT EXISTS products (
  id SERIAL PRIMARY KEY,
  name VARCHAR(255) NOT NULL,
  sku VARCHAR(100) UNIQUE,
  barcode VARCHAR(100),
  description TEXT,
  quantity INTEGER DEFAULT 0,
  min_quantity INTEGER DEFAULT 0,
  price NUMERIC(12,2) DEFAULT 0,
  created_at TIMESTAMP DEFAULT now(),
  updated_at TIMESTAMP DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_products_name ON products (LOWER(name));
CREATE INDEX IF NOT EXISTS idx_products_sku ON products (sku);

-- stock moves (historic)
CREATE TABLE IF NOT EXISTS stock_moves (
  id SERIAL PRIMARY KEY,
  product_id INTEGER REFERENCES products(id) ON DELETE CASCADE,
  change INTEGER NOT NULL,
  reason VARCHAR(255),
  created_by INTEGER REFERENCES users(id),
  created_at TIMESTAMP DEFAULT now()
);

-- suppliers
CREATE TABLE IF NOT EXISTS suppliers (
  id SERIAL PRIMARY KEY,
  name VARCHAR(255) NOT NULL,
  contact JSONB,
  created_at TIMESTAMP DEFAULT now()
);

-- purchase orders
CREATE TABLE IF NOT EXISTS purchase_orders (
  id SERIAL PRIMARY KEY,
  supplier_id INTEGER REFERENCES suppliers(id),
  status VARCHAR(50) DEFAULT 'draft',
  total NUMERIC(12,2) DEFAULT 0,
  created_at TIMESTAMP DEFAULT now(),
  updated_at TIMESTAMP DEFAULT now()
);

-- inventory counts
CREATE TABLE IF NOT EXISTS inventory_counts (
  id SERIAL PRIMARY KEY,
  name VARCHAR(255),
  performed_by INTEGER REFERENCES users(id),
  performed_at TIMESTAMP,
  notes TEXT,
  created_at TIMESTAMP DEFAULT now()
);

CREATE TABLE IF NOT EXISTS inventory_count_lines (
  id SERIAL PRIMARY KEY,
  inventory_count_id INTEGER REFERENCES inventory_counts(id) ON DELETE CASCADE,
  product_id INTEGER REFERENCES products(id),
  counted_quantity INTEGER,
  system_quantity INTEGER,
  difference INTEGER,
  notes TEXT
);
