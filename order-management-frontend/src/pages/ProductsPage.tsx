import React, { useState, useEffect, useCallback } from 'react';
import { Typography, Box, Alert } from '@mui/material';
import { useSearchParams } from 'react-router-dom';
import ProductList from '../components/product/ProductList';
import ProductFilters from '../components/product/ProductFilters';
import ProductDetail from '../components/product/ProductDetail';
import { productService } from '../services/productService';
import type { ProductDto, PagedResponse, ProductFilter } from '../types';

const ProductsPage: React.FC = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const [products, setProducts] = useState<PagedResponse<ProductDto> | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [selectedProduct, setSelectedProduct] = useState<ProductDto | null>(null);
  const [detailModalOpen, setDetailModalOpen] = useState(false);
  const [categories, setCategories] = useState<string[]>([]);

  const getFiltersFromUrl = useCallback((): ProductFilter => {
    return {
      search: searchParams.get('search') || '',
      category: searchParams.get('category') || '',
      isActive: searchParams.get('isActive') ? searchParams.get('isActive') === 'true' : undefined,
      minPrice: searchParams.get('minPrice') ? Number(searchParams.get('minPrice')) : undefined,
      maxPrice: searchParams.get('maxPrice') ? Number(searchParams.get('maxPrice')) : undefined,
      inStock: searchParams.get('inStock') ? searchParams.get('inStock') === 'true' : undefined,
    };
  }, [searchParams]);

  const getCurrentPage = useCallback((): number => {
    return Number(searchParams.get('page')) || 1;
  }, [searchParams]);

  const updateUrlParams = (filters: ProductFilter, page: number = 1) => {
    const params = new URLSearchParams();
    
    if (filters.search) params.set('search', filters.search);
    if (filters.category) params.set('category', filters.category);
    if (filters.isActive !== undefined) params.set('isActive', filters.isActive.toString());
    if (filters.minPrice !== undefined) params.set('minPrice', filters.minPrice.toString());
    if (filters.maxPrice !== undefined) params.set('maxPrice', filters.maxPrice.toString());
    if (filters.inStock !== undefined) params.set('inStock', filters.inStock.toString());
    if (page > 1) params.set('page', page.toString());

    setSearchParams(params);
  };

  const loadProducts = useCallback(async (filters: ProductFilter, page: number) => {
    try {
      setLoading(true);
      setError(null);

      const params = {
        page,
        pageSize: 12,
        search: filters.search || undefined,
        category: filters.category || undefined,
        isActive: filters.isActive,
      };

      const result = await productService.getProducts(params);
      setProducts(result);

      // Extract unique categories for filter dropdown
      if (result && result.items) {
        const uniqueCategories = Array.from(
          new Set(result.items.map((p: ProductDto) => p.category).filter(Boolean))
        ).sort() as string[];
        setCategories(prev => {
          const combined = Array.from(new Set([...prev, ...uniqueCategories])).sort();
          return combined;
        });
      }

    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to load products';
      setError(errorMessage);
      console.error('Error loading products:', err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    const filters = getFiltersFromUrl();
    const page = getCurrentPage();
    loadProducts(filters, page);
  }, [loadProducts, getFiltersFromUrl, getCurrentPage]);

  const handleFiltersChange = (newFilters: ProductFilter) => {
    updateUrlParams(newFilters, 1);
  };

  const handleSearch = (searchTerm: string) => {
    const currentFilters = getFiltersFromUrl();
    handleFiltersChange({ ...currentFilters, search: searchTerm });
  };

  const handlePageChange = (page: number) => {
    const currentFilters = getFiltersFromUrl();
    updateUrlParams(currentFilters, page);
  };

  const handleViewDetails = (product: ProductDto) => {
    setSelectedProduct(product);
    setDetailModalOpen(true);
  };

  const handleAddToCart = (product: ProductDto, quantity: number) => {
    // TODO: Implement cart functionality in Phase 4
    alert(`Added ${quantity} ${product.name} to cart!`);
  };

  const handleCloseModal = () => {
    setDetailModalOpen(false);
    setSelectedProduct(null);
  };

  const currentFilters = getFiltersFromUrl();

  return (
    <Box sx={{ 
      width: '100%', 
      py: 4, 
      px: { xs: 2, sm: 4, md: 6, lg: 8 } 
    }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Products
      </Typography>
      
      <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
        Browse our complete product catalog
      </Typography>

      <ProductFilters
        filters={currentFilters}
        onFiltersChange={handleFiltersChange}
        onSearch={handleSearch}
        categories={categories}
        loading={loading}
      />

      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      <ProductList
        products={products}
        loading={loading}
        error={error}
        onPageChange={handlePageChange}
        onViewDetails={handleViewDetails}
        emptyMessage="No products found matching your criteria"
      />

      <ProductDetail
        product={selectedProduct}
        open={detailModalOpen}
        onClose={handleCloseModal}
        onAddToCart={handleAddToCart}
        loading={loading}
      />
    </Box>
  );
};

export default ProductsPage;