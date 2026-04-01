import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router'; // IMPORTANTE
import { routes } from './app.routes';           // TUS RUTAS
import { provideHttpClient, withFetch } from '@angular/common/http';

export const appConfig: ApplicationConfig = {
  providers:[
      provideRouter(routes),  // <-- ESTA LÍNEA ES VITAL PARA QUE NO DÉ 404
      provideHttpClient(withFetch())
  ]
};
