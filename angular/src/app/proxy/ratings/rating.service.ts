import type { CreateRatingDto, RatingDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class RatingService {
  apiName = 'Default';
  

  create = (input: CreateRatingDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, RatingDto>({
      method: 'POST',
      url: '/api/app/rating',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  getByDestination = (destinationId: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<RatingDto>>({
      method: 'GET',
      url: `/api/app/rating/by-destination/${destinationId}`,
    },
    { apiName: this.apiName,...config });
  

  getMyForDestination = (destinationId: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, RatingDto>({
      method: 'GET',
      url: `/api/app/rating/my-for-destination/${destinationId}`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
