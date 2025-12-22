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
    , track_id       INT                NULL      -- 既存のID体系を踏襲
    -- 出走馬情報（出馬表）
    , frame_no       INT                NOT NULL
    , horse_no       INT                NOT NULL
    , horse_id       BIGINT             NULL      -- 取れれば入れる、なければ NULL
    , horse_name     NVARCHAR(30)       NOT NULL
    , sex            NVARCHAR(1)        NULL
    , age            INT                NULL
    , jockey_name    NVARCHAR(10)       NULL
    , carried_weight DECIMAL(3,1)       NULL
    -- 管理用（任意）
    , source_url     NVARCHAR(500)    NULL
    , scraped_at     DATETIME2(0)     NOT NULL
                      CONSTRAINT DF_TR_RaceEntry_scraped_at DEFAULT (SYSDATETIME())

    CONSTRAINT PK_TR_RaceEntry PRIMARY KEY (id)
);

-- 1レース内で馬番は一意のはずなので、ユニーク制約を付けておくと安心です
CREATE UNIQUE INDEX UX_TR_RaceEntry_race_id_umaban
ON dbo.TR_RaceEntry (race_id, horse_no);
