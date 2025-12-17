CREATE TABLE dbo.TR_RaceEntry
(
      id             INT IDENTITY(1,1) NOT NULL
    , race_id        BIGINT           NOT NULL

    -- レース基本情報（TR_RaceResult に揃える）
    , 日付           DATE             NOT NULL
    , レース名       NVARCHAR(200)    NOT NULL
    , クラス         NVARCHAR(50)     NULL      -- G1, 3歳以上1勝クラス 等
    , classSimple    NVARCHAR(20)     NULL      -- G1/G2/G3/OP/1勝 等のシンプル分類
    , WIN5_flg       BIT              NULL      -- WIN5 対象レースなら 1

    , 場名           NVARCHAR(50)     NOT NULL  -- 東京, 中山, 札幌 ...
    , 芝_ダート      NVARCHAR(10)     NOT NULL  -- 芝 / ダ / 障害 など
    , 距離           INT              NOT NULL  -- メートル
    , 開催           NVARCHAR(50)     NULL      -- 1回東京6日目 等
    , 回り           NVARCHAR(10)     NULL      -- 右 / 左
    , 馬場           NVARCHAR(10)     NULL      -- 良 / 重 等（想定で入れておく）
    , 天気           NVARCHAR(10)     NULL
    , track_id       INT              NULL      -- 既存のID体系を踏襲

    -- 出走馬情報（出馬表）
    , 枠番           INT              NOT NULL
    , 馬番           INT              NOT NULL
    , horse_id       BIGINT           NULL      -- 取れれば入れる、なければ NULL
    , 馬名           NVARCHAR(100)    NOT NULL
    , 性             NVARCHAR(10)     NULL
    , 齢             INT              NULL
    , 騎手           NVARCHAR(50)     NULL
    , 斤量           DECIMAL(4,1)     NULL

    -- 管理用（任意）
    , source_url     NVARCHAR(500)    NULL
    , scraped_at     DATETIME2(0)     NOT NULL
                      CONSTRAINT DF_TR_RaceEntry_scraped_at DEFAULT (SYSDATETIME())

    CONSTRAINT PK_TR_RaceEntry PRIMARY KEY (id)
);

-- 1レース内で馬番は一意のはずなので、ユニーク制約を付けておくと安心です
CREATE UNIQUE INDEX UX_TR_RaceEntry_race_id_umaban
ON dbo.TR_RaceEntry (race_id, 馬番);
