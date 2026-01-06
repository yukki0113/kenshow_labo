--* BackupToTempTable
drop table [ST_FramePlace]
GO

--* RestoreFromTempTable
CREATE TABLE dbo.ST_FramePlace
(
      jyo_cd         CHAR(2) NOT NULL
    , surface_type     NVARCHAR(10) NOT NULL
    , distance_class    INT NOT NULL
    , frame_no         INT NOT NULL               -- 1～8
    , n                INT NOT NULL
    , hit_place        INT NOT NULL
    , p_place          DECIMAL(9,6) NOT NULL
    , p_place_smooth   DECIMAL(9,6) NOT NULL
    , mult             DECIMAL(9,6) NOT NULL
    , updated_at       DATETIME2 NOT NULL DEFAULT SYSDATETIME()
    , CONSTRAINT PK_ST_FramePlace PRIMARY KEY (jyo_cd, surface_type, distance_band, frame_no)
);
