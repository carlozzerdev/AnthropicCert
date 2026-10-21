"""Build and bundle a Windows x64 release using the installed .NET runtime offline.

The official publish self-contained path remains available with network/runtime packs.
This fallback merges the installed framework dependency manifests and carries their files.
"""
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import zipfile

ROOT = Path(__file__).resolve().parents[1]

def main():
    env = dict(os.environ, DOTNET_CLI_HOME=str(ROOT / '.buildhome'), DOTNET_CLI_TELEMETRY_OPTOUT='1')
    subprocess.run(['python', str(ROOT / 'tools/build_bank.py')], cwd=ROOT, check=True)
    subprocess.run(['dotnet', 'publish', 'src/AnthropicCert.Desktop/AnthropicCert.Desktop.csproj',
        '-c', 'Release', '--no-restore', '--self-contained', 'false', '-o', 'artifacts/framework-publish'],
        cwd=ROOT, env=env, check=True)
    output = ROOT / 'releases/AnthropicCert-1.0.0-win-x64'
    # Overwrite only known build outputs; never touch a user's Data directory.
    output.mkdir(parents=True, exist_ok=True)
    if (output / 'Data').exists():
        raise RuntimeError('Release folder contains Data. Use a new release folder; never package personal progress.')
    shutil.copytree(ROOT / 'artifacts/framework-publish', output, dirs_exist_ok=True)
    dotnet = Path(shutil.which('dotnet')).resolve().parent
    core = sorted((dotnet / 'shared/Microsoft.NETCore.App').iterdir(), key=lambda p: tuple(int(x) for x in p.name.split('.')))[-1]
    desktop = dotnet / 'shared/Microsoft.WindowsDesktop.App' / core.name
    if not desktop.is_dir() or not core.name.startswith('10.'):
        raise RuntimeError('A matching .NET 10 Core/Desktop x64 runtime is required to package offline.')
    deps_path = output / 'AnthropicCert.deps.json'
    deps = json.loads(deps_path.read_text(encoding='utf-8'))
    target = next(iter(deps['targets'].values()))
    for framework in [core, desktop]:
        shutil.copytree(framework, output, dirs_exist_ok=True)
        framework_deps = json.loads((framework / (framework.parent.name + '.deps.json')).read_text(encoding='utf-8'))
        runtime_target = framework_deps['runtimeTarget']['name']
        target.update(framework_deps['targets'][runtime_target])
        deps['libraries'].update(framework_deps['libraries'])
    runtime_target = '.NETCoreApp,Version=v10.0/win-x64'
    deps['targets'] = {runtime_target: target}; deps['runtimeTarget']['name'] = runtime_target
    deps_path.write_text(json.dumps(deps, indent=2), encoding='utf-8')
    host = dotnet / 'host/fxr' / core.name / 'hostfxr.dll'
    shutil.copy2(host, output / 'hostfxr.dll')
    for notice in ['LICENSE.txt', 'ThirdPartyNotices.txt']:
        shutil.copy2(dotnet / notice, output / notice)
    config_path = output / 'AnthropicCert.runtimeconfig.json'
    config = json.loads(config_path.read_text(encoding='utf-8'))
    opts = config['runtimeOptions']; frameworks = opts.pop('frameworks')
    opts['includedFrameworks'] = [dict(name=f['name'], version=core.name) for f in frameworks]
    config_path.write_text(json.dumps(config, indent=2), encoding='utf-8')
    shutil.copytree(ROOT / 'Content/Sources', output / 'Content/Sources', dirs_exist_ok=True)
    for name in ['LEEME.txt', 'THIRD-PARTY-NOTICES.txt']:
        shutil.copy2(ROOT / name, output / name)
    manifest = dict(version='1.0.0', platform='win-x64', runtime=core.name, packaging='offline bundled framework manifests',
        files={str(p.relative_to(output)).replace('\\','/'): hashlib.sha256(p.read_bytes()).hexdigest()
            for p in sorted(output.rglob('*')) if p.is_file() and p.name != 'manifest.json'})
    (output / 'manifest.json').write_text(json.dumps(manifest, indent=2), encoding='utf-8')
    zip_path = Path(str(output) + '.zip')
    with zipfile.ZipFile(zip_path, 'w', zipfile.ZIP_DEFLATED, compresslevel=6) as archive:
        for file in output.rglob('*'):
            if file.is_file(): archive.write(file, Path(output.name) / file.relative_to(output))
    digest = hashlib.sha256(zip_path.read_bytes()).hexdigest()
    zip_path.with_suffix('.zip.sha256').write_text(digest + '  ' + zip_path.name + '\n', encoding='utf-8')
    print(f'Portable ZIP: {zip_path} ({zip_path.stat().st_size / 1024**2:.1f} MiB), SHA256 {digest}')

if __name__ == '__main__': main()
