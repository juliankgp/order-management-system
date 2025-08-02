import * as Yup from 'yup';

/**
 * Validation schemas that match exactly with backend validations
 * Based on CustomerService specification
 */

// Email validation
const emailValidation = Yup.string()
  .email('Invalid email format')
  .max(255, 'Email cannot be more than 255 characters')
  .required('Email is required');

// Password validation (must match backend)
const passwordValidation = Yup.string()
  .min(8, 'Password must be at least 8 characters')
  .max(100, 'Password cannot be more than 100 characters')
  .matches(
    /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_])/,
    'Password must include: lowercase, uppercase, number and special character'
  )
  .required('Password is required');

// Name validation (letters and spaces only)
const nameValidation = Yup.string()
  .matches(/^[a-zA-Z\s]+$/, 'Only letters and spaces are allowed')
  .max(100, 'Cannot be more than 100 characters')
  .required('This field is required');

// Phone validation (optional)
const phoneValidation = Yup.string()
  .nullable()
  .max(20, 'Phone number cannot be more than 20 characters')
  .matches(
    /^[+]?[0-9\-()\\s]*$/,
    'Invalid phone format'
  );

// Date of birth validation
const dateOfBirthValidation = Yup.date()
  .nullable()
  .max(new Date(), 'Date of birth must be in the past')
  .min(new Date('1900-01-01'), 'Invalid date of birth');

// Gender validation
const genderValidation = Yup.number()
  .nullable()
  .oneOf([1, 2, 3, 4], 'Invalid gender selection');

/**
 * Validation schema for user registration
 * Matches exactly with CustomerService validations
 */
export const registerValidationSchema = Yup.object({
  email: emailValidation,
  password: passwordValidation,
  confirmPassword: Yup.string()
    .oneOf([Yup.ref('password')], 'Passwords must match')
    .required('Please confirm your password'),
  firstName: nameValidation,
  lastName: nameValidation,
  phoneNumber: phoneValidation,
  dateOfBirth: dateOfBirthValidation,
  gender: genderValidation,
});

/**
 * Validation schema for login
 */
export const loginValidationSchema = Yup.object({
  email: emailValidation,
  password: Yup.string().required('Password is required'),
});

/**
 * Validation schema for profile update
 * Similar to registration but without required password
 */
export const updateProfileValidationSchema = Yup.object({
  firstName: nameValidation,
  lastName: nameValidation,
  phoneNumber: phoneValidation,
  dateOfBirth: dateOfBirthValidation,
  gender: genderValidation,
});

/**
 * Validation schema for password change
 */
export const changePasswordValidationSchema = Yup.object({
  currentPassword: Yup.string().required('Current password is required'),
  newPassword: passwordValidation,
  confirmNewPassword: Yup.string()
    .oneOf([Yup.ref('newPassword')], 'Passwords must match')
    .required('Please confirm your new password'),
});

/**
 * Individual validation functions for specific use
 */
export const validateEmail = (email: string): boolean => {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email) && email.length <= 255;
};

export const validatePassword = (password: string): boolean => {
  const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,100}$/;
  return passwordRegex.test(password);
};

export const validateName = (name: string): boolean => {
  const nameRegex = /^[a-zA-Z\s]+$/;
  return nameRegex.test(name) && name.length <= 100 && name.trim().length > 0;
};

export const validatePhone = (phone: string): boolean => {
  if (!phone || phone.trim() === '') return true; // Optional
  const phoneRegex = /^[+]?[0-9\-()\\s]*$/;
  return phoneRegex.test(phone) && phone.length <= 20;
};

/**
 * Function to get custom error messages
 */
export const getValidationErrorMessage = (field: string, error: string): string => {
  const errorMessages: Record<string, Record<string, string>> = {
    email: {
      'email': 'Enter a valid email (example: user@domain.com)',
      'required': 'Email is required',
      'max': 'Email is too long (maximum 255 characters)',
    },
    password: {
      'min': 'Password must be at least 8 characters',
      'max': 'Password is too long (maximum 100 characters)',
      'matches': 'Password must include: uppercase, lowercase, number and symbol',
      'required': 'Password is required',
    },
    confirmPassword: {
      'oneOf': 'Passwords do not match',
      'required': 'You must confirm your password',
    },
    firstName: {
      'matches': 'First name can only contain letters and spaces',
      'max': 'First name is too long (maximum 100 characters)',
      'required': 'First name is required',
    },
    lastName: {
      'matches': 'Last name can only contain letters and spaces',
      'max': 'Last name is too long (maximum 100 characters)',
      'required': 'Last name is required',
    },
    phoneNumber: {
      'matches': 'Invalid phone format (eg: +1-555-123-4567)',
      'max': 'Phone number is too long (maximum 20 characters)',
    },
    dateOfBirth: {
      'max': 'Date of birth must be in the past',
      'min': 'Invalid date of birth',
    },
    gender: {
      'oneOf': 'Please select a valid option',
    },
  };

  return errorMessages[field]?.[error] || error;
};

/**
 * Options for gender field
 */
export const genderOptions = [
  { value: 1, label: 'Male' },
  { value: 2, label: 'Female' },
  { value: 3, label: 'Other' },
  { value: 4, label: 'Prefer not to say' },
];

/**
 * Helper function to format Yup errors for React Hook Form
 */
export const formatYupErrors = (errors: Yup.ValidationError) => {
  const formattedErrors: Record<string, string> = {};
  
  errors.inner.forEach((error) => {
    if (error.path) {
      formattedErrors[error.path] = error.message;
    }
  });
  
  return formattedErrors;
};

/**
 * Function to validate complete form and return errors
 */
export const validateForm = async (
  schema: Yup.Schema<Record<string, unknown>>,
  data: Record<string, unknown>
): Promise<{ isValid: boolean; errors: Record<string, string> }> => {
  try {
    await schema.validate(data, { abortEarly: false });
    return { isValid: true, errors: {} };
  } catch (error) {
    if (error instanceof Yup.ValidationError) {
      return {
        isValid: false,
        errors: formatYupErrors(error)
      };
    }
    return {
      isValid: false,
      errors: { general: 'Unknown validation error' }
    };
  }
};