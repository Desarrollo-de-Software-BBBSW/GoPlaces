import { AuthService } from '@abp/ng.core';
import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ThemeSharedModule } from '@abp/ng.theme.shared';

import { PopularDestinationsComponent } from './popular-destinations/popular-destinations';
import { CitiesSearchComponent } from '../pages/cities-search/cities-search';

@Component({
  standalone: true,
  imports: [ 
    CommonModule,
    ThemeSharedModule,
    PopularDestinationsComponent,
    CitiesSearchComponent
  ],
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
})
export class HomeComponent {
  
  protected authService = inject(AuthService);

  get hasLoggedIn(): boolean {
    return this.authService.isAuthenticated;
  }

  login() {
    this.authService.navigateToLogin();
  }
}