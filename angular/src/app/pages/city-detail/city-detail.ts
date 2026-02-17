import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms'; 
import { ActivatedRoute, RouterModule } from '@angular/router';
import { CityService, CityDto } from 'src/app/proxy/cities';
import { AuthService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';

// Importamos el servicio de ratings
import { RatingService, RatingDto, CreateRatingDto } from '../../services/rating.service';

@Component({
  selector: 'app-city-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './city-detail.html',
  styleUrls: ['./city-detail.scss']
})
export class CityDetailComponent implements OnInit {
  city: CityDto | null = null;
  isLoading = true;
  errorMessage = '';

  userRating: RatingDto | null = null; 
  selectedScore = 0;                  
  hoverScore = 0;                      
  ratingComment = '';                  
  isRatingSubmitting = false;          
  isAuthenticated = false;             

  constructor(
    private route: ActivatedRoute,
    private cityService: CityService,
    private ratingService: RatingService, 
    private authService: AuthService,     
    private toaster: ToasterService       
  ) {}

  ngOnInit(): void {
    this.isAuthenticated = this.authService.isAuthenticated;

    // 1. Capturamos el ID de la URL
    this.route.params.subscribe(params => {
      const id = params['id']; 
      
      if (id) {
        // ✅ Corregido: id se pasa como string (GUID), sin Number()
        this.loadCity(id); 
      }
    });
  }

  loadCity(id: string) {
    this.isLoading = true;
    
    // Llamada al servicio de ciudades usando el GUID
    this.cityService.get(id).subscribe({
      next: (data) => {
        this.city = data;
        this.isLoading = false;

        // Si está logueado, verificamos si ya calificó esta ciudad
        if (this.isAuthenticated) {
          this.checkUserRating(id);
        }
      },
      error: (err) => {
        console.error(err);
        this.errorMessage = 'No se pudo cargar la información de la ciudad.';
        this.isLoading = false;
      }
    });
  }

  // ✅ Corregido: destinationId es string
  checkUserRating(destinationId: string) {
    this.ratingService.getMyForDestination(destinationId).subscribe({
      next: (result) => {
        // Si result existe, el botón de votar se ocultará en el HTML
        this.userRating = result;
      },
      error: (err) => {
        console.error('Error al verificar calificación previa:', err);
      }
    });
  }

  submitRating() {
    if (this.selectedScore < 1 || !this.city) return;

    this.isRatingSubmitting = true;

    // ✅ Corregido: Preparamos el DTO con el GUID como string
    const input: CreateRatingDto = {
      destinationId: this.city.id, 
      score: this.selectedScore,
      comment: this.ratingComment
    };

    this.ratingService.create(input).subscribe({
      next: (result) => {
        this.userRating = result; 
        this.toaster.success('¡Gracias por tu calificación!');
        this.isRatingSubmitting = false;
      },
      error: (err) => {
        this.isRatingSubmitting = false;
        const msg = err.error?.error?.message || 'Error al enviar calificación';
        this.toaster.error(msg);
      }
    });
  }
}