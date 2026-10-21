# Implementación 1.0.0

Fecha: 2026-10-21. Autorización: el usuario pide «incorpora todas las preguntas al banco local e implementa la aplicación». Este documento actualiza los diseños que figuraban como propuestas en los documentos iniciales. No equivale a cerrar las partes pendientes del banco ni la verificación oficial del blueprint.

## Decisiones efectivas
- C#/.NET 10/WinForms, cuatro proyectos con dependencias hacia dominio/aplicación. Presentador de sesión separado del formulario. Historial y estadísticas usan casos de uso y reglas compartidas.
- Un usuario local, Data junto al ejecutable, sin cuenta ni red en ejecución salvo URLs opcionales.
- Interfaz y ayudas en español; preguntas importadas y sus explicaciones en inglés; preguntas originales en español. Se informa en inicio y documentación.
- Práctica, simulacro provisional 63/120, historial, repaso de errores y estadísticas implementados.
- Fecha/mode/perfil/versión filtran agregados. No se ofrece borrado de historial ni importación arbitraria desde la UI.
- Gráfico nativo WinForms con últimos 20 intentos, sin unir modos/versiones distintos. El historial permite consultar tamaño y fecha. No hay predictor de aprobado real.

## ADR-002 · Persistencia sin dependencias adicionales
Estado: adoptado en implementación, 2026-10-21.
La SQLite propuesta se sustituye por un JSON versionado por intento. La red desde terminal no permite descargar nuevas dependencias; el historial local no exige consultas SQL ni concurrencia entre usuarios. `IAttemptRepository` mantiene el límite arquitectónico para una sustitución futura.
Cada guardado escribe un temporal con flush, reemplaza atómicamente el destino y conserva `.bak`. Archivo de bloqueo impide dos instancias con la misma carpeta. Datos dañados o versión no compatible generan error explícito y se conservan. No hay migración de formatos previos porque no existía una aplicación anterior; futuras versiones deberán definirla. Los modelos de preguntas y ayudas quedan incluidos en snapshots del intento.
Se verifica rollback en memoria al fallar una confirmación/finalización. No se simula que JSON sea SQLite ni se afirma que existen transacciones entre múltiples intentos.

## Banco incorporado y límites
Se recuperan **las 80 entradas Professional** del banco textual MIT, conservadas sin filtrar en `Content/Sources/professional-upstream.json`. Un extracto trazable y su hash permiten reconstruir la importación offline. No se dispone de commit inmutable; la procedencia se identifica por URL, fecha y hash del contenido recuperado.
**43 importadas activas + 35 originales = 78 activas**. Otras 37 importadas permanecen archivadas por variante semántica o por requerir reformulación/contraste. No se incorporan como preguntas nuevas solo para aumentar la cifra. Los otros sitios quedan en `catalog.json`: no existe permiso de reproducción verificado. No se han comprado bancos ni eludido accesos.
Revisión: técnica por Codex, fuentes conceptuales consultadas y diagnóstico de ambigüedades; no validación oficial ni revisión humana independiente. Las explicaciones importadas se conservan y se muestran como razonamiento de las opciones; las originales incluyen explicación específica por opción.
**El objetivo de 350 no está cumplido**. Tampoco se han incorporado las muestras sin permiso de copia. El hecho de que se hayan solicitado todas las preguntas no convierte una cifra anunciada en un banco redistribuible y revisado. Estos límites se informan también en la aplicación y LEEME.

## ADR-003 · ZIP con runtime local, ensamblado offline
Estado: adoptado para este entorno.
Los proyectos de producción no requieren NuGet externo. El empaquetado publica la aplicación framework-dependent y añade las copias instaladas de Core/Desktop 10.0.12, hostfxr, hostpolicy y manifiestos combinados, con `includedFrameworks` en runtimeconfig. Se preservan avisos del runtime. Este procedimiento no se presenta como una ejecución del `publish --self-contained true` convencional.
El verificador comprueba hashes, ausencia de Data, extracción en ruta con espacios/acentos y que Core/WinForms se carguen desde la carpeta del paquete, con DOTNET_ROOT apuntando a un destino inexistente. La validación definitiva en Windows 11 limpio sin .NET instalado queda pendiente: no hay una VM limpia disponible en esta sesión.

## Reproducibilidad
Ver README y `tools/build_bank.py`, `tools/build_windows.py`, `tools/verify_package.py`. No se generan preguntas mediante IA en el equipo del usuario; el banco distribuido es estático. Cambios de contenido deben mantener revisión y trazabilidad. El runtime se fija a la versión concreta detectada y se registra en manifest.json.
