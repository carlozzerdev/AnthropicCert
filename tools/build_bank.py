"""Reproducible, offline content assembly. No scraping or AI calls at runtime."""
import collections
import hashlib
import json
import pathlib
import re

ROOT = pathlib.Path(__file__).resolve().parents[1]
CONTENT = ROOT / 'Content'
DOMAINS = [
    ('D1', 'Solution Design & Architecture', 17),
    ('D2', 'Claude Models, Prompting & Context Engineering', 13),
    ('D3', 'Integration', 19),
    ('D4', 'Evaluation, Testing & Optimization', 16),
    ('D5', 'Governance, Safety & Risk Management', 14),
    ('D6', 'Stakeholder Communication & Lifecycle Management', 14),
    ('D7', 'Developer Productivity & Operational Enablement', 7),
]
REFS = {
 'agents': ('Patrones de agentes y workflows', 'https://www.anthropic.com/research/building-effective-agents', 'When (and when not) to use agents; workflows'),
 'context': ('Ingeniería de contexto', 'https://www.anthropic.com/research/effective-context-engineering-for-ai-agents', 'The anatomy of effective context; retrieval'),
 'eval': ('Criterios y evaluaciones', 'https://platform.claude.com/docs/en/test-and-evaluate/develop-tests', 'Define success criteria; build evaluations'),
 'tools': ('Uso de herramientas', 'https://platform.claude.com/docs/en/agents-and-tools/tool-use/overview', 'Client and server tools'),
 'memory': ('Memoria de proyecto', 'https://code.claude.com/docs/en/memory', 'CLAUDE.md; shared instructions'),
 'cache': ('Prompt caching', 'https://platform.claude.com/docs/en/build-with-claude/prompt-caching', 'Cache prompt prefixes; cache_control'),
 'rag': ('Citas en resultados propios', 'https://platform.claude.com/docs/en/build-with-claude/search-results', 'How it works; citation control'),
 'guard': ('Defensas y entradas no fiables', 'https://platform.claude.com/docs/en/test-and-evaluate/strengthen-guardrails/mitigate-jailbreaks', 'Guardrails and input controls'),
}
HELP = {
 'D1': ('Elegir una arquitectura por sus requisitos', 'Comienza por la tarea, las restricciones y el criterio de éxito. Usa pasos predefinidos cuando el camino se conoce; autonomía cuando hace falta decidirlo durante la ejecución.\n\nCómo decidir: compara alternativas con calidad medida, coste, latencia, control y mantenimiento.\nError habitual: añadir agentes o componentes sin una necesidad demostrable.\nEjemplo: extracción y validación con un revisor final puede resolverse con un workflow, sin un agente que invente pasos.'),
 'D2': ('Diseñar el contexto y las instrucciones', 'El contexto debe contener información útil para la tarea, no todos los datos disponibles. Define instrucciones claras, ejemplos representativos y estado de trabajo necesario.\n\nCómo decidir: identifica si falla la recuperación de información, las instrucciones o la capacidad del modelo y compara con evaluaciones.\nError habitual: aumentar el contexto o el modelo sin diagnosticar el fallo.\nEjemplo: conserva decisiones y tareas pendientes al resumir una sesión larga; no conserves únicamente las últimas frases.'),
 'D3': ('Integración con límites claros', 'Un servicio integra identidad, validación de entradas, contratos de herramientas y tratamiento de errores. La selección de herramientas debe corresponder al trabajo autorizado.\n\nCómo decidir: revisa frontera, autenticación, permisos y observabilidad de cada llamada.\nError habitual: asumir que cifrado o un prompt sustituye autorizaciones.\nEjemplo: una herramienta que lee pedidos no necesita poder borrarlos; un trabajo largo puede devolver un ID para consultar el resultado.'),
 'D4': ('Evaluar el sistema antes de cambiarlo', 'Define qué significa éxito, con casos representativos y fallos importantes. Escoge verificaciones objetivas cuando existan y rúbricas para calidad subjetiva.\n\nCómo decidir: analiza errores por grupo y conserva casos de regresión.\nError habitual: tratar una demostración o un promedio como prueba suficiente.\nEjemplo: separa facturas escaneadas de PDFs digitales para descubrir qué grupo causa los errores y valida la mejora en datos no usados al ajustar.'),
 'D5': ('Reducir autoridad y verificar decisiones', 'Trata documentos recuperados y respuestas del modelo como datos que pueden contener errores o instrucciones hostiles. Los permisos, validaciones y autorizaciones se aplican fuera del modelo.\n\nCómo decidir: estima consecuencias y reversibilidad; reserva confirmación humana para acciones que la política exige.\nError habitual: equiparar confianza verbal del modelo con prueba.\nEjemplo: una solicitud de reembolso requiere autorización del usuario autenticado antes de ejecutar la transferencia, aunque el modelo proponga hacerla.'),
 'D6': ('Comunicar límites y mantener el servicio', 'Relaciona resultados medidos con una decisión de negocio. Expón límites, alternativas y responsables de mantenimiento.\n\nCómo decidir: acuerda el objetivo y explica qué fallos quedan, su impacto y el siguiente paso.\nError habitual: prometer automatización total o esconder grupos problemáticos dentro del promedio.\nEjemplo: un piloto que ahorra tiempo puede ampliarse por etapas con seguimiento, responsable de incidencias y una vía de vuelta al proceso anterior.'),
 'D7': ('Hacer reproducible la ayuda al desarrollo', 'El contexto de repositorio, las convenciones compartidas y las pruebas hacen reproducible el trabajo asistido. Mantén las instrucciones junto al proyecto y revísalas cuando cambie.\n\nCómo decidir: mide mejoras en entrega y calidad, no solo cantidad de llamadas.\nError habitual: instrucciones personales divergentes sin una base común.\nEjemplo: documenta el comando de pruebas y las reglas del servicio en CLAUDE.md, sin secretos, y valida los cambios como cualquier otra contribución.'),
}
DEFAULT_REF = {'D1': 'agents', 'D2': 'context', 'D3': 'tools', 'D4': 'eval', 'D5': 'guard', 'D6': 'eval', 'D7': 'memory'}

