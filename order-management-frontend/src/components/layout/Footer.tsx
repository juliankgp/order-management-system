import React from 'react';
import { 
  Typography, 
  Container, 
  Grid, 
  Box,
  Divider,
  useTheme,
  alpha
} from '@mui/material';
import {
  GitHub as GitHubIcon,
  LinkedIn as LinkedInIcon,
  Twitter as TwitterIcon,
  AutoAwesome as AutoAwesomeIcon,
  Email as EmailIcon,
  Phone as PhoneIcon,
  LocationOn as LocationIcon
} from '@mui/icons-material';
import {
  FooterContainer,
  LogoBox,
  ContactBox,
  SocialIconButton,
  FooterLink,
  LegalLink
} from './Footer.styles';

const Footer: React.FC = () => {
  const theme = useTheme();

  const footerLinks = {
    product: [
      { name: 'Features', href: '#' },
      { name: 'Pricing', href: '#' },
      { name: 'API Docs', href: '#' },
      { name: 'Integrations', href: '#' }
    ],
    company: [
      { name: 'About Us', href: '#' },
      { name: 'Careers', href: '#' },
      { name: 'Blog', href: '#' },
      { name: 'Press Kit', href: '#' }
    ],
    support: [
      { name: 'Help Center', href: '#' },
      { name: 'Contact Sales', href: '#' },
      { name: 'Status Page', href: '#' },
      { name: 'Community', href: '#' }
    ],
    resources: [
      { name: 'Documentation', href: '#' },
      { name: 'Tutorials', href: '#' },
      { name: 'Webinars', href: '#' }
    ],
    legal: [
      { name: 'Privacy Policy', href: '#' },
      { name: 'Terms of Service', href: '#' },
      { name: 'Cookie Policy', href: '#' },
      { name: 'GDPR', href: '#' }
    ]
  };

  const contactInfo = [
    { icon: <EmailIcon sx={{ mr: 1.5, fontSize: 18, opacity: 0.8 }} />, text: 'support@orderflowpro.com' },
    { icon: <PhoneIcon sx={{ mr: 1.5, fontSize: 18, opacity: 0.8 }} />, text: '+1 (555) 123-4567' },
    { icon: <LocationIcon sx={{ mr: 1.5, fontSize: 18, opacity: 0.8 }} />, text: 'San Francisco, CA' }
  ];

  const socialLinks = [
    { icon: <GitHubIcon />, href: '#' },
    { icon: <LinkedInIcon />, href: '#' },
    { icon: <TwitterIcon />, href: '#' }
  ];

  return (
    <FooterContainer>
      <Container maxWidth="lg" sx={{ py: 6 }}>
        <Grid container spacing={6} justifyContent="center">
          <Grid size={{ xs: 12, md: 6 }}>
            <Box sx={{ 
              display: 'flex', 
              alignItems: 'center', 
              mb: 3, 
              justifyContent: { xs: 'center', md: 'flex-start' } 
            }}>
              <LogoBox>
                <AutoAwesomeIcon sx={{ color: 'white', fontSize: 28 }} />
              </LogoBox>
              <Typography variant="h5" fontWeight="bold">
                OrderFlow Pro
              </Typography>
            </Box>
            
            <Typography 
              variant="body1" 
              sx={{ 
                opacity: 0.9, 
                mb: 3, 
                lineHeight: 1.6, 
                maxWidth: 380, 
                textAlign: { xs: 'center', md: 'left' } 
              }}
            >
              The next-generation order management platform that empowers businesses 
              to scale efficiently with AI-powered automation.
            </Typography>
            
            <ContactBox sx={{ textAlign: { xs: 'center', md: 'left' } }}>
              {contactInfo.map((contact, index) => (
                <Box 
                  key={index}
                  sx={{ 
                    display: 'flex', 
                    alignItems: 'center', 
                    mb: 1.5, 
                    justifyContent: { xs: 'center', md: 'flex-start' } 
                  }}
                >
                  {contact.icon}
                  <Typography variant="body2" sx={{ opacity: 0.9, fontSize: '0.875rem' }}>
                    {contact.text}
                  </Typography>
                </Box>
              ))}
            </ContactBox>

            <Box sx={{ 
              display: 'flex', 
              gap: 2, 
              justifyContent: { xs: 'center', md: 'flex-start' } 
            }}>
              {socialLinks.map((social, index) => (
                <SocialIconButton key={index} size="medium" onClick={() => window.open(social.href, '_blank')}>
                  {social.icon}
                </SocialIconButton>
              ))}
            </Box>
          </Grid>

          <Grid size={{ xs: 12, md: 6 }}>
            <Grid container spacing={4} justifyContent="center">
              <Grid size={6}>
                <Box sx={{ mb: 4, textAlign: { xs: 'center', md: 'left' } }}>
                  <Typography variant="h6" gutterBottom fontWeight="600" sx={{ mb: 2 }}>
                    Product
                  </Typography>
                  <Box>
                    {footerLinks.product.map((link, index) => (
                      <FooterLink key={index} href={link.href}>
                        {link.name}
                      </FooterLink>
                    ))}
                  </Box>
                </Box>
                
                <Box sx={{ textAlign: { xs: 'center', md: 'left' } }}>
                  <Typography variant="h6" gutterBottom fontWeight="600" sx={{ mb: 2 }}>
                    Company
                  </Typography>
                  <Box>
                    {footerLinks.company.map((link, index) => (
                      <FooterLink key={index} href={link.href}>
                        {link.name}
                      </FooterLink>
                    ))}
                  </Box>
                </Box>
              </Grid>

              <Grid size={6}>
                <Box sx={{ mb: 4, textAlign: { xs: 'center', md: 'left' } }}>
                  <Typography variant="h6" gutterBottom fontWeight="600" sx={{ mb: 2 }}>
                    Support
                  </Typography>
                  <Box>
                    {footerLinks.support.map((link, index) => (
                      <FooterLink key={index} href={link.href}>
                        {link.name}
                      </FooterLink>
                    ))}
                  </Box>
                </Box>
                
                <Box sx={{ textAlign: { xs: 'center', md: 'left' } }}>
                  <Typography variant="h6" gutterBottom fontWeight="600" sx={{ mb: 2 }}>
                    Resources
                  </Typography>
                  <Box>
                    {footerLinks.resources.map((link, index) => (
                      <FooterLink key={index} href={link.href}>
                        {link.name}
                      </FooterLink>
                    ))}
                  </Box>
                </Box>
              </Grid>
            </Grid>
          </Grid>
        </Grid>

        <Divider sx={{ my: 4, borderColor: alpha(theme.palette.common.white, 0.2) }} />

        <Box sx={{ 
          display: 'flex', 
          justifyContent: 'space-between', 
          alignItems: 'center', 
          flexWrap: 'wrap', 
          gap: 2 
        }}>
          <Typography variant="body2" sx={{ opacity: 0.7 }}>
            © 2025 OrderFlow Pro. All rights reserved.
          </Typography>
          <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap' }}>
            {footerLinks.legal.map((link, index) => (
              <LegalLink key={index} href={link.href}>
                {link.name}
              </LegalLink>
            ))}
          </Box>
        </Box>
      </Container>
    </FooterContainer>
  );
};

export default Footer;