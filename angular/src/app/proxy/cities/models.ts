import type { EntityDto } from '@abp/ng.core';

export interface CityDto extends EntityDto<string> {
  name?: string;
  country?: string;
  rating: number;
}

export interface CitySearchRequestDto {
  partialName?: string;
}

export interface CitySearchResultDto {
  cities: CityDto[];
}
