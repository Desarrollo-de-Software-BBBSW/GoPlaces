import { Routes } from '@angular/router';
// Importamos los Guards de ABP para proteger las rutas (Autenticación y Permisos)
import { AuthGuard, PermissionGuard } from '@abp/ng.core';

// Importamos tu nuevo componente
import { CitiesSearchComponent } from './pages/cities-search/cities-search';

import { PublicProfileComponent } from './public-profile/public-profile';
export const APP_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () => import('./home/home.component').then(m => m.HomeComponent),
  },
  {
    path: 'account',
    loadChildren: () => import('@abp/ng.account').then(m => m.AccountModule.forLazy()),
  },
  {
    path: 'identity',
    loadChildren: () => import('@abp/ng.identity').then(m => m.IdentityModule.forLazy()),
  },
  // {
  //   path: 'tenant-management',
  //   loadChildren: () =>
  //     import('@abp/ng.tenant-management').then(m => m.TenantManagementModule.forLazy()),
  // },
  {
    path: 'setting-management',
    loadChildren: () =>
      import('@abp/ng.setting-management').then(m => m.SettingManagementModule.forLazy()),
  },
  
  // --- NUEVA RUTA PARA EL TP 8 ---
  {
    path: 'cities/search', // La URL será: http://localhost:4200/cities/search
    component: CitiesSearchComponent,
    canActivate: [AuthGuard, PermissionGuard] // Protege la ruta: solo usuarios logueados
  },

  {
  path: 'register',
  loadComponent: () => import('./pages/register/register').then(m => m.RegisterComponent)
},
{
  path: 'login',
  loadComponent: () => import('./pages/login/login').then(m => m.LoginComponent)
},

{
  path: 'my-profile',
  loadComponent: () => import('./pages/my-profile/my-profile').then(m => m.MyProfileComponent)
},
{
    path: 'profile/:userName', // <--- La parte mágica ":userName"
    component: PublicProfileComponent
},
  // -------------------------------
];