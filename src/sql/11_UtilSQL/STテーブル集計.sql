/* =========================================================
   ST集計 改良版（VW + MT_Track 正規化 / Style 4分類 / Blood/Jockey 中立化）
   - 母集団：VW_RaceResultContract
   - 正規化：MT_Track (JyoCD, Kyori, ShibaDirt, TrackCD) に一致する行だけ採用
   - 距離帯：短/マ/中/長（distance_class）
   - hit：着順1～3（複勝）
   - 平滑化：k=50
   - クリップ：0.85～1.15
   - Blood/Jockey：n<30は mult=1.0（中立）
   ========================================================= */

SET NOCOUNT ON;

DECLARE @k INT = 50;
DECLARE @clip_min DECIMAL(6,4) = 0.85;
DECLARE @clip_max DECIMAL(6,4) = 1.15;

/* ---- Style 調整パラメータ ---- */
DECLARE @min_n_style INT = 30;
DECLARE @shrink_denom_style INT = 600;
DECLARE @alpha_max_style DECIMAL(6,4) = 0.40;

/* ---- Frame 調整パラメータ ---- */
DECLARE @min_n_frame INT = 500;
DECLARE @shrink_denom_frame INT = 5000;
DECLARE @alpha_max_frame DECIMAL(6,4) = 0.25;

/* ---- blood 調整パラメータ ---- */
DECLARE @min_n_blood INT = 30;
DECLARE @min_n_blood2 INT = 30;
DECLARE @shrink_denom_blood INT = 1500;
DECLARE @alpha_max_blood DECIMAL(6,4) = 0.50;

/* ---- Jockey 調整パラメータ ---- */
DECLARE @min_n_jockey INT = 30;
DECLARE @min_n_jockey2 INT = 50;
DECLARE @shrink_denom_jockey INT = 5000;
DECLARE @alpha_max_jockey DECIMAL(6,4) = 0.25;

IF OBJECT_ID('tempdb..#BaseFiltered') IS NOT NULL DROP TABLE #BaseFiltered;
IF OBJECT_ID('tempdb..#P0Course') IS NOT NULL DROP TABLE #P0Course;
IF OBJECT_ID('tempdb..#P0SurfaceDist') IS NOT NULL DROP TABLE #P0SurfaceDist;

/* ---------------------------------------------------------
   0) 芝/ダコードのマッピング
     例：芝='1' / ダ='2' など
   --------------------------------------------------------- */
DECLARE @ShibaDirtMap TABLE
(
      surface_type NVARCHAR(10) NOT NULL PRIMARY KEY
    , shiba_dirt_cd NVARCHAR(10) NOT NULL
);

-- デフォルト（MT_Track.ShibaDirt が '芝'/'ダ' の場合）
INSERT INTO @ShibaDirtMap (surface_type, shiba_dirt_cd)
VALUES (N'芝', N'1'), (N'ダ', N'2');

/* ---------------------------------------------------------
   1) 正規化元データ（MT_TrackにJOINして“正規の条件のみ”採用）
   --------------------------------------------------------- */
/* 1) まず style無しの母集団を作る */
IF OBJECT_ID('tempdb..#BaseCore') IS NOT NULL DROP TABLE #BaseCore;
CREATE TABLE #BaseCore
(
      horse_id        BIGINT        NOT NULL
    , jyo_cd          CHAR(2)        NOT NULL
    , surface_type    NVARCHAR(10)   NOT NULL
    , distance_class  TINYINT        NOT NULL
    , frame_no        TINYINT        NULL
    , finish_pos      INT            NOT NULL
    , is_place        TINYINT        NOT NULL
    , sire            NVARCHAR(100)  NULL
    , jockey_name     NVARCHAR(10)   NULL
);

INSERT INTO #BaseCore
(
    horse_id, jyo_cd, surface_type, distance_class, frame_no, finish_pos, is_place, sire, jockey_name
)
SELECT
      v.horse_id
    , v.jyo_cd
    , st.surface_type
    , CASE
          WHEN TRY_CONVERT(INT, v.[距離]) <= 1400 THEN CAST(1 AS TINYINT)
          WHEN TRY_CONVERT(INT, v.[距離]) <= 1800 THEN CAST(2 AS TINYINT)
          WHEN TRY_CONVERT(INT, v.[距離]) <= 2400 THEN CAST(3 AS TINYINT)
          ELSE CAST(4 AS TINYINT)
      END AS distance_class
    , TRY_CONVERT(TINYINT, v.[枠番]) AS frame_no
    , TRY_CONVERT(INT, v.[着順]) AS finish_pos
    , CASE WHEN TRY_CONVERT(INT, v.[着順]) BETWEEN 1 AND 3 THEN 1 ELSE 0 END AS is_place
    , NULLIF(LTRIM(RTRIM(v.[父])), N'') AS sire
    , NULLIF(LTRIM(RTRIM(v.[騎手])), N'') AS jockey_name
