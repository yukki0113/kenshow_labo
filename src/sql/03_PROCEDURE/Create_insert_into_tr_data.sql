CREATE OR ALTER PROCEDURE [Insert_Into_TRData] AS
BEGIN 

    /* 完走 */
    INSERT 
      INTO TR_RaceResult( 
        race_id
        , horse_id
        , [馬名]
        , [騎手]
        , [馬番]
        , [走破時計]
        , [オッズ]
        , [通過順_1角]
        , [通過順_2角]
        , [通過順_3角]
        , [通過順_4角]
        , [着順]
        , [馬体重]
        , [馬体重変動]
        , [性]
        , [齢]
        , [斤量]
        , [上がり]
        , [人気]
        , [レース名]
        , [日付]
        , [開催]
        , [クラス]
        , [classSimple]
        , [芝_ダート]
        , [距離]
        , [回り]
        , [馬場]
        , [天気]
        , track_id
        , [場名]
        , WIN5_flg
      ) 
    SELECT
          F.race_id
        , F.horse_id
        , F.[馬名]
        , F.[騎手]
        , CAST(F.[馬番] AS INT) AS [馬番]
        , CAST(CONCAT('00:', F.[走破時計]) AS TIME) AS [走破時計]
        , CAST(F.[オッズ] AS DECIMAL (4, 1)) AS [オッズ]
        , CASE 
          WHEN CHARINDEX('-', F.[通過順]) = 0 
            THEN CAST(F.[通過順] AS INT) 
          ELSE CAST(PARSENAME(REPLACE (F.[通過順], '-', '.'), 4) AS INT) 
          END AS [通過順_1角]
        , CASE 
          WHEN CHARINDEX('-', F.[通過順]) = 0 
            THEN NULL 
          WHEN CHARINDEX('-', F.[通過順]) = LEN(F.[通過順]) - CHARINDEX('-', REVERSE(F.[通過順])) + 2 
            THEN CAST(PARSENAME(REPLACE (F.[通過順], '-', '.'), 3) AS INT) 
          ELSE CAST(PARSENAME(REPLACE (F.[通過順], '-', '.'), 3) AS INT) 
          END AS [通過順_2角]
        , CASE 
          WHEN CHARINDEX('-', F.[通過順]) = 0 
            THEN NULL 
          WHEN CHARINDEX('-', F.[通過順]) = LEN(F.[通過順]) - CHARINDEX('-', REVERSE(F.[通過順])) + 2 
            THEN NULL 
          ELSE CAST(PARSENAME(REPLACE (F.[通過順], '-', '.'), 2) AS INT) 
          END AS [通過順_3角]
        , CASE 
          WHEN CHARINDEX('-', F.[通過順]) = 0 
            THEN NULL 
          ELSE CAST(PARSENAME(REPLACE (F.[通過順], '-', '.'), 1) AS INT) 
          END AS [通過順_4角]
        , CAST(F.[着順] AS INT) AS [着順]
        , CAST(F.[馬体重] AS INT) AS [馬体重] 
        , CAST(F.[馬体重変動] AS INT) AS [馬体重変動]
        , F.[性]
        , CAST(F.[齢] AS INT) AS [齢]
        , CAST(F.[斤量] AS DECIMAL (3, 1)) AS [斤量]
        , CAST(F.[上がり] AS DECIMAL (3, 1)) AS [上がり]
        , CAST(F.[人気] AS INT) AS [人気]
        , CASE
          WHEN F.[レース名] LIKE '%第%回%' 
            THEN CASE 
            WHEN CHARINDEX('(', F.[レース名]) > 0 
              THEN SUBSTRING(F.[レース名], 1, CHARINDEX('(', F.[レース名]) - 1) 
            ELSE F.[レース名] 
            END 
          ELSE CASE 
            WHEN CHARINDEX('(', F.[レース名]) > 0 
              THEN SUBSTRING(F.[レース名], 1, CHARINDEX('(', F.[レース名]) - 1) 
            ELSE F.[レース名] 
            END 
          END AS [レース名]
        , CONVERT(
              DATE,
              CONCAT(
                  SUBSTRING(F.[日付], 1, CHARINDEX(N'年', F.[日付]) - 1),
                  FORMAT(
                      CONVERT(
                          INT,
                          SUBSTRING(
                              F.[日付],
                              CHARINDEX(N'年', F.[日付]) + 1,
                              CHARINDEX(N'月', F.[日付]) - CHARINDEX(N'年', F.[日付]) - 1
                          )
                      ),
                      '00'
                  ),
                  FORMAT(
                      CONVERT(
                          INT,
                          SUBSTRING(
                              F.[日付],
                              CHARINDEX(N'月', F.[日付]) + 1,
                              CHARINDEX(N'日', F.[日付]) - CHARINDEX(N'月', F.[日付]) - 1
                          )
                      ),
                      '00'
                  )
              )
          ) AS [日付]
        , F.[開催]
        , CASE 
            WHEN F.[レース名] LIKE '%(GI)%' OR F.[レース名] LIKE '%(G1)%'
              THEN 'G1' 
            WHEN F.[レース名] LIKE '%(GII)%' OR F.[レース名] LIKE '%(G2)%'
              THEN 'G2' 
            WHEN F.[レース名] LIKE '%(GIII)%' OR F.[レース名] LIKE '%(G3)%'
              THEN 'G3' 
            WHEN F.[レース名] LIKE '%(L)%' 
              THEN CONCAT(F.[クラス], '(L)') 
            ELSE F.[クラス] 
          END AS [クラス]
        , CASE
            WHEN F.classSrc IS NULL OR LTRIM(RTRIM(F.classSrc)) = N'' THEN NULL
            WHEN F.classSrc LIKE N'%新馬%' THEN N'新馬'
            WHEN F.classSrc LIKE N'%未勝利%' THEN N'未勝利'
            WHEN F.classSrc LIKE N'%500万%' OR F.classSrc LIKE N'%1勝クラス%' THEN N'1勝クラス'
            WHEN F.classSrc LIKE N'%1000万%' OR F.classSrc LIKE N'%2勝クラス%' THEN N'2勝クラス'
            WHEN F.classSrc LIKE N'%1500万%' OR F.classSrc LIKE N'%3勝クラス%' THEN N'3勝クラス'
            WHEN F.classSrc = N'L' OR F.classSrc LIKE N'%(L)%' THEN N'L'
            WHEN F.classSrc = N'OP' OR F.classSrc LIKE N'%オープン%' THEN N'OP'
            WHEN F.classSrc = N'G3' OR F.classSrc LIKE N'%GIII%' OR F.classSrc LIKE N'%G3%' THEN N'G3'
            WHEN F.classSrc = N'G2' OR F.classSrc LIKE N'%GII%'  OR F.classSrc LIKE N'%G2%' THEN N'G2'
            WHEN F.classSrc = N'G1' OR F.classSrc LIKE N'%GI%'   OR F.classSrc LIKE N'%G1%' THEN N'G1'
            ELSE F.classSrc
          END AS [classSimple]
        , F.[芝_ダート]
        , CAST(F.[距離] AS INT) AS [距離]
        , F.[回り]
        , F.[馬場]
        , F.[天気]
        , F.track_id
        , F.[場名] 
        , F.WIN5_flg AS WIN5_flg
      FROM
        (
        SELECT
              F.*
            , CASE WHEN W.race_id IS NOT NULL THEN 1 ELSE 0 END AS WIN5_flg
            , CASE 
                WHEN F.[レース名] LIKE '%(GI)%' OR F.[レース名] LIKE '%(G1)%'
                  THEN 'G1' 
                WHEN F.[レース名] LIKE '%(GII)%' OR F.[レース名] LIKE '%(G2)%'
                  THEN 'G2' 
                WHEN F.[レース名] LIKE '%(GIII)%' OR F.[レース名] LIKE '%(G3)%'
                  THEN 'G3' 
                WHEN F.[レース名] LIKE '%(L)%' 
                  THEN 'L'
                ELSE F.[クラス] 
              END AS classSrc
        FROM
          IF_RaceResult_CSV F
          LEFT JOIN MT_Win5Target W
            ON W.race_id = F.race_id
      ) F
      WHERE
        ISNUMERIC(F.[着順]) = 1
        AND NOT EXISTS ( 
          SELECT
                1 
            FROM
              TR_RaceResult T
            WHERE
              T.race_id = F.race_id
              AND T.[馬番] = F.[馬番]
        ) 

    /* 完走以外 */
    INSERT 
      INTO TR_RaceResult( 
        race_id
        , horse_id
        , [馬名]
        , [騎手]
        , [馬番]
        , [走破時計]
        , [オッズ]
        , [通過順_1角]
        , [通過順_2角]
        , [通過順_3角]
        , [通過順_4角]
        , [着順]
        , [馬体重]
        , [馬体重変動]
        , [性]
        , [齢]
        , [斤量]
        , [上がり]
        , [人気]
        , [レース名]
        , [日付]
        , [開催]
        , [クラス]
        , [classSimple]
        , [芝_ダート]
        , [距離]
        , [回り]
        , [馬場]
        , [天気]
        , track_id
        , [場名]
        , WIN5_flg
      ) 
    SELECT
          F.race_id
        , F.horse_id
        , F.[馬名]
        , F.[騎手]
        , CAST(F.[馬番] AS INT) AS [馬番]
        , CAST('00:00' AS TIME) AS [走破時計]
        , CASE WHEN ISNUMERIC(F.[オッズ]) = 0
               THEN 999.9
               ELSE CAST(F.[オッズ] AS DECIMAL (4, 1)) END AS [オッズ]
        , CASE 
          WHEN CHARINDEX('-', F.[通過順]) = 0 
            THEN CAST(F.[通過順] AS INT) 
          ELSE CAST(PARSENAME(REPLACE (F.[通過順], '-', '.'), 4) AS INT) 
          END AS [通過順_1角]
        , CASE 
          WHEN CHARINDEX('-', F.[通過順]) = 0 
            THEN NULL 
          WHEN CHARINDEX('-', F.[通過順]) = LEN(F.[通過順]) - CHARINDEX('-', REVERSE(F.[通過順])) + 2 
            THEN CAST(PARSENAME(REPLACE (F.[通過順], '-', '.'), 3) AS INT) 
          ELSE CAST(PARSENAME(REPLACE (F.[通過順], '-', '.'), 3) AS INT) 
          END AS [通過順_2角]
        , CASE 
          WHEN CHARINDEX('-', F.[通過順]) = 0 
            THEN NULL 
          WHEN CHARINDEX('-', F.[通過順]) = LEN(F.[通過順]) - CHARINDEX('-', REVERSE(F.[通過順])) + 2 
            THEN NULL 
          ELSE CAST(PARSENAME(REPLACE (F.[通過順], '-', '.'), 2) AS INT) 
          END AS [通過順_3角]
        , CASE 
          WHEN CHARINDEX('-', F.[通過順]) = 0 
            THEN NULL 
          ELSE CAST(PARSENAME(REPLACE (F.[通過順], '-', '.'), 1) AS INT) 
          END AS [通過順_4角]
        , CASE WHEN F.[着順] = N'中' THEN 99 ELSE 0 END AS [着順]
        , CAST(F.[馬体重] AS INT) AS [馬体重] 
        , CAST(F.[馬体重変動] AS INT) AS [馬体重変動]
        , F.[性]
        , CAST(F.[齢] AS INT) AS [齢]
        , CAST(F.[斤量] AS DECIMAL(3, 1)) AS [斤量]
        , CAST(0 AS DECIMAL(3, 1)) AS [上がり]
        , CASE WHEN CAST(F.[人気] AS INT) = 0
               THEN 0
               ELSE CAST(F.[人気] AS INT) END AS [人気]
        , CASE
          WHEN F.[レース名] LIKE '%第%回%' 
            THEN CASE 
            WHEN CHARINDEX('(', F.[レース名]) > 0 
              THEN SUBSTRING(F.[レース名], 1, CHARINDEX('(', F.[レース名]) - 1) 
            ELSE F.[レース名] 
            END 
          ELSE CASE 
            WHEN CHARINDEX('(', F.[レース名]) > 0 
              THEN SUBSTRING(F.[レース名], 1, CHARINDEX('(', F.[レース名]) - 1) 
            ELSE F.[レース名] 
            END 
          END AS [レース名]
        , CONVERT(
              DATE,
              CONCAT(
                  SUBSTRING(F.[日付], 1, CHARINDEX(N'年', F.[日付]) - 1),
                  FORMAT(
                      CONVERT(
                          INT,
                          SUBSTRING(
                              F.[日付],
                              CHARINDEX(N'年', F.[日付]) + 1,
                              CHARINDEX(N'月', F.[日付]) - CHARINDEX(N'年', F.[日付]) - 1
                          )
                      ),
                      '00'
                  ),
                  FORMAT(
                      CONVERT(
                          INT,
                          SUBSTRING(
                              F.[日付],
                              CHARINDEX(N'月', F.[日付]) + 1,
                              CHARINDEX(N'日', F.[日付]) - CHARINDEX(N'月', F.[日付]) - 1
                          )
                      ),
                      '00'
                  )
              )
          ) AS [日付]
        , F.[開催]
        , CASE 
            WHEN F.[レース名] LIKE '%(GI)%' OR F.[レース名] LIKE '%(G1)%'
              THEN 'G1' 
            WHEN F.[レース名] LIKE '%(GII)%' OR F.[レース名] LIKE '%(G2)%'
              THEN 'G2' 
            WHEN F.[レース名] LIKE '%(GIII)%' OR F.[レース名] LIKE '%(G3)%'
              THEN 'G3' 
            WHEN F.[レース名] LIKE '%(L)%' 
              THEN CONCAT(F.[クラス], '(L)') 
            ELSE F.[クラス] 
          END AS [クラス]
        , CASE
            WHEN F.classSrc IS NULL OR LTRIM(RTRIM(F.classSrc)) = N'' THEN NULL
            WHEN F.classSrc LIKE N'%新馬%' THEN N'新馬'
            WHEN F.classSrc LIKE N'%未勝利%' THEN N'未勝利'
            WHEN F.classSrc LIKE N'%500万%' OR F.classSrc LIKE N'%1勝クラス%' THEN N'1勝クラス'
            WHEN F.classSrc LIKE N'%1000万%' OR F.classSrc LIKE N'%2勝クラス%' THEN N'2勝クラス'
            WHEN F.classSrc LIKE N'%1500万%' OR F.classSrc LIKE N'%3勝クラス%' THEN N'3勝クラス'
            WHEN F.classSrc = N'L' OR F.classSrc LIKE N'%(L)%' THEN N'L'
            WHEN F.classSrc = N'OP' OR F.classSrc LIKE N'%オープン%' THEN N'OP'
            WHEN F.classSrc = N'G3' OR F.classSrc LIKE N'%GIII%' OR F.classSrc LIKE N'%G3%' THEN N'G3'
            WHEN F.classSrc = N'G2' OR F.classSrc LIKE N'%GII%'  OR F.classSrc LIKE N'%G2%' THEN N'G2'
            WHEN F.classSrc = N'G1' OR F.classSrc LIKE N'%GI%'   OR F.classSrc LIKE N'%G1%' THEN N'G1'
            ELSE F.classSrc
          END AS [classSimple]
        , F.[芝_ダート]
        , CAST(F.[距離] AS INT) AS [距離]
        , F.[回り]
        , F.[馬場]
        , F.[天気]
        , F.track_id
        , F.[場名]
        , F.WIN5_flg AS WIN5_flg
      FROM
        (
        SELECT
              F.*
            , CASE WHEN W.race_id IS NOT NULL THEN 1 ELSE 0 END AS WIN5_flg
            , CASE 
                WHEN F.[レース名] LIKE '%(GI)%' OR F.[レース名] LIKE '%(G1)%'
                  THEN 'G1' 
                WHEN F.[レース名] LIKE '%(GII)%' OR F.[レース名] LIKE '%(G2)%'
                  THEN 'G2' 
                WHEN F.[レース名] LIKE '%(GIII)%' OR F.[レース名] LIKE '%(G3)%'
                  THEN 'G3' 
                WHEN F.[レース名] LIKE '%(L)%' 
                  THEN 'L'
                ELSE F.[クラス] 
              END AS classSrc
        FROM
          IF_RaceResult_CSV F
          LEFT JOIN MT_Win5Target W
            ON W.race_id = F.race_id
      ) F
      WHERE
        ISNUMERIC(F.[着順]) = 0
        AND NOT EXISTS ( 
          SELECT
                1 
            FROM
              TR_RaceResult T
            WHERE
              T.race_id = F.race_id
              AND T.[馬番] = F.[馬番]
        ) 

END
GO