# Decisiones y preguntas abiertas

Estado actualizado: 2026-10-21. «Propuesto» no significa aceptado.

| ID | Tema | Estado | Decisión / siguiente paso |
|---|---|---|---|
| D01 | Plataforma | Confirmado por usuario | Windows 11, aplicación portable en ZIP |
| D02 | Método | Confirmado por usuario; actualizado | Specs primero. Implementación autorizada explícitamente el 2026-10-21 |
| D03 | Arquitectura | Confirmado por usuario | Separación presentación/funcionalidad, limpia y DRY |
| D04 | Stack | Implementado según elección de WinForms | C#/.NET 10/WinForms; MVP para sesión. Progreso JSON atómico sustituye SQLite propuesta; ADR-002 en implementation.md |
| D05 | Idiomas | Supuesto de implementación explícito | UI/ayudas español; importadas y razonamientos inglés, originales español; no hubo respuesta de preferencia |
| D06 | Alcance v1 | Parcialmente confirmado | Práctica, puntuación, historial y estadísticas incluidos; detalles de simulacro, modo de repaso de errores y editor/importación siguen propuestos |
| D07 | Datos y perfiles | Supuesto de implementación explícito | Un usuario, Data junto al ejecutable; no hubo respuesta de preferencia |
| D08 | Conectividad | Propuesto | Offline salvo apertura voluntaria de enlaces; sin API de IA |
| D09 | Contenido | Pendiente | Original o redistribuible; referencias externas sin incorporar bancos sin permiso |
| D10 | Banco ampliado | Parcialmente entregado | 80 entradas MIT archivadas, 43 activas + 35 originales = 78 activas. Objetivo de 350 pendiente; webs sin permiso quedan como enlaces. Ver implementation.md |
| D11 | CPU | Propuesto | win-x64 inicialmente; confirmar si también requiere ARM64 |
| D12 | Blueprint | Pendiente de evidencia oficial | No consolidar número, tiempo, pesos ni formatos desde terceros |
| D13 | Reloj | Propuesto | Continúa al cerrar/suspender simulacro; práctica sin cronómetro obligatorio |
| D14 | Nombre | Provisional | AnthropicCert Practice; indicar independencia de Anthropic |
| D15 | Objetivo pedagógico | Confirmado por usuario el 2026-10-21 | Practicar exámenes y aprender con ayudas; ser una ayuda sólida para aprobar la certificación |
| D16 | URLs | Confirmado por usuario el 2026-10-21 | Ampliación opcional; ayudas y explicaciones locales autosuficientes. Fuentes editoriales se conservan por trazabilidad |
| D17 | Puntuación y estadísticas | Confirmado por usuario el 2026-10-21 | Ver aciertos, fallos, puntuación total y otras métricas útiles. Cálculos y métricas concretados en scoring-statistics.md dentro del alcance delegado |

## ADR-001 · Tecnología para la aplicación Windows
Estado: revisado el 2026-10-21; WinForms elegido por el usuario. Resto del stack y MVP propuestos.
Contexto: Windows 11, ZIP sin instalación, Visual Studio y Python disponibles, separación estricta de lógica/UI.
Decisión: presentación WinForms. Propuesta de implementación: C#/.NET 10, MVP en UI y capas de dominio/aplicación/infraestructura. Publicación self-contained de carpeta.
Motivos: elección explícita del usuario, alineación con Windows y herramientas instaladas; presentadores comprobables sin formularios; despliegue sin instalar runtime en destino. La propuesta anterior WPF/MVVM queda sustituida.
Consecuencias: aplicación Windows; ZIP mayor por runtime incluido; reconstruir para distribuir parches del runtime; mantener SQL y migraciones en infraestructura. Un ZIP portable no exige EXE único.
Alternativas y costes: [plan.md](plan.md). No hay necesidad identificada de C/C++.

## Cómo registrar respuestas
Actualizar la fila correspondiente con respuesta exacta o síntesis fiel, fecha y cambios afectados. Las respuestas nuevas reemplazan propuestas, no requieren una aprobación adicional del mismo asunto. Resolver decisiones que cambien implementación antes de construir esa parte.

## Evolución en especificación 01

2026-10-21: el usuario solicita preguntas y opciones en inglés y selección entre Professional y Foundations. [01_multiexamen](../01_multiexamen/spec.md) sustituye D05 respecto al idioma de preguntas nuevas y amplía el objetivo exclusivo Professional; conserva snapshots históricos. La aplicación 1.0.0 mantiene su comportamiento hasta implementar 01.

Actualización: la implementación 01 está autorizada y creada como 1.1.0; ver ../01_multiexamen/implementation.md. Las decisiones históricas anteriores describen 1.0.0.
