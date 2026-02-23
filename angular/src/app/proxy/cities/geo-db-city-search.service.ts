import type { CityDto, CitySearchRequestDto, CitySearchResultDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class GeoDbCitySearchService {
  apiName = 'Default';
  

  getById = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CityDto>({
      method: 'GET',
      url: `/api/app/geo-db-city-search/${id}/by-id`,
    },
    { apiName: this.apiName,...config });
  

  searchCities = (request: CitySearchRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CitySearchResultDto>({
      method: 'POST',
      url: '/api/app/geo-db-city-search/search-cities',
      body: request,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
