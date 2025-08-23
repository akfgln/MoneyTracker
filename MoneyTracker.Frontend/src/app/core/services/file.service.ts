import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { ApiService } from './api.service';
import { UploadedFile, FileStatus } from '../models/transaction.model';

export interface FileUploadProgress {
  progress: number;
  fileName: string;
  status: 'uploading' | 'processing' | 'completed' | 'error';
  message?: string;
}

@Injectable({
  providedIn: 'root'
})
export class FileService {
  private readonly endpoint = 'api/files';

  constructor(private apiService: ApiService) {}

  uploadFile(
    file: File, 
    fileType: 'Receipt' | 'BankStatement',
    metadata?: any
  ): Observable<UploadedFile> {
    const additionalData = {
      fileType,
      ...metadata
    };
    
    return this.apiService.upload<UploadedFile>(`${this.endpoint}/upload`, file, additionalData)
      .pipe(
        map(response => ({
          ...response.data,
          uploadDate: new Date(response.data.uploadDate),
          createdAt: new Date(response.data.createdAt),
          updatedAt: new Date(response.data.updatedAt)
        }))
      );
  }

  uploadReceipt(file: File, transactionId?: string): Observable<UploadedFile> {
    return this.uploadFile(file, 'Receipt', { transactionId });
  }

  uploadBankStatement(
    file: File, 
    bankName: string, 
    accountId: string
  ): Observable<UploadedFile> {
    return this.uploadFile(file, 'BankStatement', { bankName, accountId });
  }

  getFile(id: string): Observable<UploadedFile> {
    return this.apiService.get<UploadedFile>(`${this.endpoint}/${id}`)
      .pipe(
        map(response => ({
          ...response.data,
          uploadDate: new Date(response.data.uploadDate),
          createdAt: new Date(response.data.createdAt),
          updatedAt: new Date(response.data.updatedAt)
        }))
      );
  }

  getUserFiles(): Observable<UploadedFile[]> {
    return this.apiService.get<UploadedFile[]>(`${this.endpoint}/user`)
      .pipe(
        map(response => response.data.map(file => ({
          ...file,
          uploadDate: new Date(file.uploadDate),
          createdAt: new Date(file.createdAt),
          updatedAt: new Date(file.updatedAt)
        })))
      );
  }

  downloadFile(id: string): Observable<Blob> {
    return this.apiService.download(`${this.endpoint}/${id}/download`);
  }

  deleteFile(id: string): Observable<boolean> {
    return this.apiService.delete<boolean>(`${this.endpoint}/${id}`)
      .pipe(map(response => response.data));
  }

  getProcessingStatus(id: string): Observable<UploadedFile> {
    return this.apiService.get<UploadedFile>(`${this.endpoint}/${id}/status`)
      .pipe(
        map(response => ({
          ...response.data,
          uploadDate: new Date(response.data.uploadDate),
          createdAt: new Date(response.data.createdAt),
          updatedAt: new Date(response.data.updatedAt)
        }))
      );
  }

  getExtractedData(id: string): Observable<any> {
    return this.apiService.get<any>(`${this.endpoint}/${id}/extracted-data`)
      .pipe(map(response => response.data));
  }

  reprocessFile(id: string): Observable<UploadedFile> {
    return this.apiService.post<UploadedFile>(`${this.endpoint}/${id}/reprocess`, {})
      .pipe(
        map(response => ({
          ...response.data,
          uploadDate: new Date(response.data.uploadDate),
          createdAt: new Date(response.data.createdAt),
          updatedAt: new Date(response.data.updatedAt)
        }))
      );
  }

  validateFileType(file: File): boolean {
    const allowedTypes = [
      'application/pdf',
      'image/jpeg',
      'image/png',
      'text/csv',
      'application/vnd.ms-excel',
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
    ];
    return allowedTypes.includes(file.type);
  }

  validateFileSize(file: File, maxSizeMB: number = 10): boolean {
    const maxSizeBytes = maxSizeMB * 1024 * 1024;
    return file.size <= maxSizeBytes;
  }

  getFileStatusColor(status: FileStatus): string {
    switch (status) {
      case FileStatus.Uploading:
        return 'primary';
      case FileStatus.Processing:
        return 'accent';
      case FileStatus.Processed:
        return 'primary';
      case FileStatus.Failed:
        return 'warn';
      default:
        return 'primary';
    }
  }

  getFileStatusIcon(status: FileStatus): string {
    switch (status) {
      case FileStatus.Uploading:
        return 'cloud_upload';
      case FileStatus.Processing:
        return 'hourglass_empty';
      case FileStatus.Processed:
        return 'check_circle';
      case FileStatus.Failed:
        return 'error';
      default:
        return 'description';
    }
  }
}
