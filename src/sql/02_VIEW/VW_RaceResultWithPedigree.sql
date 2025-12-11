CREATE VIEW VW_RaceResultWithPedigree
AS
SELECT
    r.race_id,
    r.日付,
    r.レース名,
    r.クラス,
    r.場名,
    r.芝_ダート,
    r.距離,
    r.枠番,
    r.馬番,
    r.馬名,
    r.性,
    r.齢,
    r.騎手,
    r.斤量,
    r.人気,
    r.オッズ,
    r.着順,
    r.走破時計,
    r.馬体重,
    r.馬体重変動,
    r.通過順_1角,
    r.通過順_2角,
    r.通過順_3角,
    r.通過順_4角,
    r.上がり,
    r.開催,
    r.回り,
    r.馬場,
    r.天気,
    r.track_id,
    r.horse_id,
    p.馬名 AS 血統馬名,
    p.父,
    p.母,
    p.母父,
    p.母母,
    p.父父,
    p.父母
FROM
    TR_raceresult AS r
    LEFT JOIN MT_HorsePedigree AS p
        ON r.horse_id = p.horse_id;
GO
