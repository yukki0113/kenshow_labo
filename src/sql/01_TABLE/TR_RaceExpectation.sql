--* BackupToTempTable
drop table [TR_RaceExpectation]
GO

--* RestoreFromTempTable
CREATE TABLE dbo.TR_RaceExpectation
(
    race_id         CHAR(12)      NOT NULL,
    horse_no        INT           NOT NULL,

    horse_id        BIGINT        NULL,
    horse_name      NVARCHAR(30)  NULL,
    track_name      NVARCHAR(3)   NULL,
    jyo_cd          CHAR(2)       NULL,
    surface_type    NVARCHAR(10)  NULL,
    distance_m      INT           NULL,
    distance_class  TINYINT       NULL,

    ability         DECIMAL(9,6)  NULL,  -- 0～1
    mult_course     DECIMAL(9,6)  NULL,  -- 係数
    mult_style      DECIMAL(9,6)  NULL,
    mult_frame      DECIMAL(9,6)  NULL,
    mult_blood      DECIMAL(9,6)  NULL,
    mult_jockey     DECIMAL(9,6)  NULL,

    expected_raw    DECIMAL(9,6)  NULL,  -- 合成前（0～1想定）
    expected_100    DECIMAL(9,2)  NULL,  -- 表示用 0～100

    updated_at      DATETIME2     NOT NULL CONSTRAINT DF_TR_RaceExpectation_updated_at DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_TR_RaceExpectation PRIMARY KEY (race_id, horse_no)
);
GO
