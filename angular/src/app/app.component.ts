import { Component } from '@angular/core';
import { DynamicLayoutComponent } from '@abp/ng.core';
import { LoaderBarComponent } from '@abp/ng.theme.shared';

// 👇 1. Importamos el servicio de diseño de LeptonX
import { LayoutService } from '@volo/ngx-lepton-x.core';

@Component({
  selector: 'app-root',
  template: `
    <abp-loader-bar />
    <abp-dynamic-layout />
  `,
  imports: [LoaderBarComponent, DynamicLayoutComponent],
})
export class AppComponent {
  
  // 👇 2. Inyectamos el servicio en el constructor
  constructor(private layoutService: LayoutService) {
    // 👇 3. Esta línea mágica fuerza a que la barra inicie colapsada
    this.layoutService.addClass('hover-trigger');
  }
  
}