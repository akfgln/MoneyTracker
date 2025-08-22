import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, combineLatest } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import {
  DashboardSummary,
  MonthlyTrend,
  CategorySpending,
  BudgetOverview
} from '../models/dashboard.types';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private readonly apiUrl = `${environment.apiUrl}/api/dashboard`;
  
  // State management
  private summarySubject = new BehaviorSubject<DashboardSummary | null>(null);
  private trendsSubject = new BehaviorSubject<MonthlyTrend[]>([]);
  private categorySpendingSubject = new BehaviorSubject<CategorySpending[]>([]);
  private budgetsSubject = new BehaviorSubject<BudgetOverview[]>([]);
  private loadingSubject = new BehaviorSubject<boolean>(false);
  
  public summary$ = this.summarySubject.asObservable();
  public trends$ = this.trendsSubject.asObservable();
  public categorySpending$ = this.categorySpendingSubject.asObservable();
  public budgets$ = this.budgetsSubject.asObservable();
  public loading$ = this.loadingSubject.asObservable();

  constructor(private http: HttpClient) {}

  /**
   * Get dashboard summary for current month
   */
  getDashboardSummary(year?: number, month?: number): Observable<DashboardSummary> {
    this.loadingSubject.next(true);
    
    let params = new HttpParams();
    if (year) params = params.set('year', year.toString());
    if (month) params = params.set('month', month.toString());
    
    return this.http.get<DashboardSummary>(`${this.apiUrl}/summary`, { params })
      .pipe(
        map(summary => {
          this.summarySubject.next(summary);
          this.loadingSubject.next(false);
          return summary;
        })
      );
  }

  /**
   * Get monthly trends for specified year
   */
  getMonthlyTrends(year: number): Observable<MonthlyTrend[]> {
    const params = new HttpParams().set('year', year.toString());
    
    return this.http.get<MonthlyTrend[]>(`${this.apiUrl}/trends/monthly`, { params })
      .pipe(
        map(trends => {
          // Ensure we have all 12 months
          const completeTrends = this.fillMissingMonths(trends, year);
          this.trendsSubject.next(completeTrends);
          return completeTrends;
        })
      );
  }

  /**
   * Get category spending analysis
   */
  getCategorySpending(
    period: 'month' | 'quarter' | 'year' | 'custom' = 'month',
    startDate?: Date,
    endDate?: Date
  ): Observable<CategorySpending[]> {
    let params = new HttpParams().set('period', period);
    
    if (startDate) {
      params = params.set('startDate', startDate.toISOString());
    }
    if (endDate) {
      params = params.set('endDate', endDate.toISOString());
    }
    
    return this.http.get<CategorySpending[]>(`${this.apiUrl}/category-spending`, { params })
      .pipe(
        map(spending => {
          this.categorySpendingSubject.next(spending);
          return spending;
        })
      );
  }

  /**
   * Get budget overview
   */
  getBudgetOverview(month?: number, year?: number): Observable<BudgetOverview[]> {
    let params = new HttpParams();
    if (month) params = params.set('month', month.toString());
    if (year) params = params.set('year', year.toString());
    
    return this.http.get<BudgetOverview[]>(`${this.apiUrl}/budgets`, { params })
      .pipe(
        map(budgets => {
          // Calculate additional properties
          const enhancedBudgets = budgets.map(budget => ({
            ...budget,
            percentage: budget.limit > 0 ? (budget.spent / budget.limit) * 100 : 0,
            isOverBudget: budget.spent > budget.limit,
            remainingDays: this.calculateRemainingDays()
          }));
          
          this.budgetsSubject.next(enhancedBudgets);
          return enhancedBudgets;
        })
      );
  }

  /**
   * Get recent transactions for dashboard display
   */
  getRecentTransactions(limit: number = 5): Observable<any[]> {
    const params = new HttpParams().set('limit', limit.toString());
    
    return this.http.get<any[]>(`${this.apiUrl}/recent-transactions`, { params });
  }

  /**
   * Get year-over-year comparison
   */
  getYearOverYearComparison(currentYear: number): Observable<{
    currentYear: MonthlyTrend[];
    previousYear: MonthlyTrend[];
  }> {
    const params = new HttpParams()
      .set('currentYear', currentYear.toString())
      .set('previousYear', (currentYear - 1).toString());
    
    return this.http.get<{
      currentYear: MonthlyTrend[];
      previousYear: MonthlyTrend[];
    }>(`${this.apiUrl}/year-comparison`, { params });
  }

  /**
   * Get quarterly summary
   */
  getQuarterlySummary(year: number): Observable<{
    quarters: {
      quarter: number;
      income: number;
      expenses: number;
      netAmount: number;
    }[];
  }> {
    const params = new HttpParams().set('year', year.toString());
    
    return this.http.get<{
      quarters: {
        quarter: number;
        income: number;
        expenses: number;
        netAmount: number;
      }[];
    }>(`${this.apiUrl}/quarterly`, { params });
  }

  /**
   * Get cash flow projection
   */
  getCashFlowProjection(months: number = 6): Observable<{
    projections: {
      month: string;
      projectedIncome: number;
      projectedExpenses: number;
      projectedBalance: number;
      confidence: number;
    }[];
  }> {
    const params = new HttpParams().set('months', months.toString());
    
    return this.http.get<{
      projections: {
        month: string;
        projectedIncome: number;
        projectedExpenses: number;
        projectedBalance: number;
        confidence: number;
      }[];
    }>(`${this.apiUrl}/cash-flow-projection`, { params });
  }

  /**
   * Get account balances
   */
  getAccountBalances(): Observable<{
    accounts: {
      id: string;
      name: string;
      type: string;
      balance: number;
      currency: string;
      lastUpdated: Date;
    }[];
    totalBalance: number;
  }> {
    return this.http.get<{
      accounts: {
        id: string;
        name: string;
        type: string;
        balance: number;
        currency: string;
        lastUpdated: Date;
      }[];
      totalBalance: number;
    }>(`${this.apiUrl}/accounts`);
  }

  /**
   * Get VAT summary for dashboard
   */
  getVATSummary(startDate: Date, endDate: Date): Observable<{
    totalVat: number;
    vat19: number;
    vat7: number;
    vat0: number;
    deductibleVat: number;
    netVat: number;
  }> {
    const params = new HttpParams()
      .set('startDate', startDate.toISOString())
      .set('endDate', endDate.toISOString());
    
    return this.http.get<{
      totalVat: number;
      vat19: number;
      vat7: number;
      vat0: number;
      deductibleVat: number;
      netVat: number;
    }>(`${this.apiUrl}/vat-summary`, { params });
  }

  /**
   * Refresh all dashboard data
   */
  refreshDashboard(year?: number, month?: number): Observable<{
    summary: DashboardSummary;
    trends: MonthlyTrend[];
    categorySpending: CategorySpending[];
    budgets: BudgetOverview[];
  }> {
    this.loadingSubject.next(true);
    
    const currentYear = year || new Date().getFullYear();
    const currentMonth = month || new Date().getMonth() + 1;
    
    return combineLatest([
      this.getDashboardSummary(currentYear, currentMonth),
      this.getMonthlyTrends(currentYear),
      this.getCategorySpending('month'),
      this.getBudgetOverview(currentMonth, currentYear)
    ]).pipe(
      map(([summary, trends, categorySpending, budgets]) => {
        this.loadingSubject.next(false);
        return { summary, trends, categorySpending, budgets };
      })
    );
  }

  /**
   * Fill missing months in trends data
   */
  private fillMissingMonths(trends: MonthlyTrend[], year: number): MonthlyTrend[] {
    const completeTrends: MonthlyTrend[] = [];
    const GERMAN_MONTHS = [
      'Januar', 'Februar', 'März', 'April', 'Mai', 'Juni',
      'Juli', 'August', 'September', 'Oktober', 'November', 'Dezember'
    ];
    
    for (let month = 1; month <= 12; month++) {
      const existingTrend = trends.find(t => t.monthNumber === month && t.year === year);
      
      if (existingTrend) {
        completeTrends.push(existingTrend);
      } else {
        completeTrends.push({
          month: GERMAN_MONTHS[month - 1],
          monthNumber: month,
          year,
          income: 0,
          expenses: 0,
          netAmount: 0
        });
      }
    }
    
    return completeTrends;
  }

  /**
   * Calculate remaining days in current month
   */
  private calculateRemainingDays(): number {
    const now = new Date();
    const lastDayOfMonth = new Date(now.getFullYear(), now.getMonth() + 1, 0);
    return Math.max(0, lastDayOfMonth.getDate() - now.getDate());
  }

  /**
   * Format German currency
   */
  formatGermanCurrency(amount: number): string {
    return new Intl.NumberFormat('de-DE', {
      style: 'currency',
      currency: 'EUR'
    }).format(amount);
  }

  /**
   * Format German percentage
   */
  formatGermanPercentage(value: number): string {
    return new Intl.NumberFormat('de-DE', {
      style: 'percent',
      minimumFractionDigits: 1,
      maximumFractionDigits: 1
    }).format(value / 100);
  }

  /**
   * Get budget color based on usage percentage
   */
  getBudgetColor(percentage: number): 'primary' | 'accent' | 'warn' {
    if (percentage >= 100) return 'warn';
    if (percentage >= 80) return 'accent';
    return 'primary';
  }

  /**
   * Get trend indicator
   */
  getTrendIndicator(current: number, previous: number): {
    direction: 'up' | 'down' | 'stable';
    percentage: number;
    isPositive: boolean;
  } {
    if (previous === 0) {
      return {
        direction: current > 0 ? 'up' : current < 0 ? 'down' : 'stable',
        percentage: 0,
        isPositive: current >= 0
      };
    }
    
    const change = ((current - previous) / Math.abs(previous)) * 100;
    const direction = change > 5 ? 'up' : change < -5 ? 'down' : 'stable';
    
    return {
      direction,
      percentage: Math.abs(change),
      isPositive: change >= 0
    };
  }
}