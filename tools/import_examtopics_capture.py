"""Extract supplied MHTML captures offline; never executes their scripts or fetches URLs."""
import email
from email import policy
import hashlib
from html.parser import HTMLParser
import json
from pathlib import Path
import re
import zipfile


class Node:
    def __init__(self, tag='', attrs=()):
        self.tag, self.attrs, self.children = tag, dict(attrs), []

    def find(self, cls):
        found = []
        for child in self.children:
            if isinstance(child, Node):
                if cls in child.attrs.get('class', '').split():
                    found.append(child)
                found.extend(child.find(cls))
        return found

    def text(self):
        if self.tag in ('script', 'style'):
            return ''
        return ''.join(c.text() if isinstance(c, Node) else c for c in self.children)


class Tree(HTMLParser):
    def __init__(self):
        super().__init__(convert_charrefs=True)
        self.root = Node()
        self.stack = [self.root]

    def handle_starttag(self, tag, attrs):
        n = Node(tag, attrs)
        self.stack[-1].children.append(n)
        if tag not in ('area', 'base', 'br', 'col', 'embed', 'hr', 'img', 'input', 'link', 'meta', 'param', 'source', 'track', 'wbr'):
            self.stack.append(n)

    def handle_endtag(self, tag):
        for i in range(len(self.stack)-1, 0, -1):
            if self.stack[i].tag == tag:
                del self.stack[i:]
                break

    def handle_data(self, data):
        self.stack[-1].children.append(data)


def clean(n):
    return re.sub(r'\s+', ' ', n.text()).strip()


def extract(archive):
    questions, captures = [], []
    with zipfile.ZipFile(archive) as z:
        for name in sorted(z.namelist()):
            if not name.lower().endswith('.mhtml'):
                continue
            raw = z.read(name)
            message = email.message_from_bytes(raw, policy=policy.default)
            part = next(p for p in message.walk() if p.get_content_type() == 'text/html')
            # These captures contain UTF-8 bytes without a MIME charset.
            html = part.get_payload(decode=True).decode(part.get_content_charset() or 'utf-8', errors='strict')
            tree = Tree()
            tree.feed(html)
            cards = tree.root.find('exam-question-card')
            captures.append({'file': name, 'sha256': hashlib.sha256(raw).hexdigest(), 'questions': len(cards)})
            for card in cards:
                number = int(re.search(r'Question #(\d+)', clean(card)).group(1))
                body = card.find('question-body')[0]
                prompt = next(n for n in body.find('card-text') if 'question-answer' not in n.attrs.get('class', '').split())
                options = []
                for item in body.find('multi-choice-item'):
                    letter = item.find('multi-choice-letter')[0].attrs['data-choice-letter']
                    text = re.sub(r'^[A-Z]\.\s*', '', clean(item))
                    text = re.sub(r'\s*Most Voted$', '', text)
                    options.append({'id': letter, 'text': text})
                answer_text = clean(body.find('question-answer')[0])
                answer = re.search(r'Correct Answer:\s*([A-Z]+)', answer_text).group(1)
                charts = body.find('answer-chart')
                q = {'id': f'examtopics-ccaf-{number:03}', 'number': number,
                     'certification': 'CCAR-F', 'language': 'en', 'text': clean(prompt),
                     'options': options, 'sourceSuggestedCorrect': list(answer),
                     'communityVotes': json.loads(charts[0].attrs['data-chart']) if charts else [],
                     'capture': name, 'sourceItemId': body.attrs.get('data-id'),
                     'sourceUrl': 'https://www.examtopics.com/exams/anthropic/cca-f/view/',
                     'reviewStatus': 'pending-technical-and-editorial-review',
                     'distributionPermission': 'not-verified', 'active': False}
                assert q['text'] and len(options) >= 2
                assert set(answer) <= {o['id'] for o in options}
                assert '\ufffd' not in json.dumps(q, ensure_ascii=False)
                questions.append(q)
    assert len({q['number'] for q in questions}) == len(questions), 'Duplicate question numbers'
    return {'format': 'examtopics-user-capture-candidates-v1',
            'archiveSha256': hashlib.sha256(archive.read_bytes()).hexdigest(),
            'origin': 'user-supplied archive', 'captures': captures,
            'note': 'Source answers are unverified. Not a production bank or distribution permission.',
            'questions': sorted(questions, key=lambda q: q['number'])}


if __name__ == '__main__':
    archive = Path('specs/01_multiexamen/CCA-F.zip')
    target = archive.parent / 'sources' / 'examtopics-ccaf-candidates.json'
    result = extract(archive)
    target.parent.mkdir(parents=True, exist_ok=True)
    target.write_text(json.dumps(result, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(json.dumps({'questions': len(result['questions']), 'captures': result['captures'], 'output': str(target)}, indent=2))
