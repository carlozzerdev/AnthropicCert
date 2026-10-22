# Resultados 01

Fecha: 2026-10-21. Estado: implementación 1.1.0 creada y probada parcialmente. No se dan por ejecutadas todas las variantes manuales de MA01–MA12.

## Automatización 1.1.0

`dotnet test tests/AnthropicCert.Tests -c Release --no-restore --logger 'trx;LogFileName=spec01.trx' --results-directory artifacts/tests-spec01`: **37 superadas, 0 fallidas, 0 omitidas**. Evidencia: [spec01.trx](../../../artifacts/tests-spec01/spec01.trx). Incluye regresión 00 y casos nuevos de MultiExamTests.cs. Build Desktop Release completado con 0 errores y 0 advertencias; posterior publicación integrada en tools/build_windows.py.

| Escenario | Cobertura ejecutada y límite |
|---|---|
| MA01 | Catálogo con dos bancos válidos y selección de cuotas 63/60; identidad de intentos y perfiles probada |
| MA02 | Metadatos en inglés en todo banco y revisión editorial de traducciones; 35 claves originales sin cambios. Auditoría estructural no sustituye una revisión humana independiente |
| MA03 | Cuotas por dominio, duración desde perfil incluso modificada a 17 minutos en fixture, insuficiencia sin crear intento y práctica disponible; fidelidad oficial no acreditada |
| MA04 | IDs y versiones coincidentes entre certificaciones no mezclan errores, resultados ni cobertura; regresión de fechas/modos/versiones y métricas |
| MA05 | Intento activo global, selector no transforma el pendiente, reanudación preserva certificación y vencimiento |
| MA06 | Migración de activos/finalizados/abandonados, originales y backup byte a byte, textos españoles, respuestas, flags, resultados, fechas, GUID e idempotencia |
| MA07 | Staging incompleto, fallo controlado de preparación, procedencia desconocida y versión incompatible. No se simulan pérdida de energía real, disco físicamente lleno ni toda matriz de permisos NTFS |
| MA08 | Aislamiento de banco inválido, certificaciones duplicadas, ruta absoluta/traversal, idioma/objetivo/ayuda/licencia inválidos; regresión de IDs, respuestas y URLs |
| MA09 | 149 activas, cero duplicados exactos enunciados y ninguna pregunta compartida literalmente entre bancos; variante Foundations 13 archivada. Deduplicación semántica y revisión son del agente, sin revisión independiente |
| MA10 | ExamTopics excluido de los bancos/paquete, atribución y avisos MIT conservados; candidatas aportadas permanecen en specs |
| MA11 | Construcción ZIP y verificación de manifiesto/runtime/UI descrita abajo; Windows 11 limpio no disponible |
| MA12 | Revisión/reanudación sin banco histórico probada; QA integrada de ambos exámenes y ventanas al DPI actual. Matriz 150/200%, lector de pantalla y teclado exhaustivo pendientes |

Auditoría de contenido: [content-audit-spec01.json](../../../artifacts/content-audit-spec01.json). Professional 85 (43 MIT, 42 originales), Foundations 64 (34 MIT, 30 originales). Fuentes e informes en Content/Sources. No hay revisión humana independiente, validación oficial ni objetivo de 350 cumplido.

## Paquete e interfaz

ZIP [AnthropicCert-1.1.0-win-x64.zip](../../../releases/AnthropicCert-1.1.0-win-x64.zip), construido con runtime local y manifiesto mediante el mecanismo offline documentado. Verificación final **PASS**, 499 archivos de manifiesto comprobados, ambos bancos cargados, sin Data ni candidatas/capturas ExamTopics en el paquete. SHA-256: `dbbbb03bca62edc4b15998fc75944ada6ec916c1b594908c3c74021b89eaef86`.

Evidencia: [verification.json](../../../artifacts/package-20261021-224531/verification.json), [runtime.json](../../../artifacts/package-20261021-224531/runtime.json) y [resultado UI](../../../artifacts/package-20261021-224531/ui/result.txt). Se extrae en ruta con espacios/acentos y se ocultan instalaciones externas del entorno; runtime y WinForms cargan desde la carpeta extraída. Esto no acredita una máquina limpia.

La QA integrada recorre selector, ambas prácticas, confirmación, explicación, biblioteca de ayudas, historial filtrado, estadísticas y simulacros 63/60 con soluciones ocultas. Se generan 12 capturas al DPI actual; se revisan visualmente inicio y sesiones de ambos exámenes, estadísticas Foundations y ventana de simulacro reducida. Se detectó y corrigió una opción inglesa larga que se recortaba en 1000×720: ahora mide altura con salto de línea y la captura se revisa de nuevo. No se da por completada la matriz DPI/teclado.

Cierre: suite final repetida tras la corrección visual, 37/37 superadas. `git diff --check` sin incidencias. Comprobación final de 12 documentos (spec 01, README general e índice): cero enlaces locales rotos. ZIP final: 73.981.371 bytes; SHA-256 comprobado de nuevo y coincidente con la verificación anterior.

## Evidencias documentales previas

Actualización: se ejecutó `python tools/import_examtopics_capture.py` sobre el ZIP aportado. Resultado: tres MHTML leídos offline, 30 preguntas extraídas, diez por captura, números únicos 1–30 y claves sugeridas referidas a opciones existentes. Se comprobó decodificación sin caracteres de sustitución. Es validación estructural de extracción, no revisión de corrección ni aceptación del banco activo. Código de aplicación, banco activo y paquete de distribución sin cambios por esta extracción.

En la fase documental previa se revisaron documentos base y código de modelos, servicio de examen y persistencia, sin modificar entonces la aplicación. La implementación posterior autorizada se describe arriba y en [implementation.md](../implementation.md).

Validación documental ejecutada: 9 documentos presentes y todos los destinos de enlaces Markdown locales de 01 existentes; sin enlaces locales rotos. Revisión de codificación UTF-8 realizada. Esta comprobación no acredita aceptación funcional ni revisión editorial de preguntas.
