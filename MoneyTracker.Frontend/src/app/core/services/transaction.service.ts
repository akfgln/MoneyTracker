import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';

import { ApiService } from './api.service';
import {
  Transaction,
  CreateTransactionDto,
  UpdateTransactionDto,
  TransactionQueryParameters,
  BulkUpdateTransactionsDto,
  VATCalculation,
  TransactionType
} from '../models/transaction.model';
import { PagedResult, PaginatedResponse } from '../models/api-response.model';
import { GermanFormatService } from '../../shared/services/german-format.service';

@Injectable({
  providedIn: 'root'
})
export class TransactionService {
  private readonly endpoint = 'api/transactions';

  constructor(
    private apiService: ApiService,
    private germanFormat: GermanFormatService
  ) {}

  getTransactions(params?: TransactionQueryParameters): Observable<PagedResult<Transaction>> {
    return this.apiService.getPaginated<Transaction>(this.endpoint, params)
      .pipe(
        map((response: PaginatedResponse<Transaction>) => {
          // Convert PaginatedResponse to PagedResult and transform dates
          const transformedTransactions = (response.data || []).map(transaction => ({
            ...transaction,
            transactionDate: new Date(transaction.transactionDate),
            createdAt: new Date(transaction.createdAt),
            updatedAt: new Date(transaction.updatedAt)
          }));
          
          return {
            items: transformedTransactions,
            page: response.pagination.currentPage,
            pageSize: response.pagination.pageSize,
            totalCount: response.pagination.totalCount,
            totalPages: response.pagination.totalPages,
            hasNextPage: response.pagination.hasNext,
            hasPreviousPage: response.pagination.hasPrevious
          };
        })
      );
  }

  getTransaction(id: string): Observable<Transaction> {
    return this.apiService.get<Transaction>(`${this.endpoint}/${id}`)
      .pipe(
        map(response => ({
          ...response.data,
          id: response.data?.id ?? '',
          userId: response.data?.userId ?? '',
          accountId: response.data?.accountId ?? '',
          categoryId: response.data?.categoryId ?? '',
          amount: response.data?.amount ?? 0,
          netAmount: response.data?.netAmount ?? 0,
          vatAmount: response.data?.vatAmount ?? 0,
          vatRate: response.data?.vatRate ?? 0,
          description: response.data?.description ?? '',
          transactionType: response.data?.transactionType ?? TransactionType.Expense,
          date: new Date(response.data?.transactionDate || new Date()),
          isRecurring: response.data?.isRecurring ?? false,
          isReconciled: response.data?.isReconciled ?? false,
          transactionDate: new Date(response.data?.transactionDate || new Date()),
          createdAt: new Date(response.data?.createdAt || new Date()),
          updatedAt: new Date(response.data?.updatedAt || new Date())
        }) as Transaction)
      );
  }

  createTransaction(transaction: CreateTransactionDto): Observable<Transaction> {
    // Format German inputs before sending to API
    const formattedTransaction = this.germanFormat.formatTransactionForApi(transaction);
    
    return this.apiService.post<Transaction>(this.endpoint, formattedTransaction)
      .pipe(
        map(response => ({
          ...response.data,
          id: response.data?.id ?? '',
          userId: response.data?.userId ?? '',
          accountId: response.data?.accountId ?? '',
          categoryId: response.data?.categoryId ?? '',
          amount: response.data?.amount ?? 0,
          netAmount: response.data?.netAmount ?? 0,
          vatAmount: response.data?.vatAmount ?? 0,
          vatRate: response.data?.vatRate ?? 0,
          description: response.data?.description ?? '',
          transactionType: response.data?.transactionType ?? TransactionType.Expense,
          date: new Date(response.data?.transactionDate || new Date()),
          isRecurring: response.data?.isRecurring ?? false,
          isReconciled: response.data?.isReconciled ?? false,
          transactionDate: new Date(response.data?.transactionDate || new Date()),
          createdAt: new Date(response.data?.createdAt || new Date()),
          updatedAt: new Date(response.data?.updatedAt || new Date())
        }) as Transaction)
      );
  }

