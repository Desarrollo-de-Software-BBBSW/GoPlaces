import type { CityDto, CitySearchRequestDto, CitySearchResultDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class CityService {
  apiName = 'Default';

  constructor(private restService: RestService) {}

  // ✅ Método GET (Ya corregido para recibir string/GUID)
  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CityDto>({
      method: 'GET',
      url: `/api/app/city/${id}`,
    },
    { apiName: this.apiName, ...config });

  // ✅ Método Search (Buscador)
  searchCities = (request: CitySearchRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CitySearchResultDto>({
      method: 'POST',
      url: '/api/app/city/search-cities',
      body: request,
    },
    { apiName: this.apiName, ...config });

  // ✅ NUEVO MÉTODO: Este es el que te faltaba para "Popular Destinations"
  getPopularCities = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, CityDto[]>({
      method: 'GET',
      url: '/api/app/city/popular-cities',
    },
    { apiName: this.apiName, ...config });
}