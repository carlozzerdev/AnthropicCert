# Investigación y registro de fuentes

Ampliación de búsqueda del 2026-10-21: [inventario, contraste piloto y objetivo de 350 preguntas](question-bank-expansion.md). Las cifras anunciadas por sitios externos no son cantidades verificadas para distribución.

Consulta: 2026-10-20. No se ha descargado ni incorporado ningún banco de preguntas. Las descripciones de proveedores son declaraciones de esos proveedores, no validaciones independientes de calidad o de correspondencia con el examen.

## Certificación oficial
| Fuente | Resultado | Uso permitido en esta especificación |
|---|---|---|
| [Certificación solicitada](https://anthropic-partners.skilljar.com/claude-certified-architect-professional-certification) | Acceso automatizado devuelve 403; dominio también bloqueado por robots | URL canónica aportada por usuario; contenido y guía no verificados |
| [FAQ de certificaciones](https://anthropic-partners.skilljar.com/page/faq-certifications) | Localizada en búsqueda; acceso directo 403 | Pista oficial para revisión posterior; no fijar reglas con fragmentos del buscador |
| [Claude Partner Network](https://www.anthropic.com/news/claude-partner-network) | Resultado oficial localizado | Contexto del programa; el anuncio inicial se refiere a Foundations y no valida el blueprint de Professional |

Pendiente: obtener desde acceso autorizado la guía oficial vigente de Professional, su versión, objetivos, pesos, duración, tipos de ítem y ejemplos de práctica. Si el usuario dispone de ella, podrá aportarla o indicar su enlace accesible. La inaccesibilidad no impide redactar arquitectura o requisitos generales.

## Recursos externos de práctica
| Fuente | Qué se observó | Estado para distribución |
|---|---|---|
| [ExamTopics CCAR-P](https://www.examtopics.com/exams/anthropic/ccar-p/) | URL solicitada; lectura devuelve 403 | No se inspeccionaron preguntas, respuestas, licencia ni calidad; conservar como enlace por evaluar |
| [Claude Mock Exams](https://www.claudemockexams.com/architect-professional) y [muestra](https://www.claudemockexams.com/architect-professional/sample-questions) | Sitio específico de Professional; muestra pública de 28 preguntas con explicaciones | Recurso de consulta; permiso de reproducción no verificado |
| [Szymon Paluch: práctica Professional](https://szymonpaluch.com/claude-certified-architect-professional-practice-exam) | Se presenta como práctica no oficial de 63 preguntas originales, con selección múltiple en parte del banco | No incorporar sin comprobar licencia o permiso |
| [Claude Test Prep: Professional](https://claudetestprep.com/architect-professional) | Se presenta como plataforma independiente; declara 270 preguntas y siete dominios | Consultar cobertura, no asumir autorización de copia |

Otros resultados encontrados correspondían a Foundations. Se excluyen del perfil Professional hasta poder justificar su relación con objetivos concretos.

### Blueprint provisional observado en terceros
Los sitios de práctica de Professional consultados describen 63 preguntas, 120 minutos y siete dominios. Esto NO confirma el formato oficial. La escala citada por un proveedor no debe convertirse en porcentaje de aciertos de esta aplicación.

| Dominio, sin asumir numeración oficial | Peso declarado por terceros |
|---|---|
| Solution Design & Architecture | 17% |
| Claude Models, Prompting & Context Engineering | 13% |
| Integration | 19% |
| Evaluation, Testing & Optimization | 16% |
| Governance, Safety & Risk Management | 14% |
| Stakeholder Communication & Lifecycle Management | 14% |
| Developer Productivity & Operational Enablement | 7% |

Se registran solo para orientar la investigación, no como configuración aceptada. No presentar ninguna de estas webs como recomendación de compra ni como fuente oficial.

## Fuentes técnicas oficiales Anthropic
- [Building effective agents](https://www.anthropic.com/engineering/building-effective-agents): patrones de agentes y workflows. El artículo advierte que parte del panorama de herramientas ha cambiado; verificar cada afirmación contra documentación vigente.
- [Effective context engineering for AI agents](https://www.anthropic.com/engineering/effective-context-engineering-for-ai-agents): gestión de contexto y sistemas con múltiples pasos; localizado en búsqueda oficial, lectura integral pendiente para preguntas concretas.
- [Tool use with Claude](https://platform.claude.com/docs/en/agents-and-tools/tool-use/overview): consultado; base para preguntas originales sobre herramientas e integración.
- [Demystifying evals for AI agents](https://www.anthropic.com/research/demystifying-evals-for-ai-agents): consultado; base conceptual para evaluación de sistemas y pruebas.
- [Índice de ingeniería](https://www.anthropic.com/engineering): ampliar búsqueda por objetivo del blueprint, sin suponer que todo artículo forma parte del temario.

Por cada pregunta futura registrar sección exacta, fecha y razonamiento respaldado. Un enlace general al índice no satisface por sí solo la revisión editorial.

## Evidencia técnica Microsoft
- Se consultaron documentación y tutorial de WPF para la comparación inicial. Esa propuesta queda sustituida por WinForms por decisión del usuario el 2026-10-21; no constituyen fundamento de la arquitectura actual. No se han realizado nuevas verificaciones técnicas web en esta revisión documental.
- [Versiones SDK y Visual Studio](https://learn.microsoft.com/en-us/dotnet/core/porting/versioning-sdk-msbuild-vs): `net10.0` soportado en Visual Studio 18.0 o posterior.
- [Publicación .NET](https://learn.microsoft.com/en-us/dotnet/core/deploying/): modos de publicación y runtime incluido.
- [Distribución Windows](https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/publish-first-app): publicación self-contained como carpeta apta para ZIP.

## Proceso editorial propuesto
1. Mapear cada objetivo de la guía oficial a documentación técnica oficial.
2. Escribir escenarios propios; opciones plausibles y una solución defendible según requisitos explícitos.
3. Registrar explicación de todas las opciones y referencias concretas.
4. Revisar ambigüedad, actualización técnica, traducción y derechos de distribución.
5. Publicar solo ítems revisados; mantener borradores y disputados fuera del banco activo.
6. Versionar las revisiones; preservar el contenido de intentos anteriores.
