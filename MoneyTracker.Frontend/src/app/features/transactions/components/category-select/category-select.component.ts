import { Component, OnInit, Input, Output, EventEmitter, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatTreeModule, FlatTreeControl } from '@angular/material/tree';
import { MatIconButtonModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { Observable, Subject, BehaviorSubject, combineLatest } from 'rxjs';
import { takeUntil, map, startWith } from 'rxjs/operators';
import { MatTreeFlatDataSource, MatTreeFlattener } from '@angular/material/tree';
import { TransactionService, Category } from '../../services/transaction.service';

interface FlatCategory {
  id: string;
  name: string;
  level: number;
  expandable: boolean;
  parentId?: string;
  color?: string;
  icon?: string;
  fullPath: string;
}

@Component({
  selector: 'app-category-select',
  template: `
    <div class="category-select-container">
      <!-- Simple Select Mode -->
      <mat-form-field *ngIf="displayMode === 'select'" appearance="outline" class="full-width">
        <mat-label>{{ label }}</mat-label>
        <mat-select 
          [formControl]="categoryControl"
          [placeholder]="placeholder"
          [multiple]="allowMultiple"
          [disabled]="disabled"
          (selectionChange)="onSelectionChange($event)"
        >
          <mat-option *ngIf="allowClear" value="">
            <em>Keine Kategorie</em>
          </mat-option>
          
          <mat-optgroup 
            *ngFor="let rootCategory of categoriesTree$ | async" 
            [label]="rootCategory.name"
          >
            <mat-option 
              [value]="rootCategory.id"
              [style.padding-left.px]="rootCategory.level * 16"
            >
              <mat-icon *ngIf="rootCategory.icon" class="category-icon">{{ rootCategory.icon }}</mat-icon>
              <span [style.color]="rootCategory.color">{{ rootCategory.name }}</span>
            </mat-option>
            
            <ng-container *ngTemplateOutlet="categoryOptions; context: { categories: rootCategory.children }"></ng-container>
          </mat-optgroup>
        </mat-select>
        
        <mat-icon matSuffix *ngIf="showIcon">category</mat-icon>
        <mat-hint *ngIf="hint">{{ hint }}</mat-hint>
      </mat-form-field>
      
      <!-- Tree Mode -->
      <div *ngIf="displayMode === 'tree'" class="category-tree-container">
        <div class="tree-header">
          <h4>{{ label }}</h4>
          <div class="tree-actions">
            <mat-form-field appearance="outline" class="search-field">
              <mat-label>Kategorie suchen</mat-label>
              <input matInput [formControl]="searchControl" placeholder="Suchbegriff eingeben...">
              <mat-icon matSuffix>search</mat-icon>
            </mat-form-field>
            
            <button 
              mat-icon-button
              *ngIf="allowMultiple && selectedCategories.length > 0"
              (click)="clearSelection()"
              matTooltip="Auswahl zurücksetzen"
            >
              <mat-icon>clear</mat-icon>
            </button>
          </div>
        </div>
        
        <!-- Selected Categories Chips -->
        <mat-chip-set *ngIf="allowMultiple && selectedCategories.length > 0" class="selected-chips">
          <mat-chip 
            *ngFor="let categoryId of selectedCategories"
            removable
            (removed)="removeFromSelection(categoryId)"
          >
            <span>{{ getCategoryName(categoryId) }}</span>
            <mat-icon matChipRemove>cancel</mat-icon>
          </mat-chip>
        </mat-chip-set>
        
        <!-- Category Tree -->
        <mat-tree 
          [dataSource]="dataSource" 
          [treeControl]="treeControl" 
          class="category-tree"
        >
          <mat-tree-node 
            *matTreeNodeDef="let node" 
            matTreeNodePadding
            [style.padding-left.px]="node.level * 20"
            class="tree-node"
            [class.selected]="isSelected(node.id)"
            [class.disabled]="disabled"
            (click)="toggleSelection(node.id)"
          >
            <button 
              mat-icon-button 
              disabled 
              class="tree-spacer"
            ></button>
            
            <mat-icon 
              *ngIf="node.icon" 
              class="category-icon"
              [style.color]="node.color"
            >
              {{ node.icon }}
            </mat-icon>
            
            <span class="category-name" [style.color]="node.color">
              {{ node.name }}
            </span>
            
            <mat-icon 
              *ngIf="!allowMultiple && isSelected(node.id)"
              class="selected-indicator"
            >
              check_circle
            </mat-icon>
            
            <mat-checkbox 
              *ngIf="allowMultiple"
              [checked]="isSelected(node.id)"
              [disabled]="disabled"
              (click)="$event.stopPropagation()"
              (change)="toggleSelection(node.id)"
              class="category-checkbox"
            ></mat-checkbox>
          </mat-tree-node>
          
          <mat-tree-node 
            *matTreeNodeDef="let node; when: hasChild" 
            matTreeNodePadding
            [style.padding-left.px]="node.level * 20"
            class="tree-node expandable-node"
            [class.selected]="isSelected(node.id)"
            [class.disabled]="disabled"
          >
            <button 
              mat-icon-button 
              [attr.aria-label]="'Kategorie erweitern/reduzieren'"
              matTreeNodeToggle
              class="tree-toggle"
            >
              <mat-icon class="mat-icon-rtl-mirror">
                {{ treeControl.isExpanded(node) ? 'expand_more' : 'chevron_right' }}
              </mat-icon>
            </button>
            
            <mat-icon 
              *ngIf="node.icon" 
              class="category-icon"
              [style.color]="node.color"
            >
              {{ node.icon }}
            </mat-icon>
            
            <span 
              class="category-name"
              [style.color]="node.color"
              (click)="allowParentSelection && toggleSelection(node.id)"
              [class.clickable]="allowParentSelection"
            >
              {{ node.name }}
            </span>
            
            <mat-icon 
              *ngIf="!allowMultiple && isSelected(node.id)"
              class="selected-indicator"
            >
              check_circle
            </mat-icon>
            
            <mat-checkbox 
              *ngIf="allowMultiple && allowParentSelection"
              [checked]="isSelected(node.id)"
              [disabled]="disabled"
              (click)="$event.stopPropagation()"
              (change)="toggleSelection(node.id)"
              class="category-checkbox"
            ></mat-checkbox>
          </mat-tree-node>
        </mat-tree>
        
        <!-- Empty State -->
        <div *ngIf="(filteredCategories$ | async)?.length === 0" class="empty-state">
          <mat-icon>search_off</mat-icon>
          <p>Keine Kategorien gefunden</p>
          <p class="hint">Versuchen Sie einen anderen Suchbegriff</p>
        </div>
      </div>
      
      <!-- Compact Chip Mode -->
      <div *ngIf="displayMode === 'chips'" class="category-chips-container">
        <div class="chips-header">
          <span class="chips-label">{{ label }}</span>
          <button 
            mat-icon-button
            [matMenuTriggerFor]="categoryMenu"
            matTooltip="Kategorien auswählen"
          >
            <mat-icon>add</mat-icon>
          </button>
        </div>
        
        <mat-chip-set class="category-chips">
          <mat-chip 
            *ngFor="let categoryId of selectedCategories"
            removable
            (removed)="removeFromSelection(categoryId)"
            [style.background-color]="getCategoryColor(categoryId)"
          >
            <mat-icon *ngIf="getCategoryIcon(categoryId)">{{ getCategoryIcon(categoryId) }}</mat-icon>
            <span>{{ getCategoryName(categoryId) }}</span>
            <mat-icon matChipRemove>cancel</mat-icon>
          </mat-chip>
          
          <mat-chip *ngIf="selectedCategories.length === 0" class="placeholder-chip">
            <em>Keine Kategorien ausgewählt</em>
          </mat-chip>
        </mat-chip-set>
      </div>
    </div>
    
    <!-- Category Menu for Chips Mode -->
    <mat-menu #categoryMenu="matMenu" class="category-menu">
      <div class="menu-search">
        <mat-form-field appearance="outline">
          <mat-label>Suchen</mat-label>
          <input matInput [formControl]="searchControl" placeholder="Kategorie suchen...">
          <mat-icon matSuffix>search</mat-icon>
        </mat-form-field>
      </div>
      
      <button 
        mat-menu-item
        *ngFor="let category of flatCategories$ | async"
        (click)="toggleSelection(category.id)"
        [style.padding-left.px]="category.level * 16 + 16"
      >
        <mat-icon *ngIf="category.icon" [style.color]="category.color">{{ category.icon }}</mat-icon>
        <span [style.color]="category.color">{{ category.name }}</span>
        <mat-icon *ngIf="isSelected(category.id)" class="selected-check">check</mat-icon>
      </button>
    </mat-menu>
    
    <!-- Recursive template for nested options -->
    <ng-template #categoryOptions let-categories="categories">
      <mat-option 
        *ngFor="let category of categories"
        [value]="category.id"
        [style.padding-left.px]="category.level * 16"
      >
        <mat-icon *ngIf="category.icon" class="category-icon">{{ category.icon }}</mat-icon>
        <span [style.color]="category.color">{{ category.name }}</span>
      </mat-option>
      
      <ng-container *ngFor="let category of categories">
        <ng-container *ngTemplateOutlet="categoryOptions; context: { categories: category.children }"></ng-container>
      </ng-container>
    </ng-template>
  `,
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatSelectModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatTreeModule,
    MatIconButtonModule,
    MatTooltipModule,
    MatChipsModule
  ],
  styleUrls: ['./category-select.component.scss']
})
export class CategorySelectComponent implements OnInit, OnDestroy {
  @Input() label = 'Kategorie auswählen';
  @Input() placeholder = 'Kategorie auswählen...';
  @Input() hint = '';
  @Input() disabled = false;
  @Input() allowMultiple = false;
  @Input() allowClear = true;
  @Input() allowParentSelection = true;
  @Input() showIcon = true;
  @Input() displayMode: 'select' | 'tree' | 'chips' = 'select';
  @Input() preselectedCategories: string[] = [];
  
