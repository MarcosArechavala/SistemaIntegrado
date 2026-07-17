import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Component } from '@angular/core';

@Component({
  selector: 'app-historia-clinica',
  standalone: true,
  templateUrl: './historia-clinica.component.html',
  styleUrls: ['./historia-clinica.component.css']
})
export class HistoriaClinicaComponent {
  constructor() { }
}

describe('HistoriaClinica', () => {
  let component: HistoriaClinicaComponent;
  let fixture: ComponentFixture<HistoriaClinicaComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HistoriaClinicaComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(HistoriaClinicaComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

