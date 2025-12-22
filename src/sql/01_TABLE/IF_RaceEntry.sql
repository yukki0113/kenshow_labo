CREATE TABLE dbo.IF_RaceEntry
(
      race_id        CHAR(12)       NOT NULL
    , horse_id       BIGINT         NOT NULL   -- netkeiba horse_id（このIFだけの意味）

    , horse_no       INT            NULL
    , frame_no       INT            NULL

    -- レース基本（取れない可能性があるものはNULL許容でもOK）
    , race_date      DATE           NULL
    , race_name      NVARCHAR(50)   NULL
    , race_class     NVARCHAR(20)   NULL
    , classSimple    NVARCHAR(20)   NULL
    , track_name     NVARCHAR(3)    NULL
    , surface_type   NVARCHAR(10)   NULL
    , distance_m     INT            NULL
    , meeting        NVARCHAR(12)   NULL
    , turn           NVARCHAR(1)    NULL
    , going          NVARCHAR(1)    NULL
    , weather        NVARCHAR(1)    NULL
    , track_id       INT            NULL

    -- 出走馬
    , horse_name     NVARCHAR(30)   NULL
    , sex            NVARCHAR(1)    NULL
    , age            INT            NULL
    , jockey_name    NVARCHAR(10)   NULL
    , carried_weight DECIMAL(3,1)   NULL

    -- 管理
    , source_url     NVARCHAR(500)  NULL
    , scraped_at     DATETIME2(0)   NOT NULL
                    CONSTRAINT DF_IF_RaceEntry_scraped_at DEFAULT (SYSDATETIME())

    , CONSTRAINT PK_IF_RaceEntry PRIMARY KEY (race_id, horse_id)
);

-- race_id での参照
CREATE INDEX IX_IF_RaceEntry_race_id
ON dbo.IF_RaceEntry (race_id);

-- 取り込み時刻での参照
CREATE INDEX IX_IF_RaceEntry_scraped_at
ON dbo.IF_RaceEntry (scraped_at DESC);

-- 馬番が入っている時だけ「同一レース内の馬番一意」を担保（任意だがオススメ）
CREATE UNIQUE INDEX UX_IF_RaceEntry_race_id_horse_no
ON dbo.IF_RaceEntry (race_id, horse_no)
WHERE horse_no IS NOT NULL;
