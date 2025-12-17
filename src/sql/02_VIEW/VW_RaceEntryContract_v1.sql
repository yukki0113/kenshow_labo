CREATE OR ALTER VIEW dbo.VW_RaceEntryContract_v1
AS
WITH Base AS
(
    SELECT
          e.race_id
        , e.日付
        , e.レース名
        , e.クラス
        , e.classSimple
        , e.WIN5_flg
        , e.場名
        , e.芝_ダート
        , e.距離
        , e.枠番
        , e.馬番
        , e.horse_id
        , e.馬名
        , e.性
        , e.齢
        , e.騎手
        , e.斤量

        -- 出馬表から頭数は分かるのでウインドウ関数で計算
        , COUNT(*) OVER (PARTITION BY e.race_id) AS 頭数

        , e.開催
        , e.回り
        , e.馬場
        , e.天気
        , e.track_id
    FROM dbo.TR_RaceEntry AS e
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

    -- 結果系（まだ未確定なので NULL）
    , CAST(NULL AS INT)           AS 人気
    , CAST(NULL AS DECIMAL(4, 1)) AS オッズ
    , CAST(NULL AS INT)           AS 着順
    , CAST(NULL AS TIME)          AS 走破時計
    , CAST(NULL AS INT)           AS 馬体重
    , CAST(NULL AS INT)           AS 馬体重変動
    , CAST(NULL AS INT)           AS 通過順_1角
    , CAST(NULL AS INT)           AS 通過順_2角
    , CAST(NULL AS INT)           AS 通過順_3角
    , CAST(NULL AS INT)           AS 通過順_4角
    , CAST(NULL AS DECIMAL(3, 1)) AS 上がり

    , b.開催
    , b.回り
    , b.馬場
    , b.天気
    , b.track_id

    -- 頭数と脚質（脚質は出馬表時点では不明固定）
    , b.頭数
    , N'不明'                     AS 脚質

    -- 血統（horse_id があれば JOIN）
    , p.馬名                      AS 血統馬名
    , p.父
    , p.母
    , p.母父
    , p.母母
    , p.父父
    , p.父母

    -- 払戻（出馬表時点では存在しないため NULL）
    , CAST(NULL AS INT)           AS 単勝払戻
    , CAST(NULL AS INT)           AS 単勝人気
    , CAST(NULL AS INT)           AS 複勝払戻
    , CAST(NULL AS INT)           AS 複勝人気

FROM Base AS b
LEFT JOIN dbo.MT_HorsePedigree AS p
    ON b.horse_id = p.horse_id;
GO
