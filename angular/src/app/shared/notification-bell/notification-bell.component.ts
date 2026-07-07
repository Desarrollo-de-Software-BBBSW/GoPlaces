import { ChangeDetectorRef, Component, ElementRef, HostListener, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription, interval } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { ToasterService } from '@abp/ng.theme.shared';

import { NotificationService } from 'src/app/proxy/notifications/notification.service';
import type { NotificationDto } from 'src/app/proxy/notifications/models';
import { RelativeTimePipe } from '../relative-time.pipe';

const POLL_INTERVAL_MS = 45000;
const MAX_VISIBLE_NOTIFICATIONS = 15;

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [CommonModule, RelativeTimePipe],
  templateUrl: './notification-bell.component.html',
  styleUrls: ['./notification-bell.component.scss'],
})
export class NotificationBellComponent implements OnInit, OnDestroy {
  notifications: NotificationDto[] = [];
  unreadCount = 0;
  isOpen = false;
  isLoading = false;

  private pollSubscription?: Subscription;

  constructor(
    private notificationService: NotificationService,
    private toaster: ToasterService,
    private elementRef: ElementRef,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.loadNotifications();

    this.pollSubscription = interval(POLL_INTERVAL_MS)
      .pipe(switchMap(() => this.notificationService.getMyNotifications()))
      .subscribe({
        next: (notifications) => {
          this.applyNotifications(notifications);
          this.cdr.markForCheck();
        },
        error: (err) => {
          console.error(err);
          this.toaster.error('No se pudieron actualizar las notificaciones.', 'Error');
          this.cdr.markForCheck();
        },
      });
  }

  ngOnDestroy(): void {
    this.pollSubscription?.unsubscribe();
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (this.isOpen && !this.elementRef.nativeElement.contains(event.target)) {
      this.isOpen = false;
    }
  }

  get visibleNotifications(): NotificationDto[] {
    return this.notifications.slice(0, MAX_VISIBLE_NOTIFICATIONS);
  }

  toggle(): void {
    this.isOpen = !this.isOpen;
    if (this.isOpen) {
      this.loadNotifications();
    }
  }

  loadNotifications(): void {
    this.isLoading = true;
    this.notificationService.getMyNotifications().subscribe({
      next: (notifications) => {
        this.applyNotifications(notifications);
        this.isLoading = false;
      },
      error: (err) => {
        console.error(err);
        this.toaster.error('No se pudieron cargar las notificaciones.', 'Error');
        this.isLoading = false;
      },
    });
  }

  markAsRead(notification: NotificationDto): void {
    if (!notification.id || notification.isRead) return;

    this.notificationService.changeReadState(notification.id, true).subscribe({
      next: () => {
        notification.isRead = true;
        this.recomputeUnreadCount();
      },
      error: (err) => {
        console.error(err);
        this.toaster.error('No se pudo marcar la notificación como leída.', 'Error');
      },
    });
  }

  markAllAsRead(): void {
    if (this.unreadCount === 0) return;

    this.notificationService.markAllAsRead().subscribe({
      next: () => {
        this.notifications.forEach((n) => (n.isRead = true));
        this.unreadCount = 0;
        this.toaster.success('Todas las notificaciones fueron marcadas como leídas.', '¡Listo!');
      },
      error: (err) => {
        console.error(err);
        this.toaster.error('No se pudieron marcar las notificaciones como leídas.', 'Error');
      },
    });
  }

  private applyNotifications(notifications: NotificationDto[]): void {
    this.notifications = notifications ?? [];
    this.recomputeUnreadCount();
  }

  private recomputeUnreadCount(): void {
    this.unreadCount = this.notifications.filter((n) => !n.isRead).length;
  }
}
