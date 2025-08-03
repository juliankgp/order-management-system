import { styled } from '@mui/material/styles';
import { Box, IconButton, Link } from '@mui/material';

export const FooterContainer = styled(Box)(({ theme }) => ({
  background: `linear-gradient(135deg, ${theme.palette.grey[900]} 0%, ${theme.palette.grey[800]} 100%)`,
  color: 'white',
  marginTop: 'auto'
}));

export const LogoBox = styled(Box)({
  width: 48,
  height: 48,
  borderRadius: 12,
  background: 'linear-gradient(135deg, #2563eb 0%, #7c3aed 100%)',
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  marginRight: '1.5rem'
});

export const ContactBox = styled(Box)({
  marginBottom: '1.5rem',
  textAlign: 'inherit'
});

export const SocialIconButton = styled(IconButton)({
  color: 'white',
  backgroundColor: 'rgba(255, 255, 255, 0.1)',
  '&:hover': {
    backgroundColor: 'rgba(255, 255, 255, 0.2)',
    transform: 'translateY(-2px)'
  },
  transition: 'all 0.2s ease'
});

export const FooterLink = styled(Link)({
  display: 'block',
  marginBottom: '0.5rem',
  opacity: 0.8,
  fontSize: '0.875rem',
  color: 'inherit',
  textDecoration: 'none',
  '&:hover': {
    opacity: 1,
    transform: 'translateX(4px)'
  },
  transition: 'all 0.2s ease'
});

export const LegalLink = styled(Link)({
  opacity: 0.7,
  fontSize: '0.75rem',
  color: 'inherit',
  textDecoration: 'none',
  '&:hover': {
    opacity: 1
  }
});