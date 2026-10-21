# 00 · Creación de la aplicación de práctica

Estado: implementación autorizada el 2026-10-21; primera versión creada con límites y pendientes en [implementation.md](implementation.md). Fecha inicial: 2026-10-20.
Nombre provisional: AnthropicCert Practice (pendiente; aplicación independiente, no oficial).

## Problema y objetivo
Preparar y ayudar a aprobar CCAR-P desde una aplicación WinForms para Windows 11 que se pueda compartir como ZIP. El objetivo es aprender los conceptos y el razonamiento al practicar exámenes, leer explicaciones y ayudas dentro de la aplicación y repasar errores. Las URLs son ampliación opcional; no deben ser necesarias para entender la corrección. No se garantiza aprobar el examen real.

## Requisitos confirmados por el usuario
- R01: aplicación portable Windows 11 distribuible como ZIP.
- R02: realizar exámenes de práctica.
- R03: consultar las respuestas correctas y explicaciones de cada pregunta.
- R04: poder consultar URLs relacionadas como ampliación opcional; no exigir su presencia en cada pregunta ni abrirlas para aprender o completar un intento. Mantener trazabilidad editorial de las fuentes por separado.
- R05: arquitectura limpia, presentación separada de funcionalidad y DRY.
- R06: desarrollo guiado por specs numeradas y ampliables; fase documental inicial completada, implementación solicitada el 2026-10-21.
- R07: investigar la certificación indicada, ExamTopics, otros sitios y documentación Anthropic.
- R19: usar WinForms como tecnología de presentación (confirmado el 2026-10-21).
- R20: proporcionar ayudas conceptuales locales y explicaciones autosuficientes orientadas a comprender y preparar la certificación. Vincular cada pregunta a la ayuda del concepto correspondiente.
- R21: cada examen finalizado tendrá puntuación total y estadísticas de aciertos, fallos y preguntas sin responder (confirmado el 2026-10-21). Las reglas concretas se detallan en [scoring-statistics.md](scoring-statistics.md).
- R22: permitir consultar estadísticas guardadas entre intentos para evaluar el progreso. Se incluyen resultados por dominio, tiempo, evolución y áreas de repaso según el diseño de [scoring-statistics.md](scoring-statistics.md), elaborado dentro del alcance delegado por el usuario.
- R23: ampliar el banco manteniendo contraste y fiabilidad por pregunta. El objetivo de producción pasa a 350 preguntas publicables, condicionado a revisión completa; ver [question-bank-expansion.md](question-bank-expansion.md). No contabilizar anuncios de terceros ni candidatos como preguntas disponibles.

## Alcance inicial propuesto, pendiente de respuestas
- R08: funcionamiento sin conexión; enlaces externos abiertos en el navegador del sistema.
- R09: interfaz en español; propuesta de preguntas en inglés y explicaciones en español.
- R10: práctica filtrada por dominio, con corrección al confirmar una respuesta.
- R11: simulacro cronometrado, sin revelar soluciones hasta su finalización, navegación y marcado para revisar.
- R12: historial local y resultados por dominio incluidos por R22; repaso de errores como modo de práctica sigue propuesto.
- R13: un usuario local; progreso dentro de `Data/` junto al ejecutable.
- R14: banco desacoplado de la aplicación, versionado, validado y con procedencia editorial.
- R15: soporte de selección única y múltiple; coincidencia exacta del conjunto para puntuar, sin penalización por errores. Otros formatos dependen de verificar la guía oficial.
- R16: inicializar intentos sin repetir preguntas dentro del mismo intento. Si falta contenido, informar y ofrecer una práctica reducida; no fingir un simulacro completo.
- R17: guardar respuestas y estado para recuperación; proteger historial frente a actualizaciones del banco mediante revisiones y snapshots.
- R18: distribuir inicialmente win-x64; ARM64 requiere decisión y paquete propio.

