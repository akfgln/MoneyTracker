export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName?: string;
  displayName?: string;
  preferredLanguage?: string;
  preferredCurrency?: string;
  profileImagePath?: string;
  phoneNumber?: string;
  country?: string;
  isEmailConfirmed: boolean;
  isActive?: boolean;
  roles: string[];
  dateOfBirth?: Date;
  createdAt: Date;
  lastLoginAt?: Date;  // Changed from lastLoginDate to match frontend naming
}

export interface AuthResponse {
  user: User;
  accessToken: string;
  refreshToken: string;
  expiresAt: Date;
  tokenType?: string;  // Backend provides this as "Bearer"
}

export interface LoginRequest {
  email: string;
  password: string;
  rememberMe: boolean;
}

export interface RegisterRequest {
  email: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  dateOfBirth?: Date;
  country?: string;
  preferredLanguage?: string;
  acceptTermsOfService: boolean;  // Changed from acceptedTerms
  acceptPrivacyPolicy: boolean;   // Changed from acceptedPrivacy
  marketingEmailsConsent?: boolean;
}