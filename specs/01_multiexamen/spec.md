# Especificación 01 · Multiexamen

Fecha: 2026-10-21. Estado: implementación 1.1.0 autorizada y creada; decisiones efectivas en [implementation.md](implementation.md), evidencias y pendientes en [tests/results.md](tests/results.md). El diseño inicial se conserva como referencia.
Amplía [spec 00](../00_especificacion_creacion/spec.md). Estado inicial efectivo: [implementation.md](../00_especificacion_creacion/implementation.md). Decisiones: [decisions.md](decisions.md). Evidencia: [research.md](research.md).

## Objetivo
Practicar en inglés y elegir entre Claude Certified Architect · Professional (CCAR-P), ya disponible, y Claude Certified Architect · Foundations (CCAR-F; alias CCA-F usado por ExamTopics). Preparar cada certificación con su contenido y perfil propios, explicaciones autosuficientes y progreso conservado.

## Alcance y límites
El usuario solicita después «implementa la especificacion 01», autorizando aplicación, migración, contenido revisado y paquete dentro de este alcance. El resultado y las diferencias respecto al diseño inicial se registran en implementation.md.
Se mantienen WinForms, C#/.NET 10, MVP, capas, persistencia JSON atómica por intento y distribución portable. El objetivo previo de 350 preguntas sigue referido a Professional; no se convierte en un total combinado ni se promete otro número para Foundations.

## Historias
- Como estudiante, elijo la certificación antes de comenzar y conozco qué banco y perfil utilizaré.
- Como candidato a un examen en inglés, leo enunciados y opciones en inglés y practico con esa terminología.
- Como estudiante con historial, cambio de certificación sin perder intentos ni mezclar mis resultados.
- Como responsable editorial, amplío cada banco solo con contenido revisado y distribuible.

## Requisitos
| ID | Requisito | Origen / estado |
|---|---|---|
| M01 | Selector de CCAR-P y CCAR-F antes de crear una práctica o simulacro; indicar nombre, código, versión del banco, cantidad activa y estado del perfil | Solicitado |
| M02 | Todos los enunciados, instrucciones de selección incluidas en preguntas y textos de opciones de intentos nuevos deben estar en inglés. Conservar el original inglés importado; traducir y revisar las 35 originales españolas, incrementando revisión | Solicitado; procedimiento propuesto |
| M03 | Mantener UI y ayudas en español; conservar explicaciones inglesas existentes. El idioma de todas las explicaciones queda pendiente de preferencia; no condiciona M02 | Continuidad de 00, no nueva decisión del usuario |
| M04 | Cada banco pertenece a una certificación y a una versión de blueprint; dominios, objetivos, cuotas, duración y cantidad del simulacro provienen de su perfil. Nunca completar un banco con preguntas del otro | Derivado de multiexamen |
| M05 | Práctica, simulacro, repaso, ayudas, historial, cobertura y estadísticas operan dentro de la certificación seleccionada; todos los agregados comparten ese filtro y los filtros existentes. Mostrar certificación al revisar un intento | Derivado |
| M06 | Cada intento conserva certificación, banco, perfil y versiones, reglas de puntuación y snapshots. Cambiar selector o actualizar bancos no cambia intentos activos o históricos | Derivado |
| M07 | Migrar datos v1 a formato versionado con copia previa y recuperación; identificar como CCAR-P solo registros reconocidos de v1. Conservar IDs, respuestas, fechas, estados, resultados y snapshots españoles históricos. Datos desconocidos o incompatibles se conservan y generan diagnóstico | Derivado; diseño propuesto |
| M08 | Validar bancos individualmente: identidad, versión, inglés declarado y revisado, IDs, opciones, respuestas, dominios, objetivos, referencias, ayudas, revisión y procedencia. Un banco inválido no impide usar otro válido ni altera el historial | Derivado |
| M09 | Ampliar mediante contenido original o con permiso compatible, evidencia oficial de la respuesta y distractores, ayuda local y revisión de duplicados. Registrar cantidades por certificación y estado editorial, sin sumar anuncios, traducciones ni archivadas al total activo | Investigación solicitada; futura producción propuesta |
| M10 | Registrar por fuente URL, fecha, revisión/commit o hash, alcance de licencia, atribución y cambios. ExamTopics queda como referencia externa mientras no haya permiso y procedencia adecuados; no importar preguntas presentadas como reales sin aclarar su autorización | Constitución y trazabilidad |
| M11 | Si faltan preguntas para una cuota, bloquear ese simulacro con diagnóstico de carencia; permitir práctica con las disponibles. Si no hay banco válido de Foundations, mostrarla como pendiente y no iniciar un intento vacío | Derivado |
| M12 | Ambas certificaciones funcionan offline; ZIP con contenido y avisos por banco, sin progreso personal. No introducir servicios, IA en ejecución ni descargas automáticas | Continuidad |

## Comportamiento propuesto
Conservar un único intento activo global, como en 00. Cambiar el selector permite consultar otra certificación; empezar otro intento requiere reanudar, finalizar o abandonar el pendiente mediante acción explícita. No abandonarlo automáticamente. Reanudar identifica su certificación aunque el selector señale otra.
El perfil Foundations candidato es 60 preguntas/120 minutos, con cinco dominios y pesos 27/18/20/20/15. Falta contrastar la guía oficial exacta antes de presentarlo como perfil oficial verificado. Professional conserva su estado provisional hasta su propia verificación. Las notas son porcentajes de práctica; no convertirlas linealmente en puntuación oficial escalada ni predecir aprobado.

## Compatibilidad
M02 sustituye el supuesto de preguntas originales españolas de 00-D05 para intentos nuevos. M01 amplía el objetivo exclusivo de Professional; se actualiza el principio 2 de la constitución conservando la separación entre certificaciones. El resto de 00 continúa salvo cambios explícitos aquí. Los snapshots históricos son evidencia del intento y no se traducen retrospectivamente.

## Aceptación y pendientes
Trazabilidad completa en [tests/acceptance.md](tests/acceptance.md). Entrega efectiva: 85 Professional y 64 Foundations, explicaciones según lote y pruebas ejecutadas en tests/results.md. Siguen pendientes las guías oficiales exactas, revisión y permisos de candidatos excluidos, 350 Professional, revisión humana independiente, máquina limpia y matriz manual exhaustiva. No se consideran resueltas por silencio.
