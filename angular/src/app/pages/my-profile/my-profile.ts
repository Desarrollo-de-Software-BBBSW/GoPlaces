import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ToasterService, ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { AuthService, ConfigStateService } from '@abp/ng.core'; // 👈 Agregado ConfigStateService

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
  passwordForm: FormGroup;
  isBusy = false;
  isPasswordBusy = false;

  constructor(
    private fb: FormBuilder,
    private profileService: MyProfileService,
    private toaster: ToasterService,
    private confirmation: ConfirmationService,
    private authService: AuthService,
    private config: ConfigStateService // 👈 Inyectamos esto para ver quién está logueado
  ) {
    this.buildForm();
    this.buildPasswordForm();
  }

  ngOnInit(): void {
    // DIAGNÓSTICO: Verificamos en consola quién cree Angular que es el usuario
    const currentUser = this.config.getOne('currentUser');
    console.log('🔴 USUARIO EN SESIÓN (FRONTEND):', currentUser);

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
      photoUrl: [''], // Asegúrate de que tu DTO tenga este campo
      preferences: [''] // Asegúrate de que tu DTO tenga este campo
    });
  }

  // Formulario de seguridad
  buildPasswordForm() {
    this.passwordForm = this.fb.group({
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  loadProfile() {
    this.isBusy = true;
    
    // 1. Limpiamos el formulario antes de cargar para borrar "fantasmas"
    this.form.reset();

    // 2. Pedimos los datos frescos a la API
    this.profileService.get().subscribe({
      next: (data) => {
        console.log('🟢 DATOS RECIBIDOS DE LA API:', data); // 👈 Mira esto en consola (F12)
        
        // Si data.name sigue siendo Juan, el error está en el Backend (C#)
        this.form.patchValue(data);
        this.isBusy = false;
      },
      error: (err) => {
        console.error(err);
        this.toaster.error('No se pudo cargar el perfil', 'Error');
        this.isBusy = false;
      }
    });
  }

  save() {
    if (this.form.invalid) return;

    this.isBusy = true;
    // getRawValue() incluye los campos deshabilitados (como userName)
    const input = this.form.getRawValue() as UserProfileDto; 

    this.profileService.update(input).subscribe({
      next: () => {
        this.toaster.success('Tus datos han sido actualizados', '¡Éxito!');
        this.isBusy = false;
        
        // Opcional: Recargar para asegurar que todo esté sincro
        this.loadProfile(); 
      },
      error: (err) => {
        this.toaster.error('Ocurrió un error al guardar', 'Error');
        this.isBusy = false;
      }
    });
  }

  changePassword() {
    if (this.passwordForm.invalid) return;

    this.isPasswordBusy = true;
    const input = this.passwordForm.value as ChangePasswordInputDto;

    this.profileService.changePassword(input).subscribe({
      next: () => {
        this.toaster.success('Tu contraseña ha sido actualizada', '¡Éxito!');
        this.passwordForm.reset();
        this.isPasswordBusy = false;
      },
      error: (err) => {
        const msg = err.error?.error?.message || 'Verifica tu contraseña actual.';
        this.toaster.error(msg, 'Error al cambiar contraseña');
        this.isPasswordBusy = false;
      }
    });
  }

  deleteAccount() {
    this.confirmation.warn(
      'Esta acción no se puede deshacer. Tu cuenta será inhabilitada permanentemente.',
      '¿Estás seguro de eliminar tu cuenta?',
      {
        yesText: 'Sí, eliminar mi cuenta',
        cancelText: 'Cancelar'
      }
    ).subscribe((status: Confirmation.Status) => {
      if (status === Confirmation.Status.confirm) {
        
        this.isBusy = true;
        this.profileService.delete().subscribe({
          next: () => {
            this.toaster.info('Tu cuenta ha sido eliminada.', 'Adiós');
            this.authService.logout();
          },
          error: (err) => {
            this.toaster.error('No se pudo eliminar la cuenta.', 'Error');
            this.isBusy = false;
          }
        });
      }
    });
  }
}