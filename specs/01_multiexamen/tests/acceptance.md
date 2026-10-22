# Aceptación 01

Fecha: 2026-10-21. Estado: implementación validada parcialmente mediante automatización; cobertura real en [results.md](results.md). No se dan por ejecutadas todas las variantes.

| ID | Requisitos | Escenario y resultado esperado |
|---|---|---|
| MA01 | M01, M04 | Selector muestra ambas certificaciones con sus versiones/recuentos. Iniciar cada una usa exclusivamente sus preguntas, dominios y perfil |
| MA02 | M02, M03 | Revisar todo el banco activo nuevo: enunciados, instrucciones y opciones en inglés real, no solo Language=en; originales españolas traducidas y revisadas, importadas conservan inglés. UI/ayudas mantienen idioma previsto |
| MA03 | M04, M11 | Con perfil Foundations confirmado y suficientes preguntas generar 60/120 según sus cuotas. Probar insuficiencia por dominio: bloquear simulacro y permitir práctica. No usar valores Professional ni presentar perfil provisional como oficial |
| MA04 | M05 | Con intentos de ambos exámenes y versiones, cada filtro produce historial, aciertos/fallos, cobertura, evolución y repaso solo del seleccionado. IDs iguales entre bancos no cruzan resultados ni ayudas |
| MA05 | M06 | Cambiar selector con intento activo: no mutarlo ni abandonarlo; reanudar identifica su examen y deadline. Iniciar otro informa del pendiente; finalizar/abandonar requiere acción explícita |
| MA06 | M06, M07 | Migrar v1 con intentos activos/finalizados/abandonados y snapshots españoles: backup previo, asignación reconocida CCAR-P, mismos IDs/respuestas/resultados/fechas; nuevo banco inglés no cambia histórico. Repetir migración no duplica |
| MA07 | M07 | Interrumpir migración y simular falta de espacio, escritura denegada, JSON corrupto y versión desconocida: originales/backup conservados, error visible y recuperación sin estado mixto aceptado |
| MA08 | M08 | Banco con certificación incoherente, idioma inválido, objetivo ajeno, IDs duplicados o referencia inválida queda deshabilitado con diagnóstico; otro válido funciona. Catálogo duplicado/ruta fuera de Content se rechaza |
| MA09 | M09, M10 | Lote con duplicados, revisiones, traducciones, licencia pendiente y preguntas disputadas: solo nuevas publicables elevan total por certificación; cada publicada tiene permiso, fuentes y explicación/distractores/ayuda revisados |
| MA10 | M10 | ExamTopics y otros bancos sin autorización permanecen como referencias externas; anuncios no aumentan el total. Avisos MIT acompañan solo el contenido al que se aplican |
| MA11 | M12 | Extraer ZIP sin Data en Windows 11 x64 limpio sin .NET externo; practicar/corregir ambos exámenes offline con explicaciones y ayudas; actualizar conserva progreso y migración verificable |
| MA12 | M01, M05, M06, M12 | Selector, sesiones y textos ingleses largos accesibles por teclado y DPI 100/150/200%; revisar histórico con banco actual ausente usando snapshot; ejecutar regresión pertinente de corrección, reloj, guardado y métricas 00 |

Los fixtures de perfiles no acreditan formato oficial. MA03 depende de guía contrastada para afirmar fidelidad oficial. Registrar automatización, inspección editorial y pruebas manuales por separado en results.md.
