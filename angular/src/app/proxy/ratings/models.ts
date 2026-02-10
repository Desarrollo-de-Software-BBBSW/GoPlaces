import type { EntityDto } from '@abp/ng.core';

export interface CreateRatingDto {
  destinationId: number;
  score: number;
  comment?: string;
}

export interface RatingDto extends EntityDto<string> {
  destinationId: number;
  score: number;
  comment?: string;
  userId?: string;
}