FROM dbo.VW_RaceResultContract v
CROSS APPLY
(
    SELECT
        CASE
            WHEN v.[芝/ダ] IN (N'芝') THEN N'芝'
            WHEN v.[芝/ダ] IN (N'ダ', N'ダート') THEN N'ダ'
            ELSE NULL
        END AS surface_type
) st
INNER JOIN @ShibaDirtMap sd
    ON sd.surface_type = st.surface_type
INNER JOIN dbo.MT_Track mt
    ON mt.JyoCD = v.jyo_cd
   AND mt.Kyori = TRY_CONVERT(INT, v.[距離])
   AND mt.TrackCD = v.track_cd
   AND CONVERT(NVARCHAR(10), mt.ShibaDirt) = sd.shiba_dirt_cd
WHERE v.horse_id IS NOT NULL
  AND v.jyo_cd IS NOT NULL
  AND st.surface_type IS NOT NULL
  AND TRY_CONVERT(INT, v.[距離]) IS NOT NULL
  AND TRY_CONVERT(INT, v.[着順]) IS NOT NULL;

/* 2) 対象horseだけの固定脚質を作る（直近5走多数決） */
IF OBJECT_ID('tempdb..#HorseList') IS NOT NULL DROP TABLE #HorseList;
SELECT DISTINCT horse_id INTO #HorseList FROM #BaseCore;

IF OBJECT_ID('tempdb..#HorseStyleFixed') IS NOT NULL DROP TABLE #HorseStyleFixed;
CREATE TABLE #HorseStyleFixed
(
      horse_id     BIGINT        NOT NULL PRIMARY KEY
    , style_fixed  NVARCHAR(20)  NOT NULL
);

WITH R AS
(
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
    INNER JOIN #HorseList hl
        ON hl.horse_id = v.horse_id
    WHERE v.[脚質] IS NOT NULL
      AND v.[日付] IS NOT NULL
),
L AS
(
    SELECT horse_id, style4, rn
    FROM R
    WHERE rn <= 5
      AND style4 IS NOT NULL
),
A AS
(
    SELECT
          horse_id
        , style4
        , COUNT(1) AS cnt
        , MIN(rn) AS best_rn
    FROM L
    GROUP BY horse_id, style4
),
P AS
(
    SELECT
          horse_id
        , style4
        , ROW_NUMBER() OVER (PARTITION BY horse_id ORDER BY cnt DESC, best_rn ASC) AS pick
    FROM A
)
INSERT INTO #HorseStyleFixed (horse_id, style_fixed)
SELECT horse_id, style4
FROM P
WHERE pick = 1;

/* 3) 最終の #BaseFiltered を作る（styleは固定脚質） */
IF OBJECT_ID('tempdb..#BaseFiltered') IS NOT NULL DROP TABLE #BaseFiltered;
CREATE TABLE #BaseFiltered
(
      jyo_cd          CHAR(2)        NOT NULL
    , surface_type    NVARCHAR(10)   NOT NULL
    , distance_class  TINYINT        NOT NULL
    , frame_no        TINYINT        NULL
    , finish_pos      INT            NOT NULL
    , is_place        TINYINT        NOT NULL
    , style           NVARCHAR(20)   NULL
    , sire            NVARCHAR(100)  NULL
    , jockey_name     NVARCHAR(10)   NULL
);

INSERT INTO #BaseFiltered
(
    jyo_cd, surface_type, distance_class, frame_no, finish_pos, is_place, style, sire, jockey_name
)
SELECT
      b.jyo_cd
    , b.surface_type
    , b.distance_class
    , b.frame_no
    , b.finish_pos
    , b.is_place
    , hs.style_fixed AS style
    , b.sire
    , b.jockey_name
FROM #BaseCore b
LEFT JOIN #HorseStyleFixed hs
    ON hs.horse_id = b.horse_id;

