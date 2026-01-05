--* BackupToTempTable
drop table [ST_CoursePlace]
GO

--* RestoreFromTempTable
CREATE TABLE dbo.ST_CoursePlace
(
      track_cd         CHAR(2) NOT NULL
    , surface_type     NVARCHAR(10) NOT NULL      -- '芝'/'ダ'等（VWの芝/ダをそのまま）
    , distance_band    INT NOT NULL
    , n                INT NOT NULL
    , hit_place        INT NOT NULL
    , p_place          DECIMAL(9,6) NOT NULL
    , p_place_smooth   DECIMAL(9,6) NOT NULL
    , mult             DECIMAL(9,6) NOT NULL
    , updated_at       DATETIME2 NOT NULL DEFAULT SYSDATETIME()
    , CONSTRAINT PK_ST_CoursePlace PRIMARY KEY (track_cd, surface_type, distance_band)
);
