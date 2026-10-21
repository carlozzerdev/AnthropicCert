# Constitución del proyecto

Versión: 0.2 · Actualización: 2026-10-21 · Estado: borrador con WinForms y objetivo pedagógico confirmados por el usuario.

## Principios
1. **Aprendizaje verificable.** La aplicación debe ser una ayuda sólida para preparar y aprobar CCAR-P mediante comprensión y práctica. Cada pregunta publicada incluye respuesta correcta, explicación del acierto y de los distractores y ayudas conceptuales locales. Las URLs de ampliación son opcionales para el estudiante; la evidencia y la revisión editorial siguen formando parte del contenido. No prometer aprobación garantizada.
2. **Identidad de certificación.** CCAR-P Professional es el objetivo. No mezclar guías, pesos ni preguntas de Foundations. Versionar el blueprint y declarar su nivel de verificación.
3. **Portabilidad Windows 11.** Extraer ZIP y ejecutar como usuario estándar. El destinatario no instala herramientas de desarrollo ni runtime. Arquitecturas soportadas y carpeta de datos deben declararse.
4. **Separación de responsabilidades.** Presentación WinForms, por decisión del usuario. UI → aplicación → dominio; infraestructura implementa puertos definidos hacia el interior. El punto de composición conecta implementaciones. Nada de SQL, archivos o navegación de ventanas en el dominio. Formularios y eventos no contienen reglas de examen.
5. **DRY con criterio.** Una sola definición de reglas de puntuación, selección de preguntas, validaciones y esquema de contenido. Reutilizar comportamiento real; no generalizar por semejanza accidental.
6. **Funcionamiento local.** Propuesta inicial: práctica, corrección e historial sin conexión. Internet solo al abrir referencias por acción del usuario. Sin cuentas, telemetría ni claves API por defecto.
7. **Contenido distribuible.** Incorporar preguntas originales o con permiso compatible y atribución. Mantener enlaces a recursos externos como tales. Respuestas de comunidades no constituyen prueba de corrección.
8. **Cambios guiados por especificaciones.** Cada incremento tiene alcance, criterios de aceptación, diseño, tareas y evidencias. Separar propuesto, aceptado, implementado y verificado.
9. **Datos duraderos.** Guardados transaccionales, migraciones versionadas, copia previa a migrar y errores visibles. Actualizar la aplicación no debe borrar el progreso.
10. **Accesibilidad.** Teclado, foco visible, escalado DPI y estados expresados con texto además de color.

## Gestión de cambios
El usuario puede cambiar estos principios. Registrar motivo, fecha e impacto en decisiones y specs afectadas. Las instrucciones explícitas del usuario prevalecen. No exigir herramientas de gestión o ceremonias externas para seguir este flujo.

## Definición de terminado
Documentación: coherencia, enlaces locales válidos, requisitos trazables y pendientes visibles.
Implementación futura: criterios cubiertos, pruebas pertinentes con resultados reales, revisión de contenido y ZIP verificado en entorno objetivo. La existencia de una lista de pruebas no acredita su ejecución.
