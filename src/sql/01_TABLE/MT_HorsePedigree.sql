--* BackupToTempTable
drop table [TR_RaceResult]
GO
 
--* RestoreFromTempTable
CREATE TABLE dbo.MT_HorsePedigree (
    horse_id   CHAR(10)       NOT NULL,
    horse_name NVARCHAR(100)  NULL,         -- 馬名
    sire       NVARCHAR(100)  NULL,         -- 父
    dam        NVARCHAR(100)  NULL,         -- 母
    dam_sire   NVARCHAR(100)  NULL,         -- 母父
    dam_dam    NVARCHAR(100)  NULL,         -- 母母
    sire_sire  NVARCHAR(100)  NULL,         -- 父父
    sire_dam   NVARCHAR(100)  NULL,         -- 父母
    CONSTRAINT PK_MT_HorsePedigree PRIMARY KEY (horse_id)
)
GO

CREATE INDEX IX_MT_HorsePedigree_HorseName
    ON dbo.MT_HorsePedigree (horse_name)
GO