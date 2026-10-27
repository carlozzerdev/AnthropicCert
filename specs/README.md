# Flujo de especificaciones

Este repositorio comienza vacío. Se propone un flujo documental inspirado en desarrollo por especificaciones; no se ha detectado una instalación ni un flujo previo de Spec Kit. No hacen falta comandos especiales ni instalar herramientas.

## Cómo trabajar
1. **Definir:** problema, usuarios, alcance, historias y aceptación en `spec.md`.
2. **Investigar y aclarar:** evidencia en `research.md`; propuestas y respuestas del usuario en `decisions.md`.
3. **Diseñar:** `plan.md`, `data-model.md` y contratos cuando correspondan. Vincular las decisiones técnicas a los requisitos.
4. **Especificar la validación:** escenarios en `tests/acceptance.md`, incluyendo fallos y límites.
5. **Descomponer:** `tasks.md`, con dependencias y evidencias esperadas.
6. **Revisar:** `checklist.md`. En esta fase se puede completar documentación manteniendo abiertas las decisiones de producto.
7. **Implementar cuando se solicite:** completar tareas y registrar resultados en `tests/results.md`. No marcar casos como superados por haberlos escrito.
8. **Cerrar:** actualizar estado y changelog con lo realmente entregado.

## Organización mínima
Cada carpeta `NN_nombre/` contiene `spec.md`, `plan.md`, `tasks.md`, `decisions.md`, `checklist.md` y `tests/acceptance.md`. Añadir investigación, modelo de datos o contratos solo cuando aporten información. Cada documento indica estado. Usar IDs estables de requisitos y pruebas; no renumerarlos al añadir nuevos.

Una nueva funcionalidad utiliza la siguiente carpeta libre (`01_...`, `02_...`). Declarar qué especificaciones amplía o sustituye y cómo migran datos. Conservar decisiones anteriores y señalar las reemplazadas. No copiar la constitución en cada spec.

## Índice
| Spec | Propósito | Estado |
|---|---|---|
| [00_especificacion_creacion](00_especificacion_creacion/spec.md) | Base de la aplicación de práctica | Versión 1.0.0 implementada; banco ampliado y validación oficial pendientes |
| [01_multiexamen](01_multiexamen/spec.md) | Preguntas en inglés, selección Professional/Foundations y ampliación de contenido | 1.1.0 implementada; evidencias y pendientes en tests/results.md |
| [02_libro_foundations](02_libro_foundations/spec.md) | Importación local de preguntas del EPUB Foundations | 15 añadidas; banco local 79; revisión y pruebas registradas |

La spec 00 define el producto inicial. La implementación solicitada después de la fase documental se registra en [implementation.md](00_especificacion_creacion/implementation.md), con evidencias y pendientes.
