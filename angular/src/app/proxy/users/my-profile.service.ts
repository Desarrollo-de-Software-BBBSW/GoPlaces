import type { ChangePasswordInputDto, UserProfileDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class MyProfileService {
  apiName = 'Default';
  

  changePassword = (input: ChangePasswordInputDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: '/api/app/my-profile/change-password',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: '/api/app/my-profile',
    },
    { apiName: this.apiName,...config });
  

  get = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, UserProfileDto>({
      method: 'GET',
      url: '/api/app/my-profile',
    },
    { apiName: this.apiName,...config });
  

  update = (input: UserProfileDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: '/api/app/my-profile',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
