
export interface CityDto {
  // 👇 CAMBIO: Cambia 'number' por 'string'
  id: string; 
  name: string;
  country: string;
  rating: number;
}

export interface CitySearchRequestDto {
  partialName?: string;
}

export interface CitySearchResultDto {
  cities: CityDto[];
}
