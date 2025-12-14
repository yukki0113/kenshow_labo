CREATE VIEW VW_RaceResultWithStyle
AS
WITH Base AS (
    SELECT
        r.*,
        COUNT(*) OVER (PARTITION BY r.race_id) AS 頭数
    FROM
        VW_RaceResultWithPedigree AS r
)
SELECT
    Base.race_id,
    Base.日付,
    Base.レース名,
    Base.クラス,
    Base.場名,
    Base.芝_ダート,
    Base.距離,
    Base.枠番,
    Base.馬番,
    Base.馬名,
    Base.性,
    Base.齢,
    Base.騎手,
    Base.斤量,
    Base.人気,
    Base.オッズ,
    Base.着順,
    Base.走破時計,
    Base.馬体重,
    Base.馬体重変動,
    Base.通過順_1角,
    Base.通過順_2角,
    Base.通過順_3角,
    Base.通過順_4角,
    Base.上がり,
    Base.開催,
    Base.回り,
    Base.馬場,
    Base.天気,
    Base.track_id,
    Base.horse_id,
    Base.血統馬名,
    Base.父,
    Base.母,
    Base.母父,
    Base.母母,
    Base.父父,
    Base.父母,
    Base.頭数,
    CASE
        -- ① まくり判定（最優先）
        WHEN Base.通過順_1角 IS NOT NULL
         AND Base.通過順_2角 IS NOT NULL
         AND Base.通過順_3角 IS NOT NULL
         AND Base.通過順_4角 IS NOT NULL
         AND Base.通過順_2角 > FLOOR(Base.頭数 * 2.0 / 3.0)
         AND Base.通過順_4角 <= 2
            THEN N'まくり'

        -- ② 以下は従来どおりの脚質分類
        WHEN Base.通過順_4角 IS NULL
            THEN N'不明'
        WHEN Base.通過順_4角 <= 2
            THEN N'逃げ'
        WHEN Base.通過順_4角 <= FLOOR(Base.頭数 / 3.0)
            THEN N'先行'
        WHEN Base.通過順_4角 <= FLOOR(Base.頭数 * 2.0 / 3.0)
            THEN N'差し'
        ELSE
            N'追い込み'
    END AS 脚質
FROM
    Base;
GO