  updateTransaction(id: string, transaction: UpdateTransactionDto): Observable<Transaction> {
    // Format German inputs before sending to API
    const formattedTransaction = this.germanFormat.formatTransactionForApi(transaction);
    
    return this.apiService.put<Transaction>(`${this.endpoint}/${id}`, formattedTransaction)
      .pipe(
        map(response => ({
          ...response.data,
          id: response.data?.id ?? '',
          userId: response.data?.userId ?? '',
          accountId: response.data?.accountId ?? '',
          categoryId: response.data?.categoryId ?? '',
          amount: response.data?.amount ?? 0,
          netAmount: response.data?.netAmount ?? 0,
          vatAmount: response.data?.vatAmount ?? 0,
          vatRate: response.data?.vatRate ?? 0,
          description: response.data?.description ?? '',
          transactionType: response.data?.transactionType ?? TransactionType.Expense,
          date: new Date(response.data?.transactionDate || new Date()),
          isRecurring: response.data?.isRecurring ?? false,
          isReconciled: response.data?.isReconciled ?? false,
          transactionDate: new Date(response.data?.transactionDate || new Date()),
          createdAt: new Date(response.data?.createdAt || new Date()),
          updatedAt: new Date(response.data?.updatedAt || new Date())
        }) as Transaction)
      );
  }

  deleteTransaction(id: string): Observable<boolean> {
    return this.apiService.delete<boolean>(`${this.endpoint}/${id}`)
      .pipe(map(response => response.data ?? false));
  }

  bulkUpdateTransactions(request: BulkUpdateTransactionsDto): Observable<number> {
    const formattedRequest = {
      ...request,
      updates: this.germanFormat.formatTransactionForApi(request.updates)
    };
    
    return this.apiService.post<number>(`${this.endpoint}/bulk-update`, formattedRequest)
      .pipe(map(response => response.data ?? 0));
  }

  bulkDeleteTransactions(transactionIds: string[]): Observable<number> {
    return this.apiService.post<number>(`${this.endpoint}/bulk-delete`, { transactionIds })
      .pipe(map(response => response.data ?? 0));
  }

  duplicateTransaction(id: string): Observable<Transaction> {
    return this.apiService.post<Transaction>(`${this.endpoint}/${id}/duplicate`, {})
      .pipe(
        map(response => ({
          ...response.data,
          id: response.data?.id ?? '',
          userId: response.data?.userId ?? '',
          accountId: response.data?.accountId ?? '',
          categoryId: response.data?.categoryId ?? '',
          amount: response.data?.amount ?? 0,
          netAmount: response.data?.netAmount ?? 0,
          vatAmount: response.data?.vatAmount ?? 0,
          vatRate: response.data?.vatRate ?? 0,
          description: response.data?.description ?? '',
          transactionType: response.data?.transactionType ?? TransactionType.Expense,
          date: new Date(response.data?.transactionDate || new Date()),
          isRecurring: response.data?.isRecurring ?? false,
          isReconciled: response.data?.isReconciled ?? false,
          transactionDate: new Date(response.data?.transactionDate || new Date()),
          createdAt: new Date(response.data?.createdAt || new Date()),
          updatedAt: new Date(response.data?.updatedAt || new Date())
        }) as Transaction)
      );
  }

  calculateVAT(amount: number, vatRate: number): VATCalculation {
    const grossAmount = amount;
    const netAmount = amount / (1 + vatRate);
    const vatAmount = grossAmount - netAmount;
    
    return {
      grossAmount: Math.round(grossAmount * 100) / 100,
      netAmount: Math.round(netAmount * 100) / 100,
      vatAmount: Math.round(vatAmount * 100) / 100,
      vatRate
    };
  }

  calculateNetFromGross(grossAmount: number, vatRate: number): VATCalculation {
    return this.calculateVAT(grossAmount, vatRate);
  }

  calculateGrossFromNet(netAmount: number, vatRate: number): VATCalculation {
    const grossAmount = netAmount * (1 + vatRate);
    const vatAmount = grossAmount - netAmount;
    
    return {
      grossAmount: Math.round(grossAmount * 100) / 100,
      netAmount: Math.round(netAmount * 100) / 100,
      vatAmount: Math.round(vatAmount * 100) / 100,
      vatRate
    };
  }

  reconcileTransactions(transactionIds: string[]): Observable<number> {
    return this.apiService.post<number>(`${this.endpoint}/reconcile`, { transactionIds })
      .pipe(map(response => response.data ?? 0));
  }

  exportTransactions(params?: TransactionQueryParameters): Observable<Blob> {
    return this.apiService.download(`${this.endpoint}/export`, params);
  }

  getTransactionSummary(params?: TransactionQueryParameters): Observable<any> {
    return this.apiService.get<any>(`${this.endpoint}/summary`, params)
      .pipe(map(response => response.data ?? {}));
  }
}
