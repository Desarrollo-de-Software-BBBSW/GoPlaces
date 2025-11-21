import type { EntityDto } from '@abp/ng.core';

export interface CreateRatingDto {
  destinationId: string;
  score: number;
  comment?: string;
}

export interface RatingDto extends EntityDto<string> {
  destinationId?: string;
  score: number;
  comment?: string;
  userId?: string;
}
