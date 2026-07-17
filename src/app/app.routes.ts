import { Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component'; // NUEVO
import { SainnavvWrapperComponent } from './components/sainnavv-wrapper/sainnavv-wrapper.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';


import { authGuard } from './guards/auth-guard';

export const routes: Routes = [
  // 1. Al entrar a la web, mandamos al login
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  
  // 2. Rutas públicas
  { path: 'login', component: LoginComponent },
  
  // 3. RUTAS PROTEGIDAS (Para entrar al dashboard solo hace falta estar logueado, sin código específico)
  { 
    path: 'dashboard', 
    component: DashboardComponent, 
    canActivate: [authGuard] 
  },
  
  // 4. Módulos
  { 
    path: 'medical-form', 
    component: SainnavvWrapperComponent, 
    canActivate: [authGuard], 
    data: { codigosPermitidos: ['1197', '1198', '79', '1364'] } 
  }
  
  // { path: 'laboratorio', component: LaboratorioComponent, ... } // (Lo crearemos después)
];
