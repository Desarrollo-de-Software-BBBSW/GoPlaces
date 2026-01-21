import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ToasterService } from '@abp/ng.theme.shared';

// Importamos el servicio y ambos DTOs (el de perfil y el nuevo de contraseña)
import { MyProfileService, UserProfileDto, ChangePasswordInputDto } from 'src/app/proxy/users'; 

@Component({
  selector: 'app-my-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './my-profile.html',
  styleUrls: ['./my-profile.scss']
})
export class MyProfileComponent implements OnInit {
  form: FormGroup;
  passwordForm: FormGroup; // Formulario para el cambio de clave
  isBusy = false;
  isPasswordBusy = false; // Estado de carga independiente para seguridad

  constructor(
    private fb: FormBuilder,
    private profileService: MyProfileService,
    private toaster: ToasterService
  ) {
    this.buildForm();
    this.buildPasswordForm(); // Inicializamos el formulario de contraseña
  }

  ngOnInit(): void {
    this.loadProfile();
  }

  // Formulario de datos personales
  buildForm() {
    this.form = this.fb.group({
      userName: [{ value: '', disabled: true }],
      email: ['', [Validators.required, Validators.email]],
      name: [''],
      surname: [''],
      phoneNumber: [''],
      photoUrl: [''],
      preferences: ['']
    });
  }

  // Nuevo: Formulario de seguridad
  buildPasswordForm() {
    this.passwordForm = this.fb.group({
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  loadProfile() {
    this.isBusy = true;
    this.profileService.get().subscribe({
      next: (data) => {
        this.form.patchValue(data);
        this.isBusy = false;
      },
      error: (err) => {
        this.toaster.error('No se pudo cargar el perfil', 'Error');
        this.isBusy = false;
      }
    });
  }

  save() {
    if (this.form.invalid) return;

    this.isBusy = true;
    const input = this.form.getRawValue() as UserProfileDto; 

    this.profileService.update(input).subscribe({
      next: () => {
        this.toaster.success('Tus datos han sido actualizados', '¡Éxito!');
        this.isBusy = false;
      },
      error: (err) => {
        this.toaster.error('Ocurrió un error al guardar', 'Error');
        this.isBusy = false;
      }
    });
  }

  // Nueva función para procesar el cambio de contraseña
  changePassword() {
    if (this.passwordForm.invalid) return;

    this.isPasswordBusy = true;
    const input = this.passwordForm.value as ChangePasswordInputDto;

    this.profileService.changePassword(input).subscribe({
      next: () => {
        this.toaster.success('Tu contraseña ha sido actualizada', '¡Éxito!');
        this.passwordForm.reset(); // Limpiamos los campos
        this.isPasswordBusy = false;
      },
      error: (err) => {
        // El Toaster de ABP suele mostrar errores de validación automáticamente,
        // pero añadimos este por si falla la conexión o hay un error inesperado.
        this.toaster.error('No se pudo cambiar la contraseña. Verifica los datos.', 'Error');
        this.isPasswordBusy = false;
      }
    });
  }
}