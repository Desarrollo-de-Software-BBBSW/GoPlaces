import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CityService, CityDto } from 'src/app/proxy/cities'; // 👈 Importamos el servicio y el DTO

@Component({
  selector: 'app-popular-destinations',
  standalone: true,
  imports: [CommonModule, RouterModule], // Necesarios para *ngFor y routerLink
  templateUrl: './popular-destinations.html',
  styleUrls: ['./popular-destinations.scss']
})
export class PopularDestinationsComponent implements OnInit {

  // Aquí guardamos las ciudades que vienen del servidor
  popularCities: CityDto[] = [];
  isLoading = true;

  constructor(private cityService: CityService) {}

  ngOnInit(): void {
    // Apenas nace el componente, pedimos los datos
    this.cityService.getPopularCities().subscribe({
      next: (data) => {
        this.popularCities = data;
        this.isLoading = false;
        console.log('Populares cargadas:', data);
      },
      error: (err) => {
        console.error('Error cargando populares:', err);
        this.isLoading = false;
      }
    });
  }
}