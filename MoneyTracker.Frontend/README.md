# Money Tracker - Angular Frontend Documentation

## Overview

This is a comprehensive Angular 17+ frontend application for the German Money Tracker system, built with German localization (de-DE), Angular Material design system, and modern standalone components architecture.

## ✅ Completed Features

### 🚀 Core Architecture
- **Angular 17+** with standalone components architecture
- **TypeScript** for type safety and modern development
- **SCSS** for advanced styling capabilities
- **Responsive design** with Angular Material components
- **German localization (de-DE)** with proper formatting
- **Clean architecture** with feature-based organization

### 🌐 German Localization & Formatting
- **Complete German translations** for all UI elements
- **German date formatting** (DD.MM.YYYY)
- **German number formatting** (comma as decimal separator)
- **German currency formatting** (EUR with proper locale)
- **Custom pipes** for German-specific formatting:
  - `GermanCurrencyPipe` - Formats amounts in EUR with German locale
  - `GermanDatePipe` - Multiple German date formats
  - `GermanNumberPipe` - German number formatting

### 🔐 Authentication System
- **JWT-based authentication** with automatic token refresh
- **Login component** with German validation messages
- **Secure token management** with localStorage
- **Authentication guards** for protected routes
- **Role-based access control** guards
- **Automatic logout** on token expiration
- **Remember me functionality**

### 🛠️ HTTP Infrastructure
- **HTTP Interceptors** for:
  - Authentication token injection
  - Global error handling with German messages
  - Loading state management
- **Centralized API service** with typed responses
- **Error handling** with user-friendly German messages
- **Loading indicators** during API calls

### 🎨 UI/UX Components
- **Material Design** theme with German-inspired colors
- **Responsive navigation** with sidebar and top bar
- **German form validation** with custom validators:
  - German IBAN validation
  - German phone number validation
  - Strong password requirements (12+ chars)
  - German postal code validation
  - German tax ID validation
  - Currency amount validation
- **Shared components**:
  - Loading spinner with customizable options
  - Error message component with different severity levels
  - Confirmation dialog with German text

### 📊 Dashboard
- **Welcome message** with personalized German greeting
- **Financial summary cards** showing:
  - Total balance in EUR
  - Monthly income
  - Monthly expenses
- **Quick action buttons** for common operations
- **Recent transactions list** (placeholder data)
- **German formatting** for all numerical displays

### 🏗️ Project Structure
```
src/app/
├── core/                    # Singleton services, guards, interceptors
│   ├── services/           # Auth, API, Loading, Storage services
│   ├── interceptors/       # HTTP interceptors
│   ├── guards/            # Route guards (auth, role-based)
│   └── models/            # TypeScript interfaces and models
├── shared/                 # Reusable components, pipes, validators
│   ├── components/        # Loading spinner, error messages, dialogs
│   ├── pipes/             # German formatting pipes
│   └── validators/        # German-specific form validators
├── features/              # Feature modules
│   ├── auth/              # Authentication components
│   ├── dashboard/         # Main dashboard
│   ├── transactions/      # Transaction management (placeholder)
│   ├── categories/        # Category management (placeholder)
│   ├── reports/           # Reports and analytics (placeholder)
│   └── settings/          # User settings (placeholder)
└── layout/                # Layout components (integrated in app.component)
```

### 🔧 Technical Implementation
- **Reactive forms** with comprehensive validation
- **RxJS** for reactive programming and state management
- **BehaviorSubject** for user authentication state
- **Route guards** preventing unauthorized access
- **Lazy loading** for feature modules
- **Environment configuration** for dev/prod settings
- **TypeScript interfaces** for type safety
- **SCSS mixins** and variables for consistent styling

## 🌍 German Localization Features

### Language Support
- **Primary language**: German (de-DE)
- **Fallback support**: English (en) structure ready
- **Currency**: EUR with German formatting
- **Date format**: DD.MM.YYYY
- **Number format**: 1.234,56 (German standard)

### Custom German Validators
- **IBAN Validator**: Validates German IBAN format (DE + 20 digits)
- **Phone Validator**: German phone number patterns (+49, 0049, local)
- **Password Validator**: German requirements with umlauts support
- **Postal Code**: 5-digit German postal codes
- **Tax ID**: 11-digit German tax identification number
- **Currency Amount**: German decimal separator validation

### German UI Text
All user interface elements are translated including:
- Navigation labels
- Form labels and placeholders
- Validation error messages
- Success and information messages
- Button text and actions
- Dashboard statistics labels

## 🚦 Getting Started

### Prerequisites
- Node.js 18+
- npm or yarn
- Angular CLI 17+