def ref(key):
    title, url, section = REFS[key]
    return dict(title=title, url=url, section=section)

def help_for(domain):
    title, text = HELP[domain]
    return dict(id='help-' + domain, title=title, domainId=domain, text=text, references=[ref(DEFAULT_REF[domain])])

def read_upstream():
    raw = (CONTENT / 'Sources/professional-extract.txt').read_text(encoding='utf-8')
    return json.loads('[' + raw.rsplit(']', 1)[0] + ']')

def main():
    upstream = read_upstream()
    assert len(upstream) == 80
    (CONTENT / 'Sources/professional-upstream.json').write_text(json.dumps(upstream, ensure_ascii=False, indent=2), encoding='utf-8')
    selected = {'practice': set(range(1, 36)) - {8, 11, 18, 26}, 'mock1': {1, 7, 11, 12, 13, 15}, 'mock2': set(), 'mock3': {1, 2, 5, 6, 7, 8}}
    concerns = {
        'practice-8': 'La eliminación de PHI no queda garantizada por una capa genérica; exige requisitos y revisión especializada.',
        'practice-11': 'La afirmación de inferencia dentro de la cuenta AWS no queda demostrada por elegir Bedrock; requiere verificar fronteras y servicio.',
        'practice-18': 'La afirmación universal sobre regulación y garantías requiere especificar el control concreto, no una conclusión legal genérica.',
        'practice-26': 'Responsabilidad legal demasiado absoluta sin contexto ni jurisdicción; no publicar como regla universal.',
        'mock1-6': 'Mismo problema de garantía de desidentificación que practice-8.',
        'mock1-9': 'Referencia normativa específica no revisada; necesita contraste con fuente legal primaria y alcance explícito.',
        'mock2-1': 'Variante de practice-11: la frontera de inferencia no se prueba con la mera elección de Bedrock.',
        'mock2-5': 'Variante de practice-18, sin precisar el control normativo.',
        'mock2-10': 'Variante de practice-26, responsabilidad jurídica sin matices.',
        'mock3-3': 'Residencia formulada como solución universal sin definir procesamiento, almacenamiento, registros ni políticas del servicio.'
    }
    domain_by_title = {title: ident for ident, title, _ in DOMAINS}
    questions, report = [], []
    for source in upstream:
        parts = source['id'].split('-'); group, number = parts[-2], int(parts[-1])
        active = number in selected[group]
        reason = 'activa: revisión técnica por agente, sin revisión humana independiente'
        if not active:
            reason = concerns.get(f'{group}-{number}', 'Variante semántica de un concepto y decisión ya representados en las preguntas practice; se conserva en archivo, no aumenta el banco activo.')
        report.append(dict(id=source['id'], status='active' if active else 'archived', reason=reason))
        if not active: continue
        domain = domain_by_title[source['domain']]
        key = 'cache' if source['id'].endswith('mock1-11') else DEFAULT_REF[domain]
        # Preserve upstream rationale for every option, including the explicit contrast it contains.
        options = [dict(id=k, text=v, explanation=source['rationale']) for k, v in source['options'].items()]
        wording = source['question']
        if source['id'].endswith('practice-14'):
            wording += ' The legacy client can poll a separate status endpoint using a job identifier.'
        questions.append(dict(id=source['id'], revision=1, domainId=domain, text=wording, options=options,
            correct=[source['answer']], explanation=source['rationale'], help=help_for(domain), references=[ref(key)],
            origin='Amey Thakur · CLAUDE-CERTIFICATIONS · MIT · adaptación del banco recuperado 2026-10-21',
            review='Revisión técnica por Codex 2026-10-21; fuente técnica por concepto. No es validación oficial ni revisión humana independiente.',
            language='en', family=source['id']))
    originals = json.loads((CONTENT / 'original-questions.json').read_text(encoding='utf-8'))
    translations = json.loads((CONTENT / 'original-questions-en.json').read_text(encoding='utf-8'))
    assert len(translations) == len(originals) == 35
    for i, item in enumerate(originals, 1):
        domain, text, answers, options, rationale, key = item
        english_text, english_options = translations[i-1]
        assert len(english_options) == len(options)
        questions.append(dict(id=f'original-{i:03}', revision=2, domainId=domain, text=english_text,
            correct=answers, options=[dict(id=chr(65+j), text=english_options[j], explanation=o[1]) for j, o in enumerate(options)],
            explanation=rationale, help=help_for(domain), references=[ref(key)],
            origin='Contenido original del proyecto AnthropicCert · 2026-10-21 · traducido de español a inglés en revisión 2',
            review='Autoría, traducción y revisión técnica por Codex; sin revisión humana independiente. Referencia conceptual oficial enlazada.', language='en', family=f'original-{i:03}'))
    for q in questions:
        q.update(objectiveId=q['domainId']+'.local', license='MIT' if not q['id'].startswith('original-') else 'Original del proyecto',
                 sourceUrl='https://github.com/Amey-Thakur/CLAUDE-CERTIFICATIONS' if not q['id'].startswith('original-') else 'project:AnthropicCert')
    additions = json.loads((CONTENT/'Sources/professional-additions.json').read_text(encoding='utf-8'))
    for i, (domain, text, correct, options, key) in enumerate(additions, 1):
        questions.append(dict(id=f'professional-extra-{i:03}', revision=1, domainId=domain, objectiveId=domain+'.local', text=text,
            correct=correct, options=[dict(id=chr(65+j),text=t,explanation=r) for j,(t,r) in enumerate(options)],
            explanation=' '.join(f'{chr(65+j)}: {r}' for j,(_,r) in enumerate(options)), help=help_for(domain), references=[ref(key)],
            origin='Original AnthropicCert · inglés · ampliación 01 · 2026-10-21',sourceUrl='project:AnthropicCert',license='Original del proyecto',
            review='Autoría y revisión técnica/editorial Codex 2026-10-21; no revisión humana independiente; objetivo conceptual local.',
            language='en',family=f'professional-extra-{i:03}'))
    bank = dict(schemaVersion=2, id='ccarp-local', version='1.1.0', certification='CCAR-P',
        profileId='ccarp-practice', profileVersion='1', blueprintVersion='provisional-2026-10-21',
        mockQuestionCount=63, durationMinutes=120, objectives=[d+'.local' for d,_,_ in DOMAINS],
        profileStatus='Perfil provisional de práctica, 63 preguntas/120 minutos. Datos de terceros; pendiente contraste directo con guía oficial.',
        domains=[dict(id=i, title=t, weight=w) for i,t,w in DOMAINS], questions=questions)
    (CONTENT / 'bank.json').write_text(json.dumps(bank, ensure_ascii=False, indent=2), encoding='utf-8')
    (CONTENT / 'Sources/review-report.json').write_text(json.dumps(dict(date='2026-10-21', reviewer='Codex; no independent human review',
        upstream_sha256=hashlib.sha256((CONTENT / 'Sources/professional-extract.txt').read_bytes()).hexdigest(),
        upstream_entries=80, active_imported=sum(r['status']=='active' for r in report), originals=len(originals)+len(additions), active_total=len(questions),
        items=report), ensure_ascii=False, indent=2), encoding='utf-8')
    print(f'Professional: {len(questions)} active; {len(originals)+len(additions)} original; domains: {dict(collections.Counter(q["domainId"] for q in questions))}')
    build_foundations()


