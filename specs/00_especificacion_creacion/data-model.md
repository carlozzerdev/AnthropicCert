# Modelo de datos y contratos propuestos

Estado: diseño inicial. Contratos concretos en Domain/Models.cs; persistencia JSON por intento sustituye SQLite en la primera versión, según [ADR-002](implementation.md).

## Banco JSON versionado
Bank: `schemaVersion`, `bankId`, `version`, `certificationCode`, `blueprintVersion`, `languages`, `questions`, `sources` y metadatos de distribución.
ExamProfile: ID, código CCAR-P, versión, estado de verificación, URL oficial, fecha de consulta, número de preguntas, duración y dominios con pesos. Pesos suman 100; IDs de dominio estables. Valores aún no confirmados se etiquetan provisionales.
Question: ID estable, revisión, dominio, objetivos del blueprint, idioma del enunciado y de explicación, escenario, enunciado, tipo (`single`/`multiple`), opciones con ID estable, conjunto de IDs correctos, cantidad a seleccionar si aplica, explicación global y por opción, IDs de ayudas relacionadas, fuentes editoriales obligatorias, enlaces de ampliación opcionales, autoría/procedencia, permiso/licencia y estado editorial.
StudyHelp: ID estable, revisión, dominio/objetivos, título, idioma, explicación conceptual, criterios de aplicación, errores habituales, ejemplo aplicado, fuentes editoriales y enlaces opcionales. Reutilizable por varias preguntas; contenido disponible localmente. Los snapshots del intento conservan las revisiones de ayudas asociadas para evitar divergencias con su corrección histórica.
Reference: ID, título, URL HTTP/HTTPS, editor, sección relevante, fecha de consulta, estado de verificación y relación con la explicación. Distinguir referencia técnica de fuente de la pregunta y permiso para copiarla.

Validaciones: IDs únicos, opciones no vacías, respuestas existentes y cardinalidad coherente, explicación por opción, al menos una fuente oficial editorial revisada, al menos una ayuda relacionada existente, dominio reconocido, código de certificación compatible, versión de esquema soportada y estado publicado solo tras revisión editorial. La lista de enlaces de ampliación puede estar vacía. Rechazar banco inválido sin aplicarlo parcialmente. Diferenciar enlace sintácticamente válido de enlace consultado y de evidencia que respalda la respuesta.

## Progreso SQLite
Attempt: ID, modo, estado (`inProgress`, `completed`, `abandoned`), fecha UTC de inicio/final, vencimiento si procede, perfil y versión, banco y versión, orden de preguntas/opciones, snapshots del contenido usado y resultado final.
AttemptResult: versión de reglas de puntuación, total, aciertos, fallos, sin responder, puntos obtenidos/posibles y tiempo transcurrido. Clasificación por pregunta y dominio conforme a sus snapshots. Nota y porcentajes derivados sin almacenar versiones redondeadas. Resultado ausente en intentos activos o abandonados.
Answer: intento, pregunta/revisión, IDs seleccionados, confirmada, marca de revisión y momento de guardado. Unicidad por intento/pregunta.
Settings: versión y preferencias; Profile solo si se acuerdan varios usuarios.
SchemaMigration: versión aplicada y fecha; registro del backup previo fuera de la transacción de migración.

## Contratos de comportamiento
- StartPractice(filtros, cantidad): devuelve intento o insuficiencia de contenido; sin duplicados.
- StartMock(perfil): valida cobertura y congela preguntas, opciones, revisión y vencimiento.
- SaveAnswer(intento, pregunta, IDs): valida pertenencia, estado y reglas de modo; persiste antes de confirmar éxito a la UI.
- ConfirmPracticeAnswer: bloquea selección y devuelve corrección; no disponible como consulta de solución durante simulacro activo.
- CompleteAttempt: idempotente; congela resultado calculado por el mismo servicio de dominio que usa práctica.
- ResumeAttempt: recupera estado guardado; si venció el plazo, finaliza una sola vez.
- GetReview: devuelve snapshots y explicaciones del intento finalizado; nunca recalcula historial con la revisión actual del banco.
- OpenReference: solo por acción del usuario y con esquema HTTP/HTTPS; error de navegador no afecta al intento.
- GetStudyHelp: recupera ayuda local y su versión; respeta el bloqueo de soluciones del simulacro activo. En revisión histórica usa las revisiones guardadas en el intento.
- GetAttemptResult: devuelve resumen y desglose persistidos de un intento finalizado; respeta la ocultación de soluciones y notas de simulacros activos.
- GetStatistics(filtros): agrega solo resultados finales elegibles, devuelve tamaño de muestra, valores por dominio, series temporales y cobertura por banco/versión según [scoring-statistics.md](scoring-statistics.md). Sin datos no produce porcentajes ficticios.

## Actualización y errores
Aplicar contenido válido conservando el historial anterior. ID de pregunta constante con revisión nueva no reescribe intentos antiguos. Migración fallida conserva backup y permite recuperación sin reinicialización destructiva. JSON y SQLite no contienen código ejecutable; no renderizar HTML arbitrario ni ejecutar scripts incluidos en preguntas.