  @Output() selectionChange = new EventEmitter<string | string[]>();
  @Output() categorySelected = new EventEmitter<Category>();
  @Output() categoryDeselected = new EventEmitter<Category>();
  
  categoryControl = new FormControl();
  searchControl = new FormControl();
  selectedCategories: string[] = [];
  
  // Tree control
  treeControl = new FlatTreeControl<FlatCategory>(
    node => node.level,
    node => node.expandable
  );
  
  treeFlattener = new MatTreeFlattener(
    (category: Category, level: number): FlatCategory => ({
      id: category.id,
      name: category.name,
      level: level,
      expandable: !!category.children && category.children.length > 0,
      parentId: category.parentId,
      color: category.color,
      icon: category.icon,
      fullPath: this.buildCategoryPath(category)
    }),
    node => node.level,
    node => node.expandable,
    category => category.children || []
  );
  
  dataSource = new MatTreeFlatDataSource(this.treeControl, this.treeFlattener);
  
  // Observables
  categoriesTree$: Observable<Category[]>;
  flatCategories$: Observable<FlatCategory[]>;
  filteredCategories$: Observable<FlatCategory[]>;
  
  private destroy$ = new Subject<void>();
  private categoriesSubject = new BehaviorSubject<Category[]>([]);
  
  constructor(private transactionService: TransactionService) {
    this.categoriesTree$ = this.transactionService.getCategoryTree();
    
    // Flatten categories for search and menu
    this.flatCategories$ = this.categoriesTree$.pipe(
      map(categories => this.flattenCategories(categories))
    );
    
    // Filter categories based on search
    this.filteredCategories$ = combineLatest([
      this.flatCategories$,
      this.searchControl.valueChanges.pipe(startWith(''))
    ]).pipe(
      map(([categories, searchTerm]) => 
        this.filterCategories(categories, searchTerm || '')
      )
    );
  }
  
