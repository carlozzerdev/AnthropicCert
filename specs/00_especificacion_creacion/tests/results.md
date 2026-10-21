# Resultados de validación · 1.0.0

Fecha: 2026-10-21. Implementación real creada y compilada. No se dan por completadas todas las variantes de aceptación manuales ni el objetivo editorial de 350 preguntas.

## Pruebas automatizadas
- **19 casos superados, 0 fallidos, 0 omitidos**, xUnit, configuración Release/net10.0. Evidencia: [acceptance.trx](../../../artifacts/tests/acceptance.trx).
- Validación de banco, certificación, IDs, respuestas y URLs; selección ponderada de 63 preguntas, empates e insuficiencia de contenido (A09/A10).
- Respuesta múltiple exacta con permutaciones, subconjuntos, exceso y vacío; conteos separados C/F/U y ejemplo 14/20 (A02/A03/A14).
- Confirmación bloqueada en práctica, descarte de selecciones no confirmadas y guardado (A02/A07/A15).
- Recuperación después de reiniciar, vencimiento tras tres días con duración limitada a 7.200 s y finalización idempotente (A06/A15).
- Snapshots y orden persistente frente a modificación del banco (A08).
- Segunda instancia, versión de datos incompatible, guardado con backup y corrupción explícita sin reinicializar (parte de A11).
- Fallo simulado de escritura no deja la respuesta confirmada ni el intento finalizado en memoria (A11).
- Global ponderado 83,3% frente a media por examen 70,0%, exclusión de abandonados, fechas/modos, cobertura única y grupos sin datos (A16/A17).
- Detección de retroceso del reloj en el servicio (A06).

## Interfaz y paquete
La QA integrada usa una carpeta de datos aislada y recorre inicio, selección, confirmación, explicación, persistencia, historial, filtros de estadísticas, biblioteca de ayudas y simulacro con soluciones ocultas. Se generan siete capturas y se revisan visualmente. Se corrigieron representación de ComboBox, ampersands, anchura de preguntas, espacio del gráfico y formato de tiempo acumulado.
Ventanas revisadas: principal 1240×850 y sesión a 1000×720, DPI actual del equipo. Las capturas son renderizados de los controles, no una prueba de lector de pantalla.

El script tools/verify_package.py verifica el ZIP final extraído en una ruta con espacios y acentos, hashes de manifiesto, ausencia de Data, smoke test y recorrido UI. Establece DOTNET_ROOT en una ruta inexistente y comprueba que runtime y ensamblado WinForms realmente se cargan desde la carpeta extraída. La evidencia concreta más reciente se localiza en [latest-package-verification.txt](../../../artifacts/latest-package-verification.txt); en esa carpeta están verification.json, runtime.json y ui/.

ZIP final verificado: 73.918.598 bytes, 494 archivos de manifiesto comprobados, estado PASS. SHA-256: `8bd6cec1dff37896320eb23269bf1a9b909b4e533b8eb2c2eb1cb074e77e57e3`. Evidencia: `artifacts/package-20261021-004221/verification.json`. Revisión de 17 documentos principales/specs: cero enlaces locales rotos.

Compilación Release sin advertencias ni errores. Runtime empaquetado Core/Desktop 10.0.12; SDK 10.0.401; Windows 11 del entorno. Se utiliza ensamblado offline de manifiestos del runtime, no se atribuye la entrega a un publish self-contained convencional. Se conservan LICENSE.txt y ThirdPartyNotices.txt del runtime instalado.

## Contenido
Banco activo: 78 preguntas (43 importadas MIT + 35 originales), distribuido por dominio: D1 12, D2 12, D3 15, D4 12, D5 11, D6 9, D7 7. Soporta las cuotas del simulacro provisional de 63.
Archivo de origen: 80 entradas de Professional. 37 excluidas del banco activo, con motivos en [review-report.json](../../../Content/Sources/review-report.json). Fuentes sin permiso de reproducción verificable solo como enlaces en el catálogo. No se han copiado bancos de pago ni preguntas del examen real.
Cada pregunta activa tiene ayuda local, respuesta, razonamiento, referencia conceptual oficial y procedencia. Revisión técnica por Codex; no revisión humana independiente. A18 está **parcial**, no se acredita contraste editorial independiente ni alineación completa con objetivos de guía oficial inaccesible.

## Verificaciones pendientes
- Windows 11 limpio sin .NET instalado: la resolución local del runtime está comprobada, pero no se dispone de VM limpia (A01 parcial).
- Matriz real de DPI 150/200%, lector de pantalla y recorrido exhaustivo por teclado (A12 parcial).
- Prueba manual con carpeta protegida y simulación física de pérdida de energía; se comprueban lock, atomicidad y fallos de escritura controlados, sin atribuirles esa cobertura total (A11 parcial).
- Disponibilidad real de cada URL desde el navegador del usuario; solo se permite HTTP/HTTPS y la aplicación mantiene ayudas offline (A04 parcial).
- Objetivos de rendimiento sobre un banco de 1.000 preguntas y equipo de referencia (sin medir).
- Ampliar a 350 preguntas, revisión humana y contraste directo de blueprint oficial (IMP08/DOC07 pendientes).

## Evidencias de la fase documental previa
Antes de implementar se revisaron enlaces relativos de 13, 14 y 15 documentos sin errores. Esas revisiones no se confundieron con pruebas de aplicación. La autorización posterior y las decisiones efectivas están en [implementation.md](../implementation.md).
