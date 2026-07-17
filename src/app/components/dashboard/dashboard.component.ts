import { Component, inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent {
  private router = inject(Router);
  private platformId = inject(PLATFORM_ID);
  
  nombreUsuario: string = 'Profesional';

  ngOnInit() {
    if (isPlatformBrowser(this.platformId)) {
      const rawUser = localStorage.getItem('usuarioAHS');
      if (rawUser) {
        try {
          const userData = JSON.parse(rawUser);
          // Puedes intentar extraer el nombre si viene en el token, sino dejamos un genérico
          if(userData && userData.usuario) this.nombreUsuario = userData.usuario;
        } catch(e) {}
      }
    }
  }

  // Verificar permisos reutilizando la lógica
  tieneAccesoSainnavv(): boolean {
    if (!isPlatformBrowser(this.platformId)) return false;
    const rawUser = localStorage.getItem('usuarioAHS');
    if (!rawUser) return false;
    
    try {
      const userData = JSON.parse(rawUser);
      const codigoMaestro = userData.codigoUsuario?.toString() || '';
      const listaPermisos = userData.permisos || [];
      const codigosPermitidos = ['1197', '1198', '79', '1364']; // Los de SAINNAVV

      return codigosPermitidos.includes(codigoMaestro) || 
             listaPermisos.some((p: any) => codigosPermitidos.includes(p.accesos.toString()));
    } catch (e) {
      return false;
    }
  }

  navegar(ruta: string) {
    if (ruta === 'medical-form' && !this.tieneAccesoSainnavv()) {
      alert('No tiene permisos para acceder al módulo SAINNAVV.');
      return;
    }
    this.router.navigate([`/${ruta}`]);
  }
}
