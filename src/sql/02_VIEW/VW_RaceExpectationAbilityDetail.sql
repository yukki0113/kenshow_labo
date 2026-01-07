CREATE OR ALTER VIEW dbo.VW_RaceExpectationAbilityDetail
AS
SELECT
      e.race_id
    , e.horse_no
    , e.horse_id
    , e.horse_name
    , e.race_date

    , p.rn
    , p.past_race_id
    , p.past_date
    , p.past_finish
    , p.past_field
    , p2.[グレード] AS past_grade
    , p2.[クラス]   AS past_class

    , CAST((p.past_field - p.past_finish + 1.0) / NULLIF(p.past_field, 0) AS DECIMAL(9,6)) AS raceScore
    , CAST(
        CASE
            WHEN p2.[グレード] IS NOT NULL AND (p2.[グレード] LIKE N'%G1%' OR p2.[グレード] LIKE N'%GI%') THEN 1.00
            WHEN p2.[グレード] IS NOT NULL AND (p2.[グレード] LIKE N'%G2%' OR p2.[グレード] LIKE N'%GII%') THEN 0.97
            WHEN p2.[グレード] IS NOT NULL AND (p2.[グレード] LIKE N'%G3%' OR p2.[グレード] LIKE N'%GIII%') THEN 0.95
            WHEN p2.[クラス] IS NOT NULL AND (p2.[クラス] LIKE N'%オープン%' OR p2.[クラス] LIKE N'%OP%' OR p2.[クラス] LIKE N'%リステッド%' OR p2.[クラス] LIKE N'%L%') THEN 0.93
            WHEN p2.[クラス] IS NOT NULL AND (p2.[クラス] LIKE N'%3勝%' OR p2.[クラス] LIKE N'%1600万%') THEN 0.90
            WHEN p2.[クラス] IS NOT NULL AND (p2.[クラス] LIKE N'%2勝%' OR p2.[クラス] LIKE N'%1000万%') THEN 0.87
            WHEN p2.[クラス] IS NOT NULL AND (p2.[クラス] LIKE N'%1勝%' OR p2.[クラス] LIKE N'%500万%') THEN 0.84
            WHEN p2.[クラス] IS NOT NULL AND (p2.[クラス] LIKE N'%未勝利%' OR p2.[クラス] LIKE N'%新馬%') THEN 0.80
            ELSE 0.90
        END
      AS DECIMAL(9,4)) AS class_mult

    , CAST(
        CASE
            WHEN p2.[グレード] IS NOT NULL AND (p2.[グレード] LIKE N'%G1%' OR p2.[グレード] LIKE N'%GI%') THEN 1.00
            WHEN p2.[グレード] IS NOT NULL AND (p2.[グレード] LIKE N'%G2%' OR p2.[グレード] LIKE N'%GII%') THEN 0.97
            WHEN p2.[グレード] IS NOT NULL AND (p2.[グレード] LIKE N'%G3%' OR p2.[グレード] LIKE N'%GIII%') THEN 0.95
            WHEN p2.[クラス] IS NOT NULL AND (p2.[クラス] LIKE N'%オープン%' OR p2.[クラス] LIKE N'%OP%' OR p2.[クラス] LIKE N'%リステッド%' OR p2.[クラス] LIKE N'%L%') THEN 0.93
            WHEN p2.[クラス] IS NOT NULL AND (p2.[クラス] LIKE N'%3勝%' OR p2.[クラス] LIKE N'%1600万%') THEN 0.90
            WHEN p2.[クラス] IS NOT NULL AND (p2.[クラス] LIKE N'%2勝%' OR p2.[クラス] LIKE N'%1000万%') THEN 0.87
            WHEN p2.[クラス] IS NOT NULL AND (p2.[クラス] LIKE N'%1勝%' OR p2.[クラス] LIKE N'%500万%') THEN 0.84
            WHEN p2.[クラス] IS NOT NULL AND (p2.[クラス] LIKE N'%未勝利%' OR p2.[クラス] LIKE N'%新馬%') THEN 0.80
            ELSE 0.90
        END
        *
        ((p.past_field - p.past_finish + 1.0) / NULLIF(p.past_field, 0))
      AS DECIMAL(9,6)) AS raceScore_adj

    , CAST(CASE p.rn WHEN 1 THEN 1.0 WHEN 2 THEN 0.8 WHEN 3 THEN 0.6 WHEN 4 THEN 0.4 WHEN 5 THEN 0.2 ELSE 0 END AS DECIMAL(9,2)) AS w
FROM dbo.TR_RaceEntry e
CROSS APPLY
(
    SELECT TOP (5)
          v.race_id AS past_race_id
        , v.[日付]  AS past_date
        , TRY_CONVERT(INT, v.[着順]) AS past_finish
        , TRY_CONVERT(INT, v.[頭数]) AS past_field
        , ROW_NUMBER() OVER (ORDER BY v.[日付] DESC, v.race_id DESC) AS rn
    FROM dbo.VW_RaceResultContract v
    WHERE v.horse_id = e.horse_id
      AND v.[日付] < e.race_date
      AND TRY_CONVERT(INT, v.[頭数]) BETWEEN 5 AND 18
      AND TRY_CONVERT(INT, v.[着順]) BETWEEN 1 AND TRY_CONVERT(INT, v.[頭数])
    ORDER BY v.[日付] DESC, v.race_id DESC
) p
LEFT JOIN dbo.VW_RaceResultContract p2
    ON p2.race_id = p.past_race_id;
GO
