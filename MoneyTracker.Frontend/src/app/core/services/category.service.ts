import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { ApiService } from './api.service';
import {
  Category,
  CreateCategoryDto,
  UpdateCategoryDto,
  CategoryQueryParameters,
  CategorySuggestion,
  SuggestCategoryDto,
  CategoryHierarchy,
  CategoryUsageStats,
  BulkUpdateCategoriesDto,
  CategoryType
} from '../models/transaction.model';
import { PaginatedResponse, PagedResult } from '../models/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private readonly endpoint = 'api/categories';

  constructor(private apiService: ApiService) {}

  getCategories(params?: CategoryQueryParameters): Observable<PagedResult<Category>> {
    return this.apiService.getPaginated<Category>(this.endpoint, params)
      .pipe(
        map((response: PaginatedResponse<Category>) => {
          const transformedCategories = response.data?.map(category => ({
            ...category,
            createdAt: new Date(category.createdAt),
            updatedAt: new Date(category.updatedAt)
          })) || [];
          
          // Convert PaginatedResponse to PagedResult format
          return {
            items: transformedCategories,
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

  getCategory(id: string): Observable<Category> {
    return this.apiService.get<Category>(`${this.endpoint}/${id}`)
      .pipe(
        map(response => ({
          ...response.data,
          id: response.data?.id ?? '',
          userId: response.data?.userId ?? '',
          name: response.data?.name ?? '',
          categoryType: response.data?.categoryType ?? CategoryType.Expense,
          defaultVatRate: response.data?.defaultVatRate ?? 0,
          isSystemCategory: response.data?.isSystemCategory ?? false,
          isActive: response.data?.isActive ?? true,
          sortOrder: response.data?.sortOrder ?? 0,
          createdAt: new Date(response.data?.createdAt ?? new Date()),
          updatedAt: new Date(response.data?.updatedAt ?? new Date())
        }) as Category)
      );
  }

  getCategoryHierarchy(categoryType?: string): Observable<CategoryHierarchy[]> {
    const params = categoryType ? { categoryType } : {};
    return this.apiService.get<CategoryHierarchy[]>(`${this.endpoint}/hierarchy`, params)
      .pipe(map(response => response.data || []));
  }

  createCategory(category: CreateCategoryDto): Observable<Category> {
    return this.apiService.post<Category>(this.endpoint, category)
      .pipe(
        map(response => ({
          ...response.data,
          id: response.data?.id ?? '',
          userId: response.data?.userId ?? '',
          name: response.data?.name ?? '',
          categoryType: response.data?.categoryType ?? CategoryType.Expense,
          defaultVatRate: response.data?.defaultVatRate ?? 0,
          isSystemCategory: response.data?.isSystemCategory ?? false,
          isActive: response.data?.isActive ?? true,
          sortOrder: response.data?.sortOrder ?? 0,
          createdAt: new Date(response.data?.createdAt || new Date()),
          updatedAt: new Date(response.data?.updatedAt || new Date())
        }) as Category)
      );
  }

  updateCategory(id: string, category: UpdateCategoryDto): Observable<Category> {
    return this.apiService.put<Category>(`${this.endpoint}/${id}`, category)
      .pipe(
        map(response => ({
          ...response.data,
          id: response.data?.id ?? '',
          userId: response.data?.userId ?? '',
          name: response.data?.name ?? '',
          categoryType: response.data?.categoryType ?? CategoryType.Expense,
          defaultVatRate: response.data?.defaultVatRate ?? 0,
          isSystemCategory: response.data?.isSystemCategory ?? false,
          isActive: response.data?.isActive ?? true,
          sortOrder: response.data?.sortOrder ?? 0,
          createdAt: new Date(response.data?.createdAt || new Date()),
          updatedAt: new Date(response.data?.updatedAt || new Date())
        }) as Category)
      );
  }

  deleteCategory(id: string): Observable<boolean> {
    return this.apiService.delete<boolean>(`${this.endpoint}/${id}`)
      .pipe(map(response => response.data ?? false));
  }

  suggestCategory(request: SuggestCategoryDto): Observable<CategorySuggestion[]> {
    return this.apiService.post<CategorySuggestion[]>(`${this.endpoint}/suggest`, request)
      .pipe(map(response => response.data || []));
  }

  getCategoryUsageStats(
    id: string, 
    startDate?: Date, 
    endDate?: Date
  ): Observable<CategoryUsageStats> {
    const params: any = {};
    if (startDate) params.startDate = startDate.toISOString();
    if (endDate) params.endDate = endDate.toISOString();
    
    return this.apiService.get<CategoryUsageStats>(`${this.endpoint}/${id}/usage-stats`, params)
      .pipe(map(response => response.data as CategoryUsageStats));
  }

  bulkUpdateCategories(request: BulkUpdateCategoriesDto): Observable<number> {
    return this.apiService.post<number>(`${this.endpoint}/bulk-update`, request)
      .pipe(map(response => response.data ?? 0));
  }

  importCategories(file: File): Observable<Category[]> {
    return this.apiService.upload<Category[]>(`${this.endpoint}/import`, file)
      .pipe(map(response => response.data || []));
  }

  exportCategories(categoryType?: string): Observable<Blob> {
    const params = categoryType ? { categoryType } : {};
    return this.apiService.download(`${this.endpoint}/export`, params);
  }

  mergeCategories(sourceCategoryId: string, targetCategoryId: string): Observable<boolean> {
    return this.apiService.post<boolean>(
      `${this.endpoint}/${sourceCategoryId}/merge`, 
      { targetCategoryId }
    ).pipe(map(response => response.data ?? false));
  }
}
