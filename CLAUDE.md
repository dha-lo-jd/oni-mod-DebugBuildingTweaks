# CLAUDE.md

ONI mod（Dev Pump の出力量・温度の設定と、フィルター検索の強化）。ソースは `DebugBuildingTweaks/` 配下（リポジトリ直下は `.sln`、`harness.json`、`Workshop/`、`README.md`）。Build: `dotnet build DebugBuildingTweaks.sln`（Debug は `Local.Build.props` の `ModInstallPath` へコピーする。コンパイル確認だけなら `-p:ModInstallPath=`）。PLib を ILRepack で DLL に統合している。

## 構成
- `DevPumpPatches.cs`: Harmony パッチ。`DevPump.Sim1000ms` を `DevPumpSettings.TryFillStorage` に置き換える（Prefix が false を返すと元の処理を飛ばす）。液体・気体の両 Dev Pump に `DevPumpSettings` を付ける。検索パッチは `OnSearchBarValueChanged` で dev pump かどうかの旗を立てる（`TagContainsSearchWord` は static で呼び出し元を知らないため）。
- `DevPumpSettings.cs`: ポンプごとの出力量・温度。出力の上限は元素の状態で決まる（液体 10000 g/s、気体 1000 g/s）。温度を変えると貯蔵中の分は古い温度なので捨てる。
- `DevPumpSideScreen.cs`: PLib のサイドスクリーン。並び順は -100（バニラのフィルター 0 とデバッグツール -150 の間）。
- 文字列は `STRINGS.cs` と `translations/ja.po`。

## 実機検証
Gravitas.TestHarness を使う。テストは `DebugBuildingTweaks/HarnessTests.cs`（`#if DEBUG`、`[HarnessTest]`、タグ `debugbuildingtweaks`）。`Local.Build.props` に `HarnessDllPath` が要る（雛形は `.example`）。csproj は Debug かつ `HarnessDllPath` があるときだけハーネスを参照し、ILRepack にもそのフォルダを `/lib` として渡す（渡さないと統合が失敗する）。
実行: `python C:/Users/dha_1/ONI-Modding/GravitasTestHarness/Gravitas.TestProbe/probe.py run --project . --summary --tests '{"tags":["debugbuildingtweaks"]}'`
ゲームを起動し本番の `mods.json` を書き換えるので、実行前にユーザーに確認する。詳細は harness repo の `README.md` と `protocol.md`。既定のセーブは `Live Human Testing.sav`。

テストが見ていないもの: サイドスクリーンの見た目と配置、スライダー・入力欄の操作、温度単位の表示。