FOUNDATIONS_DOMAINS = [
    ('F1', 'Agentic Architecture & Orchestration', 27),
    ('F2', 'Tool Design & MCP Integration', 18),
    ('F3', 'Claude Code Configuration & Workflows', 20),
    ('F4', 'Prompt Engineering & Structured Output', 20),
    ('F5', 'Context Management & Reliability', 15),
]
FOUNDATIONS_REFS = {
 'F1': ref('agents'), 'F2': ref('tools'), 'F3': ref('memory'),
 'F4': dict(title='Structured outputs', url='https://platform.claude.com/docs/en/build-with-claude/structured-outputs', section='JSON outputs; strict tool use; validation'),
 'F5': ref('context'),
}
FOUNDATIONS_HELP = {
 'F1': ('Orquestar con estado y límites', 'Define la dependencia entre pasos antes de delegar. Un flujo fijo usa transiciones explícitas; un agente decide sus pasos cuando la tarea lo exige. Cada ejecución necesita un límite de trabajo, condición de salida y tratamiento del fallo.\n\nCriterio: paraleliza solo tareas independientes; pasa resultados y fuentes al siguiente paso. Error habitual: asumir que los agentes comparten contexto automáticamente. Ejemplo: dos análisis independientes devuelven hallazgos estructurados al coordinador; este verifica ambos antes de sintetizar, sin ejecutar una escritura sin aprobación.'),
 'F2': ('Contratos y fronteras de herramientas', 'La descripción guía la selección, el esquema describe entradas y el código aplica autorización y validación. Un resultado de herramienta debe distinguir éxito vacío, datos válidos y error. MCP es una interfaz para capacidades, no permiso implícito para usarlas.\n\nCriterio: define qué identidad ejecuta cada operación y qué efectos permite. Error habitual: confiar en que el modelo no elegirá una operación peligrosa. Ejemplo: un lector de pedidos retorna páginas pequeñas e identificadores; una escritura verifica permisos e idempotencia en el servidor.'),
 'F3': ('Configurar trabajo reproducible', 'Guarda convenciones compartidas en memoria de proyecto versionada, instrucciones específicas con alcance y secretos en un canal apropiado. Usa automatización no interactiva para tareas acotadas con salida verificable. Los hooks ejecutan código en eventos; una instrucción escrita no equivale a un control obligatorio.\n\nCriterio: procedimiento reutilizable y trabajo con contexto aislado resuelven necesidades distintas. Error habitual: depender de la configuración personal de un desarrollador. Ejemplo: el repositorio documenta pruebas y convenciones; CI recibe el contexto necesario y valida la salida estructurada antes de publicarla.'),
 'F4': ('Diseñar y validar salidas', 'Especifica tarea, datos, ejemplos representativos y contrato de salida por separado. Representa ausencia y ambigüedad sin inventar hechos. Un esquema limita estructura; las reglas de negocio y la exactitud del contenido requieren controles adicionales.\n\nCriterio: usa salida estructurada o herramientas estrictas según el flujo y comprueba el resultado. Error habitual: confundir JSON válido con dato verdadero. Ejemplo: una fecha opcional puede ser null; una factura con total menor que cero se rechaza aunque cumpla el tipo numérico.'),
 'F5': ('Conservar contexto útil y detectar fallos', 'El contexto finito debe priorizar el objetivo, restricciones, decisiones y estado necesario. Guarda resultados voluminosos fuera del mensaje y pasa referencias con resúmenes trazables. La compactación es una transformación con pérdidas que exige conservar explícitamente información crítica.\n\nCriterio: mide éxito y fallos sobre entradas representativas, con límites de coste y tiempo. Error habitual: interpretar una respuesta segura como validada. Ejemplo: un trabajo conserva IDs de operaciones completadas y un checkpoint para reanudar sin repetir efectos.'),
}


