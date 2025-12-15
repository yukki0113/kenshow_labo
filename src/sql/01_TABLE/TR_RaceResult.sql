--* BackupToTempTable
drop table [TR_RaceResult]
GO
 
--* RestoreFromTempTable
CREATE TABLE [TR_RaceResult] ( 
  [race_id] CHAR(12) DEFAULT NULL
  , [日付] DATE DEFAULT NULL
  , [レース名] NVARCHAR (50) DEFAULT NULL
  , [クラス] NVARCHAR (20) DEFAULT NULL
  , [classSimple] NVARCHAR(10) DEFAULT NULL
  , [WIN5_flg] TINYINT NOT NULL DEFAULT 0
  , [場名] NVARCHAR (3) DEFAULT NULL
  , [芝_ダート] NVARCHAR (10) DEFAULT NULL
  , [距離] INT DEFAULT NULL
  , [枠番] INT DEFAULT NULL
  , [馬番] INT DEFAULT NULL
  , [horse_id] VARCHAR(20) NULL
  , [馬名] NVARCHAR (30) DEFAULT NULL
  , [性] NVARCHAR (1) DEFAULT NULL
  , [齢] INT DEFAULT NULL
  , [騎手] NVARCHAR (10) DEFAULT NULL
  , [斤量] DECIMAL(3, 1) DEFAULT NULL
  , [人気] INT DEFAULT NULL
  , [オッズ] DECIMAL(4, 1) DEFAULT NULL
  , [着順] INT DEFAULT NULL
  , [走破時計] TIME DEFAULT NULL
  , [馬体重] INT DEFAULT NULL
  , [馬体重変動] INT DEFAULT NULL
  , [通過順_1角] INT DEFAULT NULL
  , [通過順_2角] INT DEFAULT NULL
  , [通過順_3角] INT DEFAULT NULL
  , [通過順_4角] INT DEFAULT NULL
  , [上がり] DECIMAL(3, 1) DEFAULT NULL
  , [開催] NVARCHAR (12) DEFAULT NULL
  , [回り] NVARCHAR (1) DEFAULT NULL
  , [馬場] NVARCHAR (1) DEFAULT NULL
  , [天気] NVARCHAR (1) DEFAULT NULL
  , [track_id] NVARCHAR(2) DEFAULT NULL
  , PRIMARY KEY (race_id, 馬番)
)
GO

-- CREATE INDEX IX_TR_raceresult_馬名
-- ON TR_raceresult (馬名);

-- CREATE INDEX IX_TR_raceresult_horse_id
-- ON TR_raceresult (horse_id);
