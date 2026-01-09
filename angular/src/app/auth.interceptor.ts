import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  // Clonamos la petición y le pegamos la etiqueta "withCredentials"
  // Esto hace que el navegador envíe la cookie automáticamente
  const authReq = req.clone({
    withCredentials: true
  });

  return next(authReq);
};