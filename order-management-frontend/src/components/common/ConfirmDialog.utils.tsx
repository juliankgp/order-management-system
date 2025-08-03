import type React from 'react';
import {
  Warning as WarningIcon,
  Info as InfoIcon,
  CheckCircle as CheckCircleIcon
} from '@mui/icons-material';

export const variantConfig = {
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

export const withConfirm = <P extends { confirm: unknown }>(
  Component: React.ComponentType<P>
) => {
  return (props: Omit<P, 'confirm'>) => {
    // This would need to import useConfirm hook
    // For now, we'll keep this as a placeholder
    return <Component {...(props as P)} confirm={undefined} />;
  };
};
