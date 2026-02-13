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

  // 1. CREATE: Este suele ir en el body, así que lo dejamos igual (pero revisa que la URL sea correcta en swagger)
  create = (input: CreateRatingDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, RatingDto>({
      method: 'POST',
      url: '/api/app/rating',
      body: input,
    },
    { apiName: this.apiName, ...config });

  // 2. GET BY DESTINATION: Corregido para enviar ID en la RUTA
  getByDestination = (destinationId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, { items: RatingDto[] }>({
      method: 'GET',
      // 👇 CAMBIO CLAVE: Ponemos el ID dentro de la URL
      url: `/api/app/rating/by-destination/${destinationId}`,
      // params: { destinationId } 👈 BORRAMOS ESTO (ya no va como parámetro)
    },
    { apiName: this.apiName, ...config });

  // 3. GET MY FOR DESTINATION: Corregido para enviar ID en la RUTA (El que te da error 404)
  getMyForDestination = (destinationId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, RatingDto>({
      method: 'GET',
      // 👇 CAMBIO CLAVE: Ponemos el ID dentro de la URL
      url: `/api/app/rating/my-for-destination/${destinationId}`, 
      // params: { destinationId } 👈 BORRAMOS ESTO
    },
    { apiName: this.apiName, ...config });
}