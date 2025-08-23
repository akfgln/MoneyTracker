import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { TranslateModule } from '@ngx-translate/core';
import { Observable } from 'rxjs';

import { AuthService } from '../../core/services/auth.service';
import { User } from '../../core/models/user.model';
import { GermanCurrencyPipe } from '../../shared/pipes/german-currency.pipe';
import { GermanDatePipe } from '../../shared/pipes/german-date.pipe';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    TranslateModule,
    GermanCurrencyPipe,
    GermanDatePipe
  ],
  template: `
    <div class="dashboard-container">
      <div class="dashboard-header">
        <h1>{{ 'DASHBOARD.TITLE' | translate }}</h1>
        <p class="welcome-message" *ngIf="currentUser$ | async as user">
          {{ 'DASHBOARD.WELCOME_MESSAGE' | translate: {name: user.firstName} }}
        </p>
      </div>
      
      <div class="dashboard-grid responsive-grid three-columns">
        <!-- Balance Card -->
        <mat-card class="stat-card balance-card">
          <mat-card-header>
            <div mat-card-avatar class="card-icon balance-icon">
              <mat-icon>account_balance_wallet</mat-icon>
            </div>
            <mat-card-title>{{ 'DASHBOARD.TOTAL_BALANCE' | translate }}</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class="stat-value balance-value">
              {{ totalBalance | germanCurrency }}
            </div>
          </mat-card-content>
        </mat-card>
        
        <!-- Monthly Income Card -->
        <mat-card class="stat-card income-card">
          <mat-card-header>
            <div mat-card-avatar class="card-icon income-icon">
              <mat-icon>trending_up</mat-icon>
            </div>
            <mat-card-title>{{ 'DASHBOARD.MONTHLY_INCOME' | translate }}</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class="stat-value income-value">
              {{ monthlyIncome | germanCurrency }}
            </div>
          </mat-card-content>
        </mat-card>
        
        <!-- Monthly Expenses Card -->
        <mat-card class="stat-card expense-card">
          <mat-card-header>
            <div mat-card-avatar class="card-icon expense-icon">
              <mat-icon>trending_down</mat-icon>
            </div>
            <mat-card-title>{{ 'DASHBOARD.MONTHLY_EXPENSES' | translate }}</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class="stat-value expense-value">
              {{ monthlyExpenses | germanCurrency }}
            </div>
          </mat-card-content>
        </mat-card>
      </div>
      
      <!-- Quick Actions -->
      <mat-card class="quick-actions-card">
        <mat-card-header>
          <mat-card-title>{{ 'DASHBOARD.QUICK_ACTIONS' | translate }}</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <div class="action-buttons">
            <button mat-raised-button color="primary" class="action-button">
              <mat-icon>add</mat-icon>
              {{ 'TRANSACTIONS.ADD_NEW' | translate }}
            </button>
            <button mat-raised-button color="accent" class="action-button">
              <mat-icon>category</mat-icon>
              {{ 'CATEGORIES.ADD_NEW' | translate }}
            </button>
            <button mat-stroked-button class="action-button">
              <mat-icon>assessment</mat-icon>
              {{ 'REPORTS.GENERATE' | translate }}
            </button>
          </div>
        </mat-card-content>
      </mat-card>
      
      <!-- Recent Transactions -->
      <mat-card class="recent-transactions-card">
        <mat-card-header>
          <mat-card-title>{{ 'DASHBOARD.RECENT_TRANSACTIONS' | translate }}</mat-card-title>
          <button mat-button color="primary">{{ 'DASHBOARD.VIEW_ALL' | translate }}</button>
        </mat-card-header>
        <mat-card-content>
          <div class="no-transactions" *ngIf="recentTransactions.length === 0">
            <mat-icon>account_balance_wallet</mat-icon>
            <p>{{ 'DASHBOARD.NO_TRANSACTIONS' | translate }}</p>
          </div>
          
          <div class="transaction-list" *ngIf="recentTransactions.length > 0">
            <div class="transaction-item" *ngFor="let transaction of recentTransactions">
              <div class="transaction-icon">
                <mat-icon [ngClass]="transaction.type.toLowerCase()">{{ getTransactionIcon(transaction.type) }}</mat-icon>
              </div>
              <div class="transaction-details">
                <div class="transaction-description">{{ transaction.description }}</div>
                <div class="transaction-date">{{ transaction.date | germanDate }}</div>
              </div>
              <div class="transaction-amount" [ngClass]="transaction.type.toLowerCase()">
                {{ transaction.amount | germanCurrency }}
              </div>
            </div>
          </div>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .dashboard-container {
      padding: 24px;
      max-width: 1200px;
      margin: 0 auto;
    }
    
    .dashboard-header {
      margin-bottom: 32px;
      
      h1 {
        margin: 0 0 8px 0;
        color: #333;
        font-weight: 400;
      }
      
      .welcome-message {
        margin: 0;
        color: #666;
        font-size: 16px;
      }
    }
    
    .dashboard-grid {
      margin-bottom: 24px;
    }
    
    .stat-card {
      .card-icon {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 48px;
        height: 48px;
        border-radius: 50%;
        
        mat-icon {
          color: white;
          font-size: 24px;
        }
        
        &.balance-icon {
          background-color: #2196f3;
        }
        
        &.income-icon {
          background-color: #4caf50;
        }
        
        &.expense-icon {
          background-color: #f44336;
        }
      }
      
      .stat-value {
        font-size: 32px;
        font-weight: 600;
        margin-top: 16px;
        
        &.balance-value {
          color: #2196f3;
        }
        
        &.income-value {
          color: #4caf50;
        }
        
        &.expense-value {
          color: #f44336;
        }
      }
    }
    
    .quick-actions-card {
      margin-bottom: 24px;
      
      .action-buttons {
        display: flex;
        gap: 16px;
        flex-wrap: wrap;
        
        .action-button {
          min-width: 160px;
          height: 48px;
          
          mat-icon {
            margin-right: 8px;
          }
        }
      }
    }
    
    .recent-transactions-card {
      mat-card-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        
        mat-card-title {
          flex: 1;
        }
      }
      
      .no-transactions {
        text-align: center;
        padding: 40px 20px;
        color: #666;
        
        mat-icon {
          font-size: 48px;
          width: 48px;
          height: 48px;
          margin-bottom: 16px;
          opacity: 0.5;
        }
      }
      
      .transaction-list {
        .transaction-item {
          display: flex;
          align-items: center;
          padding: 12px 0;
          border-bottom: 1px solid #f0f0f0;
          
          &:last-child {
            border-bottom: none;
          }
          
          .transaction-icon {
            width: 40px;
            height: 40px;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            margin-right: 16px;
            
            mat-icon {
              color: white;
              font-size: 20px;
              
              &.income {
                background-color: #4caf50;
                border-radius: 50%;
                padding: 10px;
              }
              
              &.expense {
                background-color: #f44336;
                border-radius: 50%;
                padding: 10px;
              }
            }
          }
          
          .transaction-details {
            flex: 1;
            
            .transaction-description {
              font-weight: 500;
              margin-bottom: 4px;
            }
            
            .transaction-date {
              font-size: 12px;
              color: #666;
            }
          }
          
          .transaction-amount {
            font-weight: 600;
            font-size: 16px;
            
            &.income {
              color: #4caf50;
            }
            
            &.expense {
              color: #f44336;
            }
          }
        }
      }
    }
    
    @media (max-width: 768px) {
      .dashboard-container {
        padding: 16px;
      }
      
      .dashboard-grid {
        grid-template-columns: 1fr;
      }
      
      .action-buttons {
        flex-direction: column;
        
        .action-button {
          width: 100%;
        }
      }
    }
  `]
})
export class DashboardComponent implements OnInit {
  currentUser$: Observable<User | null>;
  
