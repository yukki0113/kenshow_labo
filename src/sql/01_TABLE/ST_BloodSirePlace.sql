--* BackupToTempTable
drop table [ST_BloodSirePlace]
GO

--* RestoreFromTempTable
CREATE TABLE dbo.ST_BloodSirePlace
(
      sire_name        NVARCHAR(50) NOT NULL      -- 父
    , track_cd         CHAR(2) NOT NULL
    , surface_type     NVARCHAR(10) NOT NULL
    , distance_band    INT NOT NULL
    , n                INT NOT NULL
    , hit_place        INT NOT NULL
    , p_place          DECIMAL(9,6) NOT NULL
    , p_place_smooth   DECIMAL(9,6) NOT NULL
    , mult             DECIMAL(9,6) NOT NULL
    , updated_at       DATETIME2 NOT NULL DEFAULT SYSDATETIME()
    , CONSTRAINT PK_ST_BloodSirePlace PRIMARY KEY (sire_name, track_cd, surface_type, distance_band)
);