  ngOnInit(): void {
    // Initialize selected categories
    if (this.preselectedCategories.length > 0) {
      this.selectedCategories = [...this.preselectedCategories];
      this.updateFormControl();
    }
    
    // Set up tree data
    this.categoriesTree$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(categories => {
      this.dataSource.data = categories;
      this.categoriesSubject.next(categories);
    });
    
    // Set up filtered data for tree mode
    if (this.displayMode === 'tree') {
      this.filteredCategories$.pipe(
        takeUntil(this.destroy$)
      ).subscribe(filteredCategories => {
        // Rebuild tree with filtered data
        const filteredTree = this.buildFilteredTree(filteredCategories);
        this.dataSource.data = filteredTree;
      });
    }
  }
  
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
  
  hasChild = (_: number, node: FlatCategory) => node.expandable;
  
  onSelectionChange(event: any): void {
    const value = event.value;
    
    if (this.allowMultiple) {
      this.selectedCategories = Array.isArray(value) ? value : [value];
    } else {
      this.selectedCategories = value ? [value] : [];
    }
    
    this.emitSelectionChange();
  }
  
  toggleSelection(categoryId: string): void {
    if (this.disabled) return;
    
    if (this.allowMultiple) {
      const index = this.selectedCategories.indexOf(categoryId);
      
      if (index > -1) {
        this.selectedCategories.splice(index, 1);
        this.emitCategoryDeselected(categoryId);
      } else {
        this.selectedCategories.push(categoryId);
        this.emitCategorySelected(categoryId);
      }
    } else {
      if (this.selectedCategories.includes(categoryId)) {
        this.selectedCategories = [];
        this.emitCategoryDeselected(categoryId);
      } else {
        this.selectedCategories = [categoryId];
        this.emitCategorySelected(categoryId);
      }
    }
    
    this.updateFormControl();
    this.emitSelectionChange();
  }
  
