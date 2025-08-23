import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class StorageService {

  // Local Storage methods
  setItem(key: string, value: any): void {
    try {
      const serializedValue = JSON.stringify(value);
      localStorage.setItem(key, serializedValue);
    } catch (error) {
      console.error('Error saving to localStorage:', error);
    }
  }

  getItem<T>(key: string): T | null {
    try {
      const item = localStorage.getItem(key);
      if (item === null) {
        return null;
      }
      return JSON.parse(item) as T;
    } catch (error) {
      console.error('Error retrieving from localStorage:', error);
      return null;
    }
  }

  removeItem(key: string): void {
    try {
      localStorage.removeItem(key);
    } catch (error) {
      console.error('Error removing from localStorage:', error);
    }
  }

  clear(): void {
    try {
      localStorage.clear();
    } catch (error) {
      console.error('Error clearing localStorage:', error);
    }
  }

  // Session Storage methods
  setSessionItem(key: string, value: any): void {
    try {
      const serializedValue = JSON.stringify(value);
      sessionStorage.setItem(key, serializedValue);
    } catch (error) {
      console.error('Error saving to sessionStorage:', error);
    }
  }

  getSessionItem<T>(key: string): T | null {
    try {
      const item = sessionStorage.getItem(key);
      if (item === null) {
        return null;
      }
      return JSON.parse(item) as T;
    } catch (error) {
      console.error('Error retrieving from sessionStorage:', error);
      return null;
    }
  }

  removeSessionItem(key: string): void {
    try {
      sessionStorage.removeItem(key);
    } catch (error) {
      console.error('Error removing from sessionStorage:', error);
    }
  }

  clearSession(): void {
    try {
      sessionStorage.clear();
    } catch (error) {
      console.error('Error clearing sessionStorage:', error);
    }
  }

  // Utility methods
  isLocalStorageAvailable(): boolean {
    try {
      const test = 'test';
      localStorage.setItem(test, test);
      localStorage.removeItem(test);
      return true;
    } catch {
      return false;
    }
  }

  isSessionStorageAvailable(): boolean {
    try {
      const test = 'test';
      sessionStorage.setItem(test, test);
      sessionStorage.removeItem(test);
      return true;
    } catch {
      return false;
    }
  }

  getStorageSize(): { localStorage: number; sessionStorage: number } {
    let localStorageSize = 0;
    let sessionStorageSize = 0;

    // Calculate localStorage size
    if (this.isLocalStorageAvailable()) {
      for (let key in localStorage) {
        if (localStorage.hasOwnProperty(key)) {
          localStorageSize += localStorage[key].length + key.length;
        }
      }
    }

    // Calculate sessionStorage size
    if (this.isSessionStorageAvailable()) {
      for (let key in sessionStorage) {
        if (sessionStorage.hasOwnProperty(key)) {
          sessionStorageSize += sessionStorage[key].length + key.length;
        }
      }
    }

    return {
      localStorage: localStorageSize,
      sessionStorage: sessionStorageSize
    };
  }

  // User preferences
  setUserPreference(key: string, value: any): void {
    this.setItem(`user_pref_${key}`, value);
  }

  getUserPreference<T>(key: string, defaultValue?: T): T | null {
    const preference = this.getItem<T>(`user_pref_${key}`);
    return preference !== null ? preference : (defaultValue || null);
  }

  removeUserPreference(key: string): void {
    this.removeItem(`user_pref_${key}`);
  }

  // App settings
  setAppSetting(key: string, value: any): void {
    this.setItem(`app_setting_${key}`, value);
  }

  getAppSetting<T>(key: string, defaultValue?: T): T | null {
    const setting = this.getItem<T>(`app_setting_${key}`);
    return setting !== null ? setting : (defaultValue || null);
  }

  removeAppSetting(key: string): void {
    this.removeItem(`app_setting_${key}`);
  }
}