/* 【検証ラボ】騎手・コース適性
   騎手ごとに「場(jyo_cd) × 芝ダ(surface)」の平均指数を算出。
*/
CREATE OR ALTER VIEW dbo.VW_Aptitude_Jockey AS
WITH JockeyBase AS (
    -- 騎手ごとの全体平均
    SELECT jockey_name, AVG(ability_idx) AS avg_jockey_all FROM dbo.TR_RaceAnalysis GROUP BY jockey_name
)
SELECT 
    ra.jockey_name,
    rr.jyo_cd,
    CASE WHEN rr.baba_siba_cd <> '0' THEN '1' ELSE '2' END AS surface_type_cd,
    COUNT(*) AS jockey_run_count,
    -- 騎手の全体平均に対する、その場での上振れ・下振れ
    AVG(ra.ability_idx) - MAX(jb.avg_jockey_all) AS jockey_course_score
FROM dbo.TR_RaceAnalysis ra
JOIN dbo.TR_RaceResult rr ON ra.race_id = rr.race_id AND ra.horse_id = rr.horse_id
JOIN JockeyBase jb ON ra.jockey_name = jb.jockey_name
GROUP BY ra.jockey_name, rr.jyo_cd, 
    CASE WHEN rr.baba_siba_cd <> '0' THEN '1' ELSE '2' END;
GO