import { RestService } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface CreateRatingDto {
  destinationId: number;
  score: number;
  comment?: string;
}

export interface RatingDto {
  destinationId: number;
  score: number;
  comment?: string;
  userId: string;
}

@Injectable({
  providedIn: 'root',
})
export class RatingService {
  apiName = 'Default';

  constructor(private restService: RestService) {}

  // POST: Crear calificación
  // Orden correcto: <Input, Output> -> <CreateRatingDto, RatingDto>
  create(input: CreateRatingDto): Observable<RatingDto> {
    return this.restService.request<CreateRatingDto, RatingDto>(
      {
        method: 'POST',
        url: '/api/app/rating',
        body: input, 
      },
      { apiName: this.apiName }
    );
  }

  // GET: Obtener mi calificación
  // Orden correcto: <Input, Output> -> <any, RatingDto>
  // Usamos 'any' en el primero porque un GET no tiene Input (Body).
  // GET: Obtener mi calificación
  getMyForDestination(destinationId: number): Observable<RatingDto> {
    return this.restService.request<any, RatingDto>(
      {
        method: 'GET',
        // 👇 CORRECCIÓN: Agregamos /${destinationId} al final de la URL
        url: `/api/app/rating/my-for-destination/${destinationId}`,
        // params: { destinationId }  <-- BORRAMOS ESTA LÍNEA (ya no hace falta)
      },
      { apiName: this.apiName }
    );
  }
}