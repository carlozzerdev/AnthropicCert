# Modelo de datos 01

Fecha: 2026-10-21. Estado: modelo conceptual implementado en 1.1.0 con perfiles embebidos en bancos, catálogo y almacén de intentos v2; diferencias efectivas en [implementation.md](implementation.md). Los objetivos son etiquetas locales mientras no se contraste la guía exacta.

| Entidad | Campos y restricciones |
|---|---|
| ExamCatalog | schemaVersion, entries; IDs de certificación únicos, rutas locales relativas validadas dentro de Content |
| ExamDefinition | certificationId, displayName, aliases, bankPath, profileId; alias CCA-F resuelve a CCAR-F, nunca a Professional |
| ExamProfile | id, version, certificationId, blueprintVersion, verificationStatus, officialSources, mockQuestionCount, durationMinutes, domains/objectives y pesos |
| QuestionBank v2 | id, version, certificationId, profileId/profileVersion, language=en, questions; identidad coherente con catálogo/perfil |
| Question | ID/revisión existentes, dominio/objetivo, texto/opciones ingleses, selección correcta, explicaciones, ayuda, fuentes; procedencia de traducción, licencia y estados editoriales |
| Attempt v2 | certificationId, bankId/bankVersion, profileId/profileVersion, blueprintVersion, scoringVersion, perfil necesario para revisión, snapshots e información de intento existente |
| SourceRecord | URL, consultedOn, archivo/commit o hash, licencia/permiso y alcance, atribución, cambios, revisor/fecha y estado |

No identificar certificación a partir de BankVersion: ambas pueden llamarse 1.0.0. ID de pregunta igual en dos bancos no causa cruces. Snapshots conservan textos, idioma, opciones, claves, ayudas y revisión usados originalmente. Las ayudas pueden seguir en español y no se heredan de otra certificación por coincidencia de DomainId.

Migración: solo la procedencia reconocida v1 permite asignar CCAR-P; conservar GUID, timestamps, estados y respuestas. Versionar también el almacén, con backup y rollback antes de habilitar escritura nueva. Preguntas históricas españolas permanecen en snapshots; banco actual recibe revisión inglesa nueva.
