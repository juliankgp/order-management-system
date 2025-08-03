import type { ReactNode } from 'react';

export type ConfirmVariant = 'danger' | 'warning' | 'info' | 'success';

export interface ConfirmOptions {
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

export interface ConfirmState extends ConfirmOptions {
  open: boolean;
  onConfirm?: () => void | Promise<void>;
  onCancel?: () => void;
}

export interface ConfirmContextType {
  confirm: (options: ConfirmOptions) => Promise<boolean>;
  confirmDangerous: (message: string, title?: string) => Promise<boolean>;
  confirmDelete: (itemName?: string) => Promise<boolean>;
}

export interface WithConfirmProps {
  confirm: ConfirmContextType;
}
