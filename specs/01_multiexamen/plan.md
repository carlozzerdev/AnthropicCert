# Plan técnico 01

Fecha: 2026-10-21. Estado: diseño inicial implementado con concreciones en [implementation.md](implementation.md). Base: [spec.md](spec.md), [data-model.md](data-model.md). La situación comprobada siguiente describe el código previo a 1.1.0.

## Situación comprobada
QuestionBank.Validate fija CCAR-P y schemaVersion=1 en src/AnthropicCert.Domain/Models.cs. ExamService recibe un único banco, fija 120 minutos y crea intentos con un Profile predeterminado de Professional. FileAttemptRepository exige schema-version.txt=1; los intentos carecen de certificación explícita. Es necesario propagar identidad, no limitarse a añadir un combo.

## Diseño
- Dominio: catálogo de identidades/perfiles y validación por perfil; selección y corrección compartidas. Identidades de preguntas compuestas por certificación, ID y revisión; dominios y ayudas limitados al banco. Evitar reglas duplicadas por certificación.
- Aplicación: consultar catálogo, elegir banco/perfil, crear y reanudar intentos, filtrar historial/estadísticas/errores por certificación explícita. Duración y cuotas desde perfil. Conservar un intento activo global salvo cambio posterior de MD07.
- Infraestructura: catálogo y JSON por banco; carga aislada y diagnósticos por entrada; repositorio de intentos compartido con migrador v1→v2. Catálogo inválido produce error claro. Conservar rutas relativas al ejecutable y bloqueo de Data.
- WinForms/MVP: selector e información del banco, actualización de dominios y métricas al cambiarlo; sesión ligada al snapshot. Formularios delegan casos de uso, sin reglas de puntuación ni migración.
- Herramientas editoriales: adaptar build_bank.py para producir y revisar por certificación, manifestar fuentes, revisión e idioma. Preservar fuentes archivadas y avisos de licencia.

## Secuencia
1. Inventariar contenido, contrastar guías y permisos; fijar fixtures y contrato versionado.
2. Implementar identidad/perfiles y migrador con recuperación probada.
3. Adaptar casos de uso y filtros; después selector y sesiones.
4. Traducir originales españolas y producir Foundations por lotes revisados; ampliar Professional por cobertura real.
5. Ejecutar aceptación pertinente y regresión 00, construir ZIP y validar extracción/actualización y entorno limpio disponible. Registrar límites reales.

## Migración y fallos
Antes de escribir, validar todos los registros y crear backup completo de Data (sin incluirlo en distribución). Preparar salida en staging, publicar versión final solo tras finalizar conversión y conservar recuperación determinista si se interrumpe. Repetir arranque no duplica ni transforma dos veces. No inferir Foundations de un texto libre Profile. Si no se reconoce una entrada v1, detener la migración sin modificar originales. Verificar reanudación, deadline, puntuación y lectura de histórico sin banco instalado.

## Riesgos
Los recursos web cambian entre lecturas indexadas; fijar revisión o hash antes de contar. Idioma declarado no demuestra texto inglés: revisión editorial obligatoria. Un banco grande no garantiza cuotas ni calidad. Una guía accesible no da permiso para redistribuir sus muestras o PDF.
