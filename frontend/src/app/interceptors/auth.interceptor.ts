import { HttpInterceptorFn } from '@angular/common/http';
import { environment } from './../../environments/environment';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenKey = environment.keys.tokenKey;
  const authToken = localStorage.getItem(tokenKey);
  const authReq = req.clone({
    setHeaders: {
      Authorization: `Bearer ${authToken}`
    }
  })
  return next(authReq);
};
