# Tareas y dependencias

Estado: implementación 1.0.0 creada. Progreso real y partes pendientes detallados a continuación; el objetivo de banco ampliado no está completado.

## Entrega documental actual
- [x] DOC01 Crear AGENTS.md, constitución y flujo de specs.
- [x] DOC02 Documentar requisitos confirmados y propuestas separadas.
- [x] DOC03 Investigar recursos y registrar bloqueos de acceso sin inventar contenido.
- [x] DOC04 Comparar stack y comprobar herramientas locales mediante consultas de solo lectura.
- [x] DOC05 Proponer arquitectura, modelo de datos, contratos y casos de aceptación.
- [ ] DOC06 Incorporar respuestas del usuario y ajustar alcance/idiomas/perfiles.
- [ ] DOC07 Contrastar el blueprint con la guía oficial de Professional accesible.

## Implementación autorizada y seguimiento
| Tarea | Dependencias | Evidencia de cierre |
|---|---|---|
| IMP01 Crear solución y capas con stack decidido | Solicitud de implementar, D04 | Build y dependencias entre proyectos revisadas |
| IMP02 Definir esquema de banco y perfil versionados | DOC06, DOC07, IMP01 | A09; fixtures válidos e inválidos |
| IMP03 Implementar corrección y cuotas de selección | IMP02 | A02, A03, A10 y pruebas de empates/insuficiencia |
| IMP04 Implementar persistencia, rutas y snapshots | D07, IMP01, IMP02 | A07, A08, A11; JSON atómico según ADR-002 sustituye SQLite propuesta |
| IMP05 Construir práctica WinForms y ayudas locales con enlaces opcionales | IMP03, IMP04, D05 | A02, A04, A12, A13 |
| IMP06 Construir simulacro y recuperación | IMP03, IMP04, D13 | A05, A06 |
| IMP07 Construir historial, puntuación y estadísticas; repaso como modo si se confirma | D06, D17, IMP04, IMP06 | A07, A08, A14–A17 |
| IMP08 Crear y revisar banco y ayudas conceptuales distribuibles | DOC07, D09, D10, IMP02 | Inventario editorial, ayudas autosuficientes y cobertura por objetivo |
| IMP09 Empaquetar ZIP limpio | IMP05–IMP08 según alcance | A01, A11, licencias y manifiesto |
| IMP10 Verificar entrega final | IMP09 | Resultados reales de todos los casos aplicables |

No completar tareas por la mera existencia de documentos o tests. Si se reduce el alcance, marcar tareas excluidas con motivo y referencia a la decisión, sin simular que se implementaron.

IMP08 se amplía a objetivo de 350 preguntas publicables. Trabajar por lotes de 25–50, registrar estados de revisión, eliminar duplicados semánticos y contrastar respuestas y distractores con fuentes oficiales. El inventario de 113 entradas externas candidatas no cierra esta tarea. Validación adicional: A18.

### Estado de tareas
- IMP01–IMP07: implementadas para alcance descrito en implementation.md; pruebas automatizadas y revisión visual registradas en tests/results.md. Accesibilidad exhaustiva y todos los escenarios manuales no se dan por completados.
- IMP08: parcial, 78 activas y 80 entradas de origen archivadas. Pendientes 350, revisión humana independiente y guía oficial directa.
- IMP09: ZIP con runtime, manifiesto y licencias construido; verificación del paquete en tests/results.md.
- IMP10: parcial; máquina limpia y matriz DPI/teclado exhaustiva pendientes.
