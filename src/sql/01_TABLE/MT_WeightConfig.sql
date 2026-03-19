/* 【検証ラボ：重み設定マスタ】 */
--* BackupToTempTable
drop table [MT_WeightConfig]
GO

--* RestoreFromTempTable
CREATE TABLE dbo.MT_WeightConfig (
    config_id    INT PRIMARY KEY,
    config_name  NVARCHAR(50),
    w_ability    DECIMAL(3,2) DEFAULT 1.0, -- 能力(近5)
    w_momentum   DECIMAL(3,2) DEFAULT 1.0, -- 勢い
    w_site       DECIMAL(3,2) DEFAULT 1.0, -- 場所適性(個体)
    w_dist       DECIMAL(3,2) DEFAULT 1.0, -- 距離適性(個体)
    w_sire       DECIMAL(3,2) DEFAULT 0.5, -- 血統(種牡馬)
    w_jockey     DECIMAL(3,2) DEFAULT 0.8, -- 騎手
    w_bias       DECIMAL(3,2) DEFAULT 1.0, -- 枠順
    w_going      DECIMAL(3,2) DEFAULT 0.5, -- 馬場状態
    w_style      DECIMAL(3,2) DEFAULT 0.5, -- 脚質マッチ
    is_active    BIT DEFAULT 0
);

INSERT INTO dbo.MT_WeightConfig (config_id, config_name, is_active) VALUES (1, N'Default_V3', 1);
