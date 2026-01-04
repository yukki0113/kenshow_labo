CREATE OR ALTER VIEW dbo.VW_RaceResultContract_Local
AS
SELECT *
FROM dbo.VW_RaceResultContract
WHERE jyo_cd IN
(
    SELECT code
    FROM dbo.MT_CodeDictionary
    WHERE code_type = N'RACE_COURSE'
      AND is_active = 1
      AND text_sub = N'LOCAL'
)
GO
