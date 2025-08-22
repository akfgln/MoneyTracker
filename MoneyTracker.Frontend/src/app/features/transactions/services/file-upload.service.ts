import { Injectable } from '@angular/core';
import { HttpClient, HttpEventType, HttpRequest, HttpResponse } from '@angular/common/http';
import { Observable, Subject } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface FileUploadResult {
  id: string;
  filename: string;
  originalName: string;
  mimeType: string;
  size: number;
  downloadUrl: string;
  uploadedAt: Date;
  documentType: 'receipt' | 'bank-statement';
}

export interface UploadProgress {
  loaded: number;
  total: number;
  percentage: number;
}

@Injectable({
  providedIn: 'root'
})
export class FileUploadService {
  private readonly apiUrl = `${environment.apiUrl}/api/files`;
  private uploadSubject = new Subject<{ fileId: string; progress: number }>();
  
  public uploadProgress$ = this.uploadSubject.asObservable();
  
  constructor(private http: HttpClient) {}
  
  /**
   * Upload a single file to the server
   * @param file The file to upload
   * @param documentType Type of document (receipt or bank-statement)
   * @param onProgress Optional progress callback
   * @returns Promise with upload result
   */
  async uploadFile(
    file: File, 
    documentType: 'receipt' | 'bank-statement',
    onProgress?: (progress: number) => void
  ): Promise<FileUploadResult> {
    return new Promise((resolve, reject) => {
      const formData = new FormData();
      formData.append('file', file, file.name);
      formData.append('documentType', documentType);
      
      const request = new HttpRequest('POST', `${this.apiUrl}/upload`, formData, {
        reportProgress: true,
        responseType: 'json'
      });
      
      this.http.request<FileUploadResult>(request).subscribe({
        next: (event) => {
          if (event.type === HttpEventType.UploadProgress && event.total) {
            const progress = Math.round((event.loaded / event.total) * 100);
            
            if (onProgress) {
              onProgress(progress);
            }
            
            this.uploadSubject.next({
              fileId: `${file.name}-${file.size}`,
              progress
            });
          } else if (event instanceof HttpResponse && event.body) {
            resolve(event.body);
          }
        },
        error: (error) => {
          console.error('File upload error:', error);
          reject(new Error(
            error.error?.message || 
            error.message || 
            'Fehler beim Hochladen der Datei'
          ));
        }
      });
    });
  }
  
  /**
   * Upload multiple files
   * @param files Array of files to upload
   * @param documentType Type of document
   * @param onProgress Optional progress callback for each file
   * @returns Promise with array of upload results
   */
  async uploadFiles(
    files: File[],
    documentType: 'receipt' | 'bank-statement',
    onProgress?: (fileIndex: number, progress: number) => void
  ): Promise<FileUploadResult[]> {
    const uploadPromises = files.map((file, index) => 
      this.uploadFile(
        file, 
        documentType, 
        onProgress ? (progress) => onProgress(index, progress) : undefined
      )
    );
    
    try {
      return await Promise.all(uploadPromises);
    } catch (error) {
      console.error('Multiple file upload error:', error);
      throw error;
    }
  }
  
  /**
   * Get uploaded file information
   * @param fileId The ID of the uploaded file
   * @returns Observable with file information
   */
  getFileInfo(fileId: string): Observable<FileUploadResult> {
    return this.http.get<FileUploadResult>(`${this.apiUrl}/${fileId}`);
  }
  
  /**
   * Download a file
   * @param fileId The ID of the file to download
   * @returns Observable with the file blob
   */
  downloadFile(fileId: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${fileId}/download`, {
      responseType: 'blob'
    });
  }
  
  /**
   * Delete an uploaded file
   * @param fileId The ID of the file to delete
   * @returns Observable with deletion result
   */
  deleteFile(fileId: string): Observable<{ success: boolean; message: string }> {
    return this.http.delete<{ success: boolean; message: string }>(
      `${this.apiUrl}/${fileId}`
    );
  }
  
  /**
   * Get all uploaded files for a user
   * @param documentType Optional filter by document type
   * @returns Observable with array of files
   */
  getUserFiles(documentType?: 'receipt' | 'bank-statement'): Observable<FileUploadResult[]> {
    let url = `${this.apiUrl}/user`;
    
    if (documentType) {
      url += `?documentType=${documentType}`;
    }
    
    return this.http.get<FileUploadResult[]>(url);
  }
  
  /**
   * Process uploaded receipts/bank statements for data extraction
   * @param fileId The ID of the uploaded file
   * @returns Observable with extracted data
   */
  processDocument(fileId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/${fileId}/process`, {});
  }
  
  /**
   * Validate file before upload
   * @param file The file to validate
   * @param maxSize Maximum file size in bytes
   * @returns Validation result
   */
  validateFile(file: File, maxSize: number = 10 * 1024 * 1024): {
    isValid: boolean;
    errors: string[];
  } {
    const errors: string[] = [];
    
    // Check file type
    if (file.type !== 'application/pdf') {
      errors.push('Nur PDF-Dateien sind erlaubt');
    }
    
    // Check file size
    if (file.size > maxSize) {
      const maxSizeMB = Math.round(maxSize / (1024 * 1024));
      errors.push(`Datei ist zu groß (max. ${maxSizeMB}MB)`);
    }
    
    // Check if file is empty
    if (file.size === 0) {
      errors.push('Datei ist leer');
    }
    
    // Check file name
    if (!file.name || file.name.trim().length === 0) {
      errors.push('Dateiname ist ungültig');
    }
    
    // Check for potentially dangerous file names
    const dangerousPatterns = [/\.\./g, /[<>:"|?*]/g];
    if (dangerousPatterns.some(pattern => pattern.test(file.name))) {
      errors.push('Dateiname enthält ungültige Zeichen');
    }
    
    return {
      isValid: errors.length === 0,
      errors
    };
  }
  
  /**
   * Get supported file types
   * @returns Array of supported MIME types
   */
  getSupportedFileTypes(): string[] {
    return ['application/pdf'];
  }
  
  /**
   * Get maximum file size
   * @returns Maximum file size in bytes
   */
  getMaxFileSize(): number {
    return 10 * 1024 * 1024; // 10MB
  }
  
  /**
   * Format file size for display
   * @param bytes File size in bytes
   * @returns Formatted file size string
   */
  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }
}