/* ---------------------------------------------------------
   2) p0（基準）を作る
   - Course：基準を「芝ダ×距離帯」の平均との差に変更（張り付き抑制）
   - Frame/Style/Blood/Jockey：基準は「同コース条件（jyo×芝ダ×距離帯）」
   --------------------------------------------------------- */
CREATE TABLE #P0SurfaceDist
(
      surface_type   NVARCHAR(10) NOT NULL
    , distance_class TINYINT      NOT NULL
    , n0             INT          NOT NULL
    , hit0           INT          NOT NULL
    , p0             DECIMAL(9,6) NOT NULL
    , CONSTRAINT PK_P0SurfaceDist PRIMARY KEY (surface_type, distance_class)
);

INSERT INTO #P0SurfaceDist (surface_type, distance_class, n0, hit0, p0)
SELECT
      b.surface_type
    , b.distance_class
    , COUNT(1) AS n0
    , SUM(b.is_place) AS hit0
    , CAST(SUM(CAST(b.is_place AS DECIMAL(18,6))) / NULLIF(COUNT(1), 0) AS DECIMAL(9,6)) AS p0
FROM #BaseFiltered b
GROUP BY b.surface_type, b.distance_class;

CREATE TABLE #P0Course
(
      jyo_cd         CHAR(2)       NOT NULL
    , surface_type   NVARCHAR(10)  NOT NULL
    , distance_class TINYINT       NOT NULL
    , n0             INT           NOT NULL
    , hit0           INT           NOT NULL
    , p0             DECIMAL(9,6)  NOT NULL
    , CONSTRAINT PK_P0Course PRIMARY KEY (jyo_cd, surface_type, distance_class)
);

INSERT INTO #P0Course (jyo_cd, surface_type, distance_class, n0, hit0, p0)
SELECT
      b.jyo_cd
    , b.surface_type
    , b.distance_class
    , COUNT(1) AS n0
    , SUM(b.is_place) AS hit0
    , CAST(SUM(CAST(b.is_place AS DECIMAL(18,6))) / NULLIF(COUNT(1), 0) AS DECIMAL(9,6)) AS p0
FROM #BaseFiltered b
GROUP BY b.jyo_cd, b.surface_type, b.distance_class;

/* ---------------------------------------------------------
   3) 実テーブルを全作り直し
   --------------------------------------------------------- */
TRUNCATE TABLE dbo.ST_CoursePlace;
TRUNCATE TABLE dbo.ST_FramePlace;
TRUNCATE TABLE dbo.ST_StylePlace;
TRUNCATE TABLE dbo.ST_BloodSirePlace;
TRUNCATE TABLE dbo.ST_JockeyPlace;

/* ---------------------------------------------------------
   3-1) Course：jyo_cd × surface × distance_class
   - 基準：surface × distance_class の平均（#P0SurfaceDist）
   --------------------------------------------------------- */
INSERT INTO dbo.ST_CoursePlace
(
    jyo_cd, surface_type, distance_class,
    n, hit_place, p_place, p_place_smooth, mult, updated_at
)
SELECT
      s.jyo_cd
    , s.surface_type
    , s.distance_class
    , s.n
    , s.hit_place
    , CAST(CAST(s.hit_place AS DECIMAL(18,6)) / NULLIF(s.n, 0) AS DECIMAL(9,6)) AS p_place
    , CAST((CAST(s.hit_place AS DECIMAL(18,6)) + @k * p0sd.p0) / (s.n + @k) AS DECIMAL(9,6)) AS p_place_smooth
    , CAST(
        CASE
            WHEN p0sd.p0 IS NULL OR p0sd.p0 = 0 THEN 1.0
            ELSE
                CASE
                    WHEN ((CAST(s.hit_place AS DECIMAL(18,6)) + @k * p0sd.p0) / (s.n + @k)) / p0sd.p0 < @clip_min THEN @clip_min
                    WHEN ((CAST(s.hit_place AS DECIMAL(18,6)) + @k * p0sd.p0) / (s.n + @k)) / p0sd.p0 > @clip_max THEN @clip_max
                    ELSE ((CAST(s.hit_place AS DECIMAL(18,6)) + @k * p0sd.p0) / (s.n + @k)) / p0sd.p0
                END
        END
      AS DECIMAL(9,6)) AS mult
    , SYSDATETIME()
