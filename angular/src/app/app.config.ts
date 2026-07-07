import { provideAbpCore, withOptions } from '@abp/ng.core';
import { provideAbpOAuth } from '@abp/ng.oauth';
import { provideSettingManagementConfig } from '@abp/ng.setting-management/config';
import { provideFeatureManagementConfig } from '@abp/ng.feature-management';
import { provideAbpThemeShared } from '@abp/ng.theme.shared';
import { provideIdentityConfig } from '@abp/ng.identity/config';
import { provideAccountConfig } from '@abp/ng.account/config';
import { registerLocale } from '@abp/ng.core/locale';
import { provideThemeLeptonX } from '@abp/ng.theme.lepton-x';
import { provideSideMenuLayout } from '@abp/ng.theme.lepton-x/layouts';
import { provideLogo, withEnvironmentOptions } from "@volo/ngx-lepton-x.core";
import { ApplicationConfig, APP_INITIALIZER } from '@angular/core'; // 👈 agregá APP_INITIALIZER
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { environment } from '../environments/environment';
import { APP_ROUTES } from './app.routes';
import { APP_ROUTE_PROVIDER } from './route.provider';
import { authInterceptor } from './auth.interceptor';
import { NavItemsService } from '@abp/ng.theme.shared'; // 👈 agregá esto
import { UserSearchComponent } from './shared/user-search.component';
import { NotificationBellComponent } from './shared/notification-bell/notification-bell.component';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(APP_ROUTES),
    APP_ROUTE_PROVIDER,
    provideAnimations(),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideAbpCore(
      withOptions({
        environment,
        registerLocaleFn: registerLocale(),
      }),
    ),
    provideAbpOAuth(),
    provideIdentityConfig(),
    provideSettingManagementConfig(),
    provideFeatureManagementConfig(),
    provideThemeLeptonX(),
    provideSideMenuLayout(),
    provideLogo(withEnvironmentOptions(environment)),
    provideAccountConfig(),
    provideAbpThemeShared(),

    // 👇 Agregá esto al final
    {
      provide: APP_INITIALIZER,
      useFactory: (navItems: NavItemsService) => () => {
        navItems.addItems([
          {
            id: 'user-search',
            order: 1,
            component: UserSearchComponent,
          },
          {
            id: 'notification-bell',
            order: 2,
            component: NotificationBellComponent,
          }
        ]);
      },
      deps: [NavItemsService],
      multi: true,
    }
  ]
};