### Installation
```bash
cd money-tracker-frontend
npm install
```

### Development Server
```bash
npm start
# or
ng serve
```
Navigate to `http://localhost:4200/`

### Build for Production
```bash
npm run build
# or
ng build
```

### Testing
```bash
npm test
# or
ng test
```

## 🔧 Configuration

### Environment Variables
**Development** (`src/environments/environment.ts`):
```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7001/api',
  defaultLanguage: 'de',
  currency: 'EUR',
  dateFormat: 'dd.MM.yyyy'
};
```

**Production** (`src/environments/environment.prod.ts`):
```typescript
export const environment = {
  production: true,
  apiUrl: 'https://api.moneytracker.de/api',
  // ... other settings
};
```

### API Integration
The application is configured to work with a .NET backend API:
- Authentication endpoints: `/auth/login`, `/auth/register`
- Protected endpoints require JWT Bearer token
- German error messages for API errors
- Automatic token refresh functionality

## 📱 Responsive Design

### Desktop (1024px+)
- Full sidebar navigation
- Three-column dashboard layout
- Expanded form layouts

### Tablet (768px - 1023px)
- Collapsible sidebar
- Two-column dashboard layout
- Optimized touch targets

### Mobile (< 768px)
- Overlay sidebar navigation
- Single-column layout
- Mobile-optimized forms
- Touch-friendly interfaces

## 🎨 Design System

### Color Palette
- **Primary**: Blue Grey (#607D8B) - Professional, trustworthy
- **Accent**: Amber (#FFC107) - Attention, highlights
- **Warn**: Red (#F44336) - Errors, warnings
- **Success**: Green (#4CAF50) - Positive actions

### Typography
- **Font**: Roboto (Google Fonts)
- **Headings**: 400-500 font weight
- **Body**: 400 font weight
- **Numbers**: Tabular figures for alignment

### Components
- **Cards**: Elevated design with shadows
- **Forms**: Outlined Material Design fields
- **Buttons**: Raised and flat variants
- **Navigation**: Material Design patterns

## 🔒 Security Features

### Authentication
- JWT token-based authentication
- Automatic token refresh before expiration
- Secure token storage in localStorage
- Logout on token expiration or manual action

### Route Protection
- Authentication guards on protected routes
- Role-based access control guards
- Redirect to login for unauthorized access
- Return URL handling after login

### Form Security
- Client-side validation with German messages
- XSS protection through Angular's built-in sanitization
- CSRF protection ready (requires backend implementation)
- Input sanitization and validation

## 📊 Performance Optimizations

### Bundle Optimization
- Lazy loading for feature modules
- Tree shaking for unused code elimination
- Angular's built-in optimizations
- Standalone components for smaller bundles

### Runtime Performance
- OnPush change detection where applicable
- Reactive programming patterns
- Efficient DOM updates
- Optimized image loading (ready for implementation)

## 🧪 Testing Strategy

### Unit Testing
- Jasmine and Karma configuration
- Component testing templates
- Service testing patterns
- Pipe testing utilities

### E2E Testing
- Ready for Cypress or Protractor integration
- Page object model patterns
- German UI text testing

## 🚀 Deployment

### Build Process
1. Environment-specific configuration
2. Angular CLI build with optimizations
3. Static file generation
4. German locale bundling

### Hosting Requirements
- Static file hosting (nginx, Apache, CDN)
- HTTPS required for production
- Proper routing configuration for SPA
- Gzip compression recommended

## 🔄 Future Enhancements

### Planned Features
1. **Complete Transaction Management**
   - Add, edit, delete transactions
   - Transaction categories
   - Bulk operations
   - Import/export functionality

2. **Advanced Reporting**
   - Interactive charts and graphs
   - Custom date ranges
   - PDF/Excel export
   - Spending analysis

3. **Enhanced User Management**
   - Profile management
   - Password change
   - Two-factor authentication
   - Account preferences

4. **Additional Features**
   - Dark theme support
   - Mobile app (PWA)
   - Offline functionality
   - Multi-language support expansion

## 🐛 Known Issues & Limitations

1. **Bundle Size**: Current bundle is ~715KB, optimization needed for production
2. **Mock Data**: Dashboard uses placeholder data, needs backend integration
3. **Feature Completeness**: Some features are placeholder components
4. **Testing**: Unit tests need implementation
5. **Accessibility**: ARIA labels and keyboard navigation need enhancement

## 📄 License

This project is part of the Money Tracker application suite and follows the same licensing terms.

## 👥 Support

For technical support or questions about the German localization and Angular implementation, please refer to the project documentation or contact the development team.

---

**Built with ❤️ using Angular 17, TypeScript, and German engineering principles**