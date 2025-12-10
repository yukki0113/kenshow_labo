CREATE TABLE MT_HorsePedigree (
    horse_id   VARCHAR(20)     NOT NULL,      -- /horse/2019103422/ のID部分
    馬名       NVARCHAR(100)   NULL,
    父         NVARCHAR(100)   NULL,
    母         NVARCHAR(100)   NULL,
    母父       NVARCHAR(100)   NULL,
    母母       NVARCHAR(100)   NULL,
    父父       NVARCHAR(100)   NULL,
    父母       NVARCHAR(100)   NULL,
    CONSTRAINT PK_MT_HorsePedigree PRIMARY KEY (horse_id)
);

CREATE INDEX IX_MT_HorsePedigree_馬名
ON MT_HorsePedigree (馬名);