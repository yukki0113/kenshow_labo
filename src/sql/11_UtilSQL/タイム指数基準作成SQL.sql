/* 【基準タイム算出と登録】
  全コースの3勝クラス(016)・良馬場(1)における勝ち時計の平均を算出し、マスタに登録します。
*/

TRUNCATE TABLE dbo.MT_StandardTime;
INSERT INTO dbo.MT_StandardTime (jyo_cd, track_cd, distance_m, std_time, dist_coef, update_at)
SELECT 
    jyo_cd,
    track_cd,
    [距離],
    -- time型を秒数(FLOAT)に変換して平均を算出
    AVG(CAST(DATEDIFF(MILLISECOND, '00:00:00', [走破時計]) AS FLOAT) / 1000.0) AS std_time,
    -- 距離係数は一旦 20.0 で統一（後のステップで距離別調整を行う）
    20.0 AS dist_coef,
    GETDATE() AS update_at
FROM dbo.VW_RaceResultContract
WHERE jyoken_cd4 = '016' -- 3勝クラス
  AND (baba_siba_cd = '1' OR baba_dirt_cd = '1') -- 芝・ダートいずれかの良馬場
  AND [着順] = 1 -- 1着馬のみ
GROUP BY jyo_cd, track_cd, [距離];

-- 登録件数の確認
SELECT COUNT(*) AS [登録コース数] FROM dbo.MT_StandardTime;

/* クラス間タイム差 集計クエリ
   3勝クラスの基準タイム(MT_StandardTime)と、各クラスの1着タイムを比較し、
   平均的なタイム差（秒）を算出します。
*/
SELECT 
    v.unified_class_cd,
    v.[統合クラス],
    COUNT(*) AS レース数,
    -- (基準タイム - 実走時計) の平均を算出
    -- 正の値であれば「基準より速い」、負の値であれば「基準より遅い」
    AVG(
        s.std_time - (CAST(DATEDIFF(MILLISECOND, '00:00:00', v.[走破時計]) AS FLOAT) / 1000.0)
    ) AS 平均タイム差_秒
FROM dbo.VW_RaceResultContract v
JOIN dbo.MT_StandardTime s 
    ON  v.jyo_cd = s.jyo_cd 
    AND v.track_cd = s.track_cd 
    AND v.[距離] = s.distance_m
WHERE v.[着順] = 1 
  AND (v.baba_siba_cd = '1' OR v.baba_dirt_cd = '1') -- 良馬場のみ
GROUP BY v.unified_class_cd, v.[統合クラス]
ORDER BY 平均タイム差_秒 DESC; -- 速い順（格上順）に並べる