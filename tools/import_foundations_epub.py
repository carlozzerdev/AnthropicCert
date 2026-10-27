"""Importación offline del EPUB aportado para uso local; no concede redistribución."""
import hashlib
import json
from pathlib import Path
import re
import xml.etree.ElementTree as ET
import zipfile

ROOT = Path(__file__).resolve().parents[1]
BOOK = ROOT / 'books/Claude Certified Architect Foundations.epub'
OUTPUT = ROOT / 'local-content/foundations-epub.json'


def extract(path=BOOK):
    questions = []
    chapter = None
    with zipfile.ZipFile(path) as archive:
        opf = ET.fromstring(archive.read('content.opf'))
        ns = {'o': 'http://www.idpf.org/2007/opf'}
        manifest = {x.attrib['id']: x.attrib['href'] for x in opf.findall('o:manifest/o:item', ns)}
        for item in opf.findall('o:spine/o:itemref', ns):
            name = manifest[item.attrib['idref']]
            doc = ET.fromstring(archive.read(name))
            paragraphs = [' '.join(''.join(p.itertext()).split()) for p in doc.iter()
                          if (p.tag.split('}')[-1] in ('p', 'li', 'h1', 'h2', 'h3') or (p.tag.split('}')[-1] == 'div' and not list(p)))
                          and not any(c.tag.split('}')[-1] == 'p' for c in p.iter() if c is not p)]
            text = '\n'.join(paragraphs)
            heading = re.search(r'^Chapter (\d+):', text, re.M)
            if heading:
                chapter = int(heading[1])
            starts = list(re.finditer(r'^(?:Q|Question )(\d+)\.\s+', text, re.M))
            for index, match in enumerate(starts):
                body = text[match.end():starts[index+1].start() if index+1 < len(starts) else len(text)]
                solution = re.search(r'Correct(?: answer)?:\s*([A-D])\.\s*(.*?)(?:\n\s*\n|\nKey Takeaways|\nChoosing the Intervention|$)', body, re.S)
                if not solution:
                    raise ValueError(f'Clave ausente: {name} Q{match[1]}')
                prompt = body[:solution.start()]
                choices = list(re.finditer(r'(?:^|\s)([A-D])[.)][\s\ufffd]*', prompt))
                if [m[1] for m in choices] != list('ABCD'):
                    raise ValueError(f'Opciones incompletas/ambiguas: {name} Q{match[1]}')
                options = [{'id': m[1], 'text': prompt[m.end():choices[i+1].start() if i+1 < len(choices) else len(prompt)].strip()}
                           for i, m in enumerate(choices)]
                rationale = solution[2].strip().split('\n')[0]
                ident = f'book-ccaf-ch{chapter:02}-q{int(match[1]):02}'
                questions.append(dict(id=ident, chapter=chapter, number=int(match[1]), sourceFile=name,
                                      text=prompt[:choices[0].start()].strip(), options=options,
                                      correct=[solution[1]], explanation=rationale))
    return questions


def main():
    items = extract()
    # El libro confunde una pista de autocompletado con validación obligatoria.
    for q in items:
        q['changes'] = []
        if q['chapter'] == 7 and q['number'] == 3:
            q['text'] = 'A developer wants a refactoring skill to show the expected target-file argument during slash-command autocomplete. Which frontmatter field provides this hint?'
            q['explanation'] = 'C: argument-hint displays expected arguments during autocomplete. It does not enforce required arguments or prevent execution with missing input; the skill must explicitly check for missing arguments. A isolates execution, B concerns tool permissions, and D describes the skill and its activation.'
            q['changes'].append('Corregida premisa: argument-hint no valida ni exige parámetros; documentación oficial skills consultada 2026-10-27.')
        if q['chapter'] == 7 and q['number'] == 4:
            q['text'] += ' The team wants to invoke the procedure explicitly on demand, rather than load it automatically when matching files are read.'
            q['changes'].append('Aclarada invocación a demanda para distinguir la respuesta B de reglas por rutas (A).')
        if q['chapter'] == 11 and q['number'] == 1:
            q['text'] += ' A trace confirms that history was progressively summarized and that the account number is missing from the resulting summary.'
            q['changes'].append('Añadida evidencia en la traza para evitar inferir una causa única del olvido sin diagnóstico.')
        if q['chapter'] == 7 and q['number'] == 5:
            q['explanation'] = 'B: Concrete input/output examples, including edge cases, demonstrate the intended transformation when prose admits multiple interpretations. A asks for more requirements but does not demonstrate the mapping; C plans the implementation; D can still leave interpretation gaps. The example count is illustrative, not an official or universally optimal range.'
            q['changes'].append('Eliminada atribución no contrastada de una cantidad óptima de ejemplos a la guía oficial.')
        if q['chapter'] == 9:
            q['explanation'] = q['explanation'].replace('which the exam guide explicitly identifies as failing to improve precision', 'which do not clarify the required rule categories').replace('The exam guide specifies temporarily disabling', 'Temporarily disable').replace('The exam guide specifies that ', '')
            q['explanation'] = q['explanation'].replace('A delays the trust restoration.', 'This restores useful feedback while criteria are improved. A delays the trust restoration.').replace('C increases the example count toward the 6+ range that bloats prompts.', 'C adds examples without necessarily clarifying the ambiguous decision boundary.')
            q['changes'].append('Explicación conceptual sin atribuir afirmaciones o umbrales a una guía oficial no contrastada.')
        assert len(q['options']) == 4 and all(o['text'] for o in q['options'])
        assert len(q['correct']) == 1 and q['correct'][0] in [o['id'] for o in q['options']]
        assert q['text'] and q['explanation']
    assert len({q['id'] for q in items}) == len(items)
    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    OUTPUT.write_text(json.dumps(dict(source=BOOK.relative_to(ROOT).as_posix(),
        sha256=hashlib.sha256(BOOK.read_bytes()).hexdigest(), date='2026-10-27',
        author='Written by Claude Code Instructed by V.Korostyshevskiy',
        permission='Importación local solicitada por el usuario; licencia de redistribución no verificada; excluir de ZIP público.',
        questions=items), ensure_ascii=False, indent=2), encoding='utf-8')
    print(f'EPUB: {len(items)} preguntas con cuatro opciones y clave explícita; {OUTPUT}')


if __name__ == '__main__':
    main()
