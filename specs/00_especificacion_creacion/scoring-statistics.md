# Puntuación y estadísticas

Estado: requisito confirmado por el usuario el 2026-10-21. Implementado en ExamRules y Statistics; véanse [implementación](implementation.md) y [evidencias](tests/results.md) para alcance y verificaciones pendientes.

## Resultado de cada examen
Cada pregunta vale 1 punto. Respuesta correcta: 1; incorrecta o sin responder: 0. En selección múltiple solo puntúa el conjunto exacto; no hay puntos parciales ni penalización. Las opciones son unidades de selección, no unidades estadísticas: una pregunta múltiple cuenta una sola vez.

| Métrica | Definición |
|---|---|
| Total de preguntas N | Número de preguntas congeladas al crear el intento |
| Aciertos C | Preguntas cuya selección coincide con el conjunto correcto |
| Fallos F | Preguntas con selección no vacía que no coincide con la correcta |
| Sin responder U | Preguntas sin selección al finalizar |
| Contestadas | C + F |
| Puntuación total | C puntos de N posibles |
| Nota sobre 100 | 100 × C / N, mostrada también como porcentaje de aciertos |
| Porcentaje de fallos | 100 × F / N |
| Porcentaje sin responder | 100 × U / N |
| Precisión en contestadas | 100 × C / (C + F); «Sin datos» si no hay contestadas |
| Tiempo transcurrido | Desde inicio hasta finalización efectiva, incluyendo cierres y suspensión según la política de reloj |
| Media de tiempo por pregunta | Tiempo transcurrido / N; incluye todas las preguntas, no supone seguimiento de tiempo individual |

Invariante: C + F + U = N. No se crean intentos con N = 0. Si aparecen datos antiguos inconsistentes, señalar error y excluirlos de agregados, sin corregirlos silenciosamente. Mostrar porcentajes con un decimal, calculados desde enteros sin redondeos intermedios; la suma de los porcentajes mostrados puede diferir de 100 por redondeo.

Ejemplo de aceptación: N=20, C=14, F=4, U=2: 14/20 puntos, 70,0/100, aciertos 70,0%, fallos 20,0%, sin responder 10,0%, precisión en contestadas 77,8%. En 30 minutos: media de 1 min 30 s por pregunta.

No convertir la nota sobre 100 en puntuación oficial ni usarla para declarar aprobado real. La distribución por dominios afecta a la selección, no aplica una segunda ponderación al resultado de cada pregunta.

## Pantalla de resultados e historial
Al finalizar se muestra resumen, desglose por dominio y acceso a revisión de aciertos, fallos y sin responder. Cada fila del historial muestra fecha local, modo, perfil, versión del banco, estado, C/F/U, puntos, nota y tiempo. Guardar fechas UTC y presentarlas en horario local.
Durante un simulacro activo solo se muestran progreso de contestadas, pendientes, marcadas y tiempo; no aciertos, fallos, soluciones ni nota provisional. En práctica se puede mostrar puntuación parcial de preguntas ya confirmadas, claramente etiquetada como parcial. Una selección no confirmada no revela corrección; al finalizar una práctica se consideran sin responder las preguntas no confirmadas y se descartan sus selecciones provisionales para el cálculo.

Los abandonados se conservan con su estado y progreso, pero sin resultado final y fuera de medias. Si el usuario quiere puntuación debe finalizar. El agotamiento del tiempo finaliza automáticamente. En un simulacro cronometrado la finalización efectiva es el vencimiento cuando este precede a la finalización manual; volver a abrirlo días después no añade días al tiempo del examen.

## Panel estadístico
Filtros: intervalo de fechas de finalización, modo (práctica/simulacro), perfil y versión, banco y versión, y usuario si finalmente hay perfiles. Valores por defecto: todos los días y simulacros finalizados del perfil y banco actuales. Si no hay datos, mostrar estado vacío; no sustituirlos por otro grupo sin avisar.
- Número de exámenes finalizados, preguntas evaluadas y suma de C/F/U.
- Porcentaje global ponderado por preguntas: 100 × suma(C) / suma(N). Ejemplo: 1/2 y 9/10 producen 10/12 = 83,3%, no 70,0%.
- Media de notas por examen, mejor y última nota, con sus etiquetas diferenciadas del porcentaje global.
- Tiempo total y media por examen; media por pregunta = suma(tiempos) / suma(N).
- Evolución cronológica de notas sobre 100, con fecha, tamaño del examen y modo visibles. No conectar grupos de distinto perfil, reglas o versiones sin identificarlos; comparar práctica y simulacro en series separadas.
- Tabla por dominio: preguntas evaluadas, C/F/U y porcentaje de aciertos. Dominio sin preguntas evaluadas: «Sin datos», no 0%.
- Cobertura del banco: preguntas distintas evaluadas / preguntas publicadas elegibles del banco y versión filtrados. Los repetidos suman práctica, pero no cobertura única. Si se seleccionan varias versiones, no combinar esa cobertura en una cifra ambigua; mostrarla por versión.
- Áreas de repaso: dominios ordenados por menor porcentaje de aciertos; en empate, mayor cantidad de fallos y luego ID de dominio. Mostrar tamaño de muestra y marcar «Muestra reducida» si tiene menos de 10 preguntas evaluadas. Relacionar con sus ayudas locales.

Todas las métricas respetan filtros y muestran cantidad de intentos/preguntas usados. Sin intentos elegibles: contadores en cero y porcentajes, medias y gráficos en «Sin datos». No inferir probabilidad de aprobar ni dominio de un tema únicamente por repetir preguntas conocidas.

## Arquitectura y persistencia
Un servicio de dominio define corrección y conteos; un caso de uso de aplicación calcula agregados sobre resultados finales persistidos. WinForms solo presenta los valores. Fórmulas y redondeos no se duplican entre pantallas, historial y gráficos.
Guardar versión de reglas de puntuación, resultado final y clasificación por pregunta junto a los snapshots. Agregados derivados de esos resultados; no corregir el historial con el banco vigente. Cambiar filtros no modifica datos. Una finalización repetida no añade otro examen a estadísticas. No se necesita guardar métricas derivables en tablas independientes en la primera versión.
