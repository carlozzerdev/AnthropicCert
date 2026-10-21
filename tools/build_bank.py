"""Reproducible, offline content assembly. No scraping or AI calls at runtime."""
import collections
import hashlib
import json
import pathlib

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
    for i, item in enumerate(originals, 1):
        domain, text, answers, options, rationale, key = item
        questions.append(dict(id=f'original-{i:03}', revision=1, domainId=domain, text=text,
            correct=answers, options=[dict(id=chr(65+j), text=o[0], explanation=o[1]) for j, o in enumerate(options)],
            explanation=rationale, help=help_for(domain), references=[ref(key)],
            origin='Contenido original del proyecto AnthropicCert · 2026-10-21',
            review='Autoría y revisión técnica por Codex; sin revisión humana independiente. Referencia conceptual oficial enlazada.', language='es', family=f'original-{i:03}'))
    bank = dict(schemaVersion=1, id='ccarp-local', version='1.0.0', certification='CCAR-P',
        profileStatus='Perfil provisional de práctica, 63 preguntas/120 minutos. Datos de terceros; pendiente contraste directo con guía oficial.',
        domains=[dict(id=i, title=t, weight=w) for i,t,w in DOMAINS], questions=questions)
    (CONTENT / 'bank.json').write_text(json.dumps(bank, ensure_ascii=False, indent=2), encoding='utf-8')
    (CONTENT / 'Sources/review-report.json').write_text(json.dumps(dict(date='2026-10-21', reviewer='Codex; no independent human review',
        upstream_sha256=hashlib.sha256((CONTENT / 'Sources/professional-extract.txt').read_bytes()).hexdigest(),
        upstream_entries=80, active_imported=sum(r['status']=='active' for r in report), originals=len(originals), active_total=len(questions),
        items=report), ensure_ascii=False, indent=2), encoding='utf-8')
    print(f'Bank: {len(questions)} active; {len(originals)} original; domains: {dict(collections.Counter(q["domainId"] for q in questions))}')

if __name__ == '__main__': main()
