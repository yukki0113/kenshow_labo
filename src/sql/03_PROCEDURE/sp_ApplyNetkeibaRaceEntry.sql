CREATE OR ALTER PROCEDURE dbo.sp_ApplyNetkeibaRaceEntry
(
      @import_batch_id UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @race_id CHAR(12);
    DECLARE @scraped_at DATETIME2(0);
    DECLARE @source_url NVARCHAR(500);

    ------------------------------------------------------------
    -- 0) ヘッダ取得
    ------------------------------------------------------------
    SELECT
          @race_id    = h.race_id
        , @scraped_at = h.scraped_at
        , @source_url = h.source_url
    FROM dbo.IF_Nk_RaceHeader h
    WHERE h.import_batch_id = @import_batch_id;

    IF @race_id IS NULL
    BEGIN
        THROW 50001, 'IF_Nk_RaceHeader に該当 import_batch_id が存在しません。', 1;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.IF_Nk_RaceEntryRow r WHERE r.import_batch_id = @import_batch_id)
    BEGIN
        THROW 50002, 'IF_Nk_RaceEntryRow に明細が存在しません。', 1;
    END

    BEGIN TRY
        BEGIN TRAN;

        ------------------------------------------------------------
        -- 1) IF明細の jv_horse_id を MP_HorseId で補完（NULL行だけ）
        ------------------------------------------------------------
        UPDATE r
            SET r.jv_horse_id = m.jv_horse_id
        FROM dbo.IF_Nk_RaceEntryRow r
        INNER JOIN dbo.MP_HorseId m
            ON m.nk_horse_id = r.nk_horse_id
        WHERE
            r.import_batch_id = @import_batch_id
            AND r.jv_horse_id IS NULL
            AND r.nk_horse_id IS NOT NULL;

        ------------------------------------------------------------
        -- 2) 未解決チェック（jv_horse_id が1つでもNULLなら展開しない）
        ------------------------------------------------------------
        DECLARE @unresolved_count INT;

        SELECT @unresolved_count = COUNT(*)
        FROM dbo.IF_Nk_RaceEntryRow r
        WHERE
            r.import_batch_id = @import_batch_id
            AND (r.jv_horse_id IS NULL OR LTRIM(RTRIM(r.jv_horse_id)) = N'');

        IF @unresolved_count > 0
        BEGIN
            -- ここは「処理を止めない」運用：TRは更新せず、ログを残してコミットして終了
            INSERT INTO dbo.IF_Nk_ApplyLog (import_batch_id, race_id, inserted_rows, message)
            VALUES
            (
                @import_batch_id,
                @race_id,
                0,
                CONCAT(N'SKIP: jv_horse_id 未解決行が ', @unresolved_count, N' 件あります（手動対応後に再実行してください）。')
            );

            COMMIT;
            RETURN 0;
        END

        ------------------------------------------------------------
        -- 3) ヘッダの正規化（前回案：_norm優先、無ければrawからTRY_CONVERT）
        ------------------------------------------------------------
        ;WITH H AS
        (
            SELECT
                  h.race_id

                , COALESCE(
                      h.race_no_norm,
                      TRY_CONVERT(TINYINT, REPLACE(REPLACE(h.race_no_raw, N'R', N''), N'Ｒ', N''))
                  ) AS race_no

                , COALESCE(
                      h.race_date_norm,
                      CASE
                          WHEN h.race_date_raw LIKE N'%年%月%日%' THEN
                              TRY_CONVERT(
                                  DATE,
                                  CONCAT(
                                      SUBSTRING(h.race_date_raw, 1, CHARINDEX(N'年', h.race_date_raw) - 1), N'-',
                                      RIGHT(N'00' + SUBSTRING(h.race_date_raw, CHARINDEX(N'年', h.race_date_raw) + 1, CHARINDEX(N'月', h.race_date_raw) - CHARINDEX(N'年', h.race_date_raw) - 1), 2), N'-',
                                      RIGHT(N'00' + SUBSTRING(h.race_date_raw, CHARINDEX(N'月', h.race_date_raw) + 1, CHARINDEX(N'日', h.race_date_raw) - CHARINDEX(N'月', h.race_date_raw) - 1), 2)
                                  )
                              )
                          WHEN h.race_date_raw LIKE N'%月%日%' THEN
                              TRY_CONVERT(
                                  DATE,
                                  CONCAT(
                                      LEFT(h.race_id, 4), N'-',
                                      RIGHT(N'00' + SUBSTRING(h.race_date_raw, 1, CHARINDEX(N'月', h.race_date_raw) - 1), 2), N'-',
                                      RIGHT(N'00' + SUBSTRING(h.race_date_raw, CHARINDEX(N'月', h.race_date_raw) + 1, CHARINDEX(N'日', h.race_date_raw) - CHARINDEX(N'月', h.race_date_raw) - 1), 2)
                                  )
                              )
                          ELSE NULL
                      END
                  ) AS race_date

                , COALESCE(h.race_name_norm, LEFT(h.race_name_raw, 50)) AS race_name
                , COALESCE(h.track_name_norm, LEFT(h.track_name_raw, 3)) AS track_name

                , COALESCE(
                      h.surface_type_norm,
                      CASE
                          WHEN h.surface_distance_raw LIKE N'芝%' THEN N'芝'
                          WHEN h.surface_distance_raw LIKE N'ダ%' THEN N'ダ'
                          ELSE NULL
                      END
                  ) AS surface_type

                , COALESCE(
                      h.distance_m_norm,
                      TRY_CONVERT(
                          SMALLINT,
                          CASE
                              WHEN h.surface_distance_raw IS NULL THEN NULL
                              WHEN PATINDEX(N'%[0-9]%', h.surface_distance_raw) = 0 THEN NULL
                              ELSE
                                  CASE
                                      WHEN CHARINDEX(N'm', h.surface_distance_raw) > 0 THEN
                                          SUBSTRING(
                                              h.surface_distance_raw,
                                              PATINDEX(N'%[0-9]%', h.surface_distance_raw),
                                              CHARINDEX(N'm', h.surface_distance_raw) - PATINDEX(N'%[0-9]%', h.surface_distance_raw)
                                          )
                                      ELSE
                                          SUBSTRING(h.surface_distance_raw, PATINDEX(N'%[0-9]%', h.surface_distance_raw), 10)
                                  END
                          END
                      )
                  ) AS distance_m
            FROM dbo.IF_Nk_RaceHeader h
            WHERE h.import_batch_id = @import_batch_id
        )
        ------------------------------------------------------------
        -- 4) TRを race_id 単位で置換
        ------------------------------------------------------------
        DELETE FROM dbo.TR_RaceEntry
        WHERE race_id = @race_id;

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
            , horse_id           -- ★JV体系で一本化
            , horse_name
            , sex
            , age
            , jockey_name
            , carried_weight
            , scraped_at
            , source_url
        )
        SELECT
              h.race_id
            , h.race_no
            , h.race_date
            , h.race_name
            , NULL AS race_class
            , h.track_name
            , h.surface_type
            , h.distance_m
            , NULL AS meeting
            , NULL AS turn
            , NULL AS course_inout
            , NULL AS going
            , NULL AS weather

            , TRY_CONVERT(TINYINT, r.frame_no_raw) AS frame_no

            -- horse_no（未確定は 100 + row_no）
            , COALESCE(
                  TRY_CONVERT(TINYINT, r.horse_no_raw),
                  TRY_CONVERT(TINYINT, 100 + r.row_no)
              ) AS horse_no

            -- ★horse_idは JV
            , r.jv_horse_id AS horse_id

            , LEFT(r.horse_name_raw, 30) AS horse_name
            , CASE WHEN r.sex_age_raw IS NULL OR LEN(r.sex_age_raw) = 0 THEN NULL ELSE LEFT(r.sex_age_raw, 1) END AS sex
            , TRY_CONVERT(TINYINT, SUBSTRING(r.sex_age_raw, 2, 10)) AS age
            , LEFT(r.jockey_name_raw, 10) AS jockey_name
            , TRY_CONVERT(DECIMAL(3,1), r.carried_weight_raw) AS carried_weight

            , @scraped_at
            , @source_url
        FROM H h
        INNER JOIN dbo.IF_Nk_RaceEntryRow r
            ON r.import_batch_id = @import_batch_id;

        DECLARE @inserted_rows INT = @@ROWCOUNT;

        ------------------------------------------------------------
        -- 5) 成功ログ
        ------------------------------------------------------------
        INSERT INTO dbo.IF_Nk_ApplyLog (import_batch_id, race_id, inserted_rows, message)
        VALUES (@import_batch_id, @race_id, @inserted_rows, N'OK');

        COMMIT;
        RETURN 1;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;

        DECLARE @err NVARCHAR(4000) = CONCAT(
            N'ERROR ', ERROR_NUMBER(), N': ', ERROR_MESSAGE(),
            N' (line ', ERROR_LINE(), N')'
        );

        INSERT INTO dbo.IF_Nk_ApplyLog (import_batch_id, race_id, inserted_rows, message)
        VALUES (@import_batch_id, ISNULL(@race_id, '????????????'), 0, @err);

        THROW;
    END CATCH
END
GO
