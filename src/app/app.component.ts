
import { Component, inject ,PLATFORM_ID } from '@angular/core';
import { CommonModule } from '@angular/common';
import { isPlatformBrowser } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive, Router } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.component.html'
})
export class AppComponent {
 title = 'Sainnavv - Sistema Hospitalario';
  
  // Usamos el Router de Angular para saber dónde estamos parados
  public router = inject(Router);
   private platformId = inject(PLATFORM_ID);
  // Función sencilla: devuelve TRUE si la URL actual NO contiene '/login'
  // Usamos esto para decidir si mostramos el Navbar
  showNavbar(): boolean {
    return !this.router.url.includes('/login');
  }

   tieneAcceso(codigo: string): boolean {
    if (!isPlatformBrowser(this.platformId)) return false;

    const rawUser = localStorage.getItem('usuarioAHS');
    if (!rawUser) return false;
    
    try {
      const userData = JSON.parse(rawUser);
      const codigoMaestro = userData.codigoUsuario?.toString() || '';
      const listaPermisos = userData.permisos || [];

      // Devuelve true si el código maestro coincide O si está en la lista
      return codigoMaestro === codigo || listaPermisos.some((p: any) => p.accesos.toString() === codigo);
    } catch (e) {
      return false;
    }
  }

  activeStep = 0;
  
  steps = [
    { name: 'Identificación', iconPath: 'M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z' },
    { name: 'Grupo Familiar', iconPath: 'M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z' },
    { name: 'Ingreso', iconPath: 'M11 16l-4-4m0 0l4-4m-4 4h14m-5 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h7a3 3 0 013 3v1' },
    { name: 'Intervenciones', iconPath: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01' },
    { name: 'Clínica', iconPath: 'M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.806.547M8 4h8l-1 1v5.172a2 2 0 00.586 1.414l5 5c1.26 1.26.367 3.414-1.415 3.414H4.828c-1.782 0-2.674-2.154-1.414-3.414l5-5A2 2 0 009 10.172V5L8 4z' },
    { name: 'Conclusión', iconPath: 'M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z' }
  ];

  setStep(idx: number) {
    this.activeStep = idx;
  }

 
}