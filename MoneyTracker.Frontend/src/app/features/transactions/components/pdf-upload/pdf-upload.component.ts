import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatButtonToggleGroup, MatButtonToggle } from '@angular/material/button-toggle';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FileUploadService } from '../../services/file-upload.service';
import { GermanFormatService } from '../../services/german-format.service';

export interface UploadedFile {
  id?: string;
  file: File;
  name: string;
  size: number;
  type: string;
  uploadProgress: number;
  status: 'pending' | 'uploading' | 'completed' | 'error';
  errorMessage?: string;
  downloadUrl?: string;
}

@Component({
  selector: 'app-pdf-upload',
  template: `
    <mat-card class="pdf-upload-card">
      <mat-card-header>
        <mat-card-title>PDF Upload</mat-card-title>
        <mat-card-subtitle>Laden Sie Belege und Kontoauszüge hoch</mat-card-subtitle>
      </mat-card-header>
      
      <mat-card-content>
        <!-- Drag and Drop Zone -->
        <div 
          class="drop-zone"
          [class.dragover]="isDragOver"
          [class.disabled]="isUploading"
          (dragover)="onDragOver($event)"
          (dragleave)="onDragLeave($event)"
          (drop)="onDrop($event)"
          (click)="fileInput.click()"
        >
          <div class="drop-zone-content">
            <mat-icon class="upload-icon">cloud_upload</mat-icon>
            <h3>Dateien hierher ziehen oder klicken</h3>
            <p>Nur PDF-Dateien sind erlaubt (max. {{ formatFileSize(maxFileSize) }})</p>
            
            <mat-button-toggle-group 
              class="upload-type-toggle"
              [(ngModel)]="uploadType"
              name="uploadType"
            >
              <mat-button-toggle value="receipt">Beleg</mat-button-toggle>
              <mat-button-toggle value="bank-statement">Kontoauszug</mat-button-toggle>
            </mat-button-toggle-group>
          </div>
          
          <input 
            #fileInput
            type="file"
            accept=".pdf,application/pdf"
            multiple
            (change)="onFileSelected($event)"
            style="display: none;"
          />
        </div>
        
        <!-- Upload Queue -->
        <div class="upload-queue" *ngIf="uploadedFiles.length > 0">
          <h4>Upload-Warteschlange</h4>
          
          <div class="file-item" *ngFor="let file of uploadedFiles; trackBy: trackByFileId">
            <div class="file-info">
              <mat-icon class="file-icon">picture_as_pdf</mat-icon>
              <div class="file-details">
                <div class="file-name" [matTooltip]="file.name">{{ file.name }}</div>
                <div class="file-meta">
                  {{ formatFileSize(file.size) }} • {{ file.type }} • 
                  <span [ngClass]="'status-' + file.status">{{ getStatusText(file.status) }}</span>
                </div>
              </div>
            </div>
            
            <!-- Progress Bar -->
            <mat-progress-bar 
              *ngIf="file.status === 'uploading'"
              mode="determinate"
              [value]="file.uploadProgress"
              class="upload-progress"
            ></mat-progress-bar>
            
            <!-- Error Message -->
            <div class="error-message" *ngIf="file.status === 'error' && file.errorMessage">
              <mat-icon color="warn">error</mat-icon>
              {{ file.errorMessage }}
            </div>
            
            <!-- Actions -->
            <div class="file-actions">
              <button 
                mat-icon-button
                *ngIf="file.status === 'completed' && file.downloadUrl"
                (click)="downloadFile(file)"
                matTooltip="Datei herunterladen"
              >
                <mat-icon>download</mat-icon>
              </button>
              
              <button 
                mat-icon-button
                *ngIf="file.status === 'error'"
                (click)="retryUpload(file)"
                matTooltip="Erneut versuchen"
              >
                <mat-icon>refresh</mat-icon>
              </button>
              
              <button 
                mat-icon-button
                color="warn"
                (click)="removeFile(file)"
                matTooltip="Datei entfernen"
                [disabled]="file.status === 'uploading'"
              >
                <mat-icon>delete</mat-icon>
              </button>
            </div>
          </div>
        </div>
        
        <!-- Upload Summary -->
        <div class="upload-summary" *ngIf="uploadedFiles.length > 0">
          <mat-chip-set>
            <mat-chip *ngIf="getFilesByStatus('completed').length > 0" color="primary">
              <mat-icon>check_circle</mat-icon>
              {{ getFilesByStatus('completed').length }} erfolgreich
            </mat-chip>
            
            <mat-chip *ngIf="getFilesByStatus('uploading').length > 0" color="accent">
              <mat-icon>upload</mat-icon>
              {{ getFilesByStatus('uploading').length }} wird hochgeladen
            </mat-chip>
            
            <mat-chip *ngIf="getFilesByStatus('error').length > 0" color="warn">
              <mat-icon>error</mat-icon>
              {{ getFilesByStatus('error').length }} Fehler
            </mat-chip>
          </mat-chip-set>
        </div>
      </mat-card-content>
      
      <mat-card-actions *ngIf="hasFilesToUpload()">
        <button 
          mat-raised-button 
          color="primary"
          (click)="uploadAll()"
          [disabled]="isUploading"
        >
          <mat-icon>cloud_upload</mat-icon>
          Alle hochladen ({{ getPendingFilesCount() }})
        </button>
        
        <button 
          mat-button
          (click)="clearCompleted()"
          [disabled]="getFilesByStatus('completed').length === 0"
        >
          Abgeschlossene löschen
        </button>
        
        <button 
          mat-button
          color="warn"
          (click)="clearAll()"
          [disabled]="isUploading"
        >
          Alle löschen
        </button>
      </mat-card-actions>
    </mat-card>
  `,
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    MatSnackBarModule,
    MatCardModule,
    MatChipsModule,
    MatTooltipModule,
    MatButtonToggleGroup,
    MatButtonToggle,
    MatInputModule
  ],
  styleUrls: ['./pdf-upload.component.scss']
})
export class PdfUploadComponent implements OnInit {
  @Input() uploadType: 'receipt' | 'bank-statement' = 'receipt';
  @Input() maxFileSize: number = 10 * 1024 * 1024; // 10MB
  @Input() maxFiles: number = 10;
  @Input() autoUpload: boolean = false;
  
