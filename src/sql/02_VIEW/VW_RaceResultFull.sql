CREATE VIEW VW_RaceResultFull
AS
SELECT
    b.*,
    sp.払戻金 AS 単勝払戻,
    sp.人気   AS 単勝人気,
    fp.払戻金 AS 複勝払戻,
    fp.人気   AS 複勝人気
FROM
    VW_RaceResultWithPedigree AS b
    LEFT JOIN TR_Payout AS sp
        ON  b.race_id = sp.race_id
        AND b.馬番   = sp.馬番1
        AND sp.式別  = N'単勝'
    LEFT JOIN TR_Payout AS fp
        ON  b.race_id = fp.race_id
        AND b.馬番   = fp.馬番1
        AND fp.式別  = N'複勝';
GO
