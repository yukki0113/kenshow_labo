CREATE OR ALTER VIEW dbo.VW_RaceResultWithPayout
AS
SELECT
    b.*,
    sp.payout_amount AS [単勝払戻],
    sp.popularity    AS [単勝人気],
    fp.payout_amount AS [複勝払戻],
    fp.popularity    AS [複勝人気]
FROM dbo.VW_RaceResultWithPedigree AS b
LEFT JOIN dbo.TR_Payout AS sp
    ON  b.race_id = sp.race_id
    AND b.[馬番]  = sp.horse_no_1
    AND sp.bet_type = N'単勝'
LEFT JOIN dbo.TR_Payout AS fp
    ON  b.race_id = fp.race_id
    AND b.[馬番]  = fp.horse_no_1
    AND fp.bet_type = N'複勝';
GO
