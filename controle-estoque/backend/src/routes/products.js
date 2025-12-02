const express = require('express');
const authenticate = require('../middleware/auth');
const { listProducts, createProduct, updateQuantity } = require('../controllers/productController');

const router = express.Router();

router.get('/', authenticate, listProducts);
router.post('/', authenticate, createProduct);
router.patch('/:id/quantity', authenticate, updateQuantity);

module.exports = router;
