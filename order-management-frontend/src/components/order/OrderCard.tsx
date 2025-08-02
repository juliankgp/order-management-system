import React from 'react';
import {
  Card,
  CardContent,
  Box,
  Typography,
  Chip,
  Button,
  Avatar,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  List,
  ListItem,
  ListItemAvatar,
  ListItemText,
} from '@mui/material';
import {
  ExpandMore as ExpandMoreIcon,
  Receipt as ReceiptIcon,
  LocalShipping as ShippingIcon,
  Schedule as ScheduleIcon,
  CheckCircle as CheckCircleIcon,
  Cancel as CancelIcon,
  Visibility as ViewIcon
} from '@mui/icons-material';
import type { OrderDto } from '../../types/entities';
import { OrderStatus } from '../../types/entities';
import { orderService } from '../../services/orderService';

interface OrderCardProps {
  order: OrderDto;
  onViewDetails?: (order: OrderDto) => void;
  onStatusUpdate?: (orderId: string, status: OrderStatus) => void;
  compact?: boolean;
}

export const OrderCard: React.FC<OrderCardProps> = ({ 
  order, 
  onViewDetails, 
  onStatusUpdate,
  compact = false 
}) => {
  const handleViewDetails = () => {
    if (onViewDetails) {
      onViewDetails(order);
    }
  };

  const handleStatusUpdate = (newStatus: OrderStatus) => {
    if (onStatusUpdate) {
      onStatusUpdate(order.id, newStatus);
    }
  };

  const getStatusIcon = (status: OrderStatus) => {
    switch (status) {
      case 1: return <ScheduleIcon />; // Pending
      case 2: return <CheckCircleIcon />; // Confirmed
      case 3: return <ReceiptIcon />; // Processing
      case 4: return <ShippingIcon />; // Shipped
      case 5: return <CheckCircleIcon />; // Delivered
      case 6: return <CancelIcon />; // Cancelled
      default: return <ScheduleIcon />;
    }
  };

  const canUpdateStatus = (currentStatus: OrderStatus): boolean => {
    // Only allow certain status transitions
    return currentStatus !== 5 && currentStatus !== 6; // Not delivered or cancelled
  };

  const getNextStatusOptions = (currentStatus: OrderStatus): OrderStatus[] => {
    switch (currentStatus) {
      case 1: return [2, 6]; // Pending -> Confirmed or Cancelled
      case 2: return [3, 6]; // Confirmed -> Processing or Cancelled
      case 3: return [4, 6]; // Processing -> Shipped or Cancelled
      case 4: return [5]; // Shipped -> Delivered
      default: return [];
    }
  };

  if (compact) {
    return (
      <Card sx={{ mb: 2 }}>
        <CardContent sx={{ p: 2 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
            <Box sx={{ flex: 1 }}>
              <Typography variant="h6" gutterBottom>
                Order #{order.orderNumber}
              </Typography>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                {new Date(order.orderDate).toLocaleDateString()} • {order.items.length} items
              </Typography>
              <Chip
                icon={getStatusIcon(order.status)}
                label={orderService.getOrderStatusLabel(order.status)}
                color={orderService.getOrderStatusColor(order.status)}
                size="small"
              />
            </Box>
            <Box sx={{ textAlign: 'right' }}>
              <Typography variant="h6" fontWeight={600} color="primary">
                ${order.totalAmount.toFixed(2)}
              </Typography>
              <Button
                size="small"
                startIcon={<ViewIcon />}
                onClick={handleViewDetails}
              >
                View
              </Button>
            </Box>
          </Box>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card sx={{ mb: 3 }}>
      <CardContent>
        {/* Order Header */}
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 3 }}>
          <Box>
            <Typography variant="h5" gutterBottom>
              Order #{order.orderNumber}
            </Typography>
            <Typography variant="body2" color="text.secondary" gutterBottom>
              Placed on {new Date(order.orderDate).toLocaleDateString()} at {new Date(order.orderDate).toLocaleTimeString()}
            </Typography>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mt: 1 }}>
              <Chip
                icon={getStatusIcon(order.status)}
                label={orderService.getOrderStatusLabel(order.status)}
                color={orderService.getOrderStatusColor(order.status)}
              />
              {order.customerName && (
                <Typography variant="body2" color="text.secondary">
                  Customer: {order.customerName}
                </Typography>
              )}
            </Box>
          </Box>
          
          <Box sx={{ textAlign: 'right' }}>
            <Typography variant="h4" color="primary" fontWeight={600}>
              ${order.totalAmount.toFixed(2)}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {order.items.length} {order.items.length === 1 ? 'item' : 'items'}
            </Typography>
          </Box>
        </Box>

        {/* Order Items */}
        <Accordion>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Typography variant="h6">
              Order Items ({order.items.length})
            </Typography>
          </AccordionSummary>
          <AccordionDetails>
            <List>
              {order.items.map((item) => (
                <ListItem key={item.id} divider>
                  <ListItemAvatar>
                    <Avatar variant="rounded">
                      <ReceiptIcon />
                    </Avatar>
                  </ListItemAvatar>
                  <ListItemText
                    primary={item.productName}
                    secondary={`SKU: ${item.productId} • Quantity: ${item.quantity}`}
                  />
                  <Box sx={{ textAlign: 'right' }}>
                    <Typography variant="body2" color="text.secondary">
                      ${item.unitPrice.toFixed(2)} each
                    </Typography>
                    <Typography variant="h6" fontWeight={600}>
                      ${item.subtotal.toFixed(2)}
                    </Typography>
                  </Box>
                </ListItem>
              ))}
            </List>
          </AccordionDetails>
        </Accordion>

        {/* Order Summary */}
        <Box sx={{ mt: 3, p: 2, bgcolor: 'background.default', borderRadius: 1 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
            <Typography variant="body2">Subtotal:</Typography>
            <Typography variant="body2">${order.subTotal.toFixed(2)}</Typography>
          </Box>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
            <Typography variant="body2">Tax:</Typography>
            <Typography variant="body2">${order.taxAmount.toFixed(2)}</Typography>
          </Box>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
            <Typography variant="body2">Shipping:</Typography>
            <Typography variant="body2">
              {order.shippingCost === 0 ? 'Free' : `$${order.shippingCost.toFixed(2)}`}
            </Typography>
          </Box>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', pt: 1, borderTop: '1px solid #ddd' }}>
            <Typography variant="h6" fontWeight={600}>Total:</Typography>
            <Typography variant="h6" fontWeight={600} color="primary">
              ${order.totalAmount.toFixed(2)}
            </Typography>
          </Box>
        </Box>

        {/* Notes */}
        {order.notes && (
          <Box sx={{ mt: 2, p: 2, bgcolor: 'info.light', borderRadius: 1 }}>
            <Typography variant="body2" color="info.dark">
              <strong>Notes:</strong> {order.notes}
            </Typography>
          </Box>
        )}

        {/* Actions */}
        <Box sx={{ mt: 3, display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
          <Button
            variant="outlined"
            startIcon={<ViewIcon />}
            onClick={handleViewDetails}
          >
            View Details
          </Button>
          
          {canUpdateStatus(order.status) && (
            <Box sx={{ display: 'flex', gap: 1 }}>
              {getNextStatusOptions(order.status).map((status) => (
                <Button
                  key={status}
                  variant="contained"
                  size="small"
                  color={status === 6 ? 'error' : 'primary'}
                  onClick={() => handleStatusUpdate(status)}
                >
                  {status === 6 ? 'Cancel' : `Mark as ${orderService.getOrderStatusLabel(status)}`}
                </Button>
              ))}
            </Box>
          )}
        </Box>

        {/* Timestamps */}
        <Box sx={{ mt: 2, pt: 2, borderTop: '1px solid #eee' }}>
          <Typography variant="caption" color="text.secondary">
            Created: {new Date(order.createdAt).toLocaleString()} | 
            Updated: {new Date(order.updatedAt).toLocaleString()}
          </Typography>
        </Box>
      </CardContent>
    </Card>
  );
};