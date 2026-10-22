"""Verify hashes, archive hygiene and app-local runtime resolution from an extracted ZIP."""
import hashlib
import json
import os
from pathlib import Path
import subprocess
import zipfile
from datetime import datetime

ROOT = Path(__file__).resolve().parents[1]

def main():
    archive = ROOT / 'releases/AnthropicCert-1.1.0-win-x64.zip'
    evidence = ROOT / 'artifacts' / ('package-' + datetime.now().strftime('%Y%m%d-%H%M%S'))
    extracted = evidence / 'Extracción portable con espacios y acentos'
    extracted.mkdir(parents=True)
    with zipfile.ZipFile(archive) as z:
        assert not any('/Data/' in name or name.endswith(('.bak', '.mhtml')) or 'examtopics-ccaf-candidates' in name or 'CCA-F.zip' in name for name in z.namelist()), 'Personal or unreviewed data in ZIP'
        z.extractall(extracted)
    app = extracted / 'AnthropicCert-1.1.0-win-x64'
    manifest = json.loads((app / 'manifest.json').read_text(encoding='utf-8'))
    for filename, digest in manifest['files'].items():
        assert hashlib.sha256((app / filename).read_bytes()).hexdigest() == digest, filename
    diag = evidence / 'runtime.json'
    env = dict(os.environ, DOTNET_ROOT=str(evidence / 'no-dotnet'), DOTNET_ROOT_X64=str(evidence / 'no-dotnet'),
        DOTNET_MULTILEVEL_LOOKUP='0', PATH=os.environ.get('SystemRoot', 'C:/Windows') + '/System32')
    result = subprocess.run([str(app / 'AnthropicCert.exe'), '--smoke-test', str(diag)],
        cwd=ROOT, env=env, timeout=30, capture_output=True)
    assert result.returncode == 0, result.stderr.decode(errors='replace')
    info = json.loads(diag.read_text(encoding='utf-8'))
    for key in ['runtimeDirectory', 'formsAssembly']:
        assert Path(info[key]).resolve().is_relative_to(app.resolve()), f'External runtime: {info[key]}'
    qa = evidence / 'ui'
    result = subprocess.run([str(app / 'AnthropicCert.exe'), '--ui-test', str(qa)],
        cwd=ROOT, env=env, timeout=45, capture_output=True)
    assert result.returncode == 0, (qa / 'error.txt').read_text() if (qa / 'error.txt').exists() else result.stderr.decode(errors='replace')
    assert not (app / 'Data').exists(), 'QA polluted release data'
    report = dict(status='PASS', archive=str(archive), sha256=hashlib.sha256(archive.read_bytes()).hexdigest(),
        filesVerified=len(manifest['files']), runtime=info, ui=str(qa),
        limitation='Verified on current Windows 11 with installed .NET hidden from environment; app-local runtime paths verified. Not a clean VM test.')
    (evidence / 'verification.json').write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
    (ROOT / 'artifacts/latest-package-verification.txt').write_text(str(evidence), encoding='utf-8')
    print(json.dumps(report, ensure_ascii=False, indent=2))

if __name__ == '__main__': main()