def foundations_help(domain):
    title, text = FOUNDATIONS_HELP[domain]
    return dict(id='ccarf-help-'+domain, title=title, domainId=domain, text=text, references=[FOUNDATIONS_REFS[domain]])


def build_foundations():
    path = CONTENT / 'Sources/foundations-practice.md'
    raw = path.read_text(encoding='utf-8')
    pattern = r'\*\*(\d+)\. (.*?)\.\*\* (.*?)<details><summary>Answer and rationale</summary>(.*?)</details>'
    matches = re.findall(pattern, raw, re.S)
    assert len(matches) == 35
    questions, report = [], []
    for num, title, body, solution in matches:
        number = int(num)
        domain = next(d for d,t,_ in FOUNDATIONS_DOMAINS if t in title)
        if number == 13:
            report.append(dict(number=number, status='archived', reason='Misma decisión y control de terminación que la pregunta 1.'))
            continue
        wording = body.split('\n- A.')[0].strip()
        options = re.findall(r'^- ([A-D])\. (.+)$', body, re.M)
        answer, rationale = re.search(r'\*\*([A-D])\.\*\*\s*(.*)', solution.strip(), re.S).groups()
        if number == 23:
            # A tool schema alone is not a strict-output guarantee. Make the contract explicit.
            wording = 'An extraction pipeline must return schema-conforming tool input. Which approach combines a constrained generation contract with checks at the application boundary?'
            options[0] = ('A', 'Use a supported strict tool schema, force the extraction tool when appropriate, and validate the received input and business rules')
            rationale = 'Strict tool use constrains the tool input schema; forcing the intended tool makes the extraction path explicit. Application validation must still check business rules and handle refusals or incomplete output. Prompt-only requests (B), repairing arbitrary text (C), and temperature changes (D) do not provide the same structural contract.'
        if number == 32:
            rationale = 'In a growing run, stale context can obscure relevant material; inspect traces to confirm this diagnosis. A version change (A) requires evidence, temperature (C) does not directly identify context drift, and latency (D) measures speed rather than answer quality.'
        assert len(options) == 4
        references = [FOUNDATIONS_REFS[domain]]
        if number in (6,19): references.append(dict(title='Claude Code headless',url='https://code.claude.com/docs/en/headless',section='Programmatic use; structured output'))
        if number == 21: references.append(dict(title='Hooks',url='https://code.claude.com/docs/en/hooks',section='Hook lifecycle and deterministic controls'))
        questions.append(dict(id=f'amey-ccarf-{number:03}', revision=2 if number in (23,32) else 1,
            domainId=domain, objectiveId=domain+'.local', text=wording,
            options=[dict(id=k, text=v.strip(), explanation=rationale.strip()) for k,v in options],
            correct=[answer], explanation=rationale.strip(), help=foundations_help(domain), references=references,
            origin='Amey Thakur · CLAUDE-CERTIFICATIONS · Foundations practice · MIT · adaptación 2026-10-21',
            sourceUrl='https://raw.githubusercontent.com/Amey-Thakur/CLAUDE-CERTIFICATIONS/main/architect-foundations/practice-questions.md',
            license='MIT', language='en', family=f'amey-ccarf-{number:03}',
            review='Revisión técnica/editorial por Codex 2026-10-21; referencias oficiales por concepto; sin revisión humana independiente. Objetivo local, no transcripción oficial.'))
        report.append(dict(number=number, status='active', changes='Contrato estricto y validación explícitos' if number==23 else 'Matizada explicación del diagnóstico' if number==32 else 'Adaptación de formato; razonamiento del autor conservado'))
    originals_path = CONTENT / 'foundations-original.json'
    originals = json.loads(originals_path.read_text(encoding='utf-8')) if originals_path.exists() else []
    for i, item in enumerate(originals, 1):
        domain, text, correct, options = item
        explanation = ' '.join(f'{chr(65+j)}: {reason}' for j,(_,reason) in enumerate(options))
        questions.append(dict(id=f'ccarf-original-{i:03}', revision=1, domainId=domain, objectiveId=domain+'.local',
            text=text, options=[dict(id=chr(65+j), text=t, explanation=r) for j,(t,r) in enumerate(options)],
            correct=correct, explanation=explanation, help=foundations_help(domain), references=[FOUNDATIONS_REFS[domain]],
            origin='Original AnthropicCert · inglés · 2026-10-21', sourceUrl='project:AnthropicCert', license='Original del proyecto',
            language='en', family=f'ccarf-original-{i:03}', review='Autoría y revisión técnica/editorial Codex 2026-10-21; no revisión humana independiente; objetivo conceptual local.'))
    bank = dict(schemaVersion=2,id='ccarf-local',version='1.1.0',certification='CCAR-F',profileId='ccarf-practice',profileVersion='1',
        blueprintVersion='provisional-2026-10-21',mockQuestionCount=60,durationMinutes=120,
        profileStatus='Perfil provisional: datos oficiales indexados 60/120; guía exacta y objetivos pendientes de contraste directo. Pesos candidatos 27/18/20/20/15.',
        objectives=[d+'.local' for d,_,_ in FOUNDATIONS_DOMAINS],
        domains=[dict(id=d,title=t,weight=w) for d,t,w in FOUNDATIONS_DOMAINS],questions=questions)
    (CONTENT/'foundations.json').write_text(json.dumps(bank,ensure_ascii=False,indent=2),encoding='utf-8')
    catalog=dict(schemaVersion=1,entries=[dict(certification='CCAR-P',name='Architect – Professional',aliases=['CCA-P'],bankPath='bank.json',profileId='ccarp-practice'),
        dict(certification='CCAR-F',name='Architect – Foundations',aliases=['CCA-F'],bankPath='foundations.json',profileId='ccarf-practice')])
    (CONTENT/'exams.json').write_text(json.dumps(catalog,ensure_ascii=False,indent=2),encoding='utf-8')
    (CONTENT/'Sources/foundations-review.json').write_text(json.dumps(dict(date='2026-10-21',sourceSha256=hashlib.sha256(path.read_bytes()).hexdigest(),
        sourceUrl=questions[0]['sourceUrl'],license='MIT; copyright y aviso en LICENSE-Amey-Thakur.txt',sourceEntries=35,
        activeImported=34,originals=len(originals),activeTotal=len(questions),items=report,
        examtopics='30 candidatas locales de usuario en specs; no incluidas en banco activo ni ZIP'),ensure_ascii=False,indent=2),encoding='utf-8')
    print(f'Foundations: {len(questions)} active; domains: {dict(collections.Counter(q["domainId"] for q in questions))}')

if __name__ == '__main__': main()
