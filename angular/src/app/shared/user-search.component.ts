import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-user-search',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="d-flex align-items-center" style="gap: 6px;">
      <input
        type="text"
        class="form-control form-control-sm"
        placeholder="Buscar usuario..."
        [(ngModel)]="userName"
        (keyup.enter)="search()"
        style="width: 180px;"
      />
      <button class="btn btn-sm btn-primary" (click)="search()">
        <i class="fa fa-search"></i>
      </button>
    </div>
  `,
})
export class UserSearchComponent {
  userName = '';

  constructor(private router: Router) {}

  search() {
    const trimmed = this.userName.trim();
    if (trimmed) {
      this.router.navigate(['profile', trimmed]);
      this.userName = '';
    }
  }
}