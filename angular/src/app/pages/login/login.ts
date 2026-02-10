import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ToasterService } from '@abp/ng.theme.shared';
import { Router } from '@angular/router';
import { AuthFlowService } from 'src/app/services/auth-flow.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrls: ['./login.scss']
})
export class LoginComponent {
  form: FormGroup;
  isBusy = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthFlowService,
    private toaster: ToasterService,
    private router: Router
  ) {
    this.buildForm();
  }

  buildForm() {
    this.form = this.fb.group({
      userNameOrEmail: ['', [Validators.required]],
      password: ['', [Validators.required]]
    });
  }

  login() {
    if (this.form.invalid) return;

    this.isBusy = true;
    const { userNameOrEmail, password } = this.form.value;

    this.authService.login(userNameOrEmail, password).subscribe({
      next: (res) => {
        this.toaster.success('¡Bienvenido de vuelta!', 'Sesión Iniciada');
        
        // 👇 AQUÍ ESTÁ LA SOLUCIÓN
        // Esperamos medio segundo y FORZAMOS una recarga completa de la página.
        // Esto obliga a ABP a leer el token nuevo y descargar tu perfil de usuario.
        setTimeout(() => {
          window.location.href = '/'; 
        }, 500);
      },
      error: (err) => {
        console.error('Error de login:', err);
        const msg = err.error?.error_description || 'Usuario o contraseña incorrectos';
        this.toaster.error(msg, 'Error');
        this.isBusy = false;
      }
    });
  }
}