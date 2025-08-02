import React, { useState, useEffect } from 'react';
import {
  Box,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Button,
  Chip,
  InputAdornment,
  Collapse,
  Typography,
  Switch,
  FormControlLabel,
} from '@mui/material';
import {
  Search as SearchIcon,
  FilterList as FilterIcon,
  Clear as ClearIcon,
} from '@mui/icons-material';
import type { ProductFilter } from '../../types';

export interface ProductFiltersProps {
  filters: ProductFilter;
  onFiltersChange: (filters: ProductFilter) => void;
  onSearch: (searchTerm: string) => void;
  categories?: string[];
  loading?: boolean;
  showAdvancedFilters?: boolean;
}

const ProductFilters: React.FC<ProductFiltersProps> = ({
  filters,
  onFiltersChange,
  onSearch,
  categories = [],
  loading = false,
  showAdvancedFilters = true,
}) => {
  const [searchTerm, setSearchTerm] = useState(filters.search || '');
  const [showAdvanced, setShowAdvanced] = useState(false);
  const [searchTimeout, setSearchTimeout] = useState<NodeJS.Timeout | null>(null);

  useEffect(() => {
    setSearchTerm(filters.search || '');
  }, [filters.search]);

  const handleSearchChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const value = event.target.value;
    setSearchTerm(value);

    if (searchTimeout) {
      clearTimeout(searchTimeout);
    }

    const timeout = setTimeout(() => {
      onSearch(value);
    }, 500);

    setSearchTimeout(timeout);
  };

  const handleFilterChange = (field: keyof ProductFilter, value: unknown) => {
    const newFilters = {
      ...filters,
      [field]: value,
    };
    onFiltersChange(newFilters);
  };

  const handleClearFilters = () => {
    const clearedFilters: ProductFilter = {
      search: '',
      category: '',
      isActive: undefined,
      minPrice: undefined,
      maxPrice: undefined,
      inStock: undefined,
    };
    setSearchTerm('');
    onFiltersChange(clearedFilters);
  };

  const getActiveFiltersCount = () => {
    let count = 0;
    if (filters.search) count++;
    if (filters.category) count++;
    if (filters.isActive !== undefined) count++;
    if (filters.minPrice !== undefined) count++;
    if (filters.maxPrice !== undefined) count++;
    if (filters.inStock !== undefined) count++;
    return count;
  };

  const activeFiltersCount = getActiveFiltersCount();

  return (
    <Box sx={{ mb: 3 }}>
      <Box sx={{ display: 'flex', gap: 2, alignItems: 'center', mb: 2 }}>
        <TextField
          fullWidth
          placeholder="Search products..."
          value={searchTerm}
          onChange={handleSearchChange}
          disabled={loading}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon />
              </InputAdornment>
            ),
            endAdornment: searchTerm && (
              <InputAdornment position="end">
                <Button
                  size="small"
                  onClick={() => {
                    setSearchTerm('');
                    onSearch('');
                  }}
                >
                  <ClearIcon />
                </Button>
              </InputAdornment>
            ),
          }}
        />

        <FormControl sx={{ minWidth: 150 }}>
          <InputLabel>Category</InputLabel>
          <Select
            value={filters.category || ''}
            label="Category"
            onChange={(e) => handleFilterChange('category', e.target.value)}
            disabled={loading}
          >
            <MenuItem value="">
              <em>All Categories</em>
            </MenuItem>
            {categories.map((category) => (
              <MenuItem key={category} value={category}>
                {category}
              </MenuItem>
            ))}
          </Select>
        </FormControl>

        {showAdvancedFilters && (
          <Button
            variant="outlined"
            startIcon={<FilterIcon />}
            onClick={() => setShowAdvanced(!showAdvanced)}
            sx={{ whiteSpace: 'nowrap' }}
          >
            Filters {activeFiltersCount > 0 && `(${activeFiltersCount})`}
          </Button>
        )}

        {activeFiltersCount > 0 && (
          <Button
            variant="outlined"
            color="secondary"
            startIcon={<ClearIcon />}
            onClick={handleClearFilters}
            sx={{ whiteSpace: 'nowrap' }}
          >
            Clear
          </Button>
        )}
      </Box>

      {showAdvancedFilters && (
        <Collapse in={showAdvanced}>
          <Box
            sx={{
              p: 3,
              border: '1px solid',
              borderColor: 'divider',
              borderRadius: 1,
              backgroundColor: 'background.paper',
            }}
          >
            <Typography variant="h6" gutterBottom>
              Advanced Filters
            </Typography>

            <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2, mb: 2 }}>
              <FormControl sx={{ minWidth: 120 }}>
                <InputLabel>Status</InputLabel>
                <Select
                  value={filters.isActive === undefined ? '' : (filters.isActive ? 'active' : 'inactive')}
                  label="Status"
                  onChange={(e) => {
                    const value = e.target.value as string;
                    handleFilterChange(
                      'isActive',
                      value === '' ? undefined : value === 'active'
                    );
                  }}
                  disabled={loading}
                >
                  <MenuItem value="">
                    <em>All</em>
                  </MenuItem>
                  <MenuItem value="active">Active</MenuItem>
                  <MenuItem value="inactive">Inactive</MenuItem>
                </Select>
              </FormControl>

              <FormControlLabel
                control={
                  <Switch
                    checked={filters.inStock || false}
                    onChange={(e) => handleFilterChange('inStock', e.target.checked || undefined)}
                    disabled={loading}
                  />
                }
                label="In Stock Only"
              />
            </Box>

            <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
              <TextField
                label="Min Price"
                type="number"
                value={filters.minPrice || ''}
                onChange={(e) => handleFilterChange('minPrice', e.target.value ? Number(e.target.value) : undefined)}
                disabled={loading}
                sx={{ width: 150 }}
                InputProps={{
                  startAdornment: <InputAdornment position="start">$</InputAdornment>,
                }}
              />

              <Typography variant="body2" color="text.secondary">
                to
              </Typography>

              <TextField
                label="Max Price"
                type="number"
                value={filters.maxPrice || ''}
                onChange={(e) => handleFilterChange('maxPrice', e.target.value ? Number(e.target.value) : undefined)}
                disabled={loading}
                sx={{ width: 150 }}
                InputProps={{
                  startAdornment: <InputAdornment position="start">$</InputAdornment>,
                }}
              />
            </Box>
          </Box>
        </Collapse>
      )}

      {activeFiltersCount > 0 && (
        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mt: 2 }}>
          {filters.search && (
            <Chip
              label={`Search: "${filters.search}"`}
              onDelete={() => {
                setSearchTerm('');
                handleFilterChange('search', '');
              }}
              size="small"
            />
          )}
          {filters.category && (
            <Chip
              label={`Category: ${filters.category}`}
              onDelete={() => handleFilterChange('category', '')}
              size="small"
            />
          )}
          {filters.isActive !== undefined && (
            <Chip
              label={`Status: ${filters.isActive ? 'Active' : 'Inactive'}`}
              onDelete={() => handleFilterChange('isActive', undefined)}
              size="small"
            />
          )}
          {filters.inStock && (
            <Chip
              label="In Stock Only"
              onDelete={() => handleFilterChange('inStock', undefined)}
              size="small"
            />
          )}
          {(filters.minPrice !== undefined || filters.maxPrice !== undefined) && (
            <Chip
              label={`Price: $${filters.minPrice || 0} - $${filters.maxPrice || '∞'}`}
              onDelete={() => {
                handleFilterChange('minPrice', undefined);
                handleFilterChange('maxPrice', undefined);
              }}
              size="small"
            />
          )}
        </Box>
      )}
    </Box>
  );
};

export default ProductFilters;