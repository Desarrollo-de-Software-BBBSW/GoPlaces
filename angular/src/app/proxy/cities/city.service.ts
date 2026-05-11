import type { CityDto, CitySearchRequestDto, CitySearchResultDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class CityService {
  apiName = 'Default';
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CityDto>({
      method: 'GET',
      url: `/api/app/city/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getPopularCities = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, CityDto[]>({
      method: 'GET',
      url: '/api/app/city/popular-cities',
    },
    { apiName: this.apiName,...config });
  

  searchCities = (request: CitySearchRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CitySearchResultDto>({
      method: 'POST',
      url: '/api/app/city/search-cities',
      body: request,
    },
    { apiName: this.apiName,...config });

  getList = (request: CitySearchRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CitySearchResultDto>({
      method: 'GET',
      url: '/api/app/city',
      params: {
        partialName: request.partialName,
        countryCode: request.countryCode,
        regionId: request.regionId,
        minPopulation: request.minPopulation,
      },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
