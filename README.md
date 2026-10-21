# Debug Building Tweaks

Oxygen Not Included のデバッグ建築を、実運用しやすく調整する Mod です。  
現在は **Dev Pump（液体/気体）** を対象に、出力量と温度の調整機能を追加しています。

## Steam 公開用説明文（コピペ用）

デバッグ用ポンプ（Dev Pump）を使いやすくする小規模 Tweaks です。  
液体/気体それぞれで出力量を調整でき、吐出温度もサイドスクリーンから直接変更できます。

- 液体 Dev Pump の出力量上限: **10000 g/s**
- 気体 Dev Pump の出力量上限: **1000 g/s**
- 温度設定に対応（ゲーム内温度単位表示に追従）
- 要素フィルター検索を強化（表示名＋内部IDで検索）

デバッグ検証やテスト用の利用を想定しています。  
通常プレイのバランス調整を目的とした Mod ではありません。

## 機能

### 1. Dev Pump の出力量調整

- サイドスクリーンから出力量をスライダー/数値入力で変更可能
- 上限値:
  - 液体: **10000 g/s**
  - 気体: **1000 g/s**

### 2. Dev Pump の吐出温度調整

- サイドスクリーンから温度を変更可能
- 温度表示はゲーム設定の単位（℃/K など）に追従

### 3. フィルター検索拡張

- Dev Pump の要素フィルター検索で、表示名に加えて内部 ID（例: `OxygenGas`）でもヒット

## 対応環境

- ONI build: **744825 以上**
- DLC 必須: なし
- Mod version: **1.0.0**

## 使い方

1. デバッグ建築から Dev Pump（液体/気体）を配置
2. 建物を選択してサイドスクリーンを開く
3. `Output` と `Temperature` を調整

## 注意事項

- デバッグ用途を主目的としています
- UI/サイドスクリーン系を変更する他 Mod と競合する可能性があります

## 技術メモ

- Harmony + PLib を使用
- 主なパッチ対象:
  - `DevPumpGasConfig.DoPostConfigureComplete`
  - `DevPumpLiquidConfig.DoPostConfigureComplete`
  - `DevPump.Sim1000ms`
  - `DetailsScreen.OnPrefabInit`

