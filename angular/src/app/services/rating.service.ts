import { Injectable } from '@angular/core';
import { RestService, Rest } from '@abp/ng.core';
import { Observable } from 'rxjs';

export interface RatingDto {
  id: string;
  destinationId: string; 
  score: number;
  comment?: string;
  userId: string;
  creationTime?: string;
  userName?: string;
}

export interface CreateRatingDto {
  destinationId: string;
  score: number;
  comment?: string;
}

@Injectable({
  providedIn: 'root'
})
export class RatingService {
  apiName = 'Default'; 

  constructor(private restService: RestService) {}

  // 1. CREATE
  create = (input: CreateRatingDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, RatingDto>({
      method: 'POST',
      url: '/api/app/rating',
      body: input,
    },
    { apiName: this.apiName, ...config });

  // 2. GET BY DESTINATION
  getByDestination = (destinationId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, { items: RatingDto[] }>({
      method: 'GET',
      url: `/api/app/rating/by-destination/${destinationId}`,
    },
    { apiName: this.apiName, ...config });

  // 3. GET MY FOR DESTINATION
  getMyForDestination = (destinationId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, RatingDto>({
      method: 'GET',
      url: `/api/app/rating/my-for-destination/${destinationId}`, 
    },
    { apiName: this.apiName, ...config });

  // ✅ NUEVO: 4. UPDATE (Editar calificación)
  update = (id: string, input: CreateRatingDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, RatingDto>({
      method: 'PUT',
      url: `/api/app/rating/${id}`,
      body: input,
    },
    { apiName: this.apiName, ...config });

  // ✅ NUEVO: 5. DELETE (Eliminar calificación)
  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/rating/${id}`,
    },
    { apiName: this.apiName, ...config });
    
    // ✅ NUEVO: 6. GET AVERAGE RATING (Promedio de calificación)
  getAverageRating = (destinationId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, number>({
      method: 'GET',
      url: `/api/app/rating/average-rating/${destinationId}`,
    },
    { apiName: this.apiName, ...config });
}