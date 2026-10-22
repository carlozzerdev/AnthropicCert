# Decisiones 01

Fecha: 2026-10-21. Estado: decisiones de producto explícitas registradas; propuestas diferenciadas.

| ID | Tema | Estado | Decisión |
|---|---|---|---|
| MD01 | Carpeta y alcance | Confirmado por usuario | Crear 01_multiexamen siguiendo la estructura de 00 |
| MD02 | Idioma de preguntas | Confirmado por usuario | Preguntas y opciones en inglés para practicar el idioma del examen; sustituye 00-D05 en ese ámbito |
| MD03 | Certificaciones | Confirmado por usuario | Poder seleccionar la actual CCAR-P y Foundations de la URL Skilljar facilitada; ampliar constitución §2 |
| MD04 | Ampliación | Investigación solicitada | Investigar más preguntas para ambas; no equivale a aprobar importaciones sin revisión ni fijar cifras nuevas |
| MD05 | Interfaz y ayudas | Adoptada en implementación | UI/ayudas en español; explicaciones según el idioma editorial del lote. No se añadió traducción uniforme de explicaciones |
| MD06 | Perfil Foundations | Propuesto, verificación parcial | Candidato 60/120 y cinco dominios; índice de la página oficial visible, apertura directa 403; guía exacta pendiente |
| MD07 | Intento activo | Adoptada en implementación | Uno global; selector no abandona ni modifica un intento |
| MD08 | Datos | Implementada | Catálogo y bancos independientes con perfiles embebidos; identidad explícita en intento y migración versionada con backup |
| MD09 | Cantidades | Entrega efectiva registrada | 85 Professional y 64 Foundations; objetivo de 350 Professional sigue pendiente. No es promesa de cantidad futura Foundations |
| MD10 | Implementación 01 | Autorizada explícitamente | El usuario pide «implementa la especificacion 01». Se ejecuta 1.1.0; decisiones efectivas y límites en implementation.md |
| MD11 | Preguntas de ExamTopics | Incorporación solicitada por usuario; pendiente de material autorizado | El usuario pide «añade las preguntas de examtopics». La solicitud autoriza trabajar en esa incorporación, pero no acredita permiso del titular. No se han copiado preguntas: acceso directo 403 y permiso de reproducción no verificado. Obtener material aportado por el usuario para su transformación o autorización compatible antes de incorporar |

No sustituir el original inglés por una traducción desde otra lengua cuando está disponible. Las originales locales españolas requieren traducción editorial a inglés: no llamarlas originales inglesas. Mantener su procedencia y revisión anterior.

## Actualización MD11: material aportado

El usuario aporta `CCA-F.zip` en esta carpeta: tres MHTML con 30 preguntas. Se transforma offline en [candidatas JSON](sources/examtopics-ccaf-candidates.json), con hashes y procedencia. Ya no falta material para transformar. La revisión técnica/editorial y el permiso de distribución siguen pendientes; no se considera activo ni se incluye en el ZIP de aplicación. Esta transformación está autorizada por la petición de incorporar y la entrega del archivo; no requiere volver a solicitarla.

## Decisiones adoptadas en implementación

MD05: se conserva el idioma editorial de explicaciones y el español de UI/ayudas. MD07: único intento activo global. MD08: catálogo separado, perfiles embebidos en bancos e identidad explícita de intento; migración por staging y marcador atómico. MD09: se entregan 85 Professional y 64 Foundations, sin cambiar el objetivo pendiente de 350 Professional ni convertirlo en total combinado. MD06 continúa provisional. Estas concreciones responden al alcance de implementación solicitado y no se atribuyen a elecciones explícitas adicionales del usuario.
