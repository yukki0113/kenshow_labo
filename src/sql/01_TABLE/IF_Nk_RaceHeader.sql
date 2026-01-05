--* BackupToTempTable
drop table [IF_Nk_RaceHeader]
GO
 
--* RestoreFromTempTable
CREATE TABLE dbo.IF_Nk_RaceHeader
(
      import_batch_id        UNIQUEIDENTIFIER NOT NULL
    , race_id                CHAR(12)         NOT NULL

    -- 解析結果（raw中心。可能ならC#側で_normも埋めてOK）
    , race_no_raw            NVARCHAR(20)     NULL   -- 例: "11R"
    , race_date_raw          NVARCHAR(50)     NULL   -- 例: "12月28日(日)" / "2025年12月28日(日)" など
    , race_name_raw          NVARCHAR(200)    NULL
    , track_name_raw         NVARCHAR(20)     NULL   -- 例: "中山"
    , surface_distance_raw   NVARCHAR(50)     NULL   -- 例: "芝2500m" / "ダ1800m"

    -- 任意: C#側で正規化して入れられるなら
    , race_no_norm           TINYINT          NULL
    , race_date_norm         DATE             NULL
    , track_name_norm        NVARCHAR(3)      NULL
    , surface_type_norm      NVARCHAR(10)     NULL
    , distance_m_norm        SMALLINT         NULL
    , race_name_norm         NVARCHAR(50)     NULL

    -- 管理
    , scraped_at             DATETIME2(0)     NOT NULL CONSTRAINT DF_IF_Nk_RaceHeader_scraped_at DEFAULT (SYSDATETIME())
    , source_url             NVARCHAR(500)    NULL
    , source_file            NVARCHAR(260)    NULL

    CONSTRAINT PK_IF_Nk_RaceHeader PRIMARY KEY (import_batch_id)
);
GO

CREATE INDEX IX_IF_Nk_RaceHeader_raceid_scrapedat
ON dbo.IF_Nk_RaceHeader (race_id, scraped_at DESC);
GO
