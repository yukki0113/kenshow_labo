--* BackupToTempTable
drop table [ST_StatBaseConfig]
GO

--* RestoreFromTempTable
CREATE TABLE dbo.ST_StatBaseConfig
(
      config_id        INT IDENTITY(1,1) NOT NULL
    , stat_name        NVARCHAR(30) NOT NULL   -- 'course','style','frame','blood_sire','jockey'
    , k_pseudo_n       INT NOT NULL            -- 平滑化k（疑似サンプル数）
    , clip_min         DECIMAL(6,4) NOT NULL
    , clip_max         DECIMAL(6,4) NOT NULL
    , created_at       DATETIME2 NOT NULL DEFAULT SYSDATETIME()
    , updated_at       DATETIME2 NOT NULL DEFAULT SYSDATETIME()
    , CONSTRAINT PK_ST_StatBaseConfig PRIMARY KEY (config_id)
    , CONSTRAINT UQ_ST_StatBaseConfig UNIQUE (stat_name)
);
