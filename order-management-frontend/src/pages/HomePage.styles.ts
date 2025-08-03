import { styled } from '@mui/material/styles';
import { Box, Card, Paper, Typography } from '@mui/material';

// Main container
export const PageContainer = styled(Box)({
  minHeight: '100vh',
  background: 'linear-gradient(180deg, #f8fafc 0%, #ffffff 100%)'
});

// Hero section
export const HeroContainer = styled(Box)({
  textAlign: 'center',
  paddingTop: '4rem',
  paddingBottom: '3rem'
});

export const GradientTitle = styled(Typography)(({ theme }) => ({
  background: 'linear-gradient(135deg, #2563eb 0%, #7c3aed 100%)',
  backgroundClip: 'text',
  WebkitBackgroundClip: 'text',
  WebkitTextFillColor: 'transparent',
  marginBottom: theme.spacing(3)
}));

// Feature cards
export const FeatureCard = styled(Card)({
  width: 320,
  height: 420,
  display: 'flex',
  flexDirection: 'column',
  position: 'relative',
  overflow: 'hidden',
  transition: 'all 0.3s ease-in-out',
  '&:hover': {
    transform: 'translateY(-8px)',
    boxShadow: '0 20px 40px rgba(0,0,0,0.15)'
  }
});

export const FeatureIconContainer = styled(Box)({
  height: 160,
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  position: 'relative'
});

export const FeatureIconCircle = styled(Box)({
  width: 80,
  height: 80,
  borderRadius: '50%',
  backgroundColor: 'rgba(255, 255, 255, 0.2)',
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  backdropFilter: 'blur(10px)',
  border: '2px solid rgba(255, 255, 255, 0.3)'
});

// Benefits section
export const BenefitsSection = styled(Box)(() => ({
  background: `rgba(37, 99, 235, 0.03)`,
  padding: '4rem 0'
}));

export const BenefitIconBox = styled(Box)<{ color: string }>(({ color }) => ({
  width: 48,
  height: 48,
  borderRadius: '12px',
  background: `${color}20`,
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  marginRight: '1.5rem',
  color: color
}));

// Stats cards
export const StatsCard = styled(Paper)(({ theme }) => ({
  padding: theme.spacing(2),
  textAlign: 'center',
  borderRadius: 12,
  background: `rgba(37, 99, 235, 0.04)`,
  border: `1px solid rgba(37, 99, 235, 0.12)`
}));

// Call to action
export const CTAContainer = styled(Box)({
  padding: '4rem 0',
  display: 'flex',
  justifyContent: 'center'
});

export const CTAPaper = styled(Paper)({
  padding: '3rem',
  textAlign: 'center',
  borderRadius: 16,
  background: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
  color: 'white',
  maxWidth: 600,
  width: '100%'
});