FROM
(
    SELECT
          b.jyo_cd
        , b.surface_type
        , b.distance_class
        , COUNT(1) AS n
        , SUM(b.is_place) AS hit_place
    FROM #BaseFiltered b
    GROUP BY b.jyo_cd, b.surface_type, b.distance_class
) s
INNER JOIN #P0SurfaceDist p0sd
    ON p0sd.surface_type = s.surface_type
   AND p0sd.distance_class = s.distance_class;

/* ---------------------------------------------------------
   3-2) Frame：+ frame_no（基準＝同コース条件平均 #P0Course）
   --------------------------------------------------------- */
INSERT INTO dbo.ST_FramePlace
(
    jyo_cd, surface_type, distance_class, frame_no,
    n, hit_place, p_place, p_place_smooth, mult, updated_at
)
SELECT
      s.jyo_cd
    , s.surface_type
    , s.distance_class
    , s.frame_no
    , s.n
    , s.hit_place
    , pre.p_place
    , pre.p_place_smooth
    , calc.mult_final
    , SYSDATETIME()
FROM
(
    SELECT
          b.jyo_cd
        , b.surface_type
        , b.distance_class
        , b.frame_no
        , COUNT(1) AS n
        , SUM(b.is_place) AS hit_place
    FROM #BaseFiltered b
    WHERE b.frame_no BETWEEN 1 AND 8
    GROUP BY b.jyo_cd, b.surface_type, b.distance_class, b.frame_no
) s
INNER JOIN #P0Course p0
    ON p0.jyo_cd = s.jyo_cd
   AND p0.surface_type = s.surface_type
   AND p0.distance_class = s.distance_class
CROSS APPLY
(
    SELECT
          CAST(CAST(s.hit_place AS DECIMAL(18,6)) / NULLIF(s.n, 0) AS DECIMAL(9,6)) AS p_place
        , CAST((CAST(s.hit_place AS DECIMAL(18,6)) + @k * p0.p0) / (s.n + @k) AS DECIMAL(9,6)) AS p_place_smooth
        , CAST(
              CASE
                  WHEN p0.p0 IS NULL OR p0.p0 = 0 THEN 1.0
                  ELSE ((CAST(s.hit_place AS DECIMAL(18,6)) + @k * p0.p0) / (s.n + @k)) / p0.p0
              END
          AS DECIMAL(18,10)) AS mult_raw
        , CAST(
              CASE
                  WHEN (s.n + @shrink_denom_frame) <= 0 THEN 0.0
                  ELSE CAST(s.n AS DECIMAL(18,10)) / CAST((s.n + @shrink_denom_frame) AS DECIMAL(18,10))
              END
          AS DECIMAL(18,10)) AS w
) pre
CROSS APPLY
(
    SELECT
        CAST(1.0 + (pre.w * @alpha_max_frame) * (pre.mult_raw - 1.0) AS DECIMAL(18,10)) AS mult_shrunk
) shr
CROSS APPLY
(
    SELECT
        CAST(
            CASE
                WHEN s.n < @min_n_frame THEN 1.0
                WHEN pre.mult_raw IS NULL THEN 1.0
                WHEN shr.mult_shrunk < @clip_min THEN @clip_min
                WHEN shr.mult_shrunk > @clip_max THEN @clip_max
                ELSE shr.mult_shrunk
            END
        AS DECIMAL(9,6)) AS mult_final
) calc;
/* ---------------------------------------------------------
   3-3) Style：+ style（4分類、基準＝同コース条件平均 #P0Course）
   --------------------------------------------------------- */
INSERT INTO dbo.ST_StylePlace
(
    jyo_cd, surface_type, distance_class, style,
    n, hit_place, p_place, p_place_smooth, mult, updated_at
)
SELECT
      s.jyo_cd
    , s.surface_type
    , s.distance_class
    , s.style
    , s.n
    , s.hit_place
    , calc.p_place
    , calc.p_place_smooth
    , calc.mult_final
    , SYSDATETIME()
FROM
(
    SELECT
          b.jyo_cd
        , b.surface_type
        , b.distance_class
        , b.style
        , COUNT(1) AS n
        , SUM(b.is_place) AS hit_place
    FROM #BaseFiltered b
    WHERE b.style IS NOT NULL
    GROUP BY b.jyo_cd, b.surface_type, b.distance_class, b.style
) s
INNER JOIN #P0Course p0
    ON p0.jyo_cd = s.jyo_cd
   AND p0.surface_type = s.surface_type
   AND p0.distance_class = s.distance_class
