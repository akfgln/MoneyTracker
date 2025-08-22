import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';

// Angular Material
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatBadgeModule } from '@angular/material/badge';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';

// Feature Components
import { TransactionListComponent } from '../components/transaction-list/transaction-list.component';
import { TransactionFormComponent } from '../components/transaction-form/transaction-form.component';
import { PdfUploadComponent } from '../components/pdf-upload/pdf-upload.component';
import { BulkUpdateDialogComponent } from '../components/bulk-update-dialog/bulk-update-dialog.component';

// Services
import { TransactionService, Transaction, TransactionSummary } from '../services/transaction.service';
import { GermanFormatService } from '../services/german-format.service';
import { FileUploadService, FileUploadResult } from '../services/file-upload.service';

// RxJS
import { Subject, Observable, combineLatest } from 'rxjs';
import { takeUntil, map, startWith } from 'rxjs/operators';

@Component({
  selector: 'app-transaction-management',
  template: `
    <div class="transaction-management-container">
      <!-- Header Section -->
      <div class="page-header">
        <div class="header-content">
          <h1>Transaktionsverwaltung</h1>
          <p class="page-description">
            Verwalten Sie Ihre Transaktionen, laden Sie Belege hoch und überwachen Sie Ihre Ausgaben mit deutschen Standards.
          </p>
        </div>
        
        <div class="header-actions">
          <button 
            mat-raised-button 
            color="primary"
            (click)="openTransactionForm()"
          >
            <mat-icon>add</mat-icon>
            Neue Transaktion
          </button>
          
          <button 
            mat-button
            [matBadge]="pendingUploadsCount"
            matBadgeColor="accent"
            [matBadgeHidden]="pendingUploadsCount === 0"
            (click)="selectedTabIndex = 2"
          >
            <mat-icon>cloud_upload</mat-icon>
            Dokumente
          </button>
        </div>
      </div>
      
      <!-- Summary Cards -->
      <div class="summary-section" *ngIf="summary$ | async as summary">
        <mat-card class="summary-card">
          <mat-card-content>
            <div class="summary-item">
              <mat-icon color="primary">account_balance_wallet</mat-icon>
              <div class="summary-text">
                <span class="summary-value">{{ formatCurrency(summary.totalAmount) }}</span>
                <span class="summary-label">Gesamtbetrag</span>
              </div>
            </div>
          </mat-card-content>
        </mat-card>
        
        <mat-card class="summary-card">
          <mat-card-content>
            <div class="summary-item">
              <mat-icon color="accent">receipt</mat-icon>
              <div class="summary-text">
                <span class="summary-value">{{ summary.totalTransactions }}</span>
                <span class="summary-label">Transaktionen</span>
              </div>
            </div>
          </mat-card-content>
        </mat-card>
        
        <mat-card class="summary-card">
          <mat-card-content>
            <div class="summary-item">
              <mat-icon color="warn">calculate</mat-icon>
              <div class="summary-text">
                <span class="summary-value">{{ formatCurrency(summary.totalVAT) }}</span>
                <span class="summary-label">MwSt. gesamt</span>
              </div>
            </div>
          </mat-card-content>
        </mat-card>
        
        <mat-card class="summary-card">
          <mat-card-content>
            <div class="summary-item">
              <mat-icon>trending_up</mat-icon>
              <div class="summary-text">
                <span class="summary-value">{{ formatCurrency(summary.averageAmount) }}</span>
                <span class="summary-label">Ø Betrag</span>
              </div>
            </div>
          </mat-card-content>
        </mat-card>
      </div>
      
      <!-- Loading Bar -->
      <mat-progress-bar 
        mode="indeterminate" 
        *ngIf="isLoading$ | async"
        class="loading-bar"
      ></mat-progress-bar>
      
      <!-- Main Content Tabs -->
      <mat-card class="main-content">
        <mat-tab-group 
          [(selectedIndex)]="selectedTabIndex"
          animationDuration="300ms"
          class="transaction-tabs"
        >
          <!-- Transactions List Tab -->
          <mat-tab>
            <ng-template mat-tab-label>
              <mat-icon>list</mat-icon>
              <span>Transaktionen</span>
              <mat-chip 
                *ngIf="selectedTransactions.length > 0" 
                class="selection-badge"
              >
                {{ selectedTransactions.length }}
              </mat-chip>
            </ng-template>
            
            <div class="tab-content">
              <div class="tab-header">
                <h3>Transaktionsübersicht</h3>
                
                <div class="tab-actions" *ngIf="selectedTransactions.length > 0">
                  <button 
                    mat-button
                    color="accent"
                    (click)="openBulkUpdateDialog()"
                  >
                    <mat-icon>edit</mat-icon>
                    Bearbeiten ({{ selectedTransactions.length }})
                  </button>
                  
                  <button 
                    mat-button
                    color="warn"
                    (click)="confirmBulkDelete()"
                  >
                    <mat-icon>delete</mat-icon>
                    Löschen ({{ selectedTransactions.length }})
                  </button>
                  
                  <button 
                    mat-button
                    (click)="exportSelectedTransactions()"
                  >
                    <mat-icon>download</mat-icon>
                    Export CSV
                  </button>
                </div>
              </div>
              
              <app-transaction-list
                #transactionList
                [pageSize]="50"
                [allowBulkOperations]="true"
                (transactionSelected)="onTransactionSelected($event)"
                (bulkSelectionChanged)="onBulkSelectionChanged($event)"
                (transactionEdit)="editTransaction($event)"
                (transactionDelete)="confirmDeleteTransaction($event)"
              ></app-transaction-list>
            </div>
          </mat-tab>
          
          <!-- New Transaction Tab -->
          <mat-tab>
            <ng-template mat-tab-label>
              <mat-icon>add_circle</mat-icon>
              <span>Neue Transaktion</span>
            </ng-template>
            
            <div class="tab-content">
              <div class="tab-header">
                <h3>Neue Transaktion erstellen</h3>
                <p class="tab-description">
                  Fügen Sie eine neue Transaktion mit deutschen Formatierungen hinzu.
                </p>
              </div>
              
              <app-transaction-form
                [mode]="'create'"
                (transactionSaved)="onTransactionSaved($event)"
                (cancelled)="selectedTabIndex = 0"
              ></app-transaction-form>
            </div>
          </mat-tab>
          
          <!-- Document Upload Tab -->
          <mat-tab>
            <ng-template mat-tab-label>
              <mat-icon>cloud_upload</mat-icon>
              <span>Dokumente</span>
              <mat-chip 
                *ngIf="pendingUploadsCount > 0" 
                class="upload-badge"
              >
                {{ pendingUploadsCount }}
              </mat-chip>
            </ng-template>
            
            <div class="tab-content">
              <div class="tab-header">
                <h3>Belege und Kontoauszüge</h3>
                <p class="tab-description">
                  Laden Sie PDF-Dokumente hoch für automatische Datenextraktion.
                </p>
              </div>
              
              <div class="upload-section">
                <div class="upload-types">
                  <mat-card class="upload-type-card">
                    <mat-card-header>
                      <mat-card-title>Belege hochladen</mat-card-title>
                      <mat-card-subtitle>Rechnungen, Quittungen, Kassenbons</mat-card-subtitle>
                    </mat-card-header>
                    <mat-card-content>
                      <app-pdf-upload
                        [uploadType]="'receipt'"
                        [maxFileSize]="10485760"
                        [autoUpload]="false"
                        (fileUploaded)="onReceiptUploaded($event)"
                        (uploadError)="onUploadError($event)"
                      ></app-pdf-upload>
                    </mat-card-content>
                  </mat-card>
                  
                  <mat-card class="upload-type-card">
                    <mat-card-header>
                      <mat-card-title>Kontoauszüge hochladen</mat-card-title>
                      <mat-card-subtitle>Bankauszüge, Transaktionslisten</mat-card-subtitle>
                    </mat-card-header>
                    <mat-card-content>
                      <app-pdf-upload
                        [uploadType]="'bank-statement'"
                        [maxFileSize]="20971520"
                        [autoUpload]="false"
                        (fileUploaded)="onBankStatementUploaded($event)"
                        (uploadError)="onUploadError($event)"
                      ></app-pdf-upload>
                    </mat-card-content>
                  </mat-card>
                </div>
              </div>
            </div>
          </mat-tab>
          
          <!-- Analytics Tab -->
          <mat-tab>
            <ng-template mat-tab-label>
              <mat-icon>analytics</mat-icon>
              <span>Auswertung</span>
            </ng-template>
            
            <div class="tab-content">
              <div class="tab-header">
                <h3>Transaktionsauswertung</h3>
                <p class="tab-description">
                  Detaillierte Analyse Ihrer Transaktionen mit deutschen Standards.
                </p>
              </div>
              
              <div class="analytics-section" *ngIf="summary$ | async as summary">
                <!-- Category Breakdown -->
                <mat-card class="analytics-card">
                  <mat-card-header>
                    <mat-card-title>Ausgaben nach Kategorien</mat-card-title>
                  </mat-card-header>
                  <mat-card-content>
                    <div class="category-breakdown">
                      <div 
                        class="category-item"
                        *ngFor="let category of summary.categorySummary; trackBy: trackByCategoryId"
                      >
                        <div class="category-info">
                          <span class="category-name">{{ category.categoryName }}</span>
                          <span class="category-count">{{ category.transactionCount }} Transaktionen</span>
                        </div>
                        <div class="category-amount">
                          <span class="amount">{{ formatCurrency(category.amount) }}</span>
                          <span class="percentage">{{ formatPercentage(category.percentage) }}</span>
                        </div>
                      </div>
                    </div>
                  </mat-card-content>
                </mat-card>
                
                <!-- Monthly Trend -->
                <mat-card class="analytics-card">
                  <mat-card-header>
                    <mat-card-title>Monatlicher Verlauf</mat-card-title>
                  </mat-card-header>
                  <mat-card-content>
                    <div class="monthly-trend">
                      <div 
                        class="month-item"
                        *ngFor="let month of summary.monthlyTrend; trackBy: trackByMonth"
                      >
                        <div class="month-info">
                          <span class="month-name">{{ month.month }}</span>
                          <span class="month-count">{{ month.transactionCount }} Transaktionen</span>
                        </div>
                        <div class="month-amount">
                          <span class="amount">{{ formatCurrency(month.amount) }}</span>
                        </div>
                      </div>
                    </div>
                  </mat-card-content>
                </mat-card>
              </div>
            </div>
          </mat-tab>
        </mat-tab-group>
      </mat-card>
    </div>
  `,
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    MatTabsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatSnackBarModule,
    MatProgressBarModule,
    MatChipsModule,
    MatBadgeModule,
    MatTooltipModule,
    MatDividerModule,
    TransactionListComponent,
    TransactionFormComponent,
    PdfUploadComponent
  ],
  styleUrls: ['./transaction-management.component.scss']
})
export class TransactionManagementComponent implements OnInit, OnDestroy {
  @ViewChild('transactionList') transactionList!: TransactionListComponent;
  
  selectedTabIndex = 0;
  selectedTransactions: Transaction[] = [];
  pendingUploadsCount = 0;
  
  // Observables
  summary$: Observable<TransactionSummary>;
  isLoading$: Observable<boolean>;
  
  private destroy$ = new Subject<void>();
  
  constructor(
    private transactionService: TransactionService,
    private germanFormatService: GermanFormatService,
    private fileUploadService: FileUploadService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {
    // Initialize observables
    this.isLoading$ = this.transactionService.loading$;
    
    // Get current month summary by default
    const now = new Date();
    const startOfMonth = new Date(now.getFullYear(), now.getMonth(), 1);
    const endOfMonth = new Date(now.getFullYear(), now.getMonth() + 1, 0);
    
    this.summary$ = this.transactionService.getTransactionSummary(
      startOfMonth, 
      endOfMonth
    );
  }
  
  ngOnInit(): void {
    this.loadInitialData();
  }
  
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
  
  private loadInitialData(): void {
    // Load initial transaction data
    this.transactionService.getTransactions(1, 50)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          console.log(`Loaded ${result.transactions.length} transactions`);
        },
        error: (error) => {
          this.showErrorMessage('Fehler beim Laden der Transaktionen');
          console.error('Error loading transactions:', error);
        }
      });
  }
  
  // Transaction Management
  
  openTransactionForm(): void {
    this.selectedTabIndex = 1;
  }
  
  editTransaction(transaction: Transaction): void {
    // This would typically open the form in edit mode
    // Implementation would depend on how you want to handle routing vs. in-place editing
    this.selectedTabIndex = 1;
  }
  
  onTransactionSelected(transaction: Transaction): void {
    console.log('Transaction selected:', transaction);
    // Handle single transaction selection
  }
  
  onTransactionSaved(transaction: Transaction): void {
    this.showSuccessMessage('Transaktion erfolgreich gespeichert');
    this.selectedTabIndex = 0; // Switch back to list
    
    // Refresh the transaction list
    if (this.transactionList) {
      // Trigger refresh of transaction list
    }
  }
  
  confirmDeleteTransaction(transaction: Transaction): void {
    const message = `Möchten Sie die Transaktion "${transaction.description}" wirklich löschen?`;
    
    if (confirm(message)) {
      this.deleteTransaction(transaction.id);
    }
  }
  
  private deleteTransaction(transactionId: string): void {
    this.transactionService.deleteTransaction(transactionId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.showSuccessMessage('Transaktion wurde gelöscht');
          // Refresh list
        },
        error: (error) => {
          this.showErrorMessage('Fehler beim Löschen der Transaktion');
          console.error('Delete error:', error);
        }
      });
  }
  
  // Bulk Operations
  
  onBulkSelectionChanged(transactions: Transaction[]): void {
    this.selectedTransactions = transactions;
  }
  
  openBulkUpdateDialog(): void {
    if (this.selectedTransactions.length === 0) {
      this.showErrorMessage('Bitte wählen Sie Transaktionen aus');
      return;
    }
    
    const dialogRef = this.dialog.open(BulkUpdateDialogComponent, {
      width: '600px',
      data: { selectedTransactions: this.selectedTransactions }
    });
    
    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.handleBulkUpdate(result);
      }
    });
  }
  
  private handleBulkUpdate(bulkData: any): void {
    const transactionIds = this.selectedTransactions.map(t => t.id);
    
    this.transactionService.bulkUpdateTransactions({
      transactionIds,
      updates: bulkData
    })
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: (updatedTransactions) => {
        this.showSuccessMessage(`${updatedTransactions.length} Transaktionen wurden aktualisiert`);
        this.selectedTransactions = [];
        // Refresh list
      },
      error: (error) => {
        this.showErrorMessage('Fehler beim Aktualisieren der Transaktionen');
        console.error('Bulk update error:', error);
      }
    });
  }
  
  confirmBulkDelete(): void {
    if (this.selectedTransactions.length === 0) {
      this.showErrorMessage('Bitte wählen Sie Transaktionen aus');
      return;
    }
    
    const count = this.selectedTransactions.length;
    const message = `Möchten Sie wirklich ${count} Transaktion(en) löschen?`;
    
    if (confirm(message)) {
      this.bulkDeleteTransactions();
    }
  }
  
  private bulkDeleteTransactions(): void {
    const transactionIds = this.selectedTransactions.map(t => t.id);
    
    this.transactionService.bulkDeleteTransactions(transactionIds)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.showSuccessMessage(`${transactionIds.length} Transaktionen wurden gelöscht`);
          this.selectedTransactions = [];
          // Refresh list
        },
        error: (error) => {
          this.showErrorMessage('Fehler beim Löschen der Transaktionen');
          console.error('Bulk delete error:', error);
        }
      });
  }
  
  exportSelectedTransactions(): void {
    if (this.selectedTransactions.length === 0) {
      this.showErrorMessage('Bitte wählen Sie Transaktionen aus');
      return;
    }
    
    const filename = `transaktionen_${this.formatDate(new Date())}.csv`;
    this.transactionService.downloadCSV(this.selectedTransactions, filename);
    this.showSuccessMessage('CSV-Export wurde gestartet');
  }
  
  // File Upload Handlers
  
  onReceiptUploaded(uploadResult: any): void {
    this.showSuccessMessage('Beleg wurde erfolgreich hochgeladen');
    this.pendingUploadsCount = Math.max(0, this.pendingUploadsCount - 1);
    
    // Optionally process the uploaded receipt
    this.processUploadedDocument(uploadResult.id);
  }
  
  onBankStatementUploaded(uploadResult: any): void {
    this.showSuccessMessage('Kontoauszug wurde erfolgreich hochgeladen');
    this.pendingUploadsCount = Math.max(0, this.pendingUploadsCount - 1);
    
    // Optionally process the uploaded bank statement
    this.processUploadedDocument(uploadResult.id);
  }
  
  onUploadError(error: any): void {
    this.showErrorMessage(`Upload-Fehler: ${error.error}`);
    console.error('Upload error:', error);
  }
  
  private processUploadedDocument(fileId: string): void {
    this.fileUploadService.processDocument(fileId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (extractedData) => {
          this.showSuccessMessage('Dokumentdaten wurden extrahiert');
          console.log('Extracted data:', extractedData);
          // Handle extracted data (e.g., create transactions)
        },
        error: (error) => {
          console.warn('Document processing failed:', error);
          // Not showing error to user as upload was successful
        }
      });
  }
  
  // Formatting Helpers
  
  formatCurrency(amount: number): string {
    return this.germanFormatService.formatCurrency(amount);
  }
  
  formatPercentage(percentage: number): string {
    return this.germanFormatService.formatPercentage(percentage / 100);
  }
  
  formatDate(date: Date): string {
    return this.germanFormatService.formatDate(date, 'short');
  }
  
  // TrackBy Functions
  
  trackByCategoryId(index: number, item: any): string {
    return item.categoryId;
  }
  
  trackByMonth(index: number, item: any): string {
    return item.month;
  }
  
  // UI Helper Methods
  
  private showSuccessMessage(message: string): void {
    this.snackBar.open(message, 'Schließen', {
      duration: 3000,
      panelClass: 'success-snackbar'
    });
  }
  
  private showErrorMessage(message: string): void {
    this.snackBar.open(message, 'Schließen', {
      duration: 5000,
      panelClass: 'error-snackbar'
    });
  }
}