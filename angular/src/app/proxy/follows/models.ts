import type { EntityDto } from '@abp/ng.core';

export interface SaveOrRemoveInputDto {
  destinationId?: string;
}

export interface SavedDestinationDto extends EntityDto<string> {
  destinationId?: string;
  creationTime?: string;
}
