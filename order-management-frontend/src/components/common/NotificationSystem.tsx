import React, { createContext, useContext, useState, type ReactNode } from 'react';
import { 
  Snackbar, 
  Alert, 
  Slide, 
  type SlideProps,
  IconButton
} from '@mui/material';
import { Close as CloseIcon } from '@mui/icons-material';

type NotificationType = 'success' | 'error' | 'warning' | 'info';

interface Notification {
  id: string;
  message: string;
  type: NotificationType;
  duration?: number;
  action?: ReactNode;
}

interface NotificationContextType {
  showNotification: (message: string, type?: NotificationType, duration?: number) => void;
  showSuccess: (message: string, duration?: number) => void;
  showError: (message: string, duration?: number) => void;
  showWarning: (message: string, duration?: number) => void;
  showInfo: (message: string, duration?: number) => void;
  hideNotification: (id?: string) => void;
}

const NotificationContext = createContext<NotificationContextType | undefined>(undefined);

function TransitionUp(props: SlideProps) {
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
  defaultDuration = 6000,
}) => {
  const [notifications, setNotifications] = useState<Notification[]>([]);

  const generateId = () => Math.random().toString(36).substr(2, 9);

  const showNotification = (
    message: string,
    type: NotificationType = 'info',
    duration: number = defaultDuration
  ) => {
    const id = generateId();
    const newNotification: Notification = { id, message, type, duration };

    setNotifications(prev => {
      const updated = [...prev, newNotification];
      // Keep only the latest notifications
      return updated.slice(-maxNotifications);
    });

    // Auto-hide after duration
    if (duration > 0) {
      setTimeout(() => {
        hideNotification(id);
      }, duration);
    }
  };

  const showSuccess = (message: string, duration?: number) => {
    showNotification(message, 'success', duration);
  };

  const showError = (message: string, duration?: number) => {
    showNotification(message, 'error', duration);
  };

  const showWarning = (message: string, duration?: number) => {
    showNotification(message, 'warning', duration);
  };

  const showInfo = (message: string, duration?: number) => {
    showNotification(message, 'info', duration);
  };

  const hideNotification = (id?: string) => {
    if (id) {
      setNotifications(prev => prev.filter(notification => notification.id !== id));
    } else {
      // Hide the most recent notification
      setNotifications(prev => prev.slice(0, -1));
    }
  };

  const contextValue: NotificationContextType = {
    showNotification,
    showSuccess,
    showError,
    showWarning,
    showInfo,
    hideNotification,
  };

  return (
    <NotificationContext.Provider value={contextValue}>
      {children}
      
      {/* Render notifications */}
      {notifications.map((notification, index) => (
        <Snackbar
          key={notification.id}
          open={true}
          TransitionComponent={TransitionUp}
          anchorOrigin={{ 
            vertical: 'bottom', 
            horizontal: 'right' 
          }}
          sx={{
            // Stack multiple notifications
            '& .MuiSnackbar-root': {
              position: 'relative',
              transform: `translateY(-${index * 60}px)`,
            }
          }}
        >
          <Alert
            severity={notification.type}
            variant="filled"
            sx={{ width: '100%', minWidth: 300 }}
            action={
              <IconButton
                size="small"
                aria-label="close"
                color="inherit"
                onClick={() => hideNotification(notification.id)}
              >
                <CloseIcon fontSize="small" />
              </IconButton>
            }
          >
            {notification.message}
          </Alert>
        </Snackbar>
      ))}
    </NotificationContext.Provider>
  );
};

export const useNotification = (): NotificationContextType => {
  const context = useContext(NotificationContext);
  if (!context) {
    throw new Error('useNotification must be used within a NotificationProvider');
  }
  return context;
};

// HOC for easier integration
export const withNotifications = <P extends object>(
  Component: React.ComponentType<P>
): React.FC<P> => {
  return (props: P) => (
    <NotificationProvider>
      <Component {...props} />
    </NotificationProvider>
  );
};