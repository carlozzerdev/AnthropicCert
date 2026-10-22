# Implementación 01 · 1.1.0

Fecha: 2026-10-21. Estado: implementada; comprobaciones y límites en [tests/results.md](tests/results.md). Autorización explícita: «implementa la especificacion 01». Sustituye el estado documental previo; las decisiones propuestas siguientes se concretan dentro del alcance solicitado.

## Resultado

Selector de Architect Professional (CCAR-P) y Architect Foundations (CCAR-F, alias CCA-F). Prácticas, simulacros, ayudas, repaso, historial y estadísticas separados por certificación. Enunciados, instrucciones de selección y opciones de intentos nuevos en inglés. UI y ayudas en español; explicaciones conservan el idioma editorial de cada lote. Reanudar o revisar utiliza snapshots y dominios del intento, aunque el banco actual falte.

Professional: **85 activas**, 43 adaptadas MIT y 42 originales. Las 35 originales anteriores reciben revisión 2 inglesa y se añaden siete originales. Foundations: **64 activas**, 34 adaptadas MIT y 30 originales. Del lote de 35 se archiva la 13 por decisión de terminación ya representada en la 1. Se matizan las explicaciones/contratos de 23 y 32. Hay cuotas suficientes para 63/60 preguntas sin repetir IDs. Revisión técnica/editorial Codex, no revisión humana independiente ni validación oficial.

## Contrato efectivo

`Content/exams.json` es el catálogo versionado; cada entrada vincula certificación, nombre, alias, ruta relativa y perfil. `bank.json` y `foundations.json` usan esquema 2. Los perfiles se mantienen embebidos en cada banco: ID, versión, blueprint, estado, número, minutos y dominios. Esto concreta el modelo conceptual sin duplicar un archivo adicional de perfiles. `ObjectiveId` enlaza etiquetas conceptuales locales por dominio; no se declara cobertura de los objetivos oficiales sin guía contrastada.

El catálogo rechaza identidades/alias duplicados y rutas fuera de Content o con enlaces de sistema de archivos. Un banco ausente o inválido queda pendiente con diagnóstico; otro válido sigue disponible. Con todos los bancos ausentes se permite consultar historial, con nuevas sesiones deshabilitadas. Se valida identidad banco/perfil y metadatos de preguntas, ayudas y procedencia.

`ExamService` selecciona banco explícitamente, captura identidad y perfil del intento y obtiene cantidad/duración del perfil. Persiste **un único intento activo global**: cambiar selector no lo abandona ni transforma. Reanudar conserva certificación y vencimiento; iniciar otro exige resolver explícitamente el pendiente. Las reglas de cuotas, puntuación y corrección permanecen compartidas fuera de WinForms.

## Migración v1 → v2

`IAttemptRepository` y guardado atómico se conservan. `Data/schema-version.txt` pasa a 2; nuevos intentos en `Data/attempts-v2`. Se reconoce únicamente procedencia v1 conocida (perfil Professional provisional v1 y banco 1.0.0, sin marcador de esquema por intento). Antes de publicar cambios se validan todos los registros y se copia el contenido previo —salvo lock y staging— a `Data/migration-v1-backup`, comprobando identidad de bytes.

Los originales v1 permanecen intactos. Se preparan archivos v2 completos y se publica el marcador de almacén mediante reemplazo atómico como último paso. Un fallo anterior conserva v1 como versión efectiva; reintentar vuelve a preparar los mismos GUID. No se usa el staging hasta confirmar la versión. Los snapshots, respuestas, estado, fechas, claves y resultados históricos no se traducen ni recalculan. Los dominios históricos sin título propio se muestran por ID, evitando depender del catálogo nuevo.

No es una transacción física frente a cualquier fallo de hardware. Falta prueba de corte eléctrico real; se prueban interrupciones y fallos controlados. Para volver a 1.0.0 se restaura la copia anterior completa; 1.0.0 no interpreta intentos nuevos v2.

## Contenido y fuentes

`tools/build_bank.py` ensambla ambos bancos offline, desde originales y fuentes locales con hashes e informes. La fuente Foundations se descargó del texto propio MIT de Amey Thakur, conservando aviso/copyright y registro hash en `foundations-review.json`. No se incluyen PDFs oficiales ni logos reflejados. Fuentes conceptuales oficiales consultadas el 2026-10-21: agentes/workflows, uso de herramientas, contexto, memoria de proyecto, hooks, ejecución no interactiva, salidas estructuradas y evaluaciones; URLs por pregunta/ayuda y en [research.md](research.md).

Las **30 ExamTopics** suministradas por el usuario permanecen transformadas como candidatas en specs, con respuestas sugeridas y votos separados. No están en banco activo ni ZIP: faltan contraste editorial completo y permiso de distribución verificado. No se cuenta ese lote entre las 149 activas.

## Límites conservados

- Perfiles **provisionales**: 63/120 Professional, 60/120 Foundations. Apertura de ambas páginas Skilljar devuelve 403; no se presenta una guía exacta como contrastada. Pesos Foundations candidatos 27/18/20/20/15; objetivos locales.
- El objetivo de 350 Professional sigue pendiente, y no se promete equivalencia con la puntuación escalada oficial ni aprobado.
- Sin API de IA en ejecución, servicios web, cuentas o descargas automáticas.
- Windows 11 limpio sin .NET y matriz completa DPI/teclado pendientes; resultados ejecutados y verificaciones de paquete se registran por separado.

## Distribución

ZIP `releases/AnthropicCert-1.1.0-win-x64.zip`, runtime local con el mecanismo offline de ADR-003 de 00, manifiesto y SHA-256. No lleva Data ni capturas/candidatas ExamTopics. [LEEME.txt](../../LEEME.txt) documenta inicio y migración. La validación app-local no se presenta como prueba en una VM limpia.
