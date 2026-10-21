# Ampliación del banco: inventario y fiabilidad

Fecha: 2026-10-21. Petición del usuario: ampliar el banco con preguntas contrastadas y fiables. Se amplía el objetivo documental de 140 a **350 preguntas publicables** (+210 respecto a la propuesta inicial). Es un objetivo de producción y revisión, no una cantidad disponible o ya contrastada. No existe aún un banco local de preguntas publicadas; las 140 anteriores tampoco se habían creado.

**Actualización posterior de implementación:** ya existe un banco local con 78 activas (43 adaptadas MIT y 35 originales) y archivo completo de 80 entradas Professional. Los recuentos originales de este documento describen la investigación previa. El estado actual y las exclusiones están en [implementation.md](implementation.md) y `Content/Sources/review-report.json`. El objetivo de 350 sigue pendiente.

## Resultado de la búsqueda
| Fuente | Cantidad observada o declarada | Acceso y evaluación | Uso previsto |
|---|---|---|---|
| [Amey Thakur: preguntas Professional](https://raw.githubusercontent.com/Amey-Thakur/CLAUDE-CERTIFICATIONS/main/architect-professional/practice-questions.md) | 35 preguntas | Texto público con soluciones y razonamientos; no oficial | Primer lote candidato a adaptación y contraste |
| [Amey Thakur: mock 1](https://raw.githubusercontent.com/Amey-Thakur/CLAUDE-CERTIFICATIONS/main/architect-professional/mock-exam-1.md) | 15 preguntas | Texto público con clave; semejanzas conceptuales con el lote anterior requieren deduplicación | Candidatas, no sumar automáticamente como nuevas |
| [Claude Mock Exams: muestra](https://www.claudemockexams.com/architect-professional/sample-questions) | 28 preguntas | Preguntas y explicaciones visibles; permiso de redistribución no verificado | Consulta y contraste; no incorporación automática |
| [CertSafari: muestra](https://certsafari.com/anthropic/claude-architect-professional/practice-questions) | 35 visibles; anuncia banco de 456+ | Muestra leída; banco completo no auditado, permiso no verificado | Referencia secundaria, revisión individual necesaria |
| [Szymon Paluch](https://szymonpaluch.com/claude-certified-architect-professional-practice-exam) | Anuncia 63 | Página describe banco interactivo y presenta 3 ejemplos; no se han auditado las 63 | Recurso externo; no contabilizar las 63 como verificadas |
| [Learn With Darin](https://learn.techwithdarin.com/certs/claude-architect-professional/) | Anuncia 89 | Página describe banco interactivo; no se han inspeccionado los 89 ítems | Recurso externo por evaluar |
| [Claude Test Prep](https://claudetestprep.com/architect-professional) | Anuncia 270 | Índice por dominio; no se han auditado las 270 | Fuente de consulta por evaluar |
| [PrepClubs](https://prepclubs.com/free-practice-tests/claude-certified-architect-professional) | Anuncia diagnóstico de 25 | Página muestra 3 ejemplos y describe diagnóstico | No tratar el diagnóstico como preguntas adicionales únicas |
| [Courseiva](https://courseiva.com/certifications/anthropic-claude-architect-professional/practice-test) | Anuncia 262+ | Describe cuatro dominios, 90 minutos y otra puntuación, discrepantes con los recursos de siete dominios | Excluida de selección inicial hasta resolver discrepancias |
| [ExamTopics](https://www.examtopics.com/exams/anthropic/ccar-p/) | No comprobable | Acceso 403 | Ningún ítem contabilizado |

Hay **113 entradas textuales candidatas** en los cuatro primeros lotes (35+15+28+35), antes de deduplicar, revisar todas las respuestas y resolver permisos. Esto no equivale a 113 preguntas únicas ni publicables. Los 320 ítems anunciados por el repositorio GitHub abarcan cuatro certificaciones, no solo Professional. Los otros dos mocks del repositorio no se contabilizan: su lectura raw falló en esta consulta.

## Procedencia del lote GitHub
El [README del autor](https://github.com/Amey-Thakur/CLAUDE-CERTIFICATIONS) distingue texto propio MIT de documentos y logos reflejados con otra procedencia. La [licencia MIT](https://raw.githubusercontent.com/Amey-Thakur/CLAUDE-CERTIFICATIONS/main/LICENSE) fue consultada: conservar copyright y aviso al adaptar material propio cubierto. No extender esa licencia a PDFs de Anthropic ni a otros sitios. Antes de incorporar, fijar revisión/commit y registrar archivo de origen y cambios.

## Contraste piloto realizado
Se ha comprobado el fundamento técnico de tres ítems, sin reproducirlos ni declararlos publicados:

| Ítem | Evidencia oficial consultada | Resultado y límite |
|---|---|---|
| GitHub practice Q5: camino conocido frente a autonomía | [Building effective agents](https://www.anthropic.com/research/building-effective-agents), secciones de workflows y elección de complejidad | La distinción entre control predefinido y dinámico respalda la opción indicada. Evitar interpretar menor coste como garantía universal |
| GitHub practice Q6: evaluar calidad subjetiva con rúbrica | [Define success criteria and build evaluations](https://platform.claude.com/docs/en/test-and-evaluate/develop-tests), criterios medibles y escalas cualitativas | Respalda rúbricas y métodos adecuados al objetivo; revisión de distractores y adaptación pedagógica completa siguen pendientes |
| CertSafari Q1: atribución de fuentes de una base privada | [Search results](https://platform.claude.com/docs/en/build-with-claude/search-results), funcionamiento y esquema | Respalda usar resultados propios con metadatos y citas. Al redactar ayuda debe explicarse que las citas se habilitan; permiso de reproducción del ítem no verificado |

Este piloto demuestra una vía de contraste, no acredita la calidad de los bancos completos. Una respuesta plausible o repetida por varias webs no basta. La guía oficial de Professional sigue sin acceso directo; el PDF reflejado localizado en GitHub es una pista de procedencia, no una nueva verificación oficial realizada en esta revisión.

## Criterio para publicar cada pregunta
1. ID/revisión estable, objetivo de Professional y escenario con restricciones suficientes para una solución inequívoca.
2. Fuente oficial concreta, sección, fecha de consulta y explicación de cómo respalda la respuesta; especificar versión o plataforma si condicionan el resultado.
3. Justificación de la correcta y de cada distractor; ayuda conceptual local asociada.
4. Procedencia original o permiso compatible; atribución y cambios de adaptación registrados.
5. Revisión de ambigüedad y duplicación semántica: cambiar nombres, cifras o el orden de opciones no produce una pregunta nueva.
6. Estado separado: localizada → candidata → contrastada técnicamente → revisada editorialmente → publicable. Rechazada/disputada queda fuera del banco activo.
7. Registro del responsable, fecha, evidencias y pendientes de cada revisión. No atribuir revisión humana o independiente que no se haya realizado. Ítems cambiantes vuelven a revisión ante modificación de su fuente.

## Plan para llegar a 350
Priorizar candidatos del material propio con licencia verificada, someterlos al filtro anterior y completar huecos con escenarios originales fundamentados en documentación oficial. No fijar de antemano cuántos candidatos sobrevivirán. Crear y revisar por lotes de 25–50, con recuento real por estado y objetivo.
Los pesos de cobertura definitivos y la composición de simulacros dependen de confirmar la guía oficial. No rellenar una cuota con duplicados o preguntas ambiguas. Si no se alcanza la cantidad con la calidad exigida, declarar la cantidad efectiva y mantener pendientes los restantes; no etiquetar 350 como disponibles.

## Recuentos honestos a fecha de esta revisión
- Objetivo: 350 publicables; incremento planificado: 210 sobre la propuesta de 140.
- Entradas textuales candidatas inventariadas: 113, sin deduplicación completa.
- Ítems con fundamento técnico contrastado en el piloto: 3; no son una validación editorial integral.
- Preguntas incorporadas y listas para distribuir: 0. Continúa la fase de especificaciones.
