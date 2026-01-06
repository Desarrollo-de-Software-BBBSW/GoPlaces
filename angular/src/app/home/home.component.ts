import { AuthService } from '@abp/ng.core';
import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common'; // Importación necesaria
import { ThemeSharedModule } from '@abp/ng.theme.shared'; // Importación necesaria

@Component({
  standalone: true, // <<-- ESTO ES LO QUE FALTA
  imports: [
    CommonModule,
    ThemeSharedModule 
  ],
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
})
export class HomeComponent {
  
  // Inyección moderna del servicio de autenticación
  protected authService = inject(AuthService);

  get hasLoggedIn(): boolean {
    return this.authService.isAuthenticated;
  }

  login() {
    this.authService.navigateToLogin();
  }
}