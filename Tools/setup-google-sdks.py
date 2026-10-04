#!/usr/bin/env python3
"""Restore the pinned, official Firebase Analytics + Remote Config Unity SDKs before opening Unity.

Google Mobile Ads + EDM4U are resolved by UPM. Firebase's macOS native library
exceeds GitHub's file size limit, so SDK binaries are cached locally, not committed.
No credentials are needed. The separate Assets/google-services.json is app config.
"""
import hashlib
from pathlib import Path, PurePosixPath
import tarfile
import urllib.request

ROOT = Path(__file__).resolve().parent.parent
VERSION = '13.17.0'
CACHE = ROOT / 'Artifacts/COgheServices'
REQUIRED = ['Assets/Firebase/Plugins/Firebase.Analytics.dll',
            'Assets/Firebase/Plugins/Firebase.App.dll',
            'Assets/Firebase/Editor/AnalyticsDependencies.xml']


def digest(path):
    with path.open('rb') as source:
        checksum = hashlib.sha256()
        for chunk in iter(lambda: source.read(1024 * 1024), b''):
            checksum.update(chunk)
        return checksum.hexdigest()


def install(component, sha, required):
    SHA256 = sha
    URL = f'https://dl.google.com/firebase/sdk/unity/dotnet4/Firebase{component}_{VERSION}.unitypackage'
    PACKAGE = CACHE / f'Firebase{component}_{VERSION}.unitypackage'
    STAMP = ROOT / f'Assets/Firebase/.coghe-sdk-{component.lower()}-version'
    REQUIRED = required
    if STAMP.exists() and STAMP.read_text().strip() == SHA256 and all((ROOT / p).exists() for p in REQUIRED):
        print(f'Firebase {component} {VERSION} ready.')
        return
    CACHE.mkdir(parents=True, exist_ok=True)
    if not PACKAGE.exists() or digest(PACKAGE) != SHA256:
        # Reuse a package extracted from Google's SDK zip by a previous setup.
        local = CACHE / f'firebase_unity_sdk/Firebase{component}.unitypackage'
        if local.exists() and digest(local) == SHA256:
            PACKAGE.write_bytes(local.read_bytes())
        else:
            print(f'Downloading official Firebase {component} {VERSION}...')
            temporary = PACKAGE.with_suffix('.download')
            urllib.request.urlretrieve(URL, temporary)
            if digest(temporary) != SHA256:
                temporary.unlink()
                raise RuntimeError('Firebase SDK checksum mismatch; nothing installed.')
            temporary.replace(PACKAGE)
    # A unitypackage is a tar of GUID folders, with pathname / asset / asset.meta.
    # Never extract tar paths directly. Only install SDK-owned paths below Assets.
    prefixes = ('Assets/Firebase', 'Assets/Plugins/iOS/Firebase',
                'Assets/Plugins/tvOS/Firebase', 'Assets/Editor Default Resources/Firebase')
    count = 0
    with tarfile.open(PACKAGE, 'r:gz') as archive:
        entries = {m.name: m for m in archive.getmembers() if m.isfile()}
        for name in entries:
            if not name.endswith('/pathname'):
                continue
            target = archive.extractfile(entries[name]).read().decode('utf-8').strip()
            path = PurePosixPath(target)
            if path.is_absolute() or '..' in path.parts:
                raise RuntimeError('Unsafe SDK path')
            if not any(target == p or target.startswith(p + '/') for p in prefixes):
                continue  # EDM4U is supplied by UPM, never import its bundled copy.
            folder = name.rsplit('/', 1)[0]
            for source_name, destination in ((folder + '/asset', ROOT / target),
                                             (folder + '/asset.meta', ROOT / (target + '.meta'))):
                if source_name in entries:
                    destination.parent.mkdir(parents=True, exist_ok=True)
                    destination.write_bytes(archive.extractfile(entries[source_name]).read())
                    count += 1
    if not all((ROOT / p).exists() for p in REQUIRED):
        raise RuntimeError('Firebase SDK incomplete; please inspect the official package.')
    STAMP.write_text(SHA256 + '\n')
    print(f'Installed Firebase {component} {VERSION}: {count} SDK files / metadata.')


if __name__ == '__main__':
    install('Analytics', 'adb4a3e9d5a08054e8cfda54f299801377feac7469d70ab9e706fdc0bfc30e65', REQUIRED)
    install('RemoteConfig', '42c159c17ee0344acbf2413435f7823566b199a0cbbfaf40cac899bc9da43530',
            ['Assets/Firebase/Plugins/Firebase.RemoteConfig.dll', 'Assets/Firebase/Editor/RemoteConfigDependencies.xml'])
