CREATE OR ALTER VIEW dbo.VW_RaceExpectationExplain
AS
SELECT
      x.race_id
    , x.horse_no
    , x.horse_name
    , x.track_name
    , x.jyo_cd
    , x.surface_type
    , x.distance_m
    , x.distance_class

    , x.ability
    , x.mult_course
    , x.mult_style
    , x.mult_frame
    , x.mult_blood
    , x.mult_jockey

    /* 係数→0～1化（暫定：現在のロジックと同じ） */
    , CAST((x.mult_course  - 0.85) / 0.30 AS DECIMAL(9,6)) AS course01
    , CAST((x.mult_style   - 0.85) / 0.30 AS DECIMAL(9,6)) AS style01
    , CAST((x.mult_frame   - 0.85) / 0.30 AS DECIMAL(9,6)) AS frame01
    , CAST((x.mult_blood   - 0.85) / 0.30 AS DECIMAL(9,6)) AS blood01
    , CAST((x.mult_jockey  - 0.85) / 0.30 AS DECIMAL(9,6)) AS jockey01

    /* 寄与（点数換算：*100） */
    , CAST(0.50 * x.ability * 100.0 AS DECIMAL(9,2)) AS contrib_ability
    , CAST(0.20 * ((x.mult_course - 0.85) / 0.30) * 100.0 AS DECIMAL(9,2)) AS contrib_course
    , CAST(0.10 * ((x.mult_style  - 0.85) / 0.30) * 100.0 AS DECIMAL(9,2)) AS contrib_style
    , CAST(0.05 * ((x.mult_frame  - 0.85) / 0.30) * 100.0 AS DECIMAL(9,2)) AS contrib_frame
    , CAST(0.10 * ((x.mult_blood  - 0.85) / 0.30) * 100.0 AS DECIMAL(9,2)) AS contrib_blood
    , CAST(0.05 * ((x.mult_jockey - 0.85) / 0.30) * 100.0 AS DECIMAL(9,2)) AS contrib_jockey

    , x.expected_100
    , x.updated_at
FROM dbo.TR_RaceExpectation x;
GO
