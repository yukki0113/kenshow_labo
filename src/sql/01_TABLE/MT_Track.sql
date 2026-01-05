--* BackupToTempTable
drop table [MT_Track]
GO
 
--* RestoreFromTempTable
CREATE TABLE dbo.MT_Track
(
      JyoCD      CHAR(2)        NOT NULL  -- 場所CD
    , Kyori      INT            NOT NULL  -- 距離
    , ShibaDirt  CHAR(1)        NOT NULL  -- 芝'1' / ダ'2'

    , TrackCD    CHAR(2)        NOT NULL  -- TRACK_CD（MT_CodeDictionary の TRACK を想定）

    , CONSTRAINT PK_MT_Track PRIMARY KEY (JyoCD, Kyori, ShibaDirt, TrackCD)
    , CONSTRAINT CK_MT_Track_ShibaDirt CHECK (ShibaDirt IN ('1', '2'))
    , CONSTRAINT CK_MT_Track_Kyori CHECK (Kyori > 0)
)
GO

/* 参照で TrackCD から引くケースがあるなら付けておくと便利 */
CREATE INDEX IX_MT_Track_TrackCD
    ON dbo.MT_Track (TrackCD)
GO
