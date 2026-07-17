import { inject, PLATFORM_ID } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const platformId = inject(PLATFORM_ID);

  // Si se ejecuta en el servidor (SSR), permitimos pasar
  if (!isPlatformBrowser(platformId)) return true; 

  const rawUser = localStorage.getItem('usuarioAHS');

  if (!rawUser) {
    router.navigate(['/login']);
    return false;
  }

  try {
    const userData = JSON.parse(rawUser);
    
    // 1. Obtenemos los códigos exigidos por la página a la que quiere entrar
    const codigosPermitidos: string[] = route.data['codigosPermitidos'];

    // =========================================================================
    // LA SOLUCIÓN: Si la ruta no exige ningún código (Como el DASHBOARD), 
    // lo dejamos pasar automáticamente porque ya está logueado.
    // =========================================================================
    if (!codigosPermitidos || codigosPermitidos.length === 0) {
      return true;
    }

    // 2. Extraemos los códigos que tiene el usuario
    const listaPermisos: string[] = userData.permisos ? userData.permisos.map((p: any) => p.accesos.toString().trim()) : [];
    const codigoMaestro = userData.codigoUsuario ? userData.codigoUsuario.toString().trim() : '';

    // 3. Verificamos si tiene acceso
    const tieneAcceso = codigosPermitidos.includes(codigoMaestro) || 
                        listaPermisos.some((codigo: string) => codigosPermitidos.includes(codigo));

    if (tieneAcceso) {
      return true;
    } else {
      alert('Acceso denegado: Su usuario no cuenta con el permiso para ingresar a este módulo.');
      // Lo mandamos al Dashboard en vez de cerrar su sesión
      router.navigate(['/dashboard']); 
      return false;
    }
  } catch (e) {
    router.navigate(['/login']);
    return false;
  }
};
