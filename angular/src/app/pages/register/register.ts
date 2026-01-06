import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ToasterService } from '@abp/ng.theme.shared';

// 👇 CORRECCIÓN BASADA EN TUS CAPTURAS:
// El archivo se llama 'register.service.ts', así que la clase es 'RegisterService'.
// Los modelos suelen estar en 'models.ts' pero se exportan a través del index de la carpeta.
import { RegisterService, RegisterInputDto } from 'src/app/proxy/users';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './register.html',
  styleUrls: ['./register.scss']
})
export class RegisterComponent {
  form: FormGroup;
  isSaving = false;

  constructor(
    private fb: FormBuilder,
    private registerService: RegisterService, // 👈 Nombre corregido aquí también
    private toaster: ToasterService
  ) {
    this.buildForm();
  }

  buildForm() {
    this.form = this.fb.group({
      userName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  save() {
    if (this.form.invalid) {
      return;
    }

    this.isSaving = true;

    const input = this.form.value as RegisterInputDto;

    // Nota: Si el generador usó 'registerAsync' o simplemente 'register',
    // VS Code te sugerirá el correcto al escribir el punto. 
    // Por defecto en ABP suele ser el nombre del método en C# (RegisterAsync) o simplificado (register).
    this.registerService.register(input).subscribe({
      next: () => {
        this.toaster.success('Usuario registrado correctamente', 'Éxito');
        this.isSaving = false;
        this.form.reset();
      },
      error: (err) => {
        this.toaster.error(err.error?.error?.message || 'Ocurrió un error al registrar', 'Error');
        this.isSaving = false;
      }
    });
  }
}