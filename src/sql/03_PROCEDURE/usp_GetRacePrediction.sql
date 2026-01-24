CREATE OR ALTER PROCEDURE dbo.usp_GetRacePrediction
    @race_id CHAR(12) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- 重み係数の取得
    DECLARE @wa DECIMAL(3,2), @wm DECIMAL(3,2), @ws DECIMAL(3,2), @wd DECIMAL(3,2),
            @wr DECIMAL(3,2), @wj DECIMAL(3,2), @wb DECIMAL(3,2), @wg DECIMAL(3,2), @wt DECIMAL(3,2);
    SELECT @wa=w_ability, @wm=w_momentum, @ws=w_site, @wd=w_dist, 
           @wr=w_sire, @wj=w_jockey, @wb=w_bias, @wg=w_going, @wt=w_style
    FROM dbo.MT_WeightConfig WHERE is_active = 1;

    -- 物理テーブルのクリア
    IF @race_id IS NULL TRUNCATE TABLE dbo.TR_RacePredictionResult;
    ELSE DELETE FROM dbo.TR_RacePredictionResult WHERE race_id = @race_id;

    WITH TargetEntries AS (
        /* 1. 出走予定情報の取得と条件判別 */
        SELECT 
              v.race_id, v.[日付] AS target_date, v.jyo_cd, v.surface_type AS surface_type_cd
            , v.[距離], v.horse_id, v.horse_name, v.frame_no, v.horse_no, v.[R], v.jockey_name
            , v.going AS target_going -- 1:良, 2:稍, 3:重, 4:不
            , CASE 
                WHEN v.[距離] < 1400 THEN 'S' WHEN v.[距離] < 1900 THEN 'M'
                WHEN v.[距離] < 2400 THEN 'I' ELSE 'L'
              END AS dist_category
            , p.sire AS sire_name
        FROM dbo.VW_RaceEntryContract v
        LEFT JOIN dbo.MT_HorsePedigree p ON v.horse_id = p.horse_id
        WHERE (@race_id IS NULL OR v.race_id = @race_id)
    ),
    PastResults AS (
        /* 2. 個体実績の抽出（当日より前） */
        SELECT 
              te.race_id AS target_race_id, te.horse_id, ra.ability_idx, rr.race_date, rr.jyo_cd
            , CASE WHEN rr.baba_siba_cd <> '0' THEN '1' ELSE '2' END AS surface_type_cd
            , rr.going, rr.distance_m
            , CASE
                  WHEN rr.[脚質] LIKE N'%逃%' THEN N'逃げ'
                  WHEN rr.[脚質] LIKE N'%先%' THEN N'先行'
                  WHEN rr.[脚質] LIKE N'%差%' THEN N'差し'
                  WHEN rr.[脚質] LIKE N'%追%' THEN N'追込'
                  WHEN rr.[脚質] LIKE N'%まく%' OR rr.[脚質] LIKE N'%捲%' THEN N'差し'
                  ELSE NULL
              END AS actual_style
            , ROW_NUMBER() OVER (PARTITION BY te.race_id, te.horse_id ORDER BY rr.race_date DESC) AS rn
        FROM TargetEntries te
        JOIN dbo.TR_RaceAnalysis ra ON te.horse_id = ra.horse_id
        JOIN dbo.TR_RaceResult rr ON ra.race_id = rr.race_id AND ra.horse_id = rr.horse_id 
        WHERE rr.race_date < te.target_date
    ),
    AggregatedMetrics AS (
        /* 3. 各種指標の算出（当日時点） */
        SELECT 
              p.target_race_id AS race_id, p.horse_id
            , AVG(p.ability_idx) AS avg_total
            , AVG(CASE WHEN p.rn <= 5 THEN p.ability_idx END) AS avg_last5
            , AVG(CASE WHEN p.rn <= 3 THEN p.ability_idx END) AS avg_last3
            , COUNT(*) AS career_count
            -- 個体適性(場・距離・馬場)
            , AVG(CASE WHEN p.jyo_cd = te.jyo_cd AND p.surface_type_cd = te.surface_type_cd THEN p.ability_idx END) AS site_avg
            , COUNT(CASE WHEN p.jyo_cd = te.jyo_cd AND p.surface_type_cd = te.surface_type_cd THEN 1 END) AS site_cnt
            , AVG(CASE WHEN p.dist_category = te.dist_category THEN p.ability_idx END) AS dist_avg
            , COUNT(CASE WHEN p.dist_category = te.dist_category THEN 1 END) AS dist_cnt
            , AVG(CASE WHEN p.going = te.target_going THEN p.ability_idx END) AS going_avg
            , COUNT(CASE WHEN p.going = te.target_going THEN 1 END) AS going_cnt
        FROM PastResults p
        JOIN TargetEntries te ON p.target_race_id = te.race_id AND p.horse_id = te.horse_id
        GROUP BY p.target_race_id, p.horse_id
    ),
    PredictedStyle AS (
        /* 4. 当日時点での推定脚質（直近5走の最頻値） */
        SELECT 
            target_race_id AS race_id, horse_id, actual_style AS p_style
        FROM (
            SELECT target_race_id, horse_id, actual_style, 
                   ROW_NUMBER() OVER(PARTITION BY target_race_id, horse_id ORDER BY COUNT(*) DESC, MAX(rn) ASC) as style_rn
            FROM PastResults WHERE rn <= 5 AND actual_style IS NOT NULL
            GROUP BY target_race_id, horse_id, actual_style
        ) T WHERE style_rn = 1
    )

    -- 5. 保存と統合計算
    INSERT INTO dbo.TR_RacePredictionResult (
        race_id, target_date, race_no, frame_no, horse_no, horse_name,
        raw_ability, raw_momentum, raw_site_apt, raw_dist_apt, raw_sire_apt,
        raw_jockey_apt, raw_waku_bias, raw_going_apt, raw_style_match,
        final_expected_score, predicted_style
    )
    SELECT 
          te.race_id, te.target_date, te.[R], te.frame_no, te.horse_no, te.horse_name
        , m.avg_last5 -- raw_ability
        , ISNULL(m.avg_last3 - m.avg_last5, 0) -- raw_momentum
        , ISNULL((m.site_avg - m.avg_total) * (m.site_cnt / (m.site_cnt + 3.0)), 0) -- raw_site_apt
        , ISNULL((m.dist_avg - m.avg_total) * (m.dist_cnt / (m.dist_cnt + 3.0)), 0) -- raw_dist_apt
        , ISNULL(sire.sire_aptitude_score * (sire.sire_run_count / (sire.sire_run_count + 10.0)), 0) -- raw_sire_apt
        , ISNULL(aj.jockey_course_score * (aj.jockey_run_count / (aj.jockey_run_count + 5.0)), 0) -- raw_jockey_apt
        , ISNULL(pb.bias_score, 0) -- raw_waku_bias
        , ISNULL((m.going_avg - m.avg_total) * (m.going_cnt / (m.going_cnt + 3.0)), 0) -- raw_going_apt
        , 0 -- raw_style_match（拡張用：今回は0）
        -- 【最終期待値：重み付き合算】
        , CAST(
              (@wa * ISNULL(m.avg_last5, 0))
            + (@wm * ISNULL(m.avg_last3 - m.avg_last5, 0))
            + (@ws * ISNULL((m.site_avg - m.avg_total) * (m.site_cnt / (m.site_cnt + 3.0)), 0))
            + (@wd * ISNULL((m.dist_avg - m.avg_total) * (m.dist_cnt / (m.dist_cnt + 3.0)), 0))
            + (@wr * ISNULL(sire.sire_aptitude_score * (sire.sire_run_count / (sire.sire_run_count + 10.0)), 0))
            + (@wj * ISNULL(aj.jockey_course_score * (aj.jockey_run_count / (aj.jockey_run_count + 5.0)), 0))
            + (@wb * ISNULL(pb.bias_score, 0))
            + (@wg * ISNULL((m.going_avg - m.avg_total) * (m.going_cnt / (m.going_cnt + 3.0)), 0))
          AS DECIMAL(5,1))
        , ps.p_style
    FROM TargetEntries te
    LEFT JOIN AggregatedMetrics m ON te.race_id = m.race_id AND te.horse_id = m.horse_id
    LEFT JOIN PredictedStyle ps ON te.race_id = ps.race_id AND te.horse_id = ps.horse_id
    LEFT JOIN dbo.MT_PostPositionBias pb ON te.jyo_cd = pb.jyo_cd AND te.surface_type_cd = pb.surface_type AND te.[距離] = pb.distance_m AND te.frame_no = pb.frame_no
    LEFT JOIN dbo.VW_Aptitude_Jockey aj ON te.jockey_name = aj.jockey_name AND te.jyo_cd = aj.jyo_cd AND te.surface_type_cd = aj.surface_type_cd
    LEFT JOIN (SELECT sire_name, jyo_cd, surface_type_cd, AVG(sire_aptitude_score) as sire_aptitude_score, SUM(sire_run_count) as sire_run_count FROM dbo.VW_Aptitude_Sire GROUP BY sire_name, jyo_cd, surface_type_cd) sire 
        ON te.sire_name = sire.sire_name AND te.jyo_cd = sire.jyo_cd AND te.surface_type_cd = sire.surface_type_cd;

    -- 結果表示
    SELECT * FROM dbo.TR_RacePredictionResult WHERE (@race_id IS NULL OR race_id = @race_id) ORDER BY race_id, final_expected_score DESC;
END;