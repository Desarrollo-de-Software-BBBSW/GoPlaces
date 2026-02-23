import type { EntityDto } from '@abp/ng.core';

export interface ChangePasswordInputDto {
  currentPassword: string;
  newPassword: string;
}

export interface LoginInputDto {
  userNameOrEmail: string;
  password: string;
}

export interface PublicUserProfileDto extends EntityDto<string> {
  userName?: string;
  name?: string;
  surname?: string;
  photoUrl?: string;
  preferences?: string;
}

export interface RegisterInputDto {
  userName: string;
  email: string;
  password: string;
}

export interface UserProfileDto extends EntityDto<string> {
  userName?: string;
  email: string;
  name?: string;
  surname?: string;
  photoUrl?: string;
}
