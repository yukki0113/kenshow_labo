/* 【検証ラボ：重み設定マスタ】 */
--* BackupToTempTable
drop table [MT_WeightConfig]
GO

--* RestoreFromTempTable
CREATE TABLE dbo.MT_WeightConfig (
    config_id    INT PRIMARY KEY,
    config_name  NVARCHAR(50),
    w_ability    DECIMAL(3,2), -- 基礎能力の重み
    w_momentum   DECIMAL(3,2), -- 勢いの重み
    w_aptitude   DECIMAL(3,2), -- 適性の重み
    w_bias       DECIMAL(3,2), -- 枠順・環境の重み
    is_active    BIT           -- 現在使用する設定
);

INSERT INTO dbo.MT_WeightConfig VALUES (1, N'デフォルト', 1.0, 1.0, 1.0, 1.0, 1);