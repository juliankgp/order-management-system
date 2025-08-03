import React, { useState, useCallback, useEffect, useRef } from 'react';
import {
  TextField,
  InputAdornment,
  IconButton,
  Box,
  Paper,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Popper,
  ClickAwayListener,
  Fade,
  Typography,
  Chip
} from '@mui/material';
import {
  Search as SearchIcon,
  Clear as ClearIcon,
  History as HistoryIcon,
  TrendingUp as TrendingIcon
} from '@mui/icons-material';

export interface SearchSuggestion {
  id: string;
  text: string;
  category?: string;
  icon?: React.ReactNode;
}

interface SearchBarProps {
  value?: string;
  placeholder?: string;
  onSearch: (query: string) => void;
  onChange?: (query: string) => void;
  onClear?: () => void;
  debounceMs?: number;
  size?: 'small' | 'medium';
  fullWidth?: boolean;
  variant?: 'outlined' | 'filled' | 'standard';
  
  // Advanced features
  showSuggestions?: boolean;
  suggestions?: SearchSuggestion[];
  onSuggestionSelect?: (suggestion: SearchSuggestion) => void;
  
  // History features
  enableHistory?: boolean;
  maxHistoryItems?: number;
  historyKey?: string;
  
  // Popular searches
  popularSearches?: string[];
  showPopularWhenEmpty?: boolean;
  
  // Styling
  sx?: object;
}

