import { Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component'; // NUEVO
import { SainnavvWrapperComponent } from './components/sainnavv-wrapper/sainnavv-wrapper.component';
import { SocialServiceComponent } from './components/social-service/social-service.component';

import { authGuard } from './guards/auth-guard';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  
  { 
    path: 'medical-form', 
    component: SainnavvWrapperComponent, 
    canActivate: [authGuard], 
     data: { codigosPermitidos: ['1197', '1198', '1364'] } // <--- Código para Sainnavv
  },
  
  { 
    path: 'social-service', 
    component: SocialServiceComponent, 
    canActivate: [authGuard], 
    data: { codigoPermiso: ['10', '1364'] } // <--- Código para Servicio Social (Ejemplo)
  }
];
