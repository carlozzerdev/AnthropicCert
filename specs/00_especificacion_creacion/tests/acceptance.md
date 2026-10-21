# Casos de aceptación

Estado: casos de especificación con verificaciones automatizadas y parciales de UI registradas en [results.md](results.md). No se consideran ejecutadas todas las variantes manuales. Las cifras de fixtures son datos de prueba y no describen el examen oficial.

| ID | Requisitos | Escenario y resultado esperado | Nivel futuro |
|---|---|---|---|
| A01 | R01, R08, R18 | Extraer ZIP en Windows 11 x64 limpio sin .NET/Python/VS, como usuario estándar y sin conexión; iniciar, practicar y corregir sin instalación | Paquete/manual |
| A02 | R02, R03, R10, R15 | Pregunta única con correcta B: B puntúa 1; A o vacía puntúan 0. Tras confirmar práctica se bloquea respuesta y se muestran explicación global y por opción | Dominio + UI |
| A03 | R03, R15 | Correctas A,C: solo A,C puntúa 1. A, A,B,C y vacía puntúan 0; orden C,A produce el mismo resultado. Un ID ajeno se rechaza | Dominio |
| A04 | R04, R08, R20 | Una pregunta sin enlaces de ampliación permite practicar, corregir y leer su ayuda local. Cuando hay enlaces, clic HTTP/HTTPS abre navegador. Sin conexión se conservan explicación y ayuda. Esquema ejecutable se rechaza | Integración + manual |
| A05 | R02, R11 | Iniciar simulacro, navegar, marcar y cambiar respuesta sin revelar soluciones. Finalizar muestra resultado y explicación de cada ítem. Repetir finalizar no duplica resultado | Aplicación + UI |
| A06 | R11, R17 | Reloj controlado alcanza vencimiento; intento se finaliza una vez. Cerrar y reabrir antes del plazo recupera estado; después del plazo finaliza. Suspender no regala tiempo | Aplicación + integración |
| A07 | R12, R17 | Completar 3/5 preguntas correctas: historial muestra 60%, fallos y desglose coherente. Reiniciar conserva estado. Repasar fallos crea otro intento sin modificar el original | Integración |
| A08 | R12, R14, R17 | Actualizar la revisión de una pregunta después de completar un intento; revisión histórica conserva contenido y resultado originales mediante snapshot | Integración |
| A09 | R14 | Bancos con ID duplicado, respuesta inexistente, explicación ausente, URL inválida, dominio ajeno, versión no soportada o certificación distinta se rechazan con diagnóstico sin sustituir el banco previo | Contratos + integración |
| A10 | R14, R16 | Perfil de prueba N=10, pesos 50/30/20: cuotas 5/3/2, sin duplicados. Otro perfil provoca restos empatados y usa desempate estable por ID. Dominio insuficiente informa y no genera un simulacro falso | Dominio |
| A11 | R01, R13, R17 | Ejecutar desde ruta con espacios/acentos y directorio de trabajo distinto; datos quedan junto al EXE. Probar carpeta no escribible, segunda instancia, interrupción de guardado y migración fallida: error claro, sin borrar historial; mover carpeta cerrada conserva datos | Integración + paquete |
| A12 | R05, R09 | Recorrer flujos con teclado, foco visible, escalado 100/150/200% y ventana de 1280×720; texto largo y español con acentos legibles, sin ocultar confirmar o navegación | Manual UI |
| A13 | R03, R19, R20 | En WinForms, confirmar en práctica permite leer explicación por opción y ayuda conceptual con criterios, errores y ejemplo sin abrir navegador. Durante simulacro activo no se accede a ayudas que revelen soluciones; al finalizar quedan disponibles. Actualizar una ayuda no altera su revisión histórica | Aplicación + integración + manual |
| A14 | R21 | Finalizar con N=20, C=14, F=4, U=2 y duración 30 min: 14/20 puntos, nota 70,0/100, fallos 20,0%, sin responder 10,0%, precisión 77,8% y media 90 s/pregunta. Verificar C+F+U=N, selección múltiple cuenta una sola pregunta y cero contestadas muestra precisión «Sin datos» | Dominio |
| A15 | R21, R11 | Simulacro activo oculta aciertos, fallos y nota incluso al abrir historial/panel. Finalización manual o por plazo guarda un solo resultado. Reabrir tras vencimiento no amplía duración; en práctica finalizada, selecciones no confirmadas cuentan como sin responder | Aplicación + integración |
| A16 | R22 | Intentos finalizados de 1/2 y 9/10: global 83,3%, media de notas 70,0%, mejor 90,0%. Activos y abandonados quedan excluidos. Cambiar filtros excluye fechas/modos/versiones ajenos y toda cifra respeta el mismo filtro; sin muestra, porcentajes «Sin datos» | Aplicación + integración |
| A17 | R22, R17 | Repetir preguntas aumenta evaluadas pero no cobertura única. Mostrar dominio sin evaluaciones como «Sin datos» y menos de 10 como «Muestra reducida». Evolución distingue modo/perfil/versiones. Reiniciar conserva métricas y actualizar banco no cambia resultados antiguos | Integración + manual |

## Comprobaciones adicionales de salida
- A18 (R23, revisión editorial): comprobar por cada pregunta publicada fuente oficial concreta, fundamento de respuesta y distractores, ayuda local, procedencia y estado de revisión; detectar duplicados semánticos. Verificar que el recuento de disponibles solo incluye publicables. Un lote meramente localizado o con licencia desconocida no aumenta el total; objetivo de 350 se informa como pendiente hasta alcanzarlo realmente.
- Corrección compartida por práctica y simulacro; dominio sin referencias a WinForms/SQLite. Formularios delegan en presentadores y casos de uso.
- Finalización con preguntas en blanco y cero aciertos; denominadores válidos y redondeo solo de presentación.
- Objetivos de rendimiento medidos con 1.000 preguntas; registrar CPU/RAM, versión de Windows, condiciones y tiempos.
- Cada pregunta distribuida tiene revisión, procedencia, permiso/licencia y al menos una referencia oficial que respalda la explicación; contar cobertura real por dominio.
- ZIP sin progreso, secretos ni datos de desarrollo; incluye runtime, instrucciones, contenido autorizado y avisos de terceros.
- Actualización de versión conserva Data y permite recuperar backup de migración.
- Si aparecen formatos oficiales distintos de selección única/múltiple, especificar y probar su tratamiento antes de afirmar fidelidad del simulacro.
