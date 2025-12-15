CREATE OR ALTER VIEW dbo.VW_RaceResultContract_v1
AS
WITH Base AS
(
    SELECT
          r.race_id
        , r.日付
        , r.レース名
        , r.クラス
        , r.classSimple
        , r.WIN5_flg
        , r.場名
        , r.芝_ダート
        , r.距離
        , r.枠番
        , r.馬番
        , r.horse_id
        , r.馬名
        , r.性
        , r.齢
        , r.騎手
        , r.斤量
        , r.人気
        , r.オッズ
        , r.着順
        , r.走破時計
        , r.馬体重
        , r.馬体重変動
        , r.通過順_1角
        , r.通過順_2角
        , r.通過順_3角
        , r.通過順_4角
        , r.上がり
        , r.開催
        , r.回り
        , r.馬場
        , r.天気
        , r.track_id
        , COUNT(*) OVER (PARTITION BY r.race_id) AS 頭数
    FROM dbo.TR_RaceResult AS r
)
SELECT
      b.race_id
    , b.日付
    , b.レース名
    , b.クラス
    , b.classSimple
    , b.WIN5_flg
    , b.場名
    , b.芝_ダート
    , b.距離
    , b.枠番
    , b.馬番
    , b.horse_id
    , b.馬名
    , b.性
    , b.齢
    , b.騎手
    , b.斤量
    , b.人気
    , b.オッズ
    , b.着順
    , b.走破時計
    , b.馬体重
    , b.馬体重変動
    , b.通過順_1角
    , b.通過順_2角
    , b.通過順_3角
    , b.通過順_4角
    , b.上がり
    , b.開催
    , b.回り
    , b.馬場
    , b.天気
    , b.track_id

    -- 血統（必要列だけ・LEFT JOIN）
    , p.馬名 AS 血統馬名
    , p.父
    , p.母
    , p.母父
    , p.母母
    , p.父父
    , p.父母

    -- 脚質（TRに無いのでVIEWで算出）
    , b.頭数
    , CASE
          WHEN b.通過順_1角 IS NOT NULL
           AND b.通過順_2角 IS NOT NULL
           AND b.通過順_3角 IS NOT NULL
           AND b.通過順_4角 IS NOT NULL
           AND b.通過順_2角 > FLOOR(b.頭数 * 2.0 / 3.0)
           AND b.通過順_4角 <= 2
              THEN N'まくり'
          WHEN b.通過順_4角 IS NULL
              THEN N'不明'
          WHEN b.通過順_4角 <= 2
              THEN N'逃げ'
          WHEN b.通過順_4角 <= FLOOR(b.頭数 / 3.0)
              THEN N'先行'
          WHEN b.通過順_4角 <= FLOOR(b.頭数 * 2.0 / 3.0)
              THEN N'差し'
          ELSE
              N'追い込み'
      END AS 脚質

    -- 払戻（単勝/複勝のみ）
    , sp.払戻金 AS 単勝払戻
    , sp.人気   AS 単勝人気
    , fp.払戻金 AS 複勝払戻
    , fp.人気   AS 複勝人気
FROM Base AS b
LEFT JOIN dbo.MT_HorsePedigree AS p
    ON b.horse_id = p.horse_id
LEFT JOIN dbo.TR_Payout AS sp
    ON  b.race_id = sp.race_id
    AND b.馬番   = sp.馬番1
    AND sp.式別  = N'単勝'
LEFT JOIN dbo.TR_Payout AS fp
    ON  b.race_id = fp.race_id
    AND b.馬番   = fp.馬番1
    AND fp.式別  = N'複勝';
GO