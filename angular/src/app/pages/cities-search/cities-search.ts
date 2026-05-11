import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { CitySearchResultDto, CityService, CitySearchRequestDto, CityDto } from 'src/app/proxy/cities';
import { DestinationService } from 'src/app/proxy/destinations/destination.service';
import type { CreateUpdateDestinationDto } from 'src/app/proxy/destinations/models';
import { ToasterService } from '@abp/ng.theme.shared';

@Component({
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
  ],
  selector: 'app-cities-search',
  templateUrl: './cities-search.html',
  styleUrls: ['./cities-search.scss'],
})
export class CitiesSearchComponent implements OnInit {

  searchForm: FormGroup;
  cities: CityDto[] = [];
  isLoading = false;
  errorMessage: string | null = null;

  constructor(
    private fb: FormBuilder,
    private cityService: CityService,
    private destinationService: DestinationService,
    private toaster: ToasterService,
  ) {
    this.searchForm = this.fb.group({
      partialName: [''],
      countryCode: [''],
      regionId: [''],
      minPopulation: [null],
    });
  }

  ngOnInit(): void {}

  search(): void {
    const { partialName, countryCode, regionId, minPopulation } = this.searchForm.value;

    if (!partialName || partialName.trim().length < 3) {
      this.errorMessage = 'Ingresa al menos 3 caracteres para el nombre de la ciudad.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;
    this.cities = [];

    const request: CitySearchRequestDto = {
      partialName: partialName.trim(),
      countryCode: countryCode?.trim() || undefined,
      regionId: regionId?.trim() || undefined,
      minPopulation: minPopulation != null && minPopulation !== '' ? Number(minPopulation) : undefined,
    };

    this.cityService.searchCities(request).subscribe({
      next: (result: CitySearchResultDto) => {
        this.cities = result?.cities ?? [];
        this.isLoading = false;
      },
      error: (err) => {
        console.error(err);
        this.errorMessage = 'Ocurrió un error al buscar ciudades.';
        this.cities = [];
        this.isLoading = false;
      },
    });
  }

  saveCity(city: CityDto): void {
    if (!city) return;

    const input: CreateUpdateDestinationDto = {
      name: city.name || '',
      country: city.country || '',
      population: 0,
      latitude: 0,
      longitude: 0,
    };

    this.destinationService.create(input).subscribe({
      next: (result) => {
        this.toaster.success(`Has guardado ${result.name} en tus destinos`, '¡Guardado con éxito!');
      },
      error: (err) => {
        console.error(err);
        this.toaster.error('No se pudo guardar el destino. Quizás ya existe.', 'Error');
      },
    });
  }
}
