import { Component, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { finalize } from 'rxjs/operators'; // <--- LA HERRAMIENTA CLAVE

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  
  credentials = {
    username: '',
    password: ''
  };

  isLoading = false;
  showPassword = false;
  errorMessage = '';

  private router = inject(Router);
  private http = inject(HttpClient);
  private cdr = inject(ChangeDetectorRef); // Para forzar el repintado de pantalla

  login() {
    // 1. Limpiamos alertas viejas y validamos vacíos
    this.errorMessage = '';
    if (!this.credentials.username || !this.credentials.password) {
      this.errorMessage = 'Por favor, complete usuario y contraseña.';
      return;
    }

    // 2. Prender el Loading en el Botón HTML
    this.isLoading = true;

    const url = '/api/auth/iniciar-sesion';
    const body = {
      usuario: this.credentials.username,
      clave: this.credentials.password
    };

    // 3. Ejecutar Petición con Protección (Finalize)
    this.http.post(url, body)
      .pipe(
        // EXPLICACIÓN: "finalize" se ejecutará SIEMPRE.
        // No importa si la contraseña está bien, está mal, el server explota o hay error 404.
        // Siempre vendrá aquí a apagar la ruedita y decirle al HTML que cambie.
        finalize(() => {
          this.isLoading = false;
          this.cdr.detectChanges(); 
        })
      )
      .subscribe({
        next: (response: any) => {
          console.log("Respuesta Recibida: ", response);

          try {
            let finalData = typeof response === 'string' ? JSON.parse(response) : response;

            // Verificar qué dijo la BD (Tu JSON devuelve 'result' = 0 si falla)
            if (finalData.result === 0 || finalData.data === "El Usuario no esta autorizado") {
                this.errorMessage = finalData.message || "Usuario inactivo o contraseña incorrecta.";
            } 
            else {
                // LOGEO EXITOSO
                localStorage.setItem('usuarioAHS', JSON.stringify(finalData));
                this.router.navigate(['/medical-form']); 
            }
          } catch(e) {
            this.errorMessage = "Se interrumpió la respuesta del sistema central.";
          }
        },
        error: (error) => {
          // Ya no necesitamos poner isLoading=false aquí porque finalize() ya lo hizo.
          
          if (error.status === 401) {
              // El Unauthorized lanzado en nuestro backend C#
              this.errorMessage = "⚠️ Acceso denegado. Credenciales incorrectas.";
          } 
          else if (error.status === 404) {
              this.errorMessage = "❌ No se pudo localizar la API de autenticación."; 
          }
          else if (error.status === 0) {
              this.errorMessage = "🚨 Error de Servidor: Compruebe que la IP del servidor (.NET) esté encendida o que CORS no esté bloqueando.";
          }
          else {
              this.errorMessage = `Error HTTP Código [${error.status}] al intentar logear.`;
          }
        }
      });
 }

  togglePasswordVisibility() {
    this.showPassword = !this.showPassword;
  }
}