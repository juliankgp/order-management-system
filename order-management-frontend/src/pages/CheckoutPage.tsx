import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Checkout } from '../components/order/Checkout';

const CheckoutPage: React.FC = () => {
  const navigate = useNavigate();

  const handleBack = () => {
    navigate(-1); // Go back to previous page
  };

  const handleComplete = (orderId: string) => {
    // Navigate to order confirmation or orders page
    navigate(`/orders?orderId=${orderId}`);
  };

  return (
    <Checkout 
      onBack={handleBack}
      onComplete={handleComplete}
    />
  );
};

export default CheckoutPage;