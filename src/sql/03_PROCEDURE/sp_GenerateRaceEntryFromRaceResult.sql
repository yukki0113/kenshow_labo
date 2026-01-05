CREATE OR ALTER PROCEDURE dbo.sp_GenerateRaceEntryFromRaceResult
(
      @race_id CHAR(12)
)
AS
BEGIN
    SET NOCOUNT ON;

    -- 1) 対象レースが存在するか
    IF NOT EXISTS (
        SELECT 1
        FROM dbo.VW_RaceResultContract rr
        WHERE rr.race_id = @race_id
    )
    BEGIN
        THROW 51001, '指定された race_id が VW_RaceResultContract に存在しません。', 1;
    END

    BEGIN TRY
        BEGIN TRAN;

        -- 2) 既存の出馬表（同race_id）を削除
        DELETE FROM dbo.TR_RaceEntry
        WHERE race_id = @race_id;

        -- 3) 結果ビューから出馬表を生成してINSERT
        INSERT INTO dbo.TR_RaceEntry
        (
              race_id
            , race_no
            , race_date
            , race_name
            , race_class
            , track_name
            , surface_type
            , distance_m
            , meeting
            , turn
            , course_inout
            , going
            , weather
            , frame_no
            , horse_no
            , horse_id
            , horse_name
            , sex
            , age
            , jockey_name
            , carried_weight
            , scraped_at
            , source_url
        )
        SELECT
              rr.race_id
            , MAX(rr.race_no)                         AS race_no
            , MAX(rr.[日付])                           AS race_date
            , MAX(rr.[レース名])                       AS race_name
            , MAX(rr.[クラス])                         AS race_class
            , MAX(rr.[場名])                           AS track_name
            , MAX(rr.[芝/ダ])                          AS surface_type
            , MAX(rr.[距離])                           AS distance_m
            , NULL                                     AS meeting
            , NULL                                     AS turn
            , NULL                                     AS course_inout
            , NULL                                     AS going
            , NULL                                     AS weather
            , rr.[枠番]                               AS frame_no
            , rr.[馬番]                               AS horse_no
            , rr.horse_id                              AS horse_id
            , MAX(rr.[馬名])                           AS horse_name
            , MAX(rr.[性])                             AS sex
            , MAX(rr.[齢])                             AS age
            , MAX(rr.[騎手])                           AS jockey_name
            , MAX(rr.[斤量])                           AS carried_weight
            , SYSDATETIME()                            AS scraped_at
            , NULL                                     AS source_url
        FROM dbo.VW_RaceResultContract rr
        WHERE rr.race_id = @race_id
        GROUP BY
              rr.race_id
            , rr.[枠番]
            , rr.[馬番]
            , rr.horse_id;

        COMMIT;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        THROW;
    END CATCH
END
GO