CROSS APPLY
(
    SELECT
          /* 生の複勝率 */
          CAST(CAST(s.hit_place AS DECIMAL(18,6)) / NULLIF(s.n, 0) AS DECIMAL(9,6)) AS p_place

        , /* 平滑化した複勝率 */
          CAST((CAST(s.hit_place AS DECIMAL(18,6)) + @k * p0.p0) / (s.n + @k) AS DECIMAL(9,6)) AS p_place_smooth

        , /* mult_raw（平均との差） */
          CAST(
              CASE
                  WHEN p0.p0 IS NULL OR p0.p0 = 0 THEN 1.0
                  ELSE ((CAST(s.hit_place AS DECIMAL(18,6)) + @k * p0.p0) / (s.n + @k)) / p0.p0
              END
          AS DECIMAL(18,10)) AS mult_raw

        , /* shrink の重み w */
          CAST(
              CASE
                  WHEN (s.n + @shrink_denom_style) <= 0 THEN 0.0
                  ELSE CAST(s.n AS DECIMAL(18,10)) / CAST((s.n + @shrink_denom_style) AS DECIMAL(18,10))
              END
          AS DECIMAL(18,10)) AS w
) pre
CROSS APPLY
(
    SELECT
          pre.p_place
        , pre.p_place_smooth

        , /* 1.0 への寄せ（shrink） */
          CAST(1.0 + (pre.w * @alpha_max_style) * (pre.mult_raw - 1.0) AS DECIMAL(18,10)) AS mult_shrunk
) shr
CROSS APPLY
(
    SELECT
        CAST(
            CASE
                /* 小母数は中立 */
                WHEN s.n < @min_n_style THEN 1.0
                WHEN p0.p0 IS NULL OR p0.p0 = 0 THEN 1.0

                /* clip */
                WHEN shr.mult_shrunk < @clip_min THEN @clip_min
                WHEN shr.mult_shrunk > @clip_max THEN @clip_max
                ELSE shr.mult_shrunk
            END
        AS DECIMAL(9,6)) AS mult_final

      , pre.p_place
      , pre.p_place_smooth
) calc;

/* ---------------------------------------------------------
   3-4) Blood（父）：nが小さいキーは中立（mult=1.0）
   --------------------------------------------------------- */
INSERT INTO dbo.ST_BloodSirePlace
(
    sire, jyo_cd, surface_type, distance_class,
    n, hit_place, p_place, p_place_smooth, mult, updated_at
)
SELECT
      s.sire
    , s.jyo_cd
    , s.surface_type
    , s.distance_class
    , s.n
    , s.hit_place
    , pre.p_place
    , pre.p_place_smooth
    , calc.mult_final
    , SYSDATETIME()
FROM
(
    SELECT
          b.sire
        , b.jyo_cd
        , b.surface_type
        , b.distance_class
        , COUNT(1) AS n
        , SUM(b.is_place) AS hit_place
    FROM #BaseFiltered b
    WHERE b.sire IS NOT NULL
    GROUP BY b.sire, b.jyo_cd, b.surface_type, b.distance_class
) s
INNER JOIN #P0Course p0
    ON p0.jyo_cd = s.jyo_cd
   AND p0.surface_type = s.surface_type
   AND p0.distance_class = s.distance_class
CROSS APPLY
(
    SELECT
          CAST(CAST(s.hit_place AS DECIMAL(18,6)) / NULLIF(s.n, 0) AS DECIMAL(9,6)) AS p_place
        , CAST((CAST(s.hit_place AS DECIMAL(18,6)) + @k * p0.p0) / (s.n + @k) AS DECIMAL(9,6)) AS p_place_smooth
        , CAST(
              CASE
                  WHEN p0.p0 IS NULL OR p0.p0 = 0 THEN 1.0
                  ELSE ((CAST(s.hit_place AS DECIMAL(18,6)) + @k * p0.p0) / (s.n + @k)) / p0.p0
              END
          AS DECIMAL(18,10)) AS mult_raw
        , CAST(
              CASE
                  WHEN (s.n + @shrink_denom_blood) <= 0 THEN 0.0
                  ELSE CAST(s.n AS DECIMAL(18,10)) / CAST((s.n + @shrink_denom_blood) AS DECIMAL(18,10))
              END
          AS DECIMAL(18,10)) AS w
) pre
CROSS APPLY
(
    SELECT
        CAST(1.0 + (pre.w * @alpha_max_blood) * (pre.mult_raw - 1.0) AS DECIMAL(18,10)) AS mult_shrunk
) shr
CROSS APPLY
(
    SELECT
        CAST(
            CASE
                WHEN s.n < @min_n_blood2 THEN 1.0
                WHEN pre.mult_raw IS NULL THEN 1.0
                WHEN shr.mult_shrunk < @clip_min THEN @clip_min
                WHEN shr.mult_shrunk > @clip_max THEN @clip_max
                ELSE shr.mult_shrunk
            END
        AS DECIMAL(9,6)) AS mult_final
) calc;

