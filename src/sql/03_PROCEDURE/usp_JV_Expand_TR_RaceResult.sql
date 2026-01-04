CREATE OR ALTER PROCEDURE dbo.usp_JV_Expand_TR_RaceResult
AS
BEGIN
    SET NOCOUNT ON

    /* 事前チェック：UmaにあるのにRaceが無い（ヘッダ欠落） */
    IF EXISTS (
        SELECT 1
        FROM dbo.IF_JV_RaceUma AS u
        LEFT JOIN dbo.IF_JV_Race AS r
            ON r.race_id = u.race_id
        WHERE r.race_id IS NULL
    )
    BEGIN
        RAISERROR(N'IF_JV_RaceUma に存在する race_id が IF_JV_Race に存在しません（ヘッダ欠落）。', 16, 1)
        RETURN
    END

    /* 事前チェック：RaceにあるのにUmaが無い（馬行欠落） */
    IF EXISTS (
        SELECT 1
        FROM dbo.IF_JV_Race AS r
        LEFT JOIN dbo.IF_JV_RaceUma AS u
            ON u.race_id = r.race_id
        WHERE u.race_id IS NULL
    )
    BEGIN
        RAISERROR(N'IF_JV_Race に存在する race_id に対して IF_JV_RaceUma が0件です（馬行欠落）。', 16, 1)
        RETURN
    END

    BEGIN TRY
        BEGIN TRAN

        /* 冪等：IF_JV_Race に入っている race_id を入れ直す */
        DELETE t
        FROM dbo.TR_RaceResult AS t
        INNER JOIN dbo.IF_JV_Race AS r
            ON r.race_id = t.race_id

        INSERT INTO dbo.TR_RaceResult
        (
              race_id
            , horse_no

            , race_date
            , ymd
            , jyo_cd
            , race_no

            , race_name
            , jyoken_name
            , grade_cd
            , jyoken_syubetu_cd
            , jyoken_cd4

            , track_cd
            , distance_m
            , hasso_time
            , weather_cd
            , baba_siba_cd
            , baba_dirt_cd

            , win5_flg

            , frame_no
            , horse_id
            , horse_name
            , sex_cd
            , age
            , jockey_code
            , jockey_name
            , carried_weight
            , odds
            , popularity

            , finish_time_raw
            , finish_time

            , weight_raw
            , horse_weight

            , weight_diff_raw
            , horse_weight_diff

            , pos_c1
            , pos_c2
            , pos_c3
            , pos_c4

            , final_3f_raw
            , final_3f
        )
        SELECT
              r.race_id
            , u.horse_no

            , TRY_CONVERT(DATE, STUFF(STUFF(r.ymd, 5, 0, '-'), 8, 0, '-')) AS race_date
            , r.ymd
            , r.jyo_cd
            , r.race_num

            , r.race_name
            , r.jyoken_name
            , r.grade_cd
            , r.jyoken_syubetu_cd
            , r.jyoken_cd4

            , r.track_cd
            , r.distance_m
            , r.hasso_time
            , r.weather_cd
            , r.baba_siba_cd
            , r.baba_dirt_cd

            , CASE WHEN w.race_id IS NOT NULL THEN CAST(1 AS TINYINT) ELSE CAST(0 AS TINYINT) END AS win5_flg

            , u.frame_no
            , CASE WHEN u.horse_id IS NULL THEN NULL ELSE LEFT(u.horse_id, 10) END AS horse_id
            , u.horse_name
            , u.sex_cd
            , u.age
            , u.jockey_code
            , u.jockey_name
            , u.carried_weight
            , u.odds
            , u.popularity

            , u.finish_time_raw
            , TRY_CONVERT(
                  TIME(2),
                  CASE
                      WHEN u.finish_time_raw IS NULL THEN NULL
                      WHEN CHARINDEX(':', u.finish_time_raw) > 0 THEN
                          CONCAT(
                              '00:',
                              RIGHT('00' + LEFT(u.finish_time_raw, CHARINDEX(':', u.finish_time_raw) - 1), 2),
                              ':',
                              SUBSTRING(u.finish_time_raw, CHARINDEX(':', u.finish_time_raw) + 1, 100)
                          )
                      ELSE
                          CONCAT('00:00:', u.finish_time_raw)
                  END
              ) AS finish_time

            , u.weight_raw
            , TRY_CONVERT(SMALLINT, NULLIF(LTRIM(RTRIM(u.weight_raw)), N'')) AS horse_weight

            , u.weight_diff_raw
            , TRY_CONVERT(SMALLINT, NULLIF(LTRIM(RTRIM(u.weight_diff_raw)), N'')) AS horse_weight_diff

            , u.pos_c1
            , u.pos_c2
            , u.pos_c3
            , u.pos_c4

            , u.final_3f_raw
            , TRY_CONVERT(DECIMAL(3,1), NULLIF(LTRIM(RTRIM(u.final_3f_raw)), N'')) AS final_3f
        FROM dbo.IF_JV_RaceUma AS u
        INNER JOIN dbo.IF_JV_Race AS r
            ON r.race_id = u.race_id
        LEFT JOIN dbo.MT_Win5Target AS w
            ON w.race_id = r.race_id

        COMMIT
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK

        DECLARE @ErrorMessage NVARCHAR(4000)
        DECLARE @ErrorSeverity INT
        DECLARE @ErrorState INT

        SELECT
              @ErrorMessage  = ERROR_MESSAGE()
            , @ErrorSeverity = ERROR_SEVERITY()
            , @ErrorState    = ERROR_STATE()

        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
        RETURN
    END CATCH
END
GO
