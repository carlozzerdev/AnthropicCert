# Resultados

Fecha: 2026-10-27. Revisión técnica/editorial Codex; sin revisión humana independiente.

- `python tools/import_foundations_epub.py`: 15 preguntas, cuatro opciones y clave explícita; IDs únicos. A-L01 superado.
- `python tools/build_bank.py`: 85 Professional, 79 Foundations (F1=18, F2=11, F3=21, F4=15, F5=14).
- `dotnet test tests/AnthropicCert.Tests -c Release --no-restore`: 37/37 superadas; incluye validación real de ambos bancos, cuotas y regresión de intentos/migración. A-L02 superado. Una ejecución inicial detectó procedencia local incompatible; se corrigió usando el contrato project y se repitió la suite completa con éxito.
- `dotnet build src/AnthropicCert.Desktop -c Release --no-restore`: correcto, cero avisos y errores.
- A-L03: claves contrastadas con las explicaciones del libro; conceptos revisados con las fuentes siguientes. No se verifica autenticidad de preguntas de examen ni una guía oficial exacta.

Fuentes conceptuales consultadas 2026-10-27:

- https://code.claude.com/docs/en/memory (cap. 6, alcance/importación/compactación).
- https://code.claude.com/docs/en/skills (cap. 7, fork y argument-hint; este último es una pista de autocompletado).
- https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/claude-prompting-best-practices (cap. 7/9, ejemplos y criterios explícitos).
- https://www.anthropic.com/research/effective-context-engineering-for-ai-agents (cap. 11, compactación y notas persistentes).
- https://platform.claude.com/docs/en/agents-and-tools/tool-use/tool-search-tool (cap. 11 Q3, carga diferida de herramientas).

No se ha generado un nuevo ZIP ni realizado QA visual o prueba en Windows limpio en esta ampliación de contenido.

A-L04: ejecutados `python tools/build_bank.py --distribution` y comprobación de 64 preguntas sin IDs `book-ccaf-`; después reconstrucción local con 79 y nueva compilación Desktop correcta. La exclusión en el empaquetador se revisó en código; no se ha ejecutado el empaquetado completo en esta tarea.
