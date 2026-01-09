import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ToasterService } from '@abp/ng.theme.shared';

// 👇 Importamos el servicio y el DTO que acabas de generar
import { MyProfileService } from 'src/app/proxy/users'; 
import { UserProfileDto } from 'src/app/proxy/users';

@Component({
  selector: 'app-my-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './my-profile.html',
  styleUrls: ['./my-profile.scss']
})
export class MyProfileComponent implements OnInit {
  form: FormGroup;
  isBusy = false;

  constructor(
    private fb: FormBuilder,
    private profileService: MyProfileService,
    private toaster: ToasterService
  ) {
    this.buildForm();
  }

  // Al iniciar la pantalla, pedimos los datos al Backend
  ngOnInit(): void {
    this.loadProfile();
  }

  buildForm() {
    this.form = this.fb.group({
      userName: [{ value: '', disabled: true }], // El usuario no se suele cambiar
      email: ['', [Validators.required, Validators.email]],
      name: [''],
      surname: [''],
      phoneNumber: [''],
      photoUrl: [''],   // Campo extra
      preferences: [''] // Campo extra
    });
  }

  loadProfile() {
    this.isBusy = true;
    this.profileService.get().subscribe({
      next: (data) => {
        // "patchValue" rellena el formulario automáticamente con los datos que llegan
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
    // getRawValue() incluye los campos deshabilitados (como userName) por si acaso

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
}