import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { importProvidersFrom, LOCALE_ID } from '@angular/core';
import { registerLocaleData } from '@angular/common';
import localeDE from '@angular/common/locales/de';
import { MatNativeDateModule, DateAdapter, MAT_DATE_LOCALE, MAT_DATE_FORMATS } from '@angular/material/core';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { routes } from './app/app.routes';
import { provideCharts, withDefaultRegisterables } from 'ng2-charts';

// Register German locale
registerLocaleData(localeDE);

// German date formats for Material DatePicker
export const GERMAN_DATE_FORMATS = {
  parse: {
    dateInput: 'dd.MM.yyyy',
  },
  display: {
    dateInput: 'dd.MM.yyyy',
    monthYearLabel: 'MMM yyyy',
    dateA11yLabel: 'dd.MM.yyyy',
    monthYearA11yLabel: 'MMMM yyyy',
  },
};

bootstrapApplication(AppComponent, {
  providers: [
    // Router configuration
    provideRouter(routes),
    
    // HTTP client
    provideHttpClient(withInterceptorsFromDi()),
    
    // Animations
    provideAnimationsAsync(),
    
    // Charts with Chart.js registerables
    provideCharts(withDefaultRegisterables()),
    
    // German locale configuration
    { provide: LOCALE_ID, useValue: 'de-DE' },
    { provide: MAT_DATE_LOCALE, useValue: 'de-DE' },
    { provide: MAT_DATE_FORMATS, useValue: GERMAN_DATE_FORMATS },
    
    // Import necessary modules
    importProvidersFrom(
      MatNativeDateModule,
      TranslateModule.forRoot({
        defaultLanguage: 'de',
        useDefaultLang: true,
      })
    )
  ]
}).then(appRef => {
  // Configure German translations
  const translateService = appRef.injector.get(TranslateService);
  translateService.setDefaultLang('de');
  translateService.use('de');
  
  console.log('German Financial Dashboard successfully bootstrapped!');
  console.log('Locale set to: de-DE');
  console.log('Available routes:', routes.map(r => r.path));
}).catch(err => {
  console.error('Error starting application:', err);
});