# Game dev environment (macOS) — inventory 14/10/2026 and setup steps

Asked by Mrk (Venom, 14/10/2026): the current environment, in enough detail to install the same toolset on another
machine (Unity, Blender, Tripo, …).

## Current machine

- macOS 26.5.1, Apple M4 (10 cores), 24 GB RAM, 460 GB SSD.
- The disk is almost full: 8.4 GB free on the data volume.
- One GravityBox checkout is about 1.4 GB, and it grows several GB once Unity has built its Library.
- There are ten GravityBox worktrees.
- Recommendation: a new machine with ≥ 1 TB, or an external SSD for the repos.

## Core (required for COghe / GravityBox, RingJam, 420prisonsurvival)

| Tool | Version | Note |
| --- | --- | --- |
| Unity Hub | 3.19.3 | `brew install --cask unity-hub` |
| Unity Editor | **6000.3.19f1** (revision 7689f4515d75) | Every Unity project here uses this version. Install at the default Hub path: the scripts hard-code `/Applications/Unity/Hub/Editor/6000.3.19f1`; otherwise set `UNITY_EDITOR`. |
| Unity modules | Android Build Support (+ OpenJDK, + Android SDK & NDK Tools); iOS Build Support | See the bundled toolchain below. Mac build (Mono) is built in. |
| Xcode | 26.6 (17F113) | iOS 26.5 SDK + iOS 26.5 simulator; `sudo xcode-select -s /Applications/Xcode.app` |
| CocoaPods | 1.17.0 | Needed by EDM4U for Firebase/AdMob on iOS. `brew install cocoapods` |
| Android Studio | 2026.1 | For adb, the emulator (system image android-30) and a separate SDK (platform android-36.1, build-tools 36.0.0). Optional. |
| Git | 2.53.0 | `brew install git` |
| Git LFS | 3.7.1 | Installed, but GravityBox doesn't use LFS. |
| GitHub CLI | 2.96.0 | `gh auth login`. Repo: `https://github.com/nghienvothuat-a11y/GravityBox.git` |
| Python | 3.14.3 (Homebrew) | Used by `Tools/*.py`: SDK setup, test summaries, metrics. |
| ffmpeg | Homebrew (on PATH) | Clips. `brew install ffmpeg` |

Android toolchain bundled with Unity 6000.3.19f1 (installed by the Android module):
- OpenJDK 17.0.18 (Temurin)
- NDK r27c (27.2.12479018)
- SDK build-tools 36.0.0; platforms android-34, android-35, android-36

### Unity project (GravityBox) settings

- **Packages:**
  - URP 17.3.0, Input System 1.17.0, Test Framework 1.6.0, uGUI 2.0.0.
  - Google Mobile Ads 11.5.0 and EDM4U 1.2.190, from the OpenUPM scoped registry.
- **Firebase Analytics + Remote Config 13.17.0** are not in git: the macOS native library is over 100 MB.
  - Run `python3 Tools/setup-google-sdks.py` **before opening Unity**. It downloads Google's official package and checks
    its SHA-256.
  - `Assets/google-services.json` is committed.
- **Android build:** IL2CPP, ARM64, minSdk 26, bundle `com.gravityboxlab.venom`. The debug keystore is used (no custom
  keystore yet).
- **iOS build:** IL2CPP, iOS 15.0+, bundle `com.gravityboxlab.venom`. Team and profile come from the env vars
  `COGHE_IOS_TEAM` and `COGHE_IOS_PROFILE`.
- **Scripts:**
  - Tests: `Tools/run-coghe-tests.sh`.
  - Full suite: `Tools/verify-coghe-expansion.sh` (~25 min; 837 passed / 0 failed / 31 skipped on 12/10).
  - Builds: `Tools/build-venom-android.sh --spatial`, `Tools/build-venom-ios.sh`, and Mac via
    `COgheProductUIBuilder.BuildMac`.

## 3D and AI pipeline

