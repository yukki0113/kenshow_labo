CREATE OR ALTER VIEW dbo.VW_RaceResultWithStyle
AS
WITH Base AS (
    SELECT
        r.race_id,
        r.[日付],
        r.[レース名],
        r.[クラス],
        r.[場名],
        r.[芝_ダート],
        r.[距離],
        r.[枠番],
        r.[馬番],
        r.[馬名],
        r.[性],
        r.[齢],
        r.[騎手],
        r.[斤量],
        r.[人気],
        r.[オッズ],
        r.[着順],
        r.[走破時計],
        r.[馬体重],
        r.[馬体重変動],
        r.[通過順_1角],
        r.[通過順_2角],
        r.[通過順_3角],
        r.[通過順_4角],
        r.[上がり],
        r.[開催],
        r.[回り],
        r.[馬場],
        r.[天気],
        r.track_id,
        r.horse_id,
        r.[血統馬名],
        r.[父],
        r.[母],
        r.[母父],
        r.[母母],
        r.[父父],
        r.[父母],
        COUNT(*) OVER (PARTITION BY r.race_id) AS [頭数]
    FROM dbo.VW_RaceResultWithPedigree AS r
)
SELECT
    Base.*,
    CASE
        WHEN Base.[通過順_1角] IS NOT NULL
         AND Base.[通過順_2角] IS NOT NULL
         AND Base.[通過順_3角] IS NOT NULL
         AND Base.[通過順_4角] IS NOT NULL
         AND Base.[通過順_2角] > FLOOR(Base.[頭数] * 2.0 / 3.0)
         AND Base.[通過順_4角] <= 2
            THEN N'まくり'
        WHEN Base.[通過順_4角] IS NULL
            THEN N'不明'
        WHEN Base.[通過順_4角] <= 2
            THEN N'逃げ'
        WHEN Base.[通過順_4角] <= FLOOR(Base.[頭数] / 3.0)
            THEN N'先行'
        WHEN Base.[通過順_4角] <= FLOOR(Base.[頭数] * 2.0 / 3.0)
            THEN N'差し'
        ELSE
            N'追い込み'
    END AS [脚質]
FROM Base;
GO