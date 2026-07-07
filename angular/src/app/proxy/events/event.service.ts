import type { EventDto, EventSearchRequestDto, EventSearchResultDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class EventService {
  apiName = 'Default';
  

  getEventsByDestination = (destinationId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, EventDto[]>({
      method: 'GET',
      url: `/api/app/event/events-by-destination/${destinationId}`,
    },
    { apiName: this.apiName,...config });
  

  searchEventsByCity = (request: EventSearchRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, EventSearchResultDto>({
      method: 'POST',
      url: '/api/app/event/search-events-by-city',
      body: request,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
