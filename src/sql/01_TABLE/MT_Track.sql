--* BackupToTempTable
drop table [MT_Track]
GO
 
--* RestoreFromTempTable
CREATE TABLE dbo.MT_Track
(
      track_cd    CHAR(2)        NOT NULL
    , track_name  NVARCHAR(10)   NOT NULL
    , CONSTRAINT PK_MT_Track PRIMARY KEY (track_cd)
);

CREATE UNIQUE INDEX UX_MT_Track_track_name
ON dbo.MT_Track (track_name);

INSERT INTO dbo.MT_Track (track_cd, track_name) VALUES
 (N'01', N'札幌')
,(N'02', N'函館')
,(N'03', N'福島')
,(N'04', N'新潟')
,(N'05', N'東京')
,(N'06', N'中山')
,(N'07', N'中京')
,(N'08', N'京都')
,(N'09', N'阪神')
,(N'10', N'小倉');