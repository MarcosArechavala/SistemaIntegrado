import { inject, PLATFORM_ID } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const platformId = inject(PLATFORM_ID);

  if (!isPlatformBrowser(platformId)) return true; 

  const rawUser = localStorage.getItem('usuarioAHS');
  if (!rawUser) {
    router.navigate(['/login']);
    return false;
  }

  try {
    const userData = JSON.parse(rawUser);
    
    // --- NUEVA LÓGICA DE VALIDACIÓN ---

    // 1. Códigos de la lista de permisos (si hubiera)
    const listaPermisos: string[] = userData.permisos ? userData.permisos.map((p: any) => p.accesos.toString().trim()) : [];

    // 2. El código principal del usuario (el 1364 que viene en result)
    const codigoMaestro = userData.codigoUsuario ? userData.codigoUsuario.toString().trim() : '';

    // 3. Códigos que la ruta permite (ej: ['1197', '1364', '79'])
    const codigosPermitidos: string[] = route.data['codigosPermitidos'] || [];

    // Verificamos si el CÓDIGO MAESTRO está permitido O si está en la LISTA de permisos
    const tieneAcceso = codigosPermitidos.includes(codigoMaestro) || 
                        listaPermisos.some(cp => codigosPermitidos.includes(cp));

    if (tieneAcceso) {
      return true;
    } else {
      alert('Acceso denegado: Su código de usuario (' + codigoMaestro + ') no tiene permiso para este módulo.');
      router.navigate(['/login']); 
      return false;
    }
  } catch (e) {
    router.navigate(['/login']);
    return false;
  }
};
