import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { SocialFormData, initialSocialData } from '../../models/social.types';

@Component({
  selector: 'app-social-service',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './social-service.component.html'
})
export class SocialServiceComponent {
  private http = inject(HttpClient);
  
  formData: SocialFormData = { ...initialSocialData };
  isSearching = false;
  isLocked = false; // Bloquea Nombre/Apellido si existe paciente

  // Opciones para Selects (Misma data que en React)
  options = {
    tipoDoc: ['DNI', 'LC', 'LE', 'PASAPORTE'],
    sexo: [
      { label: 'Masculino', value: 'M' },
      { label: 'Femenino', value: 'F' },
      { label: 'Indeterminado', value: 'I' }
    ],
    nacionalidad: ['Argentina', 'Uruguay', 'Paraguay', 'Otro'],
    provincia: ['CHACO', 'CORRIENTES', 'FORMOSA'],
    departamento: ['SAN FERNANDO', 'COMANDANTE FERNANDEZ'],
    localidad: ['RESISTENCIA', 'BARRANQUERAS'],
    area: ['No Especificado', 'Zona Sur', 'Zona Norte'],
    obraSocial: ['Ninguna', 'INSSSEP', 'PAMI', 'OSDE'],
    vivienda: ['Casa', 'Departamento', 'Otro'],
    agua: ['Red Publica', 'Pozo', 'Otro']
  };

  buscarPaciente(tipo: 'dni' | 'hc') {
    const valor = tipo === 'dni' ? this.formData.dni : this.formData.hcNumber;
    if (!valor) return;

    this.isSearching = true;
    const url = `http://localhost:5000/ServicioSocial/buscar?tipo=${tipo}&valor=${valor}`;

    this.http.get(url).subscribe({
      next: (data: any) => {
        if (data) {
          // Si encontramos paciente, cargamos datos y bloqueamos edición de campos protegidos
          this.formData = { ...this.formData, ...data };
          this.isLocked = true; 
          alert('Paciente Encontrado. Datos cargados.');
        } else {
          this.isLocked = false; // Permitimos cargar nuevo
          alert('Paciente no encontrado. Complete el formulario para dar de alta.');
        }
        this.isSearching = false;
      },
      error: (e) => {
        console.error(e);
        this.isSearching = false;
        alert('Error al conectar con el servidor');
      }
    });
  }

  guardarFicha() {
    // Si isLocked es true, es una actualización (usando los SPs)
    const url = `http://localhost:5000/ServicioSocial/guardar`;
    
    this.http.post(url, this.formData).subscribe({
      next: (res) => alert('Ficha Social guardada correctamente'),
      error: (err) => alert('Error al guardar: ' + err.message)
    });
  }

  reset() {
    this.formData = { ...initialSocialData };
    this.isLocked = false;
  }
}