import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export class GermanValidators {
  static germanCurrency: any | string = 'EUR';
  
  /**
   * German IBAN validator
   * Validates German IBAN format (22 characters, starts with DE)
   */
  static iban(control: AbstractControl): ValidationErrors | null {
    const value = control.value?.replace(/\s/g, '');
    if (!value) return null;
    
    // German IBAN validation (22 characters, starts with DE)
    const germanIbanPattern = /^DE[0-9]{20}$/;
    
    if (!germanIbanPattern.test(value)) {
      return { invalidIban: { actualValue: value, requiredPattern: 'DE + 20 digits' } };
    }
    
    // Additional IBAN checksum validation
    if (!GermanValidators.isValidIbanChecksum(value)) {
      return { invalidIbanChecksum: { actualValue: value } };
    }
    
    return null;
  }

  /**
   * German phone number validator
   * Validates German phone number formats
   */
  static germanPhone(control: AbstractControl): ValidationErrors | null {
    const value = control.value?.replace(/\s|-|\(|\)/g, '');
    if (!value) return null;
    
    // German phone number patterns
    const patterns = [
      /^(\+49|0049)[1-9][0-9]{1,14}$/, // International format
      /^0[1-9][0-9]{1,11}$/, // National format
      /^[1-9][0-9]{1,11}$/ // Local format without leading zero
    ];
    
    const isValid = patterns.some(pattern => pattern.test(value));
    
    if (!isValid) {
      return { invalidGermanPhone: { actualValue: value } };
    }
    
    return null;
  }

  /**
   * Strong password validator with German requirements
   * At least 12 characters, uppercase, lowercase, numbers, special characters
   */
  static strongPassword(control: AbstractControl): ValidationErrors | null {
    const value = control.value;
    if (!value) return null;
    
    const hasUpperCase = /[A-ZÀ-ÖØ-Þ]/.test(value); // Including German umlauts
    const hasLowerCase = /[a-zà-öø-ÿß]/.test(value); // Including German umlauts and ß
    const hasNumeric = /[0-9]/.test(value);
    const hasSpecialChar = /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?~`]/.test(value);
    const isValidLength = value.length >= 12;
    
    const passwordValid = hasUpperCase && hasLowerCase && hasNumeric && hasSpecialChar && isValidLength;
    
    if (!passwordValid) {
      return {
        strongPassword: {
          hasUpperCase,
          hasLowerCase,
          hasNumeric,
          hasSpecialChar,
          isValidLength,
          actualLength: value.length
        }
      };
    }
    
    return null;
  }

  /**
   * German postal code validator
   * Validates 5-digit German postal codes
   */
  static germanPostalCode(control: AbstractControl): ValidationErrors | null {
    const value = control.value;
    if (!value) return null;
    
    const germanPostalPattern = /^[0-9]{5}$/;
    
    if (!germanPostalPattern.test(value)) {
      return { invalidGermanPostalCode: { actualValue: value, requiredPattern: '5 digits (e.g., 12345)' } };
    }
    
    return null;
  }

  /**
   * German tax ID validator (Steuerliche Identifikationsnummer)
   * Validates 11-digit German tax ID
   */
  static germanTaxId(control: AbstractControl): ValidationErrors | null {
    const value = control.value?.replace(/\s/g, '');
    if (!value) return null;
    
    // German tax ID is 11 digits
    if (!/^[0-9]{11}$/.test(value)) {
      return { invalidGermanTaxId: { actualValue: value, requiredPattern: '11 digits' } };
    }
    
    // Additional validation rules for German tax ID
    const digits = value.split('').map(Number);
    
    // Check that not all digits are the same
    if (new Set(digits).size === 1) {
      return { invalidGermanTaxId: { actualValue: value, reason: 'All digits cannot be the same' } };
    }
    
    return null;
  }

  /**
   * German currency amount validator
   * Validates amounts in German format (comma as decimal separator)
   */
  static germanCurrencyAmount(maxValue?: number): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;
      if (!value) return null;
      
      // Convert German format to standard format for validation
      let normalizedValue = value.toString().replace(',', '.');
      
      // Remove currency symbols and whitespace
      normalizedValue = normalizedValue.replace(/[€\s]/g, '');
      
      const numericValue = parseFloat(normalizedValue);
      
      if (isNaN(numericValue)) {
        return { invalidCurrencyAmount: { actualValue: value } };
      }
      
      if (numericValue < 0) {
        return { negativeAmount: { actualValue: value } };
      }
      
      if (maxValue && numericValue > maxValue) {
        return { maxAmountExceeded: { actualValue: value, maxValue } };
      }
      
      // Check for more than 2 decimal places
      const decimalPart = normalizedValue.split('.')[1];
      if (decimalPart && decimalPart.length > 2) {
        return { tooManyDecimals: { actualValue: value, maxDecimals: 2 } };
      }
      
      return null;
    };
  }

  /**
   * Confirm password validator
   * Validates that password confirmation matches the original password
   */
  static confirmPassword(passwordControlName: string = 'password'): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.parent) return null;
      
      const password = control.parent.get(passwordControlName);
      const confirmPassword = control;
      
      if (!password || !confirmPassword) return null;
      
      if (password.value !== confirmPassword.value) {
        return { passwordMismatch: true };
      }
      
      return null;
    };
  }

  /**
   * German date validator
   * Validates dates in German format (DD.MM.YYYY)
   */
  static germanDate(control: AbstractControl): ValidationErrors | null {
    const value = control.value;
    if (!value) return null;
    
    // Check German date format DD.MM.YYYY
    const germanDatePattern = /^([0-3][0-9])\.([0-1][0-9])\.(\d{4})$/;
    const match = value.match(germanDatePattern);
    
    if (!match) {
      return { invalidGermanDate: { actualValue: value, requiredFormat: 'DD.MM.YYYY' } };
    }
    
    const day = parseInt(match[1], 10);
    const month = parseInt(match[2], 10);
    const year = parseInt(match[3], 10);
    
    // Validate date values
    if (month < 1 || month > 12) {
      return { invalidMonth: { actualValue: month } };
    }
    
    if (day < 1 || day > 31) {
      return { invalidDay: { actualValue: day } };
    }
    
    // Check if date is valid
    const testDate = new Date(year, month - 1, day);
    if (testDate.getFullYear() !== year || testDate.getMonth() !== month - 1 || testDate.getDate() !== day) {
      return { invalidDate: { actualValue: value } };
    }
    
    return null;
  }

  /**
   * Helper method for IBAN checksum validation
   */
  private static isValidIbanChecksum(iban: string): boolean {
    // Move the first 4 chars to the end
    const rearranged = iban.substring(4) + iban.substring(0, 4);
    
    // Replace letters with numbers (A=10, B=11, ..., Z=35)
    const numericString = rearranged.replace(/[A-Z]/g, char => 
      (char.charCodeAt(0) - 55).toString()
    );
    
    // Calculate mod 97
    let remainder = 0;
    for (let i = 0; i < numericString.length; i++) {
      remainder = (remainder * 10 + parseInt(numericString[i], 10)) % 97;
    }
    
    return remainder === 1;
  }
}