# Plan técnico

Estado: diseño inicial con implementación 1.0.0. Las decisiones efectivas que sustituyen propuestas (JSON frente a SQLite y empaquetado offline) se registran en [implementation.md](implementation.md). Depende de: [spec.md](spec.md), [decisions.md](decisions.md).

## Tecnología recomendada
C# con .NET 10 y WinForms; objetivo futuro `net10.0-windows`. WinForms es elección confirmada del usuario; se propone MVP para separar formularios y presentación. Publicación self-contained para win-x64 en carpeta comprimida. No se exige un único EXE: el ZIP contendrá runtime, aplicación, contenido y licencias. Evitar trimming y Native AOT en la primera entrega; validar dependencias WinForms con la configuración real.

| Opción | Adecuación al proyecto | Decisión propuesta |
|---|---|---|
| C# / WPF | Alternativa considerada inicialmente | Descartada por instrucción del usuario |
| Python / PySide6 | Viable y rápido, exige empaquetado de Python/Qt y revisar licencias de distribución | Alternativa si el usuario prefiere Python |
| C# / WinForms | UI nativa Windows; separación mediante formularios, interfaces de vista y presentadores | Elegida por el usuario |
| Avalonia | Aporta plataformas adicionales que ahora no se requieren | Reconsiderar si aparece ese requisito |
| C++ / Qt | Viable, añade complejidad de construcción y distribución sin necesidad actual | No necesaria |
| Web / Electron | Posible, añade navegador empaquetado o servidor y otra pila | No preferida |

Estas valoraciones son decisiones de diseño, no resultados de benchmarks.

## Entorno comprobado
2026-10-20: SDK .NET 10.0.401, Visual Studio Community 2026 18.9.3 y carga `Microsoft.VisualStudio.Workload.ManagedDesktop` detectados; Python 3.11 disponible. En esa comprobación inicial aún no se habían creado proyectos; posteriormente se implementó la versión 1.0.0.
No se necesita C/C++ para la propuesta. Si se optara por C++/Qt, habría que verificar/instalar carga «Desarrollo para el escritorio con C++», MSVC, Windows SDK, CMake y Qt con su licencia; nada de esto se ha instalado.

## Dependencias y límites
- Domain: preguntas, ayudas conceptuales, conjuntos de respuestas, perfil, cálculo, selección y estados de intento. Sin WinForms, SQLite, reloj del sistema ni generador aleatorio global.
- Application → Domain: comenzar intento, responder, finalizar, recuperar y consultar resultados. Define puertos de repositorio, reloj, aleatoriedad y apertura de referencias.
- Infrastructure → Application y Domain: JSON, SQLite, rutas portables, migraciones y adaptadores del sistema operativo.
- Desktop → Application: formularios y UserControls WinForms implementan interfaces de vista; presentadores coordinan interacción y llaman casos de uso. Eventos delegan en presentadores. Punto de composición en Desktop conecta Infrastructure; sin reglas de corrección ni acceso a SQLite en formularios o presentadores. Los archivos Designer se limitan a composición visual.
- Tests futuros: dominio y aplicación con reloj/aleatoriedad controlables; integración con SQLite y archivos temporales; verificación manual de UI y ZIP.

Proyectos implementados: `src/AnthropicCert.Domain`, `.Application`, `.Infrastructure`, `.Desktop` y `tests/AnthropicCert.Tests`.
Propuestas de bibliotecas: Microsoft.Data.Sqlite y xUnit. MVP mediante interfaces y clases sencillas, sin biblioteca de MVVM. Revisar versiones compatibles, licencias y necesidad antes de incorporarlas; no añadir ORM, bus de eventos o contenedor complejo por defecto. JSON mediante System.Text.Json.

## Persistencia y distribución
Contenido en `Content/`, progreso en `Data/`, logs locales mínimos en `Data/Logs/`; resolver desde directorio de la aplicación, independiente del directorio de trabajo. Usuario debe extraer en carpeta escribible. SQLite no se distribuye con historial de desarrollo. Segunda instancia sobre el mismo Data se rechaza de forma clara para evitar sesiones concurrentes ambiguas.
Migraciones con versión, copia previa y recuperación; la copia del ZIP nuevo sobre una instalación debe preservar Data. Paquete limpio incluye EXE, runtime, banco autorizado, instrucciones de uso y avisos de terceros. Crear ZIP sin datos personales y generar manifiesto/hash de entrega. Empaquetado ARM64 separado si se solicita.

## Etapas futuras
1. Resolver decisiones de producto y verificar guía oficial; revisar este plan.
2. Definir contratos definitivos y fixtures de contenido; montar arquitectura.
3. Implementar banco, validación, corrección y selección; verificar reglas.
4. Añadir práctica, ayudas locales compartidas por concepto y navegación opcional de referencias. Las ayudas deben explicar cuándo aplicar cada concepto y sus errores habituales, sin depender de Internet.
5. Añadir simulacros, persistencia e historial según alcance acordado.
   Incluir puntuación, panel de estadísticas, evolución y áreas de repaso conforme a [scoring-statistics.md](scoring-statistics.md). Cálculos compartidos fuera de WinForms; resultados y gráficos no revelan notas de simulacros activos.
6. Revisar preguntas y accesibilidad; publicar y probar ZIP limpio.

Cada etapa se contrasta con [tests/acceptance.md](tests/acceptance.md). Estado real en [tasks.md](tasks.md); evidencias y pendientes en [tests/results.md](tests/results.md).

## Riesgos
Guía oficial inaccesible: no congelar pesos ni formatos. Bancos de terceros: revisar permisos antes de distribuir. Calidad pedagógica: revisión humana y trazabilidad de cada pregunta. Portabilidad: probar sistema limpio y rutas con espacios/acentos. Actualizaciones: preservar snapshots e historial. Carpeta protegida: fallo explícito sin cambiar silenciosamente el destino del progreso.
