CREATE OR ALTER PROCEDURE dbo.usp_Calc_RaceExpectation
    @race_id CHAR(12)
AS
BEGIN
    SET NOCOUNT ON;

    /* ---- パラメータ（暫定） ---- */
    DECLARE @n_recent INT = 5;

    DECLARE @w1 DECIMAL(9,6) = 1.0;
    DECLARE @w2 DECIMAL(9,6) = 0.8;
    DECLARE @w3 DECIMAL(9,6) = 0.6;
    DECLARE @w4 DECIMAL(9,6) = 0.4;
    DECLARE @w5 DECIMAL(9,6) = 0.2;

    /* 合成重み（暫定：あなたの設計案ベース） */
    DECLARE @w_ability DECIMAL(9,6) = 0.50;
    DECLARE @w_course  DECIMAL(9,6) = 0.20;
    DECLARE @w_style   DECIMAL(9,6) = 0.10;
    DECLARE @w_frame   DECIMAL(9,6) = 0.05; -- 仮（後で調整）
    DECLARE @w_blood   DECIMAL(9,6) = 0.10; -- 仮（後で調整）
    DECLARE @w_jockey  DECIMAL(9,6) = 0.05;

    /* 既存結果を消して作り直し（1レース単位） */
    DELETE FROM dbo.TR_RaceExpectation WHERE race_id = @race_id;

    ;WITH E AS
    (
        SELECT
              e.race_id
            , e.horse_no
            , e.horse_id
            , e.horse_name
            , e.track_name
            , e.surface_type
            , e.distance_m
            , e.race_date
            , e.frame_no
            , e.jockey_name
        FROM dbo.TR_RaceEntry e
        WHERE e.race_id = @race_id
    ),
    J AS
    (
        SELECT
              e.*
            , cd.code AS jyo_cd
            , CASE
                  WHEN e.distance_m <= 1400 THEN CAST(1 AS TINYINT)
                  WHEN e.distance_m <= 1800 THEN CAST(2 AS TINYINT)
                  WHEN e.distance_m <= 2400 THEN CAST(3 AS TINYINT)
                  ELSE CAST(4 AS TINYINT)
              END AS distance_class
        FROM E e
        LEFT JOIN dbo.MT_CodeDictionary cd
            ON cd.code_type = N'RACE_COURSE'
           AND cd.is_active = 1
           AND cd.text_sub = N'CENTRAL'
           AND LTRIM(RTRIM(cd.text)) = LTRIM(RTRIM(e.track_name))
    ), Past AS
    (
    SELECT
          j.race_id
        , j.horse_no
        , v.race_id AS past_race_id
        , v.[日付]  AS past_date
        , cv.finish_i AS past_finish
        , cv.field_i  AS past_field
        , ROW_NUMBER() OVER (PARTITION BY j.race_id, j.horse_no ORDER BY v.[日付] DESC, v.race_id DESC) AS rn
    FROM J j
    INNER JOIN dbo.VW_RaceResultContract v
        ON v.horse_id = j.horse_id
       AND v.[日付] < j.race_date
    CROSS APPLY
    (
        SELECT
              TRY_CONVERT(INT, v.[着順]) AS finish_i
            , TRY_CONVERT(INT, v.[頭数]) AS field_i
    ) cv
    WHERE cv.finish_i IS NOT NULL
      AND cv.field_i  IS NOT NULL
      AND cv.field_i BETWEEN 5 AND 18
      AND cv.finish_i BETWEEN 1 AND cv.field_i
    )
    , PastScore AS
    (
        SELECT
            p.race_id
            , p.horse_no
            , p.rn
            , base.raceScore
            , CAST(
                CASE
                    /* 1) グレード優先 */
                    WHEN p2.[グレード] IS NOT NULL AND (p2.[グレード] LIKE N'%G1%' OR p2.[グレード] LIKE N'%GI%') THEN 1.00
                    WHEN p2.[グレード] IS NOT NULL AND (p2.[グレード] LIKE N'%G2%' OR p2.[グレード] LIKE N'%GII%') THEN 0.97
                    WHEN p2.[グレード] IS NOT NULL AND (p2.[グレード] LIKE N'%G3%' OR p2.[グレード] LIKE N'%GIII%') THEN 0.95

                    /* 2) クラス（条件） */
                    WHEN p2.[クラス] IS NOT NULL AND (p2.[クラス] LIKE N'%オープン%' OR p2.[クラス] LIKE N'%OP%' OR p2.[クラス] LIKE N'%リステッド%' OR p2.[クラス] LIKE N'%L%') THEN 0.93
                    WHEN p2.[クラス] IS NOT NULL AND (p2.[クラス] LIKE N'%3勝%' OR p2.[クラス] LIKE N'%1600万%') THEN 0.90
                    WHEN p2.[クラス] IS NOT NULL AND (p2.[クラス] LIKE N'%2勝%' OR p2.[クラス] LIKE N'%1000万%') THEN 0.87
                    WHEN p2.[クラス] IS NOT NULL AND (p2.[クラス] LIKE N'%1勝%' OR p2.[クラス] LIKE N'%500万%') THEN 0.84
                    WHEN p2.[クラス] IS NOT NULL AND (p2.[クラス] LIKE N'%未勝利%' OR p2.[クラス] LIKE N'%新馬%') THEN 0.80

                    ELSE 0.90  -- 不明は中庸（暫定）
                END
            AS DECIMAL(9,4)) AS class_mult

            , CAST(
                CASE
                    WHEN base.raceScore IS NULL THEN NULL
                    WHEN base.raceScore * 
                        (CASE
                            WHEN p2.[グレード] IS NOT NULL AND (p2.[グレード] LIKE N'%G1%' OR p2.[グレード] LIKE N'%GI%') THEN 1.00
                            WHEN p2.[グレード] IS NOT NULL AND (p2.[グレード] LIKE N'%G2%' OR p2.[グレード] LIKE N'%GII%') THEN 0.97
                            WHEN p2.[グレード] IS NOT NULL AND (p2.[グレード] LIKE N'%G3%' OR p2.[グレード] LIKE N'%GIII%') THEN 0.95
                            WHEN p2.[クラス] IS NOT NULL AND (p2.[クラス] LIKE N'%オープン%' OR p2.[クラス] LIKE N'%OP%' OR p2.[クラス] LIKE N'%リステッド%' OR p2.[クラス] LIKE N'%L%') THEN 0.93
                            WHEN p2.[クラス] IS NOT NULL AND (p2.[クラス] LIKE N'%3勝%' OR p2.[クラス] LIKE N'%1600万%') THEN 0.90
                            WHEN p2.[クラス] IS NOT NULL AND (p2.[クラス] LIKE N'%2勝%' OR p2.[クラス] LIKE N'%1000万%') THEN 0.87
                            WHEN p2.[クラス] IS NOT NULL AND (p2.[クラス] LIKE N'%1勝%' OR p2.[クラス] LIKE N'%500万%') THEN 0.84
                            WHEN p2.[クラス] IS NOT NULL AND (p2.[クラス] LIKE N'%未勝利%' OR p2.[クラス] LIKE N'%新馬%') THEN 0.80
                            ELSE 0.90
                        END) > 1.0
                    THEN 1.0
                    ELSE base.raceScore *
                        (CASE
                            WHEN p2.[グレード] IS NOT NULL AND (p2.[グレード] LIKE N'%G1%' OR p2.[グレード] LIKE N'%GI%') THEN 1.00
                            WHEN p2.[グレード] IS NOT NULL AND (p2.[グレード] LIKE N'%G2%' OR p2.[グレード] LIKE N'%GII%') THEN 0.97
                            WHEN p2.[グレード] IS NOT NULL AND (p2.[グレード] LIKE N'%G3%' OR p2.[グレード] LIKE N'%GIII%') THEN 0.95
                            WHEN p2.[クラス] IS NOT NULL AND (p2.[クラス] LIKE N'%オープン%' OR p2.[クラス] LIKE N'%OP%' OR p2.[クラス] LIKE N'%リステッド%' OR p2.[クラス] LIKE N'%L%') THEN 0.93
                            WHEN p2.[クラス] IS NOT NULL AND (p2.[クラス] LIKE N'%3勝%' OR p2.[クラス] LIKE N'%1600万%') THEN 0.90
                            WHEN p2.[クラス] IS NOT NULL AND (p2.[クラス] LIKE N'%2勝%' OR p2.[クラス] LIKE N'%1000万%') THEN 0.87
                            WHEN p2.[クラス] IS NOT NULL AND (p2.[クラス] LIKE N'%1勝%' OR p2.[クラス] LIKE N'%500万%') THEN 0.84
                            WHEN p2.[クラス] IS NOT NULL AND (p2.[クラス] LIKE N'%未勝利%' OR p2.[クラス] LIKE N'%新馬%') THEN 0.80
                            ELSE 0.90
                        END)
                END
            AS DECIMAL(18,10)) AS raceScore_adj
        FROM Past p
        INNER JOIN dbo.VW_RaceResultContract p2
            ON p2.race_id = p.past_race_id
        CROSS APPLY
        (
            SELECT
                CAST((CAST(p.past_field AS DECIMAL(18,6)) - CAST(p.past_finish AS DECIMAL(18,6)) + 1.0)
                / NULLIF(CAST(p.past_field AS DECIMAL(18,6)), 0) AS DECIMAL(18,10)) AS raceScore
        ) base
        WHERE p.rn <= @n_recent
    )
    ,Ability AS
    (
        SELECT
              ps.race_id
            , ps.horse_no
            , CAST(
                (SUM(CASE WHEN ps.rn = 1 THEN ps.raceScore * @w1
                          WHEN ps.rn = 2 THEN ps.raceScore * @w2
                          WHEN ps.rn = 3 THEN ps.raceScore * @w3
                          WHEN ps.rn = 4 THEN ps.raceScore * @w4
                          WHEN ps.rn = 5 THEN ps.raceScore * @w5
                          ELSE 0 END))
                /
                NULLIF((SUM(CASE WHEN ps.rn = 1 THEN @w1
                                 WHEN ps.rn = 2 THEN @w2
                                 WHEN ps.rn = 3 THEN @w3
                                 WHEN ps.rn = 4 THEN @w4
                                 WHEN ps.rn = 5 THEN @w5
                                 ELSE 0 END)), 0)
              AS DECIMAL(9,6)) AS ability
        FROM PastScore ps
        GROUP BY ps.race_id, ps.horse_no
    ),
    HS AS
    (
        /* 対象馬だけで固定脚質（直近5走多数決）を作る：高速版 */
        SELECT
              v.horse_id
            , CASE
                  WHEN v.[脚質] LIKE N'%逃%' THEN N'逃げ'
                  WHEN v.[脚質] LIKE N'%先%' THEN N'先行'
                  WHEN v.[脚質] LIKE N'%差%' THEN N'差し'
                  WHEN v.[脚質] LIKE N'%追%' THEN N'追込'
                  WHEN v.[脚質] LIKE N'%まく%' OR v.[脚質] LIKE N'%捲%' THEN N'差し'
                  ELSE NULL
              END AS style4
            , ROW_NUMBER() OVER (PARTITION BY v.horse_id ORDER BY v.[日付] DESC, v.race_id DESC) AS rn
        FROM dbo.VW_RaceResultContract v
        INNER JOIN (SELECT DISTINCT horse_id FROM J WHERE horse_id IS NOT NULL) t
            ON t.horse_id = v.horse_id
        WHERE v.[脚質] IS NOT NULL
          AND v.[日付] IS NOT NULL
    ),
    HS5 AS
    (
        SELECT horse_id, style4, rn
        FROM HS
        WHERE rn <= 5 AND style4 IS NOT NULL
    ),
    HS_AGG AS
    (
        SELECT horse_id, style4, COUNT(1) AS cnt, MIN(rn) AS best_rn
        FROM HS5
        GROUP BY horse_id, style4
    ),
    HS_PICK AS
    (
        SELECT
              horse_id
            , style4
            , ROW_NUMBER() OVER (PARTITION BY horse_id ORDER BY cnt DESC, best_rn ASC) AS pick
        FROM HS_AGG
    ),
    HS_FIXED AS
    (
        SELECT horse_id, style4 AS style_fixed
        FROM HS_PICK
        WHERE pick = 1
    )
    INSERT INTO dbo.TR_RaceExpectation
    (
        race_id, horse_no,
        horse_id, horse_name, track_name, jyo_cd, surface_type, distance_m, distance_class,
        ability,
        mult_course, mult_style, mult_frame, mult_blood, mult_jockey,
        expected_raw, expected_100
    )
    SELECT
          j.race_id
        , j.horse_no
        , j.horse_id
        , j.horse_name
        , j.track_name
        , j.jyo_cd
        , j.surface_type
        , j.distance_m
        , j.distance_class

        , ISNULL(a.ability, 0.50) AS ability  -- 過去走不足は暫定0.50（後で調整可）

        , ISNULL(cp.mult, 1.0) AS mult_course
        , ISNULL(sp.mult, 1.0) AS mult_style
        , ISNULL(fp.mult, 1.0) AS mult_frame
        , ISNULL(bp.mult, 1.0) AS mult_blood
        , ISNULL(jp.mult, 1.0) AS mult_jockey

        , CAST(
            /* 合成：Abilityは0～1、適性は係数→0～1に寄せるため (mult-0.85)/0.30 を使用（暫定） */
            (
              @w_ability * ISNULL(a.ability, 0.50)
              + @w_course  * CAST((ISNULL(cp.mult, 1.0) - 0.85) / 0.30 AS DECIMAL(18,10))
              + @w_style   * CAST((ISNULL(sp.mult, 1.0) - 0.85) / 0.30 AS DECIMAL(18,10))
              + @w_frame   * CAST((ISNULL(fp.mult, 1.0) - 0.85) / 0.30 AS DECIMAL(18,10))
              + @w_blood   * CAST((ISNULL(bp.mult, 1.0) - 0.85) / 0.30 AS DECIMAL(18,10))
              + @w_jockey  * CAST((ISNULL(jp.mult, 1.0) - 0.85) / 0.30 AS DECIMAL(18,10))
            )
          AS DECIMAL(9,6)
          ) AS expected_raw

        , CAST(
            (
              @w_ability * ISNULL(a.ability, 0.50)
              + @w_course  * ((ISNULL(cp.mult, 1.0) - 0.85) / 0.30)
              + @w_style   * ((ISNULL(sp.mult, 1.0) - 0.85) / 0.30)
              + @w_frame   * ((ISNULL(fp.mult, 1.0) - 0.85) / 0.30)
              + @w_blood   * ((ISNULL(bp.mult, 1.0) - 0.85) / 0.30)
              + @w_jockey  * ((ISNULL(jp.mult, 1.0) - 0.85) / 0.30)
            ) * 100.0
          AS DECIMAL(9,2)
          ) AS expected_100
    FROM J j
    LEFT JOIN Ability a
        ON a.race_id = j.race_id
       AND a.horse_no = j.horse_no

    LEFT JOIN dbo.ST_CoursePlace cp
        ON cp.jyo_cd = j.jyo_cd
       AND cp.surface_type = j.surface_type
       AND cp.distance_class = j.distance_class

    LEFT JOIN HS_FIXED hf
        ON hf.horse_id = j.horse_id

    LEFT JOIN dbo.ST_StylePlace sp
        ON sp.jyo_cd = j.jyo_cd
       AND sp.surface_type = j.surface_type
       AND sp.distance_class = j.distance_class
       AND sp.style = hf.style_fixed

    LEFT JOIN dbo.ST_FramePlace fp
        ON fp.jyo_cd = j.jyo_cd
       AND fp.surface_type = j.surface_type
       AND fp.distance_class = j.distance_class
       AND fp.frame_no = j.frame_no

    LEFT JOIN dbo.MT_HorsePedigree ped
        ON ped.horse_id_num = j.horse_id

    LEFT JOIN dbo.ST_BloodSirePlace bp
        ON bp.sire = ped.sire
       AND bp.jyo_cd = j.jyo_cd
       AND bp.surface_type = j.surface_type
       AND bp.distance_class = j.distance_class

    LEFT JOIN dbo.ST_JockeyPlace jp
        ON jp.jockey_name = j.jockey_name
       AND jp.jyo_cd = j.jyo_cd
       AND jp.surface_type = j.surface_type
       AND jp.distance_class = j.distance_class;

END
GO
