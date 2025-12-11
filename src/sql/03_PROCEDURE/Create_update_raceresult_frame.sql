CREATE PROCEDURE UpdateFrameNumbers
AS
BEGIN
    -- Temporary table to store race_id and maximum horse number for each race
    CREATE TABLE #MaxHorseNumbers (
        race_id char(12),
        max_horse_number INT
    )

    -- Inserting race_id and maximum horse number into temporary table
    INSERT INTO #MaxHorseNumbers (race_id, max_horse_number)
    SELECT race_id, MAX(ISNULL(馬番, 0)) AS max_horse_number
    FROM tr_raceresult
    WHERE 枠番 IS NULL
    GROUP BY race_id

    -- Update frame numbers based on maximum horse number for each race
    UPDATE tr_raceresult
    SET 枠番 = 
        CASE 
            WHEN max_horse_number <= 8 THEN ISNULL(馬番, 0)
            WHEN max_horse_number = 9 THEN CASE WHEN 馬番 IN (8, 9) THEN 8 ELSE ISNULL(馬番, 0) END
            WHEN max_horse_number = 10 THEN CASE WHEN 馬番 IN (7, 8) THEN 7 WHEN 馬番 IN (9, 10) THEN 8 ELSE ISNULL(馬番, 0) END
            WHEN max_horse_number = 11 THEN CASE WHEN 馬番 IN (6, 7) THEN 6 WHEN 馬番 IN (8, 9) THEN 7 WHEN 馬番 IN (10, 11) THEN 8 ELSE ISNULL(馬番, 0) END
            WHEN max_horse_number = 12 THEN CASE WHEN 馬番 IN (5, 6) THEN 5 WHEN 馬番 IN (7, 8) THEN 6 WHEN 馬番 IN (9, 10) THEN 7 WHEN 馬番 IN (11, 12) THEN 8 ELSE ISNULL(馬番, 0) END
            WHEN max_horse_number = 13 THEN CASE WHEN 馬番 IN (4, 5) THEN 4 WHEN 馬番 IN (6, 7) THEN 5 WHEN 馬番 IN (8, 9) THEN 6 WHEN 馬番 IN (10, 11) THEN 7 WHEN 馬番 IN (12, 13) THEN 8 ELSE ISNULL(馬番, 0) END
            WHEN max_horse_number = 14 THEN CASE WHEN 馬番 IN (3, 4) THEN 3 WHEN 馬番 IN (5, 6) THEN 4 WHEN 馬番 IN (7, 8) THEN 5 WHEN 馬番 IN (9, 10) THEN 6 WHEN 馬番 IN (11, 12) THEN 7 WHEN 馬番 IN (13, 14) THEN 8 ELSE ISNULL(馬番, 0) END
            WHEN max_horse_number = 15 THEN CASE WHEN 馬番 IN (2, 3) THEN 2 WHEN 馬番 IN (4, 5) THEN 3 WHEN 馬番 IN (6, 7) THEN 4 WHEN 馬番 IN (8, 9) THEN 5 WHEN 馬番 IN (10, 11) THEN 6 WHEN 馬番 IN (12, 13) THEN 7 WHEN 馬番 IN (14, 15) THEN 8 ELSE ISNULL(馬番, 0) END
            WHEN max_horse_number = 16 THEN CASE WHEN 馬番 IN (1, 2) THEN 1 WHEN 馬番 IN (3, 4) THEN 2 WHEN 馬番 IN (5, 6) THEN 3 WHEN 馬番 IN (7, 8) THEN 4 WHEN 馬番 IN (9, 10) THEN 5 WHEN 馬番 IN (11, 12) THEN 6 WHEN 馬番 IN (13, 14) THEN 7 WHEN 馬番 IN (15, 16) THEN 8 ELSE ISNULL(馬番, 0) END
            WHEN max_horse_number = 17 THEN CASE WHEN 馬番 IN (1, 2) THEN 1 WHEN 馬番 IN (3, 4) THEN 2 WHEN 馬番 IN (5, 6) THEN 3 WHEN 馬番 IN (7, 8) THEN 4 WHEN 馬番 IN (9, 10) THEN 5 WHEN 馬番 IN (11, 12) THEN 6 WHEN 馬番 IN (13, 14) THEN 7 WHEN 馬番 = 15 THEN 8 ELSE ISNULL(馬番, 0) END
            WHEN max_horse_number = 18 THEN CASE WHEN 馬番 IN (1, 2) THEN 1 WHEN 馬番 IN (3, 4) THEN 2 WHEN 馬番 IN (5, 6) THEN 3 WHEN 馬番 IN (7, 8) THEN 4 WHEN 馬番 IN (9, 10) THEN 5 WHEN 馬番 IN (11, 12) THEN 6 WHEN 馬番 IN (13, 14) THEN 7 WHEN 馬番 IN (15, 16) THEN 8 ELSE ISNULL(馬番, 0) END
            ELSE ISNULL(馬番, 0)
        END
    FROM tr_raceresult rr
    INNER JOIN #MaxHorseNumbers mhn ON rr.race_id = mhn.race_id

    -- Drop temporary table
    DROP TABLE #MaxHorseNumbers
END
GO
