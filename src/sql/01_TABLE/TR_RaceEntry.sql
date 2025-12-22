CREATE TABLE dbo.TR_RaceEntry
(
      id             INT IDENTITY(1,1)  NOT NULL
    , race_id        CHAR(12)           NOT NULL

    -- レース基本情報（TR_RaceResult に揃える）
    , race_date      DATE               NOT NULL
    , race_name      NVARCHAR(50)       NOT NULL
    , race_class     NVARCHAR(20)       NULL      -- G1, 3歳以上1勝クラス 等
    , classSimple    NVARCHAR(20)       NULL      -- G1/G2/G3/OP/1勝 等のシンプル分類
    , track_name     NVARCHAR(3)        NOT NULL  -- 東京, 中山, 札幌 ...
    , surface_type   NVARCHAR(10)       NOT NULL  -- 芝 / ダ / 障害 など
    , distance_m     INT                NOT NULL  -- メートル
    , meeting        NVARCHAR(12)       NULL      -- 1回東京6日目 等
    , turn           NVARCHAR(1)        NULL      -- 右 / 左
    , going          NVARCHAR(1)        NULL      -- 良 / 重 等（想定で入れておく）
    , weather        NVARCHAR(1)        NULL
    , track_id       INT                NULL

    -- 出走馬情報（出馬表）
    , frame_no       INT                NULL
    , horse_no       INT                NULL

    , horse_id       BIGINT             NOT NULL   -- JV基準に統一済み（TR以降はこちらが正）
    , horse_name     NVARCHAR(30)       NOT NULL
    , sex            NVARCHAR(1)        NULL
    , age            INT                NULL
    , jockey_name    NVARCHAR(10)       NULL
    , carried_weight DECIMAL(3,1)       NULL

    -- 管理用（任意）
    , source_url     NVARCHAR(500)      NULL
    , scraped_at     DATETIME2(0)       NOT NULL
                      CONSTRAINT DF_TR_RaceEntry_scraped_at DEFAULT (SYSDATETIME())

    , CONSTRAINT PK_TR_RaceEntry PRIMARY KEY (id)
);

-- 同一レースに同一馬が2頭以上はあり得ない前提を担保
CREATE UNIQUE INDEX UX_TR_RaceEntry_race_id_horse_id
ON dbo.TR_RaceEntry (race_id, horse_id);

-- 馬番が入っている時だけ「同一レース内の馬番一意」を担保（任意だがオススメ）
CREATE UNIQUE INDEX UX_TR_RaceEntry_race_id_horse_no
ON dbo.TR_RaceEntry (race_id, horse_no)
WHERE horse_no IS NOT NULL;

-- race_id 参照が多いはずなので（任意）
CREATE INDEX IX_TR_RaceEntry_race_id
ON dbo.TR_RaceEntry (race_id);
