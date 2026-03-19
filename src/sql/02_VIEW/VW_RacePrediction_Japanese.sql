/* 【検証ラボ】予測結果・日本語化ビュー
   - 列名を和名にリネーム
   - レースIDから「会場名」を自動変換
*/
CREATE OR ALTER VIEW dbo.VW_RacePrediction_Japanese AS
SELECT
    target_date AS [開催日],
    CASE SUBSTRING(race_id, 9, 2)
        WHEN '01' THEN N'札幌'
        WHEN '02' THEN N'函館'
        WHEN '03' THEN N'福島'
        WHEN '04' THEN N'新潟'
        WHEN '05' THEN N'東京'
        WHEN '06' THEN N'中山'
        WHEN '07' THEN N'中京'
        WHEN '08' THEN N'京都'
        WHEN '09' THEN N'阪神'
        WHEN '10' THEN N'小倉'
        ELSE N'他'
    END AS [会場],
    race_no AS [R],
    frame_no AS [枠],
    horse_no AS [馬番],
    horse_name AS [馬名],
    final_expected_score AS [★最終期待値], -- 目立つように★を付与
    
    -- 主要パラメータ
    ability_last5_avg AS [基礎能力],
    momentum_score AS [勢い],
    aptitude_site_score AS [場所適性],
    aptitude_dist_score AS [距離適性],
    
    -- サブパラメータ（生スコア）
    raw_sire_apt AS [血統],
    raw_jockey_apt AS [騎手],
    raw_waku_bias AS [枠補正],
    raw_going_apt AS [馬場適性],
    
    -- その他情報
    predicted_style AS [推定脚質],
    race_class AS [クラス],
    career_count AS [戦数],
    
    -- システムID（結合用）
    race_id
FROM dbo.TR_RacePredictionResult;
GO