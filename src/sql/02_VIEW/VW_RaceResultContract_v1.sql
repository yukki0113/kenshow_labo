CREATE OR ALTER VIEW dbo.VW_RaceResultContract_v1
AS
WITH Base AS
(
    SELECT
          r.race_id
        , r.race_date       AS [日付]
        , r.race_name       AS [レース名]
        , r.race_class      AS [クラス]
        , r.class_simple    AS classSimple
        , r.win5_flg        AS WIN5_flg
        , r.track_name      AS [場名]
        , r.surface_type    AS [芝_ダート]
        , r.distance_m      AS [距離]
        , r.frame_no        AS [枠番]
        , r.horse_no        AS [馬番]
        , r.horse_id
        , r.horse_name      AS [馬名]
        , r.sex             AS [性]
        , r.age             AS [齢]
        , r.jockey_name     AS [騎手]
        , r.carried_weight  AS [斤量]
        , r.popularity      AS [人気]
        , r.odds            AS [オッズ]
        , r.finish_pos      AS [着順]
        , r.finish_time     AS [走破時計]
        , r.horse_weight    AS [馬体重]
        , r.horse_weight_diff AS [馬体重変動]
        , r.pos_corner1     AS [通過順_1角]
        , r.pos_corner2     AS [通過順_2角]
        , r.pos_corner3     AS [通過順_3角]
        , r.pos_corner4     AS [通過順_4角]
        , r.final_3f        AS [上がり]
        , r.meeting         AS [開催]
        , r.turn            AS [回り]
        , r.going           AS [馬場]
        , r.weather         AS [天気]
        , r.track_id
        , COUNT(*) OVER (PARTITION BY r.race_id) AS [頭数]
    FROM dbo.TR_RaceResult AS r
)
SELECT
      b.race_id
    , b.[日付]
    , b.[レース名]
    , b.[クラス]
    , b.classSimple
    , b.WIN5_flg
    , b.[場名]
    , b.[芝_ダート]
    , b.[距離]
    , b.[枠番]
    , b.[馬番]
    , b.horse_id
    , b.[馬名]
    , b.[性]
    , b.[齢]
    , b.[騎手]
    , b.[斤量]
    , b.[人気]
    , b.[オッズ]
    , b.[着順]
    , b.[走破時計]
    , b.[馬体重]
    , b.[馬体重変動]
    , b.[通過順_1角]
    , b.[通過順_2角]
    , b.[通過順_3角]
    , b.[通過順_4角]
    , b.[上がり]
    , b.[開催]
    , b.[回り]
    , b.[馬場]
    , b.[天気]
    , b.track_id

    -- 血統（必要列だけ・LEFT JOIN）
    , p.horse_name  AS [血統馬名]
    , p.sire        AS [父]
    , p.dam         AS [母]
    , p.dam_sire    AS [母父]
    , p.dam_dam     AS [母母]
    , p.sire_sire   AS [父父]
    , p.sire_dam    AS [父母]

    -- 脚質（TRに無いのでVIEWで算出）
    , b.[頭数]
    , CASE
          WHEN b.[通過順_1角] IS NOT NULL
           AND b.[通過順_2角] IS NOT NULL
           AND b.[通過順_3角] IS NOT NULL
           AND b.[通過順_4角] IS NOT NULL
           AND b.[通過順_2角] > FLOOR(b.[頭数] * 2.0 / 3.0)
           AND b.[通過順_4角] <= 2
              THEN N'まくり'
          WHEN b.[通過順_4角] IS NULL
              THEN N'不明'
          WHEN b.[通過順_4角] <= 2
              THEN N'逃げ'
          WHEN b.[通過順_4角] <= FLOOR(b.[頭数] / 3.0)
              THEN N'先行'
          WHEN b.[通過順_4角] <= FLOOR(b.[頭数] * 2.0 / 3.0)
              THEN N'差し'
          ELSE
              N'追い込み'
      END AS [脚質]

    -- 払戻（単勝/複勝のみ）
    , sp.payout_amount AS [単勝払戻]
    , sp.popularity    AS [単勝人気]
    , fp.payout_amount AS [複勝払戻]
    , fp.popularity    AS [複勝人気]
FROM Base AS b
LEFT JOIN dbo.MT_HorsePedigree AS p
    ON b.horse_id = p.horse_id
LEFT JOIN dbo.TR_Payout AS sp
    ON  b.race_id = sp.race_id
    AND b.[馬番]  = sp.horse_no_1
    AND sp.bet_type = N'単勝'
LEFT JOIN dbo.TR_Payout AS fp
    ON  b.race_id = fp.race_id
    AND b.[馬番]  = fp.horse_no_1
    AND fp.bet_type = N'複勝';
GO
