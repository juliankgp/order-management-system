import React from 'react';
import { 
  Typography, 
  Box, 
  Container, 
  Grid, 
  CardContent, 
  CardActions,
  Button,
  Chip,
  useTheme
} from '@mui/material';
import { 
  ShoppingCart as ShoppingCartIcon,
  Assignment as OrderIcon,
  Inventory as ProductIcon,
  AccountCircle as AccountIcon,
  TrendingUp as TrendingIcon,
  Security as SecurityIcon,
  AutoAwesome as AutoAwesomeIcon,
  Rocket as RocketIcon,
  Shield as ShieldIcon,
  Analytics as AnalyticsIcon,
  CloudSync as CloudSyncIcon,
  Star as StarIcon
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import {
  PageContainer,
  HeroContainer,
  GradientTitle,
  FeatureCard,
  FeatureIconContainer,
  FeatureIconCircle,
  BenefitsSection,
  BenefitIconBox,
  StatsCard,
  CTAContainer,
  CTAPaper
} from './HomePage.styles';

const HomePage: React.FC = () => {
  const navigate = useNavigate();
  const { isAuthenticated, user } = useAuth();
  const theme = useTheme();

  const features = [
    {
      title: 'Smart Product Catalog',
      description: 'Advanced inventory management with AI-powered search, smart filtering, and real-time stock monitoring.',
      icon: <ProductIcon sx={{ fontSize: 40, color: 'white' }} />,
      gradient: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
      action: () => navigate(isAuthenticated ? '/products' : '/login')
    },
    {
      title: 'Intelligent Orders',
      description: 'Streamlined order processing with automated workflows, real-time tracking, and smart analytics.',
      icon: <OrderIcon sx={{ fontSize: 40, color: 'white' }} />,
      gradient: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
      action: () => navigate(isAuthenticated ? '/orders' : '/login')
    },
    {
      title: 'Seamless Shopping',
      description: 'Intuitive shopping experience with smart recommendations, quick checkout, and secure payments.',
      icon: <ShoppingCartIcon sx={{ fontSize: 40, color: 'white' }} />,
      gradient: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
      action: () => navigate(isAuthenticated ? '/products' : '/login')
    }
  ];

  const benefits = [
    { text: 'AI-Powered Analytics & Insights', icon: <AnalyticsIcon />, color: '#3b82f6' },
    { text: 'Enterprise-Grade Security', icon: <ShieldIcon />, color: '#10b981' },
    { text: 'Lightning-Fast Performance', icon: <RocketIcon />, color: '#f59e0b' },
    { text: 'Cloud-Native Architecture', icon: <CloudSyncIcon />, color: '#7c3aed' }
  ];

  const stats = [
    { number: '10K+', label: 'Products Managed', icon: <ProductIcon /> },
    { number: '50K+', label: 'Orders Processed', icon: <OrderIcon /> },
    { number: '99.9%', label: 'Uptime SLA', icon: <TrendingIcon /> },
    { number: '24/7', label: 'Support Available', icon: <SecurityIcon /> }
  ];

  return (
    <PageContainer>
      <Container maxWidth="lg">
        <HeroContainer>
          <Chip 
            icon={<AutoAwesomeIcon />}
            label="Next-Generation Order Management"
            variant="outlined"
            sx={{ 
              mb: 3, 
              borderColor: theme.palette.primary.main,
              color: theme.palette.primary.main,
              fontWeight: 600
            }}
          />
          
          <GradientTitle variant="h1" component="h1" gutterBottom>
            OrderFlow Pro
          </GradientTitle>
          
          <Typography variant="h5" color="text.secondary" sx={{ maxWidth: 700, mx: 'auto', mb: 4, lineHeight: 1.5 }}>
            {isAuthenticated 
              ? `Welcome back, ${user?.fullName}! Your business dashboard is ready with real-time insights and powerful tools.`
              : 'Transform your business operations with AI-powered order management, intelligent analytics, and seamless customer experiences.'
            }
          </Typography>

          {isAuthenticated && (
            <Grid container spacing={3} justifyContent="center" mb={4}>
              {stats.map((stat, index) => (
                <Grid item xs={6} sm={3} key={index}>
                  <StatsCard elevation={2}>
                    <Box sx={{ color: theme.palette.primary.main, mb: 1 }}>
                      {stat.icon}
                    </Box>
                    <Typography variant="h6" fontWeight="bold" color="primary">
                      {stat.number}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      {stat.label}
                    </Typography>
                  </StatsCard>
                </Grid>
              ))}
            </Grid>
          )}
          
          {!isAuthenticated && (
            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'center', flexWrap: 'wrap', mb: 6 }}>
              <Button 
                variant="contained" 
                size="large" 
                onClick={() => navigate('/login')}
                startIcon={<AccountIcon />}
                sx={{ px: 4, py: 1.5, fontSize: '1rem' }}
              >
                Sign In
              </Button>
              <Button 
                variant="outlined" 
                size="large" 
                onClick={() => navigate('/register')}
                sx={{ px: 4, py: 1.5, fontSize: '1rem' }}
              >
                Start Free Trial
              </Button>
            </Box>
          )}
        </HeroContainer>
      </Container>

      <Container maxWidth="lg" sx={{ py: 8 }}>
        <Box textAlign="center" mb={6}>
          <Typography variant="h3" component="h2" gutterBottom fontWeight="bold">
            Powerful Features for Modern Business
          </Typography>
          <Typography variant="h6" color="text.secondary" sx={{ maxWidth: 600, mx: 'auto' }}>
            Everything you need to manage orders, inventory, and customer relationships in one unified platform.
          </Typography>
        </Box>
        
        <Grid container spacing={4} justifyContent="center">
          {features.map((feature, index) => (
            <Grid item xs={12} sm={6} md={4} key={index} sx={{ display: 'flex', justifyContent: 'center' }}>
              <FeatureCard>
                <FeatureIconContainer sx={{ background: feature.gradient }}>
                  <FeatureIconCircle>
                    {feature.icon}
                  </FeatureIconCircle>
                </FeatureIconContainer>
                
                <CardContent sx={{ height: 180, textAlign: 'center', p: 3, display: 'flex', flexDirection: 'column' }}>
                  <Box sx={{ height: 60, display: 'flex', alignItems: 'center', justifyContent: 'center', mb: 2 }}>
                    <Typography variant="h6" component="h3" fontWeight="bold">
                      {feature.title}
                    </Typography>
                  </Box>
                  <Box sx={{ height: 100, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                    <Typography variant="body2" color="text.secondary" sx={{ lineHeight: 1.6 }}>
                      {feature.description}
                    </Typography>
                  </Box>
                </CardContent>
                
                <CardActions sx={{ height: 80, p: 3, pt: 0, display: 'flex', alignItems: 'center' }}>
                  <Button 
                    variant="contained" 
                    onClick={feature.action}
                    fullWidth
                    sx={{ 
                      height: 48,
                      background: feature.gradient,
                      color: 'white',
                      fontWeight: 600,
                      textTransform: 'none',
                      '&:hover': {
                        background: feature.gradient,
                        filter: 'brightness(1.1)'
                      }
                    }}
                  >
                    {isAuthenticated ? 'Open ' + feature.title.split(' ')[0] : 'Get Started'}
                  </Button>
                </CardActions>
              </FeatureCard>
            </Grid>
          ))}
        </Grid>
      </Container>

      <BenefitsSection>
        <Container maxWidth="lg">
          <Box textAlign="center" mb={6}>
            <Typography variant="h3" component="h2" gutterBottom fontWeight="bold">
              Why Industry Leaders Choose OrderFlow Pro
            </Typography>
            <Typography variant="h6" color="text.secondary" sx={{ maxWidth: 700, mx: 'auto', lineHeight: 1.6 }}>
              Built on cutting-edge technology with enterprise-grade features that scale with your business growth and evolving needs.
            </Typography>
          </Box>
          
          <Grid container spacing={6} alignItems="center" justifyContent="center">
            <Grid item xs={12} md={6}>
              <Box sx={{ textAlign: { xs: 'center', md: 'left' } }}>
                {benefits.map((benefit, index) => (
                  <Box key={index} sx={{ display: 'flex', alignItems: 'center', mb: 3, justifyContent: { xs: 'center', md: 'flex-start' } }}>
                    <BenefitIconBox color={benefit.color}>
                      {benefit.icon}
                    </BenefitIconBox>
                    <Typography variant="h6" fontWeight="600">
                      {benefit.text}
                    </Typography>
                  </Box>
                ))}
              </Box>
            </Grid>
            
            <Grid item xs={12} md={6} sx={{ display: 'flex', justifyContent: 'center' }}>
              <FeatureCard>
                <FeatureIconContainer sx={{ background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)' }}>
                  <FeatureIconCircle>
                    <RocketIcon sx={{ fontSize: 40, color: 'white' }} />
                  </FeatureIconCircle>
                </FeatureIconContainer>
                
                <CardContent sx={{ height: 180, textAlign: 'center', p: 3, display: 'flex', flexDirection: 'column', color: 'white', background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)' }}>
                  <Box sx={{ height: 60, display: 'flex', alignItems: 'center', justifyContent: 'center', mb: 2 }}>
                    <Typography variant="h6" component="h3" fontWeight="bold" color="white">
                      Ready to Launch?
                    </Typography>
                  </Box>
                  <Box sx={{ height: 100, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                    <Typography variant="body2" sx={{ lineHeight: 1.6, opacity: 0.9, color: 'white' }}>
                      Join 10,000+ businesses using OrderFlow Pro to streamline operations and boost productivity.
                    </Typography>
                  </Box>
                </CardContent>
                
                <CardActions sx={{ height: 80, p: 3, pt: 0, display: 'flex', alignItems: 'center', justifyContent: 'center', background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)' }}>
                  <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
                    {[1,2,3,4,5].map((star) => (
                      <StarIcon key={star} sx={{ color: '#fbbf24', fontSize: 16 }} />
                    ))}
                    <Typography variant="body2" sx={{ ml: 1, fontWeight: 600, fontSize: '0.75rem', color: 'white' }}>
                      4.9/5 (2,000+ reviews)
                    </Typography>
                  </Box>
                </CardActions>
              </FeatureCard>
            </Grid>
          </Grid>
        </Container>
      </BenefitsSection>

      {!isAuthenticated && (
        <CTAContainer>
          <Container maxWidth="md">
            <Box sx={{ display: 'flex', justifyContent: 'center' }}>
              <CTAPaper elevation={8}>
                <Typography variant="h3" gutterBottom fontWeight="bold">
                  Start Your Free Trial Today
                </Typography>
                <Typography variant="h6" sx={{ opacity: 0.95, mb: 4 }}>
                  No credit card required. Full access to all features for 30 days.
                </Typography>
                <Button 
                  variant="contained"
                  size="large"
                  onClick={() => navigate('/register')}
                  sx={{ 
                    px: 6, 
                    py: 2, 
                    fontSize: '1.1rem',
                    background: 'rgba(255, 255, 255, 0.2)',
                    backdropFilter: 'blur(10px)',
                    border: '1px solid rgba(255, 255, 255, 0.3)',
                    '&:hover': {
                      background: 'rgba(255, 255, 255, 0.3)',
                      transform: 'translateY(-2px)'
                    }
                  }}
                >
                  Get Started for Free
                </Button>
              </CTAPaper>
            </Box>
          </Container>
        </CTAContainer>
      )}
    </PageContainer>
  );
};

export default HomePage;