import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { ToasterService } from '@abp/ng.theme.shared';

// Importamos el nuevo servicio y el DTO
// (Asegúrate que la ruta del import sea correcta tras el generate-proxy)
import { PublicUserService, PublicUserProfileDto } from 'src/app/proxy/users'; 

@Component({
  selector: 'app-public-profile',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './public-profile.html',
  styleUrls: ['./public-profile.scss']
})
export class PublicProfileComponent implements OnInit {
  userProfile: PublicUserProfileDto | null = null;
  isLoading = true;
  errorMessage = '';

  constructor(
    private route: ActivatedRoute,
    private publicUserService: PublicUserService,
    private toaster: ToasterService
  ) {}

  ngOnInit(): void {
    // 1. Capturamos el nombre de usuario de la URL
    this.route.params.subscribe(params => {
      const userName = params['userName']; // Ojo: debe coincidir con lo que pongamos en el routing
      if (userName) {
        this.loadProfile(userName);
      }
    });
  }

  loadProfile(userName: string) {
    this.isLoading = true;
    this.errorMessage = '';

    this.publicUserService.getByUserName(userName).subscribe({
      next: (data) => {
        this.userProfile = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error(err);
        this.errorMessage = 'No pudimos encontrar a este usuario.';
        this.isLoading = false;
      }
    });
  }
}