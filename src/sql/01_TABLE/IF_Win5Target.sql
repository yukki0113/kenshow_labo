--* BackupToTempTable
drop table [IF_JV_Win5Target]
GO
 
--* RestoreFromTempTable
CREATE TABLE dbo.IF_JV_Win5Target (
    race_id     CHAR(12)     NOT NULL,
    win5_date   DATE         NOT NULL,
    leg_no      TINYINT      NOT NULL,

    source_kubun CHAR(1)     NULL,   -- WF headDataKubun（どの段階のレコードか）
    make_ymd     CHAR(8)     NULL,   -- WF headMakeDate

    CONSTRAINT PK_IF_JV_Win5Target PRIMARY KEY (race_id),
    CONSTRAINT CK_IF_JV_Win5Target_LegNo CHECK (leg_no BETWEEN 1 AND 5)
)
GO

CREATE INDEX IX_IF_JV_Win5Target_DateLeg ON dbo.IF_JV_Win5Target(win5_date, leg_no)
GO
