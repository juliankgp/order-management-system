import React, { createContext, useContext, useState, useCallback, ReactNode } from 'react';
import { 
  Snackbar, 
  Alert, 
  AlertTitle, 
  IconButton,
  Slide
} from '@mui/material';
import type { SlideProps } from '@mui/material/Slide';
import { Close as CloseIcon } from '@mui/icons-material';

type NotificationType = 'success' | 'error' | 'warning' | 'info';

interface Notification {
  id: string;
  type: NotificationType;
  title?: string;
  message: string;
  duration?: number;
  action?: ReactNode;
}

interface NotificationContextType {
  showNotification: (notification: Omit<Notification, 'id'>) => void;
  showSuccess: (message: string, title?: string) => void;
  showError: (message: string, title?: string) => void;
  showWarning: (message: string, title?: string) => void;
  showInfo: (message: string, title?: string) => void;
  hideNotification: (id?: string) => void;
}

const NotificationContext = createContext<NotificationContextType | undefined>(undefined);

function SlideTransition(props: SlideProps) {
  return <Slide {...props} direction="up" />;
}

interface NotificationProviderProps {
  children: ReactNode;
  maxNotifications?: number;
  defaultDuration?: number;
}

export const NotificationProvider: React.FC<NotificationProviderProps> = ({ 
  children, 
  maxNotifications = 3,
  defaultDuration = 6000
}) => {
  const [notifications, setNotifications] = useState<Notification[]>([]);

  const generateId = () => Date.now().toString() + Math.random().toString(36).substr(2, 9);

  const showNotification = useCallback((notification: Omit<Notification, 'id'>) => {
    const id = generateId();
    const newNotification: Notification = {
      ...notification,
      id,
      duration: notification.duration ?? defaultDuration
    };

    setNotifications(prev => {
      const updated = [...prev, newNotification];
      // Keep only the latest notifications
      return updated.slice(-maxNotifications);
    });

    // Auto-hide after duration
    if (newNotification.duration && newNotification.duration > 0) {
      setTimeout(() => {
        hideNotification(id);
      }, newNotification.duration);
    }
  }, [maxNotifications, defaultDuration]);

  const hideNotification = useCallback((id?: string) => {
    if (id) {
      setNotifications(prev => prev.filter(n => n.id !== id));
    } else {
      // Hide the oldest notification
      setNotifications(prev => prev.slice(1));
    }
  }, []);

  const showSuccess = useCallback((message: string, title?: string) => {
    showNotification({ type: 'success', message, title });
  }, [showNotification]);

  const showError = useCallback((message: string, title?: string) => {
    showNotification({ type: 'error', message, title, duration: 8000 });
  }, [showNotification]);

  const showWarning = useCallback((message: string, title?: string) => {
    showNotification({ type: 'warning', message, title, duration: 7000 });
  }, [showNotification]);

  const showInfo = useCallback((message: string, title?: string) => {
    showNotification({ type: 'info', message, title });
  }, [showNotification]);

  const contextValue: NotificationContextType = {
    showNotification,
    showSuccess,
    showError,
    showWarning,
    showInfo,
    hideNotification
  };

  return (
    <NotificationContext.Provider value={contextValue}>
      {children}
      {notifications.map((notification, index) => (
        <Snackbar
          key={notification.id}
          open={true}
          onClose={() => hideNotification(notification.id)}
          TransitionComponent={SlideTransition}
          anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
          style={{
            marginBottom: index * 70 // Stack notifications
          }}
        >
          <Alert
            severity={notification.type}
            onClose={() => hideNotification(notification.id)}
            action={
              notification.action || (
                <IconButton
                  size="small"
                  aria-label="close"
                  color="inherit"
                  onClick={() => hideNotification(notification.id)}
                >
                  <CloseIcon fontSize="small" />
                </IconButton>
              )
            }
            sx={{ minWidth: 350 }}
          >
            {notification.title && (
              <AlertTitle>{notification.title}</AlertTitle>
            )}
            {notification.message}
          </Alert>
        </Snackbar>
      ))}
    </NotificationContext.Provider>
  );
};

export const useNotifications = (): NotificationContextType => {
  const context = useContext(NotificationContext);
  if (!context) {
    throw new Error('useNotifications must be used within a NotificationProvider');
  }
  return context;
};

// Helper hook for API error handling
export const useApiErrorHandler = () => {
  const { showError } = useNotifications();

  return useCallback((error: unknown, defaultMessage = 'An unexpected error occurred') => {
    const err = error as any;
    if (err?.response?.data?.message) {
      showError(err.response.data.message);
    } else if (err?.message) {
      showError(err.message);
    } else {
      showError(defaultMessage);
    }
  }, [showError]);
};

export default NotificationProvider;