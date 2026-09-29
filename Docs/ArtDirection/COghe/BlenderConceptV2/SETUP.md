# Blender — ghi nhận cài đặt

Ngày 28/09/2026, Mac arm64, macOS 26.5.1.

- Đã cài **Blender 5.2.1 LTS**, build hash `9e2066aef7ef` vào `/Applications/Blender.app`.
- Executable: `/Applications/Blender.app/Contents/MacOS/Blender`.
- DMG giữ tại `/Users/mrk/Downloads/blender-5.2.1-macos-arm64.dmg`.
- Homebrew cask cung cấp URL/hash nhưng tải từ download.blender.org bị HTTP 403. Đã dùng mirror RWTH Aachen: https://ftp.halifax.rwth-aachen.de/blender/release/Blender5.2/blender-5.2.1-macos-arm64.dmg . Cài trực tiếp từ DMG; không ghi là Homebrew quản lý app.
- SHA-256 DMG: `6409e21de80994db5f4c4a34486b6fd43cea21085b912f7491c53e923acb65a3`, khớp metadata cask Blender đã truy vấn.
- `codesign --verify --deep --strict` đạt; `spctl --assess --type execute -v` trả `accepted`, `Notarized Developer ID`. Không vô hiệu Gatekeeper.
- `--version` trả 5.2.1 LTS; smoke test background factory startup đạt, có mesh mặc định và FBX export operator `EXPORT_SCENE_OT_fbx`; exit code 0.
- Chưa render benchmark, chưa xuất một asset gameplay; smoke test chỉ xác nhận ứng dụng khởi chạy và exporter có sẵn.

Lệnh kiểm tra:

```sh
/Applications/Blender.app/Contents/MacOS/Blender --version
```

Phiên bản được cố định cho thử nghiệm này; không tuyên bố đây là patch mới nhất. Xem [kế hoạch 10 màn](IMPLEMENTATION_PLAN.md) trước khi tạo asset.
