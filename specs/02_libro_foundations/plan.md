# Plan

Estado: ejecutado, 2026-10-27.

1. Leer EPUB como ZIP/XML sin dependencias; respetar manifest/spine, texto inline y opciones en bloques div.
2. Fallar ante opciones incompletas o clave ausente; conservar extracción y modificaciones en `local-content/foundations-epub.json`.
3. Ensamblar opcionalmente el lote local desde build_bank; usar metadatos compatibles con el validador existente, sin modificar dominio o UI.
4. Excluir preguntas `book-ccaf-` del contenido del ZIP público durante empaquetado. `--distribution` permite generar explícitamente el banco base sin lote local.
5. Ejecutar reconstrucción offline, validación de bancos, pruebas .NET y compilación Desktop.
