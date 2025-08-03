// Layout Components
export { default as ProtectedRoute } from './ProtectedRoute';

// Error Handling
export { default as ErrorBoundary } from './ErrorBoundary';

// Loading Components
export { 
  default as LoadingSpinner,
  LoadingSkeleton,
  PageLoading,
  ButtonLoading
} from './Loading';

// Notifications
export { 
  default as NotificationProvider,
  NotificationProvider as Notifications,
  useNotifications,
  useApiErrorHandler
} from './Notifications';

// Confirmation Dialogs
export { 
  default as ConfirmProvider,
  ConfirmProvider as Confirm,
  useConfirm,
  withConfirm
} from './ConfirmDialog';
export type { WithConfirmProps } from './ConfirmDialog';

// Search
export { default as SearchBar } from './SearchBar';
export type { SearchSuggestion } from './SearchBar';