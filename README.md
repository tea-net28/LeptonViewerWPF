# Lepton Viewer WPF
FLIR System 社のサーマルカメラ Lepton シリーズのカメラ画像を表示する WPF アプリケーション

![](https://github.com/tea-net28/LeptonViewerWPF/blob/main/_Docs/Images/01.gif)

## 主な機能
- サーモグラフィ画像の表示
- 最大・最小温度の設定による画像調整
- 画像を元にした顔検出機能

## 開発環境
.NET 8.0
### nuget パッケージ
- OpenCvSharp4
- OpenCvSharp4.WpfExtensions
- MaterialDesign Themes
### dll ファイル
- Lepton SDK for Windows
    https://www.flir.jp/developer/lepton-integration/lepton-integration-windows/
### デバイス
FLIR System - Lepton 3.5
Group Gets - Pure Thermal 3