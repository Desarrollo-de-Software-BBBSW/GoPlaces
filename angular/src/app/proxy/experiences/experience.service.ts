import type { CreateUpdateExperienceDto, ExperienceDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto, PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ExperienceService {
  apiName = 'Default';
  

  create = (input: CreateUpdateExperienceDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExperienceDto>({
      method: 'POST',
      url: '/api/app/experience',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/experience/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExperienceDto>({
      method: 'GET',
      url: `/api/app/experience/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getExperiencesByRating = (rating: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ExperienceDto>>({
      method: 'GET',
      url: '/api/app/experience/experiences-by-rating',
      params: { rating },
    },
    { apiName: this.apiName,...config });
  

  getList = (input: PagedAndSortedResultRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ExperienceDto>>({
      method: 'GET',
      url: '/api/app/experience',
      params: { sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getOtherUsersExperiences = (destinationId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ExperienceDto>>({
      method: 'GET',
      url: `/api/app/experience/other-users-experiences/${destinationId}`,
    },
    { apiName: this.apiName,...config });
  

  searchExperiencesByKeyword = (keyword: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ExperienceDto>>({
      method: 'POST',
      url: '/api/app/experience/search-experiences-by-keyword',
      params: { keyword },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateExperienceDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExperienceDto>({
      method: 'PUT',
      url: `/api/app/experience/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
