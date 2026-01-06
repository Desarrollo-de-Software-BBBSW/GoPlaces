import type { EntityDto } from '@abp/ng.core';

export interface CreateUpdateDestinationDto {
  name?: string;
  country: string;
  population: number;
  imageUrl?: string;
  lastUpdatedDate?: string;
  latitude: number;
  longitude: number;
}

export interface DestinationDto extends EntityDto<string> {
  name?: string;
  country?: string;
  population: number;
  imageUrl?: string;
  lastUpdatedDate?: string;
  latitude: number;
  longitude: number;
}