/* ---------------------------------------------------------
   3-5) Jockey：nが小さいキーは中立（mult=1.0）
   --------------------------------------------------------- */
INSERT INTO dbo.ST_JockeyPlace
(
    jockey_name, jyo_cd, surface_type, distance_class,
    n, hit_place, p_place, p_place_smooth, mult, updated_at
)
SELECT
      s.jockey_name
    , s.jyo_cd
    , s.surface_type
    , s.distance_class
    , s.n
    , s.hit_place
    , pre.p_place
    , pre.p_place_smooth
    , calc.mult_final
    , SYSDATETIME()
FROM
(
    SELECT
          b.jockey_name
        , b.jyo_cd
        , b.surface_type
        , b.distance_class
        , COUNT(1) AS n
        , SUM(b.is_place) AS hit_place
    FROM #BaseFiltered b
    WHERE b.jockey_name IS NOT NULL
    GROUP BY b.jockey_name, b.jyo_cd, b.surface_type, b.distance_class
) s
INNER JOIN #P0Course p0
    ON p0.jyo_cd = s.jyo_cd
   AND p0.surface_type = s.surface_type
   AND p0.distance_class = s.distance_class
CROSS APPLY
(
    SELECT
          CAST(CAST(s.hit_place AS DECIMAL(18,6)) / NULLIF(s.n, 0) AS DECIMAL(9,6)) AS p_place
        , CAST((CAST(s.hit_place AS DECIMAL(18,6)) + @k * p0.p0) / (s.n + @k) AS DECIMAL(9,6)) AS p_place_smooth
        , CAST(
              CASE
                  WHEN p0.p0 IS NULL OR p0.p0 = 0 THEN 1.0
                  ELSE ((CAST(s.hit_place AS DECIMAL(18,6)) + @k * p0.p0) / (s.n + @k)) / p0.p0
              END
          AS DECIMAL(18,10)) AS mult_raw
        , CAST(
              CASE
                  WHEN (s.n + @shrink_denom_jockey) <= 0 THEN 0.0
                  ELSE CAST(s.n AS DECIMAL(18,10)) / CAST((s.n + @shrink_denom_jockey) AS DECIMAL(18,10))
              END
          AS DECIMAL(18,10)) AS w
) pre
CROSS APPLY
(
    SELECT
        CAST(1.0 + (pre.w * @alpha_max_jockey) * (pre.mult_raw - 1.0) AS DECIMAL(18,10)) AS mult_shrunk
) shr
CROSS APPLY
(
    SELECT
        CAST(
            CASE
                WHEN s.n < @min_n_jockey2 THEN 1.0
                WHEN pre.mult_raw IS NULL THEN 1.0
                WHEN shr.mult_shrunk < @clip_min THEN @clip_min
                WHEN shr.mult_shrunk > @clip_max THEN @clip_max
                ELSE shr.mult_shrunk
            END
        AS DECIMAL(9,6)) AS mult_final
) calc;

/* ---------------------------------------------------------
   4) ざっくり確認
   --------------------------------------------------------- */
SELECT 'ST_CoursePlace' AS tbl, COUNT(1) AS rows_cnt FROM dbo.ST_CoursePlace
UNION ALL SELECT 'ST_FramePlace', COUNT(1) FROM dbo.ST_FramePlace
UNION ALL SELECT 'ST_StylePlace', COUNT(1) FROM dbo.ST_StylePlace
UNION ALL SELECT 'ST_BloodSirePlace', COUNT(1) FROM dbo.ST_BloodSirePlace
UNION ALL SELECT 'ST_JockeyPlace', COUNT(1) FROM dbo.ST_JockeyPlace;