| Tool | Version | Install |
| --- | --- | --- |
| Blender | **5.2.0 LTS** | blender.org (or `brew install --cask blender`) |
| Blender MCP add-on (Blender Lab) | extension `mcp` 1.0.0 | blender.org/lab/mcp-server → install the extension in Blender (Preferences → Get Extensions), enable it, start its server |
| blender-mcp server | 1.0.0 | `uv tool install blender-mcp` → `~/.local/bin/blender-mcp` |
| Tripo CLI (text/image → 3D) | tripo-cli 0.5.1 | `npm i -g tripo-cli`, then `tripo login` (the API key is stored per profile; not in git); `tripo mcp` serves MCP |
| Claude Code | 2.1.292 | Anthropic installer or `npm i -g @anthropic-ai/claude-code` |
| Codex CLI | 0.155.1 | `brew install --cask codex` |
| Buzz desktop | — | Workspace `~/.buzz`, relay `wss://darknite.communities.buzz.xyz` |
| Node.js | 24.18.0 (Homebrew) | `brew install node`, for tripo-cli |
| uv / pipx | 0.11.3 | `brew install uv pipx` |

Register the MCP servers in Claude Code (both are also registered in Codex's `~/.codex/config.toml`):

```bash
claude mcp add blender -s user -- ~/.local/bin/blender-mcp
claude mcp add tripo -s user -- tripo mcp
```

Codex `~/.codex/config.toml`:

```toml
[mcp_servers.blender]
command = "/Users/<you>/.local/bin/blender-mcp"
[mcp_servers.tripo]
command = "/opt/homebrew/bin/tripo"
args = ["mcp"]
```

### Python helper environment (charts, audio, clip work)

Used by `Tools/level_metrics/*.py`, chart and clip scripts:

```bash
uv venv ~/.venvs/gamedev && source ~/.venvs/gamedev/bin/activate
uv pip install matplotlib==3.11.2 pillow==12.3.0 numpy==2.5.3 scipy==1.18.1 soundfile==0.14.0 \
  opencv-python-headless==5.0.0.93 imageio-ffmpeg==0.6.0
```

## Editors and utilities

- **Code editors:** VS Code 1.131 and Cursor.
- **CLI utilities:** jq, ripgrep, fd, tree, wget, tmux.
- **apkeep:** pulls APKs for market research.
- **Not needed for the games:** Docker 29.3, Obsidian.

## Setup order on a new Mac

1. Install Xcode from the App Store and open it once to install the iOS platform. Then
   `sudo xcode-select -s /Applications/Xcode.app`.
2. Install Homebrew, then the tools:

   ```bash
   brew install git git-lfs gh python@3.14 uv pipx node ffmpeg cocoapods jq ripgrep
   brew install --cask unity-hub visual-studio-code android-studio blender codex
   ```

3. **Unity Hub:**
   1. Sign in and activate the license.
   2. Install **6000.3.19f1**: Installs → Install Editor → Archive, or `unityhub://6000.3.19f1/7689f4515d75`.
   3. Tick Android Build Support (with OpenJDK and Android SDK & NDK Tools) and iOS Build Support.
4. **Code:**

   ```bash
   gh auth login
   git clone https://github.com/nghienvothuat-a11y/GravityBox.git && cd GravityBox
   git checkout NewGraphic
   git config http.postBuffer 524288000   # large pushes
   python3 Tools/setup-google-sdks.py      # Firebase SDK before Unity opens
   ```

   Open the project in Unity Hub (Add project from disk). The first import takes a while.
5. **Check it works:**

   ```bash
   bash Tools/run-coghe-tests.sh smoke "GravityBox.Tests.COgheSpatialCampaignTests.SpatialPlusG1Solve"
   bash Tools/verify-coghe-expansion.sh   # full suite, ~25 min
   ```

6. **3D/AI:** set up Blender 5.2.0 + the MCP extension, `uv tool install blender-mcp`, `npm i -g tripo-cli`,
   `tripo login`, Claude Code / Codex and the MCP registration above, and the Python venv.

## Accounts and secrets to bring (never in git)

- Unity account and license.
- GitHub login: `gh auth login`.
- Tripo account: `tripo login`.
- Apple Developer team and provisioning profile (iOS store builds).
- Android release keystore, once there is a store build.
- Claude / OpenAI accounts for the agents.
- Buzz account / keys.
