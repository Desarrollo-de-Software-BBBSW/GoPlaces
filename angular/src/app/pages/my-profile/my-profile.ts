import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ToasterService, ConfirmationService, Confirmation } from '@abp/ng.theme.shared'; // <--- Agregados ConfirmationService y Confirmation
import { AuthService } from '@abp/ng.core'; // <--- Agregado AuthService

// Importamos el servicio y ambos DTOs
import { MyProfileService, UserProfileDto, ChangePasswordInputDto } from 'src/app/proxy/users'; 

@Component({
  selector: 'app-my-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './my-profile.html', // Asegúrate que el nombre coincida con tu archivo
  styleUrls: ['./my-profile.scss'] // Asegúrate que el nombre coincida con tu archivo
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
    private confirmation: ConfirmationService, // <--- Inyectado
    private authService: AuthService            // <--- Inyectado
  ) {
    this.buildForm();
    this.buildPasswordForm();
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

  // Formulario de seguridad
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
        this.toaster.error('No se pudo cambiar la contraseña. Verifica los datos.', 'Error');
        this.isPasswordBusy = false;
      }
    });
  }

  // Función para eliminar cuenta
  deleteAccount() {
    this.confirmation.warn(
      'Esta acción no se puede deshacer. Tu cuenta será inhabilitada permanentemente.',
      '¿Estás seguro de eliminar tu cuenta?',
      {
        // CORRECCIÓN: Usamos 'yesText' y 'cancelText' en lugar de confirmButtonText
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