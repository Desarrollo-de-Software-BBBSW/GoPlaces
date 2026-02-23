import type { AuditedEntityDto } from '@abp/ng.core';

export interface CreateUpdateExperienceDto {
  destinationId: string;
  title: string;
  description?: string;
  price: number;
  date: string;
  rating: string;
}

export interface ExperienceDto extends AuditedEntityDto<string> {
  destinationId?: string;
  title?: string;
  description?: string;
  price: number;
  date?: string;
  rating?: string;
}
