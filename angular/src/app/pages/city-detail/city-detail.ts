import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router'; // RouterModule para el botón 'Volver'
import { CityService, CityDto } from 'src/app/proxy/cities'; // Asegúrate que la ruta sea correcta

@Component({
  selector: 'app-city-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './city-detail.html',
  styleUrls: ['./city-detail.scss'] // Crearemos este archivo vacío o con estilos simples
})
export class CityDetailComponent implements OnInit {
  city: CityDto | null = null;
  isLoading = true;
  errorMessage = '';

  constructor(
    private route: ActivatedRoute,
    private cityService: CityService
  ) {}

  ngOnInit(): void {
    // Capturamos el ID de la URL
    this.route.params.subscribe(params => {
      const id = params['id'];
      
      if (id) {
        // Convertimos el string de la URL a número con Number() o el + 
        this.loadCity(Number(id));
      }
    });
  }

  loadCity(id: number) {
    this.isLoading = true;
    
    // Llamamos al método nuevo que generó el proxy (puede llamarse get o getAsync)
    this.cityService.get(id).subscribe({
      next: (data) => {
        this.city = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error(err);
        this.errorMessage = 'No se pudo cargar la información de la ciudad.';
        this.isLoading = false;
      }
    });
  }
}