import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs'; // Necesario para simular la respuesta

// 1. Importamos el nombre CORRECTO del componente
import { CitiesSearchComponent } from './cities-search';
// 2. Importamos el servicio para poder simularlo (mock)
import { CityService } from 'src/app/proxy/cities';

describe('CitiesSearchComponent', () => {
  let component: CitiesSearchComponent;
  let fixture: ComponentFixture<CitiesSearchComponent>;

  // Creamos un servicio falso simple para que el test no falle al intentar conectar a la API real
  const mockCityService = {
    searchCities: () => of({ cities: [] })
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      // Como es un componente Standalone, lo importamos aquí
      imports: [CitiesSearchComponent],
      // Proveemos el servicio falso en lugar del real
      providers: [
        { provide: CityService, useValue: mockCityService }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CitiesSearchComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});