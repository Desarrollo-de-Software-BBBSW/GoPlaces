import { Component, OnInit } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { finalize, switchMap, debounceTime, distinctUntilChanged, catchError, of } from 'rxjs';
import { CommonModule } from '@angular/common'; 

// Importación del Proxy
import { CitySearchResultDto, CityService, CitySearchRequestDto, CityDto } from 'src/app/proxy/cities';

@Component({
  standalone: true, 
  imports: [
    CommonModule,             
    ReactiveFormsModule      
    // HEMOS ELIMINADO ThemeSharedModule y Layouts para evitar el error de validación
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

  constructor(private cityAppService: CityService) { }

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
              console.error(error); // Útil para ver el error real en consola
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
}