  @Output() fileUploaded = new EventEmitter<UploadedFile>();
  @Output() filesUploaded = new EventEmitter<UploadedFile[]>();
  @Output() uploadError = new EventEmitter<{ file: UploadedFile; error: string }>();
  
  uploadedFiles: UploadedFile[] = [];
  isDragOver = false;
  isUploading = false;
  
  constructor(
    private fileUploadService: FileUploadService,
    private germanFormatService: GermanFormatService,
    private snackBar: MatSnackBar
  ) {}
  
  ngOnInit(): void {
    // Initialize component
  }
  
  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = true;
  }
  
  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
  }
  
  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
    
    const files = event.dataTransfer?.files;
    if (files) {
      this.handleFiles(Array.from(files));
    }
  }
  
  onFileSelected(event: Event): void {
    const target = event.target as HTMLInputElement;
    const files = target.files;
    
    if (files) {
      this.handleFiles(Array.from(files));
      target.value = ''; // Clear input
    }
  }
  
  private handleFiles(files: File[]): void {
    const validFiles = files.filter(file => this.validateFile(file));
    
    if (validFiles.length === 0) {
      return;
    }
    
    // Check file count limit
    if (this.uploadedFiles.length + validFiles.length > this.maxFiles) {
      this.snackBar.open(
        `Maximale Anzahl von ${this.maxFiles} Dateien erreicht`,
        'Schließen',
        { duration: 3000 }
      );
      return;
    }
    
    // Add files to upload queue
    const uploadFiles: UploadedFile[] = validFiles.map(file => ({
      file,
      name: file.name,
      size: file.size,
      type: file.type,
      uploadProgress: 0,
      status: 'pending'
    }));
    
    this.uploadedFiles.push(...uploadFiles);
    
    if (this.autoUpload) {
      this.uploadFiles(uploadFiles);
    }
    
    this.snackBar.open(
      `${validFiles.length} Datei(en) zur Warteschlange hinzugefügt`,
      'Schließen',
      { duration: 2000 }
    );
  }
  
  private validateFile(file: File): boolean {
    // Check file type
    if (file.type !== 'application/pdf') {
      this.snackBar.open(
        `"${file.name}" ist kein gültiges PDF`,
        'Schließen',
        { duration: 3000 }
      );
      return false;
    }
    
    // Check file size
    if (file.size > this.maxFileSize) {
      this.snackBar.open(
        `"${file.name}" ist zu groß (max. ${this.formatFileSize(this.maxFileSize)})`,
        'Schließen',
        { duration: 3000 }
      );
      return false;
    }
    
    // Check for duplicates
    const duplicate = this.uploadedFiles.find(f => 
      f.name === file.name && f.size === file.size
    );
    
    if (duplicate) {
      this.snackBar.open(
        `"${file.name}" wurde bereits hinzugefügt`,
        'Schließen',
        { duration: 3000 }
      );
      return false;
    }
    
    return true;
  }
  
  uploadAll(): void {
    const pendingFiles = this.uploadedFiles.filter(f => f.status === 'pending');
    if (pendingFiles.length > 0) {
      this.uploadFiles(pendingFiles);
    }
  }
  
  private async uploadFiles(files: UploadedFile[]): Promise<void> {
    this.isUploading = true;
    
    const uploadPromises = files.map(file => this.uploadSingleFile(file));
    
    try {
      await Promise.all(uploadPromises);
      
      const completedFiles = files.filter(f => f.status === 'completed');
      if (completedFiles.length > 0) {
        this.filesUploaded.emit(completedFiles);
        this.snackBar.open(
          `${completedFiles.length} Datei(en) erfolgreich hochgeladen`,
          'Schließen',
          { duration: 3000 }
        );
      }
    } catch (error) {
      console.error('Upload error:', error);
    } finally {
      this.isUploading = false;
    }
  }
  
  private async uploadSingleFile(uploadFile: UploadedFile): Promise<void> {
    uploadFile.status = 'uploading';
    uploadFile.uploadProgress = 0;
    
    try {
      const result = await this.fileUploadService.uploadFile(
        uploadFile.file,
        this.uploadType,
        (progress) => {
          uploadFile.uploadProgress = progress;
        }
      );
      
      uploadFile.status = 'completed';
      uploadFile.id = result.id;
      uploadFile.downloadUrl = result.downloadUrl;
      uploadFile.uploadProgress = 100;
      
      this.fileUploaded.emit(uploadFile);
    } catch (error: any) {
      uploadFile.status = 'error';
      uploadFile.errorMessage = error.message || 'Upload fehlgeschlagen';
      uploadFile.uploadProgress = 0;
      
      this.uploadError.emit({ file: uploadFile, error: error.message });
    }
  }
  
  retryUpload(file: UploadedFile): void {
    file.status = 'pending';
    file.errorMessage = undefined;
    this.uploadSingleFile(file);
  }
  
  removeFile(file: UploadedFile): void {
    const index = this.uploadedFiles.indexOf(file);
    if (index > -1) {
      this.uploadedFiles.splice(index, 1);
    }
  }
  
  downloadFile(file: UploadedFile): void {
    if (file.downloadUrl) {
      window.open(file.downloadUrl, '_blank');
    }
  }
  
  clearCompleted(): void {
    this.uploadedFiles = this.uploadedFiles.filter(f => f.status !== 'completed');
  }
  
  clearAll(): void {
    this.uploadedFiles = [];
  }
  
  hasFilesToUpload(): boolean {
    return this.uploadedFiles.length > 0;
  }
  
  getPendingFilesCount(): number {
    return this.uploadedFiles.filter(f => f.status === 'pending').length;
  }
  
  getFilesByStatus(status: UploadedFile['status']): UploadedFile[] {
    return this.uploadedFiles.filter(f => f.status === status);
  }
  
  getStatusText(status: UploadedFile['status']): string {
    switch (status) {
      case 'pending': return 'Wartend';
      case 'uploading': return 'Wird hochgeladen';
      case 'completed': return 'Abgeschlossen';
      case 'error': return 'Fehler';
      default: return status;
    }
  }
  
  formatFileSize(bytes: number): string {
    return this.germanFormatService.formatFileSize(bytes);
  }
  
  trackByFileId(index: number, file: UploadedFile): string {
    return file.id || `${file.name}-${file.size}`;
  }
}