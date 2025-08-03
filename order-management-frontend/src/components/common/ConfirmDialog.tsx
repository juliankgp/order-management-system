/* eslint-disable react-refresh/only-export-components */
import React, { createContext, useContext, useState, useCallback, type ReactNode } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  Button,
  Box,
  IconButton,
  Typography,
  useTheme,
  useMediaQuery
} from '@mui/material';
import {
  Close as CloseIcon,
  Warning as WarningIcon,
  Delete as DeleteIcon,
  Info as InfoIcon,
  CheckCircle as CheckCircleIcon
} from '@mui/icons-material';

type ConfirmVariant = 'danger' | 'warning' | 'info' | 'success';

interface ConfirmOptions {
  title?: string;
  message: string;
  variant?: ConfirmVariant;
  confirmText?: string;
  cancelText?: string;
  showCancel?: boolean;
  dangerous?: boolean;
  icon?: ReactNode;
  maxWidth?: 'xs' | 'sm' | 'md' | 'lg' | 'xl';
}

interface ConfirmState extends ConfirmOptions {
  open: boolean;
  onConfirm?: () => void | Promise<void>;
  onCancel?: () => void;
}

interface ConfirmContextType {
  confirm: (options: ConfirmOptions) => Promise<boolean>;
  confirmDangerous: (message: string, title?: string) => Promise<boolean>;
  confirmDelete: (itemName?: string) => Promise<boolean>;
}

const ConfirmContext = createContext<ConfirmContextType | undefined>(undefined);

const variantConfig = {
  danger: {
    color: '#d32f2f',
    icon: <WarningIcon />,
    confirmColor: 'error' as const
  },
  warning: {
    color: '#ed6c02',
    icon: <WarningIcon />,
    confirmColor: 'warning' as const
  },
  info: {
    color: '#0288d1',
    icon: <InfoIcon />,
    confirmColor: 'primary' as const
  },
  success: {
    color: '#2e7d32',
    icon: <CheckCircleIcon />,
    confirmColor: 'success' as const
  }
};

interface ConfirmProviderProps {
  children: ReactNode;
}

export const ConfirmProvider: React.FC<ConfirmProviderProps> = ({ children }) => {
  const theme = useTheme();
  const fullScreen = useMediaQuery(theme.breakpoints.down('sm'));
  const [state, setState] = useState<ConfirmState>({
    open: false,
    message: '',
    variant: 'info',
    confirmText: 'Confirm',
    cancelText: 'Cancel',
    showCancel: true,
    maxWidth: 'sm'
  });

  const confirm = useCallback((options: ConfirmOptions): Promise<boolean> => {
    return new Promise((resolve) => {
      setState({
        ...options,
        open: true,
        variant: options.variant ?? 'info',
        confirmText: options.confirmText ?? 'Confirm',
        cancelText: options.cancelText ?? 'Cancel',
        showCancel: options.showCancel ?? true,
        maxWidth: options.maxWidth ?? 'sm',
        onConfirm: () => {
          setState(prev => ({ ...prev, open: false }));
          resolve(true);
        },
        onCancel: () => {
          setState(prev => ({ ...prev, open: false }));
          resolve(false);
        }
      });
    });
  }, []);

  const confirmDangerous = useCallback((message: string, title = 'Confirm Action'): Promise<boolean> => {
    return confirm({
      title,
      message,
      variant: 'danger',
      confirmText: 'Yes, Continue',
      cancelText: 'Cancel',
      dangerous: true
    });
  }, [confirm]);

  const confirmDelete = useCallback((itemName = 'this item'): Promise<boolean> => {
    return confirm({
      title: 'Delete Confirmation',
      message: `Are you sure you want to delete ${itemName}? This action cannot be undone.`,
      variant: 'danger',
      confirmText: 'Delete',
      cancelText: 'Cancel',
      dangerous: true,
      icon: <DeleteIcon />
    });
  }, [confirm]);

  const handleClose = () => {
    if (state.onCancel) {
      state.onCancel();
    }
  };

  const handleConfirm = async () => {
    if (state.onConfirm) {
      await state.onConfirm();
    }
  };

  const handleCancel = () => {
    if (state.onCancel) {
      state.onCancel();
    }
  };

  const config = variantConfig[state.variant || 'info'];

  const contextValue: ConfirmContextType = {
    confirm,
    confirmDangerous,
    confirmDelete
  };

  return (
    <ConfirmContext.Provider value={contextValue}>
      {children}
      <Dialog
        open={state.open}
        onClose={handleClose}
        maxWidth={state.maxWidth}
        fullWidth
        fullScreen={fullScreen}
        aria-labelledby="confirm-dialog-title"
        aria-describedby="confirm-dialog-description"
      >
        <DialogTitle id="confirm-dialog-title">
          <Box display="flex" alignItems="center" justifyContent="space-between">
            <Box display="flex" alignItems="center" gap={1}>
              {state.icon || (
                <Box sx={{ color: config.color }}>
                  {config.icon}
                </Box>
              )}
              <Typography variant="h6" component="span">
                {state.title || 'Confirm'}
              </Typography>
            </Box>
            <IconButton
              aria-label="close"
              onClick={handleClose}
              size="small"
            >
              <CloseIcon />
            </IconButton>
          </Box>
        </DialogTitle>

        <DialogContent>
          <DialogContentText id="confirm-dialog-description">
            {state.message}
          </DialogContentText>
        </DialogContent>

        <DialogActions sx={{ p: 2, pt: 1 }}>
          {state.showCancel && (
            <Button 
              onClick={handleCancel}
              variant="outlined"
              size="large"
            >
              {state.cancelText}
            </Button>
          )}
          <Button
            onClick={handleConfirm}
            variant={state.dangerous ? 'contained' : 'contained'}
            color={config.confirmColor}
            size="large"
            autoFocus
          >
            {state.confirmText}
          </Button>
        </DialogActions>
      </Dialog>
    </ConfirmContext.Provider>
  );
};

export const useConfirm = (): ConfirmContextType => {
  const context = useContext(ConfirmContext);
  if (!context) {
    throw new Error('useConfirm must be used within a ConfirmProvider');
  }
  return context;
};

// Higher-order component for easy integration
export interface WithConfirmProps {
  confirm: ConfirmContextType;
}

export const withConfirm = <P extends WithConfirmProps>(
  Component: React.ComponentType<P>
) => {
  return (props: Omit<P, 'confirm'>) => {
    const confirm = useConfirm();
    return <Component {...(props as P)} confirm={confirm} />;
  };
};

export default ConfirmProvider;