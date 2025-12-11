CREATE TABLE dbo.TR_Payout (
    -- 物理PK（自動採番）
    id INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_TR_Payout PRIMARY KEY,

    -- TR_raceresult と型を完全一致させる（CHAR(12)）
    race_id CHAR(12) NOT NULL,

    -- 式別：'単勝' / '複勝' / '馬連' など
    式別 NVARCHAR(20) NOT NULL,

    -- 馬番（組み合わせに応じて使用）
    馬番1 INT NULL,
    馬番2 INT NULL,
    馬番3 INT NULL,

    -- 払戻金（円）
    払戻金 INT NOT NULL,

    -- 人気（1人気,2人気…）
    人気 INT NULL
);

CREATE INDEX IX_Payout_Race_BetType
    ON dbo.TR_Payout (race_id, 式別);

CREATE INDEX IX_Payout_Race_BetType_Horse
    ON dbo.TR_Payout (race_id, 式別, 馬番1);

ALTER TABLE dbo.TR_Payout
ADD CONSTRAINT UQ_Payout_Race_BetType_Horses
    UNIQUE (race_id, 式別, 馬番1, 馬番2, 馬番3);
