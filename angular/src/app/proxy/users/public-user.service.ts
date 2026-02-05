import type { PublicUserProfileDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PublicUserService {
  apiName = 'Default';
  

  getByUserName = (userName: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PublicUserProfileDto>({
      method: 'GET',
      url: '/api/app/public-user/by-user-name',
      params: { userName },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
