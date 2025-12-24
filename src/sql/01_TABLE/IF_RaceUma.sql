--* BackupToTempTable
drop table [IF_JV_RaceUma]
GO
 
--* RestoreFromTempTable
CREATE TABLE dbo.IF_JV_RaceUma (
    race_id          CHAR(12)      NOT NULL,

    horse_no         INT           NOT NULL,   -- Umaban（TRのhorse_no想定）
    frame_no         INT           NULL,       -- Wakuban

    horse_id         VARCHAR(20)   NULL,       -- KettoNum をそのまま格納（形式はJV準拠）
    horse_name       NVARCHAR(30)  NULL,       -- Bamei

    sex_cd           CHAR(1)       NULL,       -- SexCD
    age              INT           NULL,       -- Barei

    jockey_code      CHAR(5)       NULL,       -- KisyuCode
    jockey_name      NVARCHAR(10)  NULL,       -- KisyuRyakusyo

    carried_weight   DECIMAL(3,1)  NULL,       -- Futan
    odds             DECIMAL(6,2)  NULL,       -- Odds（元はTEXTなのでC#で安全に変換）
    popularity       INT           NULL,       -- Ninki

    finish_pos       INT           NULL,       -- KakuteiJyuni（無い場合はNyusenJyuni）
    finish_time_raw  NVARCHAR(16)  NULL,       -- Time（まずRAWで）
    weight_raw       NVARCHAR(8)   NULL,       -- BaTaijyu
    weight_diff_raw  NVARCHAR(8)   NULL,       -- ZogenSa(+符号はZogenFugo)

    pos_c1           INT           NULL,       -- Jyuni1c
    pos_c2           INT           NULL,       -- Jyuni2c
    pos_c3           INT           NULL,       -- Jyuni3c
    pos_c4           INT           NULL,       -- Jyuni4c
    final_3f_raw     NVARCHAR(8)   NULL,       -- HaronTimeL3

    data_kubun       CHAR(1)       NULL,       -- headDataKubun
    make_date        CHAR(8)       NULL,       -- headMakeDate
    ymd              CHAR(8)       NULL,       -- 取り込み時に埋めてもよい（検索用）

    CONSTRAINT PK_IF_JV_RaceUma PRIMARY KEY (race_id, horse_no)
);

CREATE INDEX IX_IF_JV_RaceUma_Ymd ON dbo.IF_JV_RaceUma(ymd);
CREATE INDEX IX_IF_JV_RaceUma_Horse ON dbo.IF_JV_RaceUma(horse_id);
