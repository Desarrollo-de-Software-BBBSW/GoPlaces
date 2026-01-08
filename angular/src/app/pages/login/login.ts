import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ToasterService } from '@abp/ng.theme.shared';
import { Router } from '@angular/router'; // Para redirigir al Home después del login

// 👇 IMPORTANTE: Revisa en tu carpeta 'proxy/users' cómo se llaman exactamente estos archivos.
// Si tu servicio era IMyLoginAppService, puede que se llame 'MyLoginService'.
// Ajusta el nombre del import si VS Code te marca error.
import { LoginService } from 'src/app/proxy/users'; 
import { LoginInputDto } from 'src/app/proxy/users';

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
    private loginService: LoginService, // Inyectamos el servicio del Proxy
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

    const input = this.form.value as LoginInputDto;

    this.loginService.login(input).subscribe({
      next: (result) => {
        // Tu servicio devuelve un booleano (true/false) según lo que programamos en el back
        if (result) {
          this.toaster.success('¡Bienvenido de vuelta!', 'Login Exitoso');
          // Redirigir al inicio después de 1 segundo
          setTimeout(() => {
            this.router.navigate(['/']); 
          }, 1000);
        }
        this.isBusy = false;
      },
      error: (err) => {
        this.toaster.error(err.error?.error?.message || 'Usuario o contraseña incorrectos', 'Error');
        this.isBusy = false;
      }
    });
  }
}