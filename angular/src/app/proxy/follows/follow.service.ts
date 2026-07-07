import type { SaveOrRemoveInputDto, SavedDestinationDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class FollowService {
  apiName = 'Default';
  

  getMyFavorites = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, SavedDestinationDto[]>({
      method: 'GET',
      url: '/api/app/follow/my-favorites',
    },
    { apiName: this.apiName,...config });
  

  removeDestination = (input: SaveOrRemoveInputDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: '/api/app/follow/destination',
      params: { destinationId: input.destinationId },
    },
    { apiName: this.apiName,...config });
  

  saveDestination = (input: SaveOrRemoveInputDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SavedDestinationDto>({
      method: 'POST',
      url: '/api/app/follow/save-destination',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
