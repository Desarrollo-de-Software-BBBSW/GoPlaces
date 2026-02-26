import { Component, OnInit } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { finalize, switchMap, debounceTime, distinctUntilChanged, catchError, of } from 'rxjs';
import { CommonModule } from '@angular/common'; 
import { RouterModule } from '@angular/router'; // 👈 1. IMPORTA ESTO

// Importación del Proxy de Ciudades
import { CitySearchResultDto, CityService, CitySearchRequestDto, CityDto } from 'src/app/proxy/cities';
import { DestinationService } from 'src/app/proxy/destinations/destination.service';
import type { CreateUpdateDestinationDto } from 'src/app/proxy/destinations/models'; 
import { ToasterService } from '@abp/ng.theme.shared';

@Component({
  standalone: true, 
  imports: [
    CommonModule,             
    ReactiveFormsModule,
    RouterModule // 👈 2. AGRÉGALO AL ARREGLO
  ],
  selector: 'app-cities-search',
  templateUrl: './cities-search.html', 
  styleUrls: ['./cities-search.scss'], 
})

export class CitiesSearchComponent implements OnInit {
  
  searchControl = new FormControl(''); 
  cities: CityDto[] = [];      
  isLoading = false;           
  errorMessage: string | null = null; 

  constructor(
    private cityAppService: CityService,
    private destinationService: DestinationService,
    private toaster: ToasterService // 👈 2. INYECTAMOS EL TOASTER AQUÍ
  ) { }

  ngOnInit(): void {
    this.searchControl.valueChanges
      .pipe(
        debounceTime(300), 
        distinctUntilChanged(), 
        switchMap(searchTerm => {
          const query = (searchTerm || '').trim();

          if (query.length < 3) {
            this.cities = [];
            this.errorMessage = null;
            return of(null);
          }

          this.isLoading = true;
          this.errorMessage = null; 

          const input: CitySearchRequestDto = {
            partialName: query
          };
          
          return this.cityAppService.searchCities(input).pipe(
            finalize(() => this.isLoading = false), 
            catchError(error => {
              console.error(error); 
              this.errorMessage = 'Ocurrió un error al buscar ciudades.';
              this.cities = [];
              return of(null); 
            })
          );
        })
      )
      .subscribe(
        (result: CitySearchResultDto | null) => {
          if (result && result.cities) {
            this.cities = result.cities;
          } else {
            this.cities = [];
          }
        }
      );
  }

  // 👇 3. FUNCIÓN ACTUALIZADA CON TOASTER
  saveCity(city: CityDto): void {
    if (!city) return;

    const input: CreateUpdateDestinationDto = {
      name: city.name || '',
      country: city.country || '',
      population: 0,
      latitude: 0,
      longitude: 0
    };

    this.destinationService.create(input).subscribe({
      next: (result) => {
        // ✅ ÉXITO: Mensaje verde flotante
        this.toaster.success(`Has guardado ${result.name} en tus destinos`, '¡Guardado con éxito!');
      },
      error: (err) => {
        console.error(err);
        // ❌ ERROR: Mensaje rojo flotante
        this.toaster.error('No se pudo guardar el destino. Quizás ya existe.', 'Error');
      }
    });
  }
}