const SearchBar: React.FC<SearchBarProps> = ({
  value = '',
  placeholder = 'Search...',
  onSearch,
  onChange,
  onClear,
  debounceMs = 300,
  size = 'medium',
  fullWidth = true,
  variant = 'outlined',
  showSuggestions = false,
  suggestions = [],
  onSuggestionSelect,
  enableHistory = false,
  maxHistoryItems = 5,
  historyKey = 'searchHistory',
  popularSearches = [],
  showPopularWhenEmpty = false,
  sx
}) => {
  const [query, setQuery] = useState(value);
  const [showDropdown, setShowDropdown] = useState(false);
  const [searchHistory, setSearchHistory] = useState<string[]>([]);
  const searchRef = useRef<HTMLDivElement>(null);
  const debounceRef = useRef<NodeJS.Timeout | undefined>(undefined);

  // Load search history from localStorage
  useEffect(() => {
    if (enableHistory) {
      const saved = localStorage.getItem(historyKey);
      if (saved) {
        try {
          setSearchHistory(JSON.parse(saved));
        } catch (e) {
          console.warn('Failed to parse search history:', e);
        }
      }
    }
  }, [enableHistory, historyKey]);

  // Save search history to localStorage
  const saveToHistory = useCallback((searchQuery: string) => {
    if (!enableHistory || !searchQuery.trim()) return;

    const updated = [
      searchQuery,
      ...searchHistory.filter(item => item !== searchQuery)
    ].slice(0, maxHistoryItems);

    setSearchHistory(updated);
    localStorage.setItem(historyKey, JSON.stringify(updated));
  }, [enableHistory, searchHistory, maxHistoryItems, historyKey]);

  // Debounced search
  useEffect(() => {
    if (debounceRef.current) {
      clearTimeout(debounceRef.current);
    }

    debounceRef.current = setTimeout(() => {
      if (onChange) {
        onChange(query);
      }
    }, debounceMs);

    return () => {
      if (debounceRef.current) {
        clearTimeout(debounceRef.current);
      }
    };
  }, [query, onChange, debounceMs]);

  const handleInputChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = event.target.value;
    setQuery(newValue);
    
    if (showSuggestions && newValue.length > 0) {
      setShowDropdown(true);
    }
  };

  const handleSearch = useCallback(() => {
    const trimmedQuery = query.trim();
    if (trimmedQuery) {
      onSearch(trimmedQuery);
      saveToHistory(trimmedQuery);
      setShowDropdown(false);
    }
  }, [query, onSearch, saveToHistory]);

  const handleKeyPress = (event: React.KeyboardEvent) => {
    if (event.key === 'Enter') {
      handleSearch();
    } else if (event.key === 'Escape') {
      setShowDropdown(false);
    }
  };

  const handleClear = () => {
    setQuery('');
    setShowDropdown(false);
    if (onClear) {
      onClear();
    }
    if (onChange) {
      onChange('');
    }
  };

  const handleSuggestionClick = (suggestion: SearchSuggestion) => {
    setQuery(suggestion.text);
    setShowDropdown(false);
    
    if (onSuggestionSelect) {
      onSuggestionSelect(suggestion);
    } else {
      onSearch(suggestion.text);
      saveToHistory(suggestion.text);
    }
  };

  const handleHistoryClick = (historyItem: string) => {
    setQuery(historyItem);
    setShowDropdown(false);
    onSearch(historyItem);
  };

  const handleFocus = () => {
    if (showSuggestions || enableHistory || showPopularWhenEmpty) {
      setShowDropdown(true);
    }
  };

  const handleClickAway = () => {
    setShowDropdown(false);
  };

  const getDropdownItems = () => {
    const items: React.ReactNode[] = [];

    // Show suggestions first if query exists
    if (query.trim() && suggestions.length > 0) {
      items.push(
        <Typography
          key="suggestions-header"
          variant="caption"
          sx={{ px: 2, py: 1, color: 'text.secondary', fontWeight: 'bold' }}
        >
          Suggestions
        </Typography>
      );

      suggestions
        .filter(s => s.text.toLowerCase().includes(query.toLowerCase()))
        .slice(0, 5)
        .forEach(suggestion => {
          items.push(
            <ListItem
              key={suggestion.id}
              component="button"
              onClick={() => handleSuggestionClick(suggestion)}
            >
              <ListItemIcon>
                {suggestion.icon || <SearchIcon />}
              </ListItemIcon>
              <ListItemText 
                primary={suggestion.text}
                secondary={suggestion.category}
              />
            </ListItem>
          );
        });
    }

    // Show search history
    if (enableHistory && searchHistory.length > 0) {
      if (items.length > 0) {
        items.push(<Box key="divider-1" sx={{ borderTop: 1, borderColor: 'divider', my: 1 }} />);
      }
      
      items.push(
        <Typography
          key="history-header"
          variant="caption"
          sx={{ px: 2, py: 1, color: 'text.secondary', fontWeight: 'bold' }}
        >
          Recent Searches
        </Typography>
      );

      searchHistory.slice(0, 3).forEach((historyItem, index) => {
        items.push(
          <ListItem
            key={`history-${index}`}
            component="button"
            onClick={() => handleHistoryClick(historyItem)}
          >
            <ListItemIcon>
              <HistoryIcon />
            </ListItemIcon>
            <ListItemText primary={historyItem} />
          </ListItem>
        );
      });
    }

    // Show popular searches when empty
    if (showPopularWhenEmpty && !query.trim() && popularSearches.length > 0) {
      if (items.length > 0) {
        items.push(<Box key="divider-2" sx={{ borderTop: 1, borderColor: 'divider', my: 1 }} />);
      }

      items.push(
        <Typography
          key="popular-header"
          variant="caption"
          sx={{ px: 2, py: 1, color: 'text.secondary', fontWeight: 'bold' }}
        >
          Popular Searches
        </Typography>
      );

      items.push(
        <Box key="popular-chips" sx={{ px: 2, py: 1 }}>
          {popularSearches.slice(0, 5).map((popular, index) => (
            <Chip
              key={index}
              label={popular}
              size="small"
              onClick={() => handleHistoryClick(popular)}
              sx={{ mr: 1, mb: 1 }}
              icon={<TrendingIcon fontSize="small" />}
            />
          ))}
        </Box>
      );
    }

    return items;
  };

  const dropdownItems = getDropdownItems();

  return (
    <ClickAwayListener onClickAway={handleClickAway}>
      <Box sx={{ position: 'relative', ...sx }}>
        <TextField
          ref={searchRef}
          value={query}
          onChange={handleInputChange}
          onKeyPress={handleKeyPress}
          onFocus={handleFocus}
          placeholder={placeholder}
          size={size}
          fullWidth={fullWidth}
          variant={variant}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon />
              </InputAdornment>
            ),
            endAdornment: query && (
              <InputAdornment position="end">
                <IconButton
                  aria-label="clear search"
                  onClick={handleClear}
                  size="small"
                  edge="end"
                >
                  <ClearIcon />
                </IconButton>
              </InputAdornment>
            )
          }}
        />

        <Popper
          open={showDropdown && dropdownItems.length > 0}
          anchorEl={searchRef.current}
          placement="bottom-start"
          style={{ width: searchRef.current?.offsetWidth, zIndex: 1300 }}
          transition
        >
          {({ TransitionProps }) => (
            <Fade {...TransitionProps}>
              <Paper elevation={4} sx={{ maxHeight: 300, overflow: 'auto' }}>
                <List dense>
                  {dropdownItems}
                </List>
              </Paper>
            </Fade>
          )}
        </Popper>
      </Box>
    </ClickAwayListener>
  );
};

export default SearchBar;