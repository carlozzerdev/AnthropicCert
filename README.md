# AnthropicCert

Aplicación WinForms **1.1.0** para practicar **Claude Certified Architect – Professional (CCAR-P) y Foundations (CCAR-F)** en Windows 11 x64. Funcionamiento local, sin cuentas ni API de IA. Aplicación independiente y no oficial. [Implementación 01](specs/01_multiexamen/implementation.md).

## Ejecutar

Extraer [AnthropicCert-1.1.0-win-x64.zip](releases/AnthropicCert-1.1.0-win-x64.zip) completo y abrir `AnthropicCert.exe`. Leer [LEEME.txt](LEEME.txt). Los intentos se guardan en `Data/` junto al ejecutable; conservar esa carpeta al actualizar. La publicación lleva el runtime incluido. Al actualizar desde 1.0.0 se conserva una copia previa y los originales v1; el formato nuevo usa `Data/attempts-v2`. Los textos y resultados históricos se conservan.

## Funciones

- Práctica por dominio, selección única/múltiple, confirmación y explicaciones.
- Selector de certificación; bancos, historial, ayudas, repaso y estadísticas separados.
- Simulacro provisional de 63 preguntas Professional o 60 Foundations, 120 minutos; cuotas por dominio, navegación y marcado.
- Guardado y recuperación de intentos, finalización por vencimiento y revisión histórica con snapshots.
- Historial y repaso de errores.
- Estadísticas filtradas: C/F/U, puntos, porcentajes, tiempos, dominios, cobertura y evolución.
- Ayudas conceptuales en español; enlaces técnicos opcionales.

## Contenido y límites

Professional: **85 preguntas** (43 adaptaciones MIT y 42 originales). Foundations: **64** (34 adaptaciones MIT y 30 originales). Las 80 entradas Professional recuperadas se conservan en [Content/Sources/professional-upstream.json](Content/Sources/professional-upstream.json); 37 se excluyen del banco activo por variantes o revisión pendiente. Informes de [Professional](Content/Sources/review-report.json) y [Foundations](Content/Sources/foundations-review.json). Las fuentes sin permiso de copia se conservan como enlaces en el [catálogo](Content/Sources/catalog.json). Las 30 preguntas ExamTopics aportadas se conservan como candidatas locales en specs; revisión y permiso de distribución pendientes, fuera del banco activo y del ZIP.

El objetivo de 350 Professional **sigue pendiente**. Las preguntas cuentan con revisión técnica del agente y referencias conceptuales, sin revisión humana independiente ni validación oficial. Enunciados y opciones nuevos en inglés; ayudas en español y explicaciones según el idioma editorial del lote. Perfiles provisionales mientras no se contrasten las guías exactas mediante acceso directo; objetivos del contenido locales, no transcripción oficial. La nota de práctica no reproduce la escala oficial.

## Arquitectura

`Domain`: modelos, validación del banco, corrección y cuotas. `Application`: casos de uso, repositorios abstractos, reloj y agregación estadística. `Infrastructure`: JSON versionado y guardado atómico por intento con backup y bloqueo de carpeta. `Desktop`: WinForms y presentador del examen. No hay reglas de puntuación en los formularios.

SQLite se sustituyó por archivos JSON en esta entrega: sin dependencias de almacenamiento adicionales, suficientes para el historial local. Ver [decisiones de implementación](specs/00_especificacion_creacion/implementation.md).

## Construir y verificar

Requisitos del desarrollador: Windows, SDK .NET 10, componentes de escritorio; Python 3.11 solo para ensamblar contenido y empaquetar. El usuario final no necesita estas herramientas.

```powershell
python tools/build_bank.py
dotnet restore AnthropicCert.sln
dotnet build AnthropicCert.sln -c Release --no-restore
dotnet test tests/AnthropicCert.Tests -c Release --no-restore
python tools/build_windows.py
python tools/verify_package.py
```

La aplicación no tiene paquetes NuGet de producción. Las pruebas usan xUnit; en este entorno se restauraron desde copias de los paquetes ya instalados. `NuGet.offline.config` permite restaurar los proyectos sin dependencias con fuentes vacías; las pruebas necesitan su caché o un feed disponible.

El empaquetador utiliza el runtime instalado .NET 10 Core/Desktop x64, combina sus manifiestos y copia el host para producir una distribución autocontenida sin acceso a NuGet. Se registra la versión y los hashes. Es un procedimiento de empaquetado offline, distinto del `dotnet publish --self-contained true` convencional; debe comprobarse con el verificador y finalmente en una máquina limpia.

QA integrada (genera capturas y usa datos aislados):

```powershell
AnthropicCert.exe --ui-test C:\ruta\a\resultados-qa-nuevos
AnthropicCert.exe --smoke-test
```

No usar la misma carpeta de QA dos veces: contiene historial de prueba. Las pruebas, evidencias y limitaciones actuales se registran en [tests/results.md](specs/01_multiexamen/tests/results.md); las de 1.0.0 permanecen en la especificación 00.
