CREATE OR ALTER VIEW dbo.VW_RaceExpectation
AS
SELECT
      x.race_id
    , x.horse_no
    , x.horse_name
    , x.track_name
    , x.surface_type
    , x.distance_m
    , x.ability
    , x.mult_course
    , x.mult_style
    , x.mult_frame
    , x.mult_blood
    , x.mult_jockey
    , x.expected_100
    , x.updated_at
FROM dbo.TR_RaceExpectation x;
GO
