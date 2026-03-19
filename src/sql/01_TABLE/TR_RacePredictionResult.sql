/* 【検証ラボ】予測結果保存テーブル
   SQLiteへのエクスポートや、モバイルサイトでの表示用ソースとして使用します。
*/
--* BackupToTempTable
drop table [TR_RacePredictionResult]
GO

--* RestoreFromTempTable
CREATE TABLE dbo.TR_RacePredictionResult (
    race_id              CHAR(12)      NOT NULL,
    target_date          DATE          NOT NULL,
    race_no              TINYINT       NOT NULL,
    frame_no             TINYINT       NULL,
    horse_no             TINYINT       NOT NULL,
    horse_name           NVARCHAR(30)  NOT NULL,
    race_class           NVARCHAR(20)  NULL,
    
    -- 表示用指数
    ability_last5_avg    DECIMAL(5,1)  NULL,
    momentum_score       DECIMAL(4,1)  NULL,
    aptitude_site_score  DECIMAL(4,1)  NULL,
    aptitude_dist_score  DECIMAL(4,1)  NULL,
    final_expected_score DECIMAL(5,1)  NULL,
    
    -- 信頼度情報
    career_count         INT           NULL,
    site_experience_count INT           NULL,
    dist_experience_count INT           NULL,
    
    -- 分析用詳細生スコア
    raw_ability          DECIMAL(5,1)  NULL,
    raw_momentum         DECIMAL(4,1)  NULL,
    raw_site_apt         DECIMAL(4,1)  NULL,
    raw_dist_apt         DECIMAL(4,1)  NULL,
    raw_sire_apt         DECIMAL(4,1)  NULL,
    raw_jockey_apt       DECIMAL(4,1)  NULL,
    raw_waku_bias        DECIMAL(4,1)  NULL,
    raw_going_apt        DECIMAL(4,1)  NULL,
    raw_style_match      DECIMAL(4,1)  NULL,
    
    predicted_style      NVARCHAR(10)  NULL,
    created_at           DATETIME      DEFAULT GETDATE(),
    CONSTRAINT PK_TR_RacePredictionResult PRIMARY KEY (race_id, horse_no)
);