import type { EntityDto } from '@abp/ng.core';

export interface EventDto extends EntityDto<string> {
  name?: string;
  description?: string;
  startDate?: string;
  venue?: string;
  city?: string;
  url?: string;
  ticketMasterId?: string;
  destinationId?: string;
}

export interface EventSearchRequestDto {
  city?: string;
  startDateFrom?: string;
  startDateTo?: string;
  destinationId?: string;
}

export interface EventSearchResultDto {
  events: EventDto[];
}
