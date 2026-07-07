
export interface NotificationDto {
  id?: string;
  title?: string;
  message?: string;
  isRead: boolean;
  creationTime?: string;
}

export interface NotifyDestinationChangeInputDto {
  destinationId?: string;
  changeDescription?: string;
}