  removeFromSelection(categoryId: string): void {
    const index = this.selectedCategories.indexOf(categoryId);
    if (index > -1) {
      this.selectedCategories.splice(index, 1);
      this.updateFormControl();
      this.emitSelectionChange();
      this.emitCategoryDeselected(categoryId);
    }
  }
  
  clearSelection(): void {
    const previousSelection = [...this.selectedCategories];
    this.selectedCategories = [];
    this.updateFormControl();
    this.emitSelectionChange();
    
    previousSelection.forEach(categoryId => {
      this.emitCategoryDeselected(categoryId);
    });
  }
  
  isSelected(categoryId: string): boolean {
    return this.selectedCategories.includes(categoryId);
  }
  
  getCategoryName(categoryId: string): string {
    return this.findCategoryById(categoryId)?.name || 'Unbekannte Kategorie';
  }
  
  getCategoryColor(categoryId: string): string {
    return this.findCategoryById(categoryId)?.color || '#666';
  }
  
  getCategoryIcon(categoryId: string): string | undefined {
    return this.findCategoryById(categoryId)?.icon;
  }
  
  private updateFormControl(): void {
    if (this.allowMultiple) {
      this.categoryControl.setValue(this.selectedCategories);
    } else {
      this.categoryControl.setValue(this.selectedCategories[0] || null);
    }
  }
  
  private emitSelectionChange(): void {
    const value = this.allowMultiple ? this.selectedCategories : this.selectedCategories[0] || null;
    this.selectionChange.emit(value);
  }
  
  private emitCategorySelected(categoryId: string): void {
    const category = this.findCategoryById(categoryId);
    if (category) {
      this.categorySelected.emit(category);
    }
  }
  
  private emitCategoryDeselected(categoryId: string): void {
    const category = this.findCategoryById(categoryId);
    if (category) {
      this.categoryDeselected.emit(category);
    }
  }
  
  private findCategoryById(categoryId: string): Category | undefined {
    const allCategories = this.categoriesSubject.value;
    return this.findCategoryInTree(allCategories, categoryId);
  }
  
  private findCategoryInTree(categories: Category[], categoryId: string): Category | undefined {
    for (const category of categories) {
      if (category.id === categoryId) {
        return category;
      }
      
      if (category.children) {
        const found = this.findCategoryInTree(category.children, categoryId);
        if (found) {
          return found;
        }
      }
    }
    
    return undefined;
  }
  
  private flattenCategories(categories: Category[], level: number = 0, path: string = ''): FlatCategory[] {
    const result: FlatCategory[] = [];
    
    for (const category of categories) {
      const currentPath = path ? `${path} > ${category.name}` : category.name;
      
      result.push({
        id: category.id,
        name: category.name,
        level: level,
        expandable: !!category.children && category.children.length > 0,
        parentId: category.parentId,
        color: category.color,
        icon: category.icon,
        fullPath: currentPath
      });
      
      if (category.children) {
        result.push(...this.flattenCategories(category.children, level + 1, currentPath));
      }
    }
    
    return result;
  }
  
  private filterCategories(categories: FlatCategory[], searchTerm: string): FlatCategory[] {
    if (!searchTerm.trim()) {
      return categories;
    }
    
    const term = searchTerm.toLowerCase();
    return categories.filter(category => 
      category.name.toLowerCase().includes(term) ||
      category.fullPath.toLowerCase().includes(term)
    );
  }
  
  private buildFilteredTree(filteredCategories: FlatCategory[]): Category[] {
    // This is a simplified version - in a real implementation,
    // you might want to preserve the tree structure even with filtering
    const categoryMap = new Map<string, Category>();
    
    // Convert flat categories back to tree structure
    // This is simplified and might need more sophisticated logic
    return [];
  }
  
  private buildCategoryPath(category: Category): string {
    // Build full path for search purposes
    let path = category.name;
    let current = category;
    
    // Note: This would need parent lookup logic in a real implementation
    return path;
  }
}