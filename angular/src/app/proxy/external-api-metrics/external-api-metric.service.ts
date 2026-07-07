import type { ApiUsageMetricDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ExternalApiMetricService {
  apiName = 'Default';
  

  getMetrics = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ApiUsageMetricDto[]>({
      method: 'GET',
      url: '/api/app/external-api-metric/metrics',
    },
    { apiName: this.apiName,...config });
  

  logCall = (apiName: string, endpoint: string, durationMs: number, isSuccess: boolean, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: '/api/app/external-api-metric/log-call',
      params: { apiName, endpoint, durationMs, isSuccess },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
