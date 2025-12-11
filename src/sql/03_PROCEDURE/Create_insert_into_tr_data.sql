CREATE PROCEDURE [Insert_Into_TRData] AS
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
        , [芝_ダート]
        , [距離]
        , [回り]
        , [馬場]
        , [天気]
        , track_id
        , [場名]
      ) 
    SELECT
          race_id
        , horse_id
        , [馬名]
        , [騎手]
        , CAST([馬番] AS INT) AS [馬番]
        , CAST(CONCAT('00:', [走破時計]) AS TIME) AS [走破時計]
        , CAST([オッズ] AS DECIMAL (4, 1)) AS [オッズ]
        , CASE 
          WHEN CHARINDEX('-', [通過順]) = 0 
            THEN CAST([通過順] AS INT) 
          ELSE CAST(PARSENAME(REPLACE ([通過順], '-', '.'), 4) AS INT) 
          END AS [通過順_1角]
        , CASE 
          WHEN CHARINDEX('-', [通過順]) = 0 
            THEN NULL 
          WHEN CHARINDEX('-', [通過順]) = LEN([通過順]) - CHARINDEX('-', REVERSE([通過順])) + 2 
            THEN CAST(PARSENAME(REPLACE ([通過順], '-', '.'), 3) AS INT) 
          ELSE CAST(PARSENAME(REPLACE ([通過順], '-', '.'), 3) AS INT) 
          END AS [通過順_2角]
        , CASE 
          WHEN CHARINDEX('-', [通過順]) = 0 
            THEN NULL 
          WHEN CHARINDEX('-', [通過順]) = LEN([通過順]) - CHARINDEX('-', REVERSE([通過順])) + 2 
            THEN NULL 
          ELSE CAST(PARSENAME(REPLACE ([通過順], '-', '.'), 2) AS INT) 
          END AS [通過順_3角]
        , CASE 
          WHEN CHARINDEX('-', [通過順]) = 0 
            THEN NULL 
          ELSE CAST(PARSENAME(REPLACE ([通過順], '-', '.'), 1) AS INT) 
          END AS [通過順_4角]
        , CAST([着順] AS INT) AS [着順]
        , CAST([馬体重] AS INT) AS [馬体重] 
        , CAST([馬体重変動] AS INT) AS [馬体重変動]
        , [性]
        , CAST([齢] AS INT) AS [齢]
        , CAST([斤量] AS DECIMAL (3, 1)) AS [斤量]
        , CAST([上がり] AS DECIMAL (3, 1)) AS [上がり]
        , CAST([人気] AS INT) AS [人気]
        , CASE
          WHEN [レース名] LIKE '%第%回%' 
            THEN CASE 
            WHEN CHARINDEX('(', [レース名]) > 0 
              THEN SUBSTRING([レース名], 1, CHARINDEX('(', [レース名]) - 1) 
            ELSE [レース名] 
            END 
          ELSE CASE 
            WHEN CHARINDEX('(', [レース名]) > 0 
              THEN SUBSTRING([レース名], 1, CHARINDEX('(', [レース名]) - 1) 
            ELSE [レース名] 
            END 
          END AS [レース名]
        , CONVERT(DATE, CONCAT(SUBSTRING([日付], 1, CHARINDEX('年', [日付]) - 1), FORMAT(CONVERT(INT, SUBSTRING([日付], CHARINDEX('年', [日付]) + 1, CHARINDEX('月', [日付]) - CHARINDEX('年', [日付]) - 1)), '00'), FORMAT(CONVERT(INT, SUBSTRING([日付], CHARINDEX('月', [日付]) + 1, CHARINDEX('日', [日付]) - CHARINDEX('月', [日付]) - 1)), '00'))) AS [日付]
        , [開催]
        , CASE 
          WHEN [レース名] LIKE '%(G1)%' 
            THEN 'GI' 
          WHEN [レース名] LIKE '%(G2)%' 
            THEN 'GII' 
          WHEN [レース名] LIKE '%(G3)%' 
            THEN 'GIII' 
          WHEN [レース名] LIKE '%(L)%' 
            THEN CONCAT([クラス], '(L)') 
          ELSE [クラス] 
          END AS [クラス]
        , [芝_ダート]
        , CAST([距離] AS INT) AS [距離]
        , [回り]
        , [馬場]
        , [天気]
        , track_id
        , [場名] 
      FROM
        IF_RaceResult_CSV 
      WHERE
        ISNUMERIC([着順]) = 1
        AND NOT EXISTS ( 
          SELECT
                1 
            FROM
              TR_RaceResult 
            WHERE
              TR_RaceResult.race_id = IF_RaceResult_CSV.race_id
              AND TR_RaceResult.[馬番] = IF_RaceResult_CSV.[馬番]
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
        , [芝_ダート]
        , [距離]
        , [回り]
        , [馬場]
        , [天気]
        , track_id
        , [場名]
      ) 
    SELECT
          race_id AS race_id
        , horse_id AS horse_id
        , [馬名] AS [馬名]
        , [騎手] AS [騎手]
        , CAST([馬番] AS INT) AS [馬番]
        , CAST('00:00' AS TIME) AS [走破時計]
        , CASE WHEN ISNUMERIC([オッズ]) = 0 THEN 999.9 ELSE CAST([オッズ] AS DECIMAL (4, 1)) END AS [オッズ]
        , CASE 
          WHEN CHARINDEX('-', [通過順]) = 0 
            THEN CAST([通過順] AS INT) 
          ELSE CAST(PARSENAME(REPLACE ([通過順], '-', '.'), 4) AS INT) 
          END AS [通過順_1角]
        , CASE 
          WHEN CHARINDEX('-', [通過順]) = 0 
            THEN NULL 
          WHEN CHARINDEX('-', [通過順]) = LEN([通過順]) - CHARINDEX('-', REVERSE([通過順])) + 2 
            THEN CAST(PARSENAME(REPLACE ([通過順], '-', '.'), 3) AS INT) 
          ELSE CAST(PARSENAME(REPLACE ([通過順], '-', '.'), 3) AS INT) 
          END AS [通過順_2角]
        , CASE 
          WHEN CHARINDEX('-', [通過順]) = 0 
            THEN NULL 
          WHEN CHARINDEX('-', [通過順]) = LEN([通過順]) - CHARINDEX('-', REVERSE([通過順])) + 2 
            THEN NULL 
          ELSE CAST(PARSENAME(REPLACE ([通過順], '-', '.'), 2) AS INT) 
          END AS [通過順_3角]
        , CASE 
          WHEN CHARINDEX('-', [通過順]) = 0 
            THEN NULL 
          ELSE CAST(PARSENAME(REPLACE ([通過順], '-', '.'), 1) AS INT) 
          END AS [通過順_4角]
        , CASE WHEN [着順] = '中' THEN 99 ELSE 0 END AS [着順]
        , CAST([馬体重] AS INT) AS [馬体重]
        , CAST([馬体重変動] AS INT) AS [馬体重変動]
        , [性] AS [性]
        , CAST([齢] AS INT) AS [齢]
        , CAST([斤量] AS DECIMAL(3, 1)) AS [斤量]
        , CAST(0 AS DECIMAL(3, 1)) AS [上がり]
        , CASE WHEN CAST([人気] AS INT) = 0 THEN 0 ELSE CAST([人気] AS INT) END AS [人気]
        , CASE
          WHEN [レース名] LIKE '%第%回%' 
            THEN CASE 
            WHEN CHARINDEX('(', [レース名]) > 0 
              THEN SUBSTRING([レース名], 1, CHARINDEX('(', [レース名]) - 1) 
            ELSE [レース名] 
            END 
          ELSE CASE 
            WHEN CHARINDEX('(', [レース名]) > 0 
              THEN SUBSTRING([レース名], 1, CHARINDEX('(', [レース名]) - 1) 
            ELSE [レース名] 
            END 
          END AS [レース名]
        , CONVERT(DATE, CONCAT(SUBSTRING([日付], 1, CHARINDEX('年', [日付]) - 1), FORMAT(CONVERT(INT, SUBSTRING([日付], CHARINDEX('年', [日付]) + 1, CHARINDEX('月', [日付]) - CHARINDEX('年', [日付]) - 1)), '00'), FORMAT(CONVERT(INT, SUBSTRING([日付], CHARINDEX('月', [日付]) + 1, CHARINDEX('日', [日付]) - CHARINDEX('月', [日付]) - 1)), '00'))) AS [日付]
        , [開催] AS [開催]
        , CASE 
          WHEN [レース名] LIKE '%(G1)%' 
            THEN 'GI' 
          WHEN [レース名] LIKE '%(G2)%' 
            THEN 'GII' 
          WHEN [レース名] LIKE '%(G3)%' 
            THEN 'GIII' 
          WHEN [レース名] LIKE '%(L)%' 
            THEN CONCAT([クラス], '(L)') 
          ELSE [クラス] 
          END AS [クラス]
        , [芝_ダート] AS [芝_ダート]
        , CAST([距離] AS INT) AS [距離]
        , [回り] AS [回り]
        , [馬場] AS [馬場]
        , [天気] AS [天気]
        , track_id AS track_id
        , [場名] AS [場名] 
      FROM
        IF_RaceResult_CSV 
      WHERE
        ISNUMERIC([着順]) = 0
        AND NOT EXISTS ( 
          SELECT
                1 
            FROM
              TR_RaceResult 
            WHERE
              TR_RaceResult.race_id = IF_RaceResult_CSV.race_id
              AND TR_RaceResult.[馬番] = IF_RaceResult_CSV.[馬番]
        ) 
    
    
END
GO

