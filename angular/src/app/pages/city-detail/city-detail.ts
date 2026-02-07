import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms'; // 👈 IMPORTANTE: Para usar [(ngModel)] en el textarea
import { ActivatedRoute, RouterModule } from '@angular/router';
import { CityService, CityDto } from 'src/app/proxy/cities';
import { AuthService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';

// Importamos el servicio que creamos en el paso anterior
// Asegúrate de que la ruta sea correcta según donde creaste la carpeta 'services'
import { RatingService, RatingDto, CreateRatingDto } from '../../services/rating.service';

@Component({
  selector: 'app-city-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule], // 👈 Agregamos FormsModule aquí
  templateUrl: './city-detail.html',
  styleUrls: ['./city-detail.scss']
})
export class CityDetailComponent implements OnInit {
  // Datos de la ciudad
  city: CityDto | null = null;
  isLoading = true;
  errorMessage = '';

  // Datos para el Rating
  userRating: RatingDto | null = null; // Guardará el voto si ya existe
  selectedScore = 0;                   // Estrellas que marca el usuario
  hoverScore = 0;                      // Estrellas al pasar el mouse
  ratingComment = '';                  // Comentario del usuario
  isRatingSubmitting = false;          // Spinner del botón enviar
  isAuthenticated = false;             // Estado del login

  constructor(
    private route: ActivatedRoute,
    private cityService: CityService,
    private ratingService: RatingService, // 👈 Inyectamos nuestro servicio de ratings
    private authService: AuthService,     // 👈 Para verificar login
    private toaster: ToasterService       // 👈 Para mensajes de éxito/error
  ) {}

  ngOnInit(): void {
    // 1. Verificamos si el usuario está logueado
    this.isAuthenticated = this.authService.isAuthenticated;

    // 2. Capturamos el ID de la URL
    this.route.params.subscribe(params => {
      const id = params['id'];
      
      if (id) {
        this.loadCity(Number(id));
      }
    });
  }

  loadCity(id: number) {
    this.isLoading = true;
    
    this.cityService.get(id).subscribe({
      next: (data) => {
        this.city = data;
        this.isLoading = false;

        // Una vez que tenemos la ciudad, si el usuario está logueado,
        // verificamos si ya la calificó anteriormente.
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

  // --- LÓGICA DE RATING ---

  checkUserRating(destinationId: number) {
    this.ratingService.getMyForDestination(destinationId).subscribe({
      next: (result) => {
        // Si result es null, no ha votado. Si tiene datos, ya votó.
        this.userRating = result;
      },
      error: (err) => console.error('Error cargando rating:', err)
    });
  }

  submitRating() {
    // Validaciones básicas
    if (this.selectedScore < 1 || !this.city) return;

    this.isRatingSubmitting = true;

    // Preparamos el objeto para enviar al backend (int Id)
    const input: CreateRatingDto = {
      destinationId: this.city.id, // Esto ya es un número (int) gracias a tu DTO
      score: this.selectedScore,
      comment: this.ratingComment
    };

    this.ratingService.create(input).subscribe({
      next: (result) => {
        this.userRating = result; // Actualizamos la vista para mostrar "Ya votaste"
        this.toaster.success('¡Gracias por tu calificación!');
        this.isRatingSubmitting = false;
      },
      error: (err) => {
        this.isRatingSubmitting = false;
        // Mostramos el error que venga del backend (ej: "Ya votaste")
        const msg = err.error?.error?.message || 'Error al enviar calificación';
        this.toaster.error(msg);
      }
    });
  }
}