## Historias y aceptación
| Historia | Resultado observable | Pruebas |
|---|---|---|
| H01 Extraer y ejecutar | Inicia en Windows 11 limpio sin instalar .NET ni elevar permisos | A01, A11 |
| H02 Estudiar | Confirmar respuesta muestra selección, solución, explicación y referencias | A02, A03, A04 |
| H03 Simular | Cronómetro, navegación, revisión y finalización con soluciones ocultas durante el intento | A05, A06 |
| H04 Revisar progreso | Consultar intentos y practicar fallos sin alterar los originales | A07, A08 |
| H05 Mantener contenido | Bancos inválidos se rechazan con diagnóstico y sin pérdida de datos | A09, A10 |
| H06 Usar con comodidad | Navegación por teclado y lectura correcta con DPI alto | A12 |
| H07 Aprender para la certificación | Tras corregir, consultar una ayuda local que explica el concepto, cuándo aplicarlo y un ejemplo; funciona sin URLs | A13 |
| H08 Conocer resultados | Ver aciertos, fallos, sin responder, puntos, porcentaje y tiempos de cada examen finalizado | A14, A15 |
| H09 Evaluar evolución | Filtrar historial, consultar resultados por dominio y localizar áreas de repaso con muestras visibles | A16, A17 |

## Reglas de examen propuestas
El perfil del examen almacena número de preguntas, duración, dominios, pesos y fuente; no se incrustan estos valores en la UI. Hasta disponer de evidencia oficial accesible, cualquier perfil basado en terceros se etiqueta «provisional» y el modo se llama «simulacro de práctica».
Distribuir cuotas por mayor resto: parte entera de N × peso, y plazas restantes por fracción descendente con desempate por ID de dominio. No sustituir silenciosamente un dominio sin suficientes preguntas. Guardar IDs y orden seleccionados para reproducir el intento.
Una pregunta vale un punto si el conjunto seleccionado coincide exactamente con el correcto; no contestada vale cero. Porcentaje = aciertos / total × 100. Mostrar este resultado como métrica de práctica, sin convertirlo a la escala oficial ni prometer aprobado real. Un umbral personal, si se añade, debe llamarse objetivo de práctica.
En práctica, confirmar bloquea la respuesta antes de revelar solución. El simulacro permite cambiarla hasta finalizar. La finalización, manual o por tiempo agotado, debe ser idempotente. Propuesta: reloj continúa al cerrar o suspender; guardar vencimiento UTC y detectar retrocesos del reloj, sin pretender seguridad de examen supervisado.
Las ayudas que podrían revelar la respuesta aparecen tras confirmar en práctica y tras finalizar en simulacro. Propuesta de navegación: biblioteca de ayudas por dominio accesible fuera de un simulacro activo. Cada ayuda desarrolla el concepto, criterios para elegir una solución, errores habituales y un ejemplo aplicado; las preguntas reutilizan ayudas por ID para evitar duplicación.

## Calidad y límites
Contenido y explicaciones disponibles localmente. Abrir referencias requiere conexión. Enlaces HTTP/HTTPS únicamente, sin ejecutar contenido del banco. Objetivos a medir: inicio en menos de 5 s y cambio de pregunta en menos de 200 ms con 1.000 preguntas, equipo de referencia a registrar antes de medir. Datos SQLite transaccionales; carpeta sin permiso de escritura genera mensaje claro y conserva los datos existentes.
El objetivo ampliado es 350 preguntas publicables, repartidas según blueprint confirmado; sustituye la propuesta de 140 y no es una promesa de contenido ya disponible. Combinar contenido original y adaptaciones con permiso compatible, según el inventario y controles de [question-bank-expansion.md](question-bank-expansion.md). Cada pregunta debe tener al menos una fuente oficial concreta revisada en sus metadatos editoriales, explicación por opción, ayuda conceptual relacionada y licencia/procedencia registrada. Los enlaces visibles de ampliación pueden estar vacíos sin invalidar la pregunta.

## Fuera del alcance inicial propuesto
Pagos, cuentas en nube, sincronización, API de IA en ejecución, scraping automático, compra de bancos, editor visual e importación desde cualquier formato arbitrario. El usuario puede ampliar este alcance; un banco JSON mantenido en el proyecto no equivale a un editor para usuarios.

## Pendientes
Ver `decisions.md` e [implementation.md](implementation.md). Implementación inicial adopta idiomas mixtos identificados, un usuario portable, win-x64 y repaso de errores. Siguen pendientes editor/importación, ampliación efectiva a 350, revisión humana independiente y contraste directo de la guía oficial. Historial y estadísticas forman parte del alcance solicitado.
