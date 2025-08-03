import { styled } from '@mui/material/styles';
import { AppBar, Box, Typography, Button, Avatar } from '@mui/material';

export const StyledAppBar = styled(AppBar)(() => ({
  background: 'rgba(255, 255, 255, 0.95)',
  backdropFilter: 'blur(20px)',
  borderBottom: `1px solid rgba(37, 99, 235, 0.12)`,
  boxShadow: 'none'
}));

export const LogoContainer = styled(Box)({
  display: 'flex',
  alignItems: 'center',
  cursor: 'pointer',
  '&:hover': {
    opacity: 0.8
  }
});

export const LogoIcon = styled(Box)({
  width: 40,
  height: 40,
  borderRadius: 8,
  background: 'linear-gradient(135deg, #2563eb 0%, #7c3aed 100%)',
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  marginRight: '1rem'
});

export const LogoText = styled(Typography)({
  fontWeight: 700,
  background: 'linear-gradient(135deg, #2563eb 0%, #7c3aed 100%)',
  backgroundClip: 'text',
  WebkitBackgroundClip: 'text',
  WebkitTextFillColor: 'transparent'
});

export const NavButton = styled(Button)(() => ({
  marginLeft: '0.25rem',
  marginRight: '0.25rem',
  borderRadius: 8,
  textTransform: 'none',
  fontWeight: 600
}));

export const ProfileButton = styled(Button)(() => ({
  marginLeft: '1rem',
  borderRadius: 12,
  textTransform: 'none',
  minWidth: 'auto',
  paddingLeft: '0.5rem',
  paddingRight: '0.5rem',
  '&:hover': {
    background: `rgba(37, 99, 235, 0.08)`
  }
}));

export const ProfileAvatar = styled(Avatar)({
  width: 36,
  height: 36,
  background: 'linear-gradient(135deg, #2563eb 0%, #7c3aed 100%)',
  fontSize: '0.875rem',
  fontWeight: 600,
  marginRight: '0.5rem'
});

export const GetStartedButton = styled(Button)({
  marginLeft: '0.25rem',
  marginRight: '0.25rem',
  borderRadius: 8,
  textTransform: 'none',
  fontWeight: 600,
  background: 'linear-gradient(135deg, #2563eb 0%, #7c3aed 100%)',
  '&:hover': {
    background: 'linear-gradient(135deg, #1d4ed8 0%, #6d28d9 100%)'
  }
});