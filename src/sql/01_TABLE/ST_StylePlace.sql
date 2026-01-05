--* BackupToTempTable
drop table [ST_StylePlace]
GO

--* RestoreFromTempTable
CREATE TABLE dbo.ST_StylePlace
(
      track_cd         CHAR(2) NOT NULL
    , surface_type     NVARCHAR(10) NOT NULL
    , distance_band    INT NOT NULL
    , style            NVARCHAR(10) NOT NULL      -- '逃げ','先行','差し','追込' 等（VWの脚質）
    , n                INT NOT NULL
    , hit_place        INT NOT NULL
    , p_place          DECIMAL(9,6) NOT NULL
    , p_place_smooth   DECIMAL(9,6) NOT NULL
    , mult             DECIMAL(9,6) NOT NULL
    , updated_at       DATETIME2 NOT NULL DEFAULT SYSDATETIME()
    , CONSTRAINT PK_ST_StylePlace PRIMARY KEY (track_cd, surface_type, distance_band, style)
);
