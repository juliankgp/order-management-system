import React from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  DialogContentText,
  Button,
  Box,
  Typography,
  useTheme,
  useMediaQuery,
} from '@mui/material';
import {
  Warning as WarningIcon,
  Delete as DeleteIcon,
  Info as InfoIcon,
  Error as ErrorIcon,
} from '@mui/icons-material';

export type ConfirmDialogType = 'warning' | 'error' | 'info' | 'delete';

interface ConfirmDialogProps {
  open: boolean;
  onClose: () => void;
  onConfirm: () => void;
  title?: string;
  message?: string;
  type?: ConfirmDialogType;
  confirmText?: string;
  cancelText?: string;
  loading?: boolean;
  maxWidth?: 'xs' | 'sm' | 'md' | 'lg' | 'xl';
}

const ConfirmDialog: React.FC<ConfirmDialogProps> = ({
  open,
  onClose,
  onConfirm,
  title,
  message,
  type = 'warning',
  confirmText,
  cancelText = 'Cancel',
  loading = false,
  maxWidth = 'sm',
}) => {
  const theme = useTheme();
  const fullScreen = useMediaQuery(theme.breakpoints.down('sm'));

  const getTypeConfig = () => {
    switch (type) {
      case 'error':
        return {
          icon: <ErrorIcon sx={{ color: theme.palette.error.main, fontSize: 48 }} />,
          title: title || 'Error',
          confirmText: confirmText || 'OK',
          confirmColor: 'error' as const,
          confirmVariant: 'contained' as const,
        };
      case 'delete':
        return {
          icon: <DeleteIcon sx={{ color: theme.palette.error.main, fontSize: 48 }} />,
          title: title || 'Delete Item',
          confirmText: confirmText || 'Delete',
          confirmColor: 'error' as const,
          confirmVariant: 'contained' as const,
        };
      case 'info':
        return {
          icon: <InfoIcon sx={{ color: theme.palette.info.main, fontSize: 48 }} />,
          title: title || 'Information',
          confirmText: confirmText || 'OK',
          confirmColor: 'primary' as const,
          confirmVariant: 'contained' as const,
        };
      case 'warning':
      default:
        return {
          icon: <WarningIcon sx={{ color: theme.palette.warning.main, fontSize: 48 }} />,
          title: title || 'Are you sure?',
          confirmText: confirmText || 'Confirm',
          confirmColor: 'warning' as const,
          confirmVariant: 'contained' as const,
        };
    }
  };

  const config = getTypeConfig();

  const handleConfirm = async () => {
    try {
      await onConfirm();
      onClose();
    } catch (error) {
      // Error handling should be done by the caller
      console.error('Confirm action failed:', error);
    }
  };

  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth={maxWidth}
      fullWidth
      fullScreen={fullScreen}
      aria-labelledby="confirm-dialog-title"
      aria-describedby="confirm-dialog-description"
    >
      <DialogTitle id="confirm-dialog-title">
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          {config.icon}
          <Typography variant="h6" component="div">
            {config.title}
          </Typography>
        </Box>
      </DialogTitle>

      {message && (
        <DialogContent>
          <DialogContentText id="confirm-dialog-description">
            {message}
          </DialogContentText>
        </DialogContent>
      )}

      <DialogActions sx={{ p: 3, gap: 1 }}>
        <Button
          onClick={onClose}
          variant="outlined"
          disabled={loading}
          size="large"
        >
          {cancelText}
        </Button>
        <Button
          onClick={handleConfirm}
          color={config.confirmColor}
          variant={config.confirmVariant}
          disabled={loading}
          size="large"
          autoFocus
        >
          {loading ? 'Processing...' : config.confirmText}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default ConfirmDialog;

// Hook for easier usage
export const useConfirmDialog = () => {
  const [dialogState, setDialogState] = React.useState<{
    open: boolean;
    props: Partial<ConfirmDialogProps>;
  }>({
    open: false,
    props: {},
  });

  const showDialog = (props: Partial<ConfirmDialogProps>) => {
    return new Promise<boolean>((resolve) => {
      setDialogState({
        open: true,
        props: {
          ...props,
          onConfirm: () => {
            if (props.onConfirm) {
              props.onConfirm();
            }
            resolve(true);
          },
          onClose: () => {
            if (props.onClose) {
              props.onClose();
            }
            resolve(false);
          },
        },
      });
    });
  };

  const hideDialog = () => {
    setDialogState({ open: false, props: {} });
  };

  const ConfirmDialogComponent = () => (
    <ConfirmDialog
      {...dialogState.props}
      open={dialogState.open}
      onClose={hideDialog}
      onConfirm={dialogState.props.onConfirm || (() => {})}
    />
  );

  return {
    showDialog,
    hideDialog,
    ConfirmDialogComponent,
  };
};