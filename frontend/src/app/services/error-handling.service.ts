import { Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ErrorHandlingService {
  constructor() {}

  handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An unknown error occurred.';

    if (error.status === 400) {
      if (error.error?.errors) {
        const userFriendlyErrors: string[] = [];
        for (const field in error.error.errors) {
          if (Object.prototype.hasOwnProperty.call(error.error.errors, field)) {
            const firstErrorMessage = error.error.errors[field][0];
            userFriendlyErrors.push(`${field}: ${firstErrorMessage}`);
          }
        }
        errorMessage = userFriendlyErrors.join(' | ');
      } else if (error.error?.message) {
        errorMessage = error.error.message;
      }
    } else if (error.status === 401) {
      errorMessage =
        error.error?.message || 'Invalid email or password. Please try again.';
    } else if (error.status === 403) {
      errorMessage =
        error.error?.message ||
        'You do not have permission to perform this action.';
    } else if (error.status === 404) {
      errorMessage =
        error.error?.message || 'The requested resource was not found.';
    } else if (error.status === 500) {
      errorMessage =
        error.error?.message ||
        'A server error occurred. Please try again later.';
    }

    console.error('HTTP Error:', error);

    return throwError(() => new Error(errorMessage));
  }
}
