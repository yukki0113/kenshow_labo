--* BackupToTempTable
drop table [TR_RaceResult]
GO
 
--* RestoreFromTempTable
CREATE TABLE dbo.TR_RaceResult (
    race_id          CHAR(12)      NOT NULL,
    race_date        DATE          NULL,                -- 日付
    race_name        NVARCHAR(50)  NULL,                -- レース名
    race_class       NVARCHAR(20)  NULL,                -- クラス
    class_simple     NVARCHAR(10)  NULL,
    win5_flg         TINYINT       NOT NULL DEFAULT 0,
    track_name       NVARCHAR(3)   NULL,                -- 場名
    surface_type     NVARCHAR(10)  NULL,                -- 芝_ダート
    distance_m       INT           NULL,                -- 距離
    frame_no         INT           NULL,                -- 枠番
    horse_no         INT           NOT NULL,            -- 馬番
    horse_id         VARCHAR(20)   NULL, 
    horse_name       NVARCHAR(30)  NULL,                -- 馬名
    sex              NVARCHAR(1)   NULL,                -- 性
    age              INT           NULL,                -- 齢
    jockey_name      NVARCHAR(10)  NULL,                -- 騎手
    carried_weight   DECIMAL(3,1)  NULL,                -- 斤量
    popularity       INT           NULL,                -- 人気
    odds             DECIMAL(4,1)  NULL,                -- オッズ
    finish_pos       INT           NULL,                -- 着順
    finish_time      TIME          NULL,                -- 走破時計
    horse_weight     INT           NULL,                -- 馬体重
    horse_weight_diff INT          NULL,                -- 馬体重増減
    pos_corner1      INT           NULL,                -- 通過順_1角
    pos_corner2      INT           NULL,                -- 通過順_2角
    pos_corner3      INT           NULL,                -- 通過順_3角
    pos_corner4      INT           NULL,                -- 通過順_4角
    final_3f         DECIMAL(3,1)  NULL,                -- 上がり
    meeting          NVARCHAR(12)  NULL,                -- 開催
    turn             NVARCHAR(1)   NULL,                -- 回り
    going            NVARCHAR(1)   NULL,                -- 馬場
    weather          NVARCHAR(1)   NULL,                -- 天気 
    track_id         NVARCHAR(2)   NULL,

    CONSTRAINT PK_TR_RaceResult PRIMARY KEY (race_id, horse_no)
);

CREATE INDEX IX_TR_RaceResult_HorseId
    ON dbo.TR_RaceResult (horse_id);

CREATE INDEX IX_TR_RaceResult_RaceDate
    ON dbo.TR_RaceResult (race_date);
GO

