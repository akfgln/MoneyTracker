# German Financial Dashboard and Reporting System

Ein umfassendes, deutschsprachiges Finanz-Dashboard zur Verwaltung und Berichterstattung von Unternehmensfinanzen.

## Überblick

Dieses System bietet eine vollständige Lösung für die Finanzverwaltung mit Fokus auf deutsche Geschäftspraktiken, Steuervorschriften und Lokalisierung. Es enthält ein interaktives Dashboard, umfassende Berichtserstellung und Export-Funktionen.

## Hauptfunktionen

### 📊 Dashboard
- **Finanzübersicht**: Zusammenfassende Karten für Einnahmen, Ausgaben, Gewinn und geschätzte Steuern
- **Interaktive Diagramme**: Linien- und Doughnut-Charts mit deutscher Lokalisierung
- **Budgetüberwachung**: Visueller Fortschritt mit Kategorien und Prozentsatzen
- **Schnellaktionen**: Direkte Bericht-Generierung und Datenexport

### 📈 Berichtssystem
- **Monatsberichte**: Detaillierte monatliche Finanzübersichten
- **Jahresberichte**: Umfassende jährliche Zusammenfassungen
- **UStVA (Umsatzsteuervoranmeldung)**: Quartalsmeldungen für deutsche Steuerbehörden
- **Kategorieberichte**: Flexible Zeitraum- und Kategorieauswahl
- **Multiple Exportformate**: PDF, CSV und Excel mit deutscher Formatierung

### 💰 Deutsche Lokalisierung
- **Währung**: Euro-Formatierung mit Komma als Dezimaltrennzeichen
- **Datumsformat**: dd.MM.yyyy (deutsches Format)
- **Steuersätze**: 19% und 7% MwSt. entsprechend deutschen Vorschriften
- **Geschäftskategorien**: Deutsche Ausgabenkategorien
- **UI-Sprache**: Vollständig deutsche Benutzeroberfläche

## Technische Architektur

### Frontend-Stack
- **Angular 17**: Moderne TypeScript-basierte Frontend-Entwicklung
- **Angular Material**: Professional UI-Komponenten
- **ng2-charts (Chart.js)**: Interaktive Datenvisualisierung
- **ngx-translate**: Internationalisierung und Lokalisierung
- **TailwindCSS**: Utility-first CSS-Framework

### Komponenten-Architektur

#### Core Components
- `DashboardComponent`: Hauptdashboard mit Finanzübersicht
- `ReportsComponent`: Umfassende Berichtserstellung

#### Core Services
- `ChartConfigService`: Deutsche Chart-Konfigurationen
- `ReportService`: API-Kommunikation für Berichte
- `ExportService`: Client-seitige Dateigenerierung

## Installation und Setup

### Voraussetzungen
- Node.js ≥ 18.0.0
- npm ≥ 9.0.0
- Angular CLI ≥ 17.0.0

### Installation

```bash
# Repository klonen
git clone https://github.com/your-username/german-financial-dashboard.git
cd german-financial-dashboard

# Abhängigkeiten installieren
npm install

# Entwicklungsserver starten
npm start
```

Die Anwendung ist verfügbar unter `http://localhost:4200`

### Produktionsbuild

```bash
# Produktionsbuild erstellen
npm run build:prod

# Bundle-Analyse (optional)
npm run analyze
```

## Projekt-Struktur

```
src/
├── app/
│   ├── features/
│   │   ├── dashboard/
│   │   │   └── dashboard.component.ts
│   │   └── reports/
│   │       └── reports.component.ts
│   ├── core/
│   │   └── services/
│   │       ├── chart-config.service.ts
│   │       ├── report.service.ts
│   │       └── export.service.ts
│   ├── app.component.ts
│   ├── app.routes.ts
│   └── main.ts
└── assets/
    └── i18n/
        └── de.json
```

## Verwendung

### Dashboard Navigation

1. **Dashboard**: Hauptübersicht mit Zusammenfassungskarten und Charts
2. **Berichte**: Generierung verschiedener Finanzberichte
3. **Transaktionen**: Verwaltung von Ein- und Ausgaben (geplant)
4. **Kategorien**: Verwaltung von Ausgabenkategorien (geplant)

### Berichtsgenerierung

#### Monatsbericht
1. Wählen Sie Jahr und Monat
2. Wählen Sie das Exportformat (PDF/CSV/Excel)
3. Klicken Sie auf "Bericht generieren" oder "Vorschau"

#### UStVA (Umsatzsteuervoranmeldung)
1. Wählen Sie Jahr und Quartal
2. Wählen Sie das Format
3. Generieren Sie die UStVA für die Steuerbehörden

#### Kategoriebericht
1. Definieren Sie den Datumsbereich
2. Wählen Sie spezifische Kategorien (optional)
3. Exportieren Sie in gewünschtem Format

