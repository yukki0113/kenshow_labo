/* 【検証ラボ：枠順バイアス分析と登録】
   各コース・距離において、枠番ごとの平均指数の「乖離」を算出します。
   例：中山2500mの平均指数が100で、1枠の馬の平均が105なら、+5.0のバイアスと判定。
*/
TRUNCATE TABLE dbo.MT_PostPositionBias;

INSERT INTO dbo.MT_PostPositionBias (jyo_cd, surface_type, distance_m, frame_no, bias_score, description)
WITH CourseWakuStats AS (
    SELECT 
          r.jyo_cd
        , CASE WHEN r.baba_siba_cd <> '0' THEN '1' ELSE '2' END AS surface_type
        , r.distance_m
        , r.frame_no
        , AVG(ra.ability_idx) AS waku_avg
        , COUNT(*) AS sample_count
    FROM dbo.TR_RaceAnalysis ra
    JOIN dbo.TR_RaceResult r ON ra.race_id = r.race_id AND ra.horse_id = r.horse_id
    GROUP BY r.jyo_cd, CASE WHEN r.baba_siba_cd <> '0' THEN '1' ELSE '2' END, r.distance_m, r.frame_no
),
CourseTotalStats AS (
    SELECT 
          jyo_cd
        , surface_type
        , distance_m
        , AVG(waku_avg) AS course_avg
    FROM CourseWakuStats
    GROUP BY jyo_cd, surface_type, distance_m
)
SELECT 
      w.jyo_cd
    , w.surface_type
    , w.distance_m
    , w.frame_no
    , CAST(w.waku_avg - c.course_avg AS DECIMAL(3,1)) AS bias_score
    , N'Analysis-based bias (Samples:' + CAST(w.sample_count AS NVARCHAR(10)) + N')'
FROM CourseWakuStats w
JOIN CourseTotalStats c ON w.jyo_cd = c.jyo_cd AND w.surface_type = c.surface_type AND w.distance_m = c.distance_m
WHERE w.sample_count >= 30; -- 統計的有意性を保つため、30戦以上のデータがある条件に限定