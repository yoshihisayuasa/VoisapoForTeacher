Voisapo レイヤリング (DDD) と依存関係

レイヤと名前空間
- Domain レイヤ
	- Namespace: Scripts.Domain
	- 物理配置: Assets/Scripts/Domain
	- 備考: 純粋な C# のみ。Unity API や他レイヤへの参照を置かない。

- Application レイヤ（現状: csproj のみ / ソース欠如）
	- Namespace (予定): Scripts.Application
	- 物理配置 (予定): Assets/Scripts/Application
	- 役割: ユースケース (アプリケーションサービス) / ドメイン操作のオーケストレーション / トランザクション境界 / Port(インターフェース) 定義
	- 状態: `Scripts.Application.csproj` に Compile 項目はあるが該当ファイルが存在しないため実質未使用
	- 選択肢:
		1. 復活: Ports & UseCases を再作成し UI ロジック肥大を抑制
		2. 削除: csproj から該当エントリを除去し 3 レイヤ構成に単純化
	- 復活する場合の最小ファイル例:
		- Ports: `IMelodyPlaybackPort`, `IPianoKeyPlaybackPort`, `IAudioVolumePort`
		- UseCases: `PlayMelodyUseCase`, `StopMelodyUseCase`, `HighlightKeysUseCase`

- Infrastructure レイヤ
	- Namespace: Scripts.Infrastructure
	- 物理配置: Assets/Scripts/Infrastructure
	- 依存: Scripts.Domain

- UI レイヤ
	- ルート Namespace: Scripts.UI
	- 主なサブ名前空間:
		- Scripts.UI.Piano (PianoController, PianoKeyUI)
		- Scripts.UI.Melody (MelodyPlayer, MelodyManager, MelodyJsonLoader)
	- 物理配置: Assets/Scripts/UI （旧フォルダ PianoControll, MelodyControll が物理的に残存）
	- 依存: Scripts.Domain, Scripts.Infrastructure

依存ルール
- UI -> Domain, Infrastructure
- (将来的に Application を挿入するなら): UI -> Application -> Domain ; Application -> (Ports 経由で) Infrastructure
- Infrastructure -> Domain
- Domain -> （他レイヤへ依存しない）

推奨依存図 (現在 / Application 復活時)

現在 (Application 無し):

 UI ---> Domain
	|       ^
	|       |
	+-----> Infrastructure

理想 (Application 復活案):

 UI ---> Application ---> Domain
	|            ^           ^
	|            |           |
	+----------> Infrastructure (Ports 実装) 

※ UI から Infrastructure 直接参照を徐々に Application 経由へ移管するとテスト容易性が向上。

各レイヤの責務 (Responsibilities)

Domain
- 何を扱うか: エンティティ / 値オブジェクト / ドメインサービス / ドメインイベント / ポリシー / 不変条件 (invariants)
- 責務: 業務ルールの表現と整合性維持。状態遷移の正当性検証。
- 入出力: 引数/戻り値は純粋な C# 型 (UnityEngine.* を避ける)。
- してよい: 計算 / ルール判定 / 不変条件チェック / 仕様語彙のモデル化
- してはいけない: I/O / ファイル / ネットワーク / UI 更新 / フレーム更新依存 / MonoBehaviour 継承

Infrastructure
- 何を扱うか: 外部システム適応 (保存, 読込, ネットワーク, PlayFab, Photon, デバイス) / Port 実装。
- 責務: 技術的詳細のカプセル化と安定した抽象 (Port) への適合。リソース管理 (接続 / キャッシュ / パス / シリアライズ)。
- 入出力: Port が定義するインターフェースに準拠。Domain 型 ⇄ 永続/転送フォーマット 変換。
- してよい: API / SDK 呼出し / ファイル I/O / ネット I/O / キャッシュ戦略。
- してはいけない: ドメインルール判断 / UI 更新 / MonoBehaviour の直接 UI 操作 (必要ならイベント or Port 経由)。

UI
- 何を扱うか: 表示 / 入力イベント / ViewModel 変換 / ユーザ操作をユースケースに橋渡し。
- 責務: 入力監視, 値のバインド, フィードバック (色・サウンド再生指示), Reactive 連結。状態最小化 (肥大化防止)。
- 入出力: Unity イベント → Application のユースケース呼出し / Application 結果 → 表示要素更新。
- してよい: イベント購読, 軽量な表示状態キャッシュ, デバッグ表示。
- してはいけない: 複雑なドメイン計算 / 外部サービス直接呼出し / 永続化ロジック / 乱雑なシングルトン乱用。

Cross-cutting (横断関心)
- ロギング/メトリクス: Application または Infrastructure で注入。UI では過度に埋め込まない。
- エラーハンドリング: Domain は例外最小化し明示的結果型も検討。Application で集約し UI へユーザフレンドリなメッセージ化。
- バリデーション: フォーマット/入力系 → UI or Application。ビジネス整合性 → Domain。

典型的アンチパターンと対策
- MonoBehaviour がユースケース/永続化/計算を全部抱える → UseCase クラス抽出。
- Domain が UnityEngine.Color などプレゼン層型を参照 → 値を抽象 (ex: NoteHighlightType) に置換。
- Infrastructure から UI を直接呼ぶ → イベント / メッセージ / Port 逆方向通知 (Observer) で切離し。
- サービスロケータ乱用で依存不透明 → コンストラクタ/フィールド注入 (将来 DI 容器) へ移行。

代表的ファイル（抜粋）
- Domain: ValueObjects/ValueObject.cs, PianoNoteEnum.cs, PianoNote.cs, PianoKeyDomain.cs, Volume.cs, BPM.cs, Melody.cs
- Infrastructure: PianoKeyInfrastructure.cs, VolumeManager.cs, EarphoneModeManager.cs, BPMManager.cs
- UI: UI/PianoKeyUI.cs, UI/BPMUI.cs, UI/VolumeSliderUI.cs, UI/EarphoneModeUI.cs, UI.Piano/PianoController.cs, UI.Melody/MelodyPlayer.cs, UI.Melody/MelodyManager.cs, UI.Melody/MelodyJsonLoader.cs

フォルダ構成に関するメモ
- 歴史的理由で MelodyControll / PianoControll フォルダが Assets/Scripts 直下に残っている。
- 推奨: Unity Editor 上で Assets/Scripts/UI/Melody と Assets/Scripts/UI/Piano へ移動（.meta GUID を維持し参照切れを防止）。

最近の構成変更メモ
- `IKeyPlayer` インターフェースを削除 (単一実装のみ & 差し替え要求低のため)。`MelodyPlayer` は `PianoController.Instance` を直接参照。
- 遅延取得は現在未使用 (毎回 Instance 参照方針)。
- 将来 Application 復活時は `IKeyPlayer` 相当の Port を Application 側に再導入し UI 実装を疎結合化可能。

R3 利用ポリシー
- R3 の利用は UI / （将来的な）Application 層のイベント合成に限定（例: PianoController）。
- Domain / Infrastructure では利用を避け純粋性と結合度低減を維持する。

ガイドライン（追加）
1. MonoBehaviour 内で複数ドメイン操作が混在したら UseCase 化を検討。
2. UI から Domain の状態変更が増えたら Port 経由へ抽出しテスト用スタブを用意。
3. 新しい外部サービス追加時は Infrastructure にアダプタを追加し Interface を Domain/Application に定義。
4. 循環参照を検出したら即座に Application (調停) 抽出を検討。
5. 1ファイル 300 行超 or メソッド 50 行超は責務分割サイン。
