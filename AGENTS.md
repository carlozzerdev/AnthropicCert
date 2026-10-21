# Instrucciones del proyecto

## Objetivo y estado actual
Crear una aplicación de escritorio para practicar Claude Certified Architect – Professional (CCAR-P), portable en ZIP para Windows 11, con respuestas explicadas y referencias por pregunta.
El usuario ha autorizado implementar la aplicación e incorporar contenido local el 2026-10-21. Mantener specs y evidencias junto con el código. Consultar `specs/00_especificacion_creacion/implementation.md` para las decisiones efectivas y límites de la entrega.

## Antes de trabajar
1. Leer `specs/constitution.md`, `specs/README.md` y la especificación afectada.
2. Consultar `specs/00_especificacion_creacion/decisions.md` para distinguir decisiones del usuario, propuestas y pendientes.
3. Seguir requisitos identificados y mantener trazabilidad con los casos de aceptación.
4. Respetar los archivos existentes y los cambios del usuario. No sustituir decisiones aceptadas por preferencias del agente.

## Flujo
Usar carpetas `specs/NN_nombre/`. El flujo y la plantilla mínima están en `specs/README.md`. Actualizar spec, plan, tests y tareas cuando cambie el alcance. No dar por aprobada una propuesta por falta de respuesta. Una petición explícita de implementación autoriza el alcance indicado; no pedir confirmaciones repetidas.

## Arquitectura y calidad
Separar presentación, aplicación, dominio e infraestructura. Dominio sin dependencias de UI, almacenamiento o red. Usar WinForms, elegido por el usuario; patrón MVP propuesto para separar formularios y lógica de presentación. Reglas y corrección fuera de formularios y manejadores de eventos. Aplicar DRY a reglas, validación y contratos, evitando abstracciones especulativas.
Stack implementado: C# / .NET 10 / WinForms, MVP para sesión de examen, bancos JSON versionados y progreso JSON atómico por intento detrás de IAttemptRepository. SQLite queda como alternativa futura; ver ADR-002. No crear servicios web ni integrar IA en tiempo de ejecución sin una especificación que lo justifique.
Priorizar preparación para aprobar mediante comprensión: explicaciones autosuficientes, ayudas conceptuales locales y repaso. Las URLs son ampliación opcional; aprender o completar un intento no debe depender de abrirlas. Conservar fuentes editoriales aunque no se expongan todos sus enlaces en la interfaz.

## Contenido y fuentes
Priorizar documentación oficial Anthropic y la guía oficial de la versión exacta de CCAR-P. Registrar URL, fecha de consulta, alcance, revisión editorial y permiso/licencia para contenido distribuido. No atribuir carácter oficial a preguntas propias. No asumir que acceso público permite redistribuir preguntas. No inventar verificaciones ni eludir accesos restringidos. Diferenciar evidencia oficial, declaraciones de terceros e hipótesis.

## Entrega futura
ZIP self-contained por arquitectura, inicialmente win-x64 propuesto. Sin instalación, administrador, SDK ni runtime externo. Rutas relativas a la carpeta del ejecutable. No publicar progreso personal dentro del ZIP de distribución. Probar el paquete en Windows 11 limpio sin .NET preinstalado.

## Comunicación y validación
Escribir documentación y conversar en español. Presentar alternativas y decisiones pendientes de manera explícita. No afirmar que pasan pruebas no ejecutadas. Durante esta fase revisar documentos y enlaces locales; los casos en `tests/` son especificaciones, no pruebas realizadas. Antes de entregar implementación ejecutar los casos pertinentes y registrar evidencias.
