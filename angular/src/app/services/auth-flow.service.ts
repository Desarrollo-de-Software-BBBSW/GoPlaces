import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { OAuthService } from 'angular-oauth2-oidc';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { EnvironmentService } from '@abp/ng.core';

@Injectable({
  providedIn: 'root'
})
export class AuthFlowService {

  constructor(
    private http: HttpClient,
    private oAuthService: OAuthService,
    private environmentService: EnvironmentService
  ) { }

  login(username: string, password: string): Observable<any> {
    const env = this.environmentService.getEnvironment();
    const issuer = env.oAuthConfig.issuer;
    const clientId = env.oAuthConfig.clientId;
    const clientSecret = env.oAuthConfig.dummyClientSecret || '';

    // URL Limpia (evita el doble slash)
    const cleanIssuer = issuer.endsWith('/') ? issuer.slice(0, -1) : issuer;
    const tokenUrl = `${cleanIssuer}/connect/token`;

    const body = new URLSearchParams();
    body.set('grant_type', 'password');
    body.set('username', username);
    body.set('password', password);
    body.set('client_id', clientId);
    body.set('scope', 'offline_access GoPlaces openid profile email'); 
    if (clientSecret) body.set('client_secret', clientSecret);

    const headers = new HttpHeaders({ 'Content-Type': 'application/x-www-form-urlencoded' });

    return this.http.post(tokenUrl, body.toString(), { headers }).pipe(
      tap((response: any) => {
        if (response.access_token) {
           console.log('✅ Token obtenido. Guardando datos de sesión completos...');
           
           // --- INICIO DE LA CORRECCIÓN ---
           
           // 1. Calculamos cuándo vence el token (Ahora + segundos de vida)
           const expiresInMilli = (response.expires_in || 3600) * 1000;
           const now = new Date().getTime();
           const expiration = now + expiresInMilli;

           // 2. Guardamos TODO lo que la librería necesita para "recordar" la sesión
           localStorage.setItem('access_token', response.access_token);
           localStorage.setItem('access_token_stored_at', '' + now);
           localStorage.setItem('expires_at', '' + expiration); // <--- ESTO FALTABA
           
           if (response.id_token) {
             localStorage.setItem('id_token', response.id_token);
             // Esto ayuda a decodificar los datos del usuario (claims)
             localStorage.setItem('id_token_claims_obj', JSON.stringify(this.parseJwt(response.id_token)));
           }
           
           if (response.refresh_token) {
             localStorage.setItem('refresh_token', response.refresh_token);
           }

           // --- FIN DE LA CORRECCIÓN ---
        }
      })
    );
  }

  // Función auxiliar para leer los datos dentro del token
  private parseJwt(token: string) {
    try {
      var base64Url = token.split('.')[1];
      var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      var jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function(c) {
          return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
      }).join(''));
      return JSON.parse(jsonPayload);
    } catch (e) {
      return {};
    }
  }
}