### Datenexport

Alle Berichte unterstützen:
- **PDF**: Formatierte Berichte für Präsentationen
- **CSV**: Strukturierte Daten mit deutscher Formatierung
- **Excel**: Tabellenkalkulation mit deutschen Zahlenformaten

## Deutsche Lokalisierungs-Features

### Währungsformatierung
```typescript
// Beispiel: 1234.56 wird zu "1.234,56 €"
new Intl.NumberFormat('de-DE', {
  style: 'currency',
  currency: 'EUR'
}).format(1234.56)
```

### Datumsformatierung
```typescript
// Beispiel: 2024-12-25 wird zu "25.12.2024"
new Date().toLocaleDateString('de-DE')
```

### Chart-Tooltips
```typescript
// Deutsche Währungsformatierung in Chart-Tooltips
tooltip: {
  callbacks: {
    label: function(context) {
      return `${context.dataset.label}: ${formatGermanCurrency(context.parsed.y)}`;
    }
  }
}
```

## API-Integration

### Backend-Endpunkte (Beispiel)

```typescript
// Beispiel API-Struktur
const API_ENDPOINTS = {
  monthlyReport: '/api/reports/monthly',
  yearlyReport: '/api/reports/yearly',
  vatReport: '/api/reports/vat',
  categoryReport: '/api/reports/category',
  transactions: '/api/reports/transactions',
  summary: '/api/reports/summary'
};
```

### Datenmodelle

```typescript
interface MonthlyReportRequest {
  year: number;
  month: number;
  format: 'PDF' | 'CSV' | 'EXCEL';
  includeDetails?: boolean;
  includeTaxInfo?: boolean;
}

interface VATSummary {
  sales19: number;      // Umsätze 19%
  vat19: number;        // USt 19%
  sales7: number;       // Umsätze 7%
  vat7: number;         // USt 7%
  inputVat: number;     // Vorsteuer
  total: number;        // Zahllast/Erstattung
}
```

## Entwicklung und Erweiterung

### Neue Berichte hinzufügen

1. Erweitern Sie den `ReportService` um neue Endpunkte
2. Fügen Sie entsprechende TypeScript-Interfaces hinzu
3. Erweitern Sie die `ReportsComponent` UI
4. Aktualisieren Sie den `ExportService` für neue Formate

### Deutsche Lokalisierung erweitern

1. Aktualisieren Sie `src/assets/i18n/de.json`
2. Nutzen Sie den `ChartConfigService` für neue Chart-Typen
3. Verwenden Sie `Intl.NumberFormat` und `Intl.DateTimeFormat`

### Custom Charts hinzufügen

```typescript
// Beispiel: Neues Balkendiagramm
getGermanBarChartOptions(): ChartOptions {
  return {
    // Deutsche Konfiguration
    scales: {
      y: {
        ticks: {
          callback: (value) => this.formatGermanCurrency(value)
        }
      }
    }
  };
}
```

## Testing

```bash
# Unit Tests ausführen
npm test

# End-to-End Tests
npm run e2e

# Test Coverage
npm run test:coverage
```

## Deployment

### Production Build

```bash
npm run build:prod
```

### Docker Deployment (Optional)

```dockerfile
FROM node:18-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci --only=production
COPY . .
RUN npm run build:prod

FROM nginx:alpine
COPY --from=build /app/dist/german-financial-dashboard /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

## Beitrag zum Projekt

1. Fork das Repository
2. Erstellen Sie einen Feature-Branch (`git checkout -b feature/amazing-feature`)
3. Committen Sie Ihre Änderungen (`git commit -m 'Add amazing feature'`)
4. Pushen Sie den Branch (`git push origin feature/amazing-feature`)
5. Öffnen Sie einen Pull Request

## Lizenz

MIT License - siehe [LICENSE](LICENSE) Datei für Details.

## Support und Dokumentation

Für weitere Unterstützung:
- 📚 [Wiki](https://github.com/your-username/german-financial-dashboard/wiki)
- 🐛 [Issues](https://github.com/your-username/german-financial-dashboard/issues)
- 💬 [Discussions](https://github.com/your-username/german-financial-dashboard/discussions)

## Changelog

### Version 1.0.0
- ✅ Umfassendes Dashboard mit deutschen Finanzübersichten
- ✅ Vollständig lokalisierte Berichtserstellung
- ✅ UStVA (Umsatzsteuervoranmeldung) Unterstützung
- ✅ Multi-Format Export (PDF, CSV, Excel)
- ✅ Responsive Design für Mobile und Desktop
- ✅ Deutsche Lokalisierung (de-DE)
- ✅ Angular Material Design System
- ✅ Chart.js Integration mit deutscher Formatierung

---

**Entwickelt mit ❤️ für deutsche Unternehmen und Steuerberater**