  // Mock data - replace with real data from services
  totalBalance = 5250.75;
  monthlyIncome = 3500.00;
  monthlyExpenses = 2250.30;
  
  recentTransactions = [
    {
      id: '1',
      description: 'Lebensmitteleinkäufe',
      amount: -85.45,
      date: new Date('2025-08-22'),
      type: 'Expense'
    },
    {
      id: '2',
      description: 'Gehalt',
      amount: 3500.00,
      date: new Date('2025-08-21'),
      type: 'Income'
    },
    {
      id: '3',
      description: 'Tankstelle',
      amount: -65.00,
      date: new Date('2025-08-20'),
      type: 'Expense'
    }
  ];

  constructor(private authService: AuthService) {
    this.currentUser$ = this.authService.currentUser$;
  }

  ngOnInit(): void {
    // Load dashboard data
    this.loadDashboardData();
  }

  private loadDashboardData(): void {
    // TODO: Implement actual data loading from services
    console.log('Loading dashboard data...');
  }

  getTransactionIcon(type: string): string {
    switch (type.toLowerCase()) {
      case 'income':
        return 'arrow_upward';
      case 'expense':
        return 'arrow_downward';
      case 'transfer':
        return 'swap_horiz';
      default:
        return 'account_balance_wallet';
    }
  }
}