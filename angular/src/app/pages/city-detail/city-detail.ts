import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms'; 
import { ActivatedRoute, RouterModule } from '@angular/router';
import { CityService, CityDto } from 'src/app/proxy/cities';
import { AuthService } from '@abp/ng.core';
import { ToasterService, ConfirmationService, Confirmation } from '@abp/ng.theme.shared';

// Asegúrate de que la ruta de tu servicio sea la correcta
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
  isEditing = false; 

  // ✅ NUEVO: Variable para guardar el promedio
  averageRating: number = 0;

  constructor(
    private route: ActivatedRoute,
    private cityService: CityService,
    private ratingService: RatingService, 
    private authService: AuthService,     
    private toaster: ToasterService,
    private confirmation: ConfirmationService 
  ) {}

  ngOnInit(): void {
    this.isAuthenticated = this.authService.isAuthenticated;

    this.route.params.subscribe(params => {
      const id = params['id']; 
      if (id) {
        this.loadCity(id); 
      }
    });
  }

  loadCity(id: string) {
    this.isLoading = true;
    this.cityService.get(id).subscribe({
      next: (data) => {
        this.city = data;
        this.isLoading = false;
        
        // ✅ NUEVO: Cargamos el promedio global siempre
        this.loadAverageRating(id);

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

  // ✅ NUEVO: Método para consultar el promedio al backend
  loadAverageRating(destinationId: string) {
    // Nota: Si abp generate-proxy le puso otro nombre (ej: getAverageRatingAsync), cámbialo aquí.
    this.ratingService.getAverageRating(destinationId).subscribe({
      next: (average) => {
        this.averageRating = average;
      },
      error: (err) => {
        console.error('Error al cargar el promedio global:', err);
      }
    });
  }

  checkUserRating(destinationId: string) {
    this.ratingService.getMyForDestination(destinationId).subscribe({
      next: (result) => {
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

    const input: CreateRatingDto = {
      destinationId: this.city.id, 
      score: this.selectedScore,
      comment: this.ratingComment
    };

    if (this.isEditing && this.userRating) {
      this.ratingService.update(this.userRating.id, input).subscribe({
        next: (result) => {
          this.userRating = result; 
          this.isEditing = false;
          this.toaster.success('¡Calificación actualizada!');
          this.isRatingSubmitting = false;
          
          // ✅ NUEVO: Refrescamos el promedio tras editar
          this.loadAverageRating(this.city!.id);
        },
        error: (err) => {
          this.isRatingSubmitting = false;
          const msg = err.error?.error?.message || 'Error al actualizar calificación';
          this.toaster.error(msg);
        }
      });
    } else {
      this.ratingService.create(input).subscribe({
        next: (result) => {
          this.userRating = result; 
          this.toaster.success('¡Gracias por tu calificación!');
          this.isRatingSubmitting = false;
          
          // ✅ NUEVO: Refrescamos el promedio tras calificar
          this.loadAverageRating(this.city!.id);
        },
        error: (err) => {
          this.isRatingSubmitting = false;
          const msg = err.error?.error?.message || 'Error al enviar calificación';
          this.toaster.error(msg);
        }
      });
    }
  }

  enableEdit() {
    if (this.userRating) {
      this.isEditing = true;
      this.selectedScore = this.userRating.score;
      this.ratingComment = this.userRating.comment || '';
    }
  }

  cancelEdit() {
    this.isEditing = false;
    this.selectedScore = 0;
    this.ratingComment = '';
  }

  deleteRating() {
    if (!this.userRating) return;

    this.confirmation.warn(
      '¿Estás seguro de que deseas eliminar tu calificación?',
      'Eliminar Calificación'
    ).subscribe((status: Confirmation.Status) => {
      if (status === Confirmation.Status.confirm) {
        this.ratingService.delete(this.userRating!.id).subscribe({
          next: () => {
            this.userRating = null;
            this.isEditing = false;
            this.selectedScore = 0;
            this.ratingComment = '';
            this.toaster.info('Calificación eliminada.');
            
            // ✅ NUEVO: Refrescamos el promedio tras borrar
            this.loadAverageRating(this.city!.id);
          },
          error: (err) => {
            const msg = err.error?.error?.message || 'Error al eliminar calificación';
            this.toaster.error(msg);
          }
        });
      }
    });
  }
}