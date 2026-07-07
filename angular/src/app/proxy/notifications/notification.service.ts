import type { NotificationDto, NotifyDestinationChangeInputDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  apiName = 'Default';
  

  changeReadState = (id: string, isRead: boolean, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/notification/${id}/change-read-state`,
      params: { isRead },
    },
    { apiName: this.apiName,...config });
  

  getMyNotifications = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, NotificationDto[]>({
      method: 'GET',
      url: '/api/app/notification/my-notifications',
    },
    { apiName: this.apiName,...config });
  

  markAllAsRead = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: '/api/app/notification/mark-all-as-read',
    },
    { apiName: this.apiName,...config });
  

  notifyDestinationChange = (input: NotifyDestinationChangeInputDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: '/api/app/notification/notify-destination-change',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
