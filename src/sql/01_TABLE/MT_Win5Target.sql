CREATE TABLE MT_Win5Target (
    race_id    VARCHAR(20) NOT NULL,  -- 202509050111 など
    win5_date  DATE        NOT NULL,  -- 2025-12-07 など
    leg_no     TINYINT     NOT NULL,  -- WIN5の第何レース目か（1〜5）

    CONSTRAINT PK_MT_Win5Target PRIMARY KEY (race_id),
    CONSTRAINT CK_MT_Win5Target_LegNo CHECK (leg_no BETWEEN 1 AND 5)
);

-- 日付＋脚番での集計・検索用にインデックスを張っておくと便利かもしれません
CREATE INDEX IX_MT_Win5Target_Date_LegNo
    ON MT_Win5Target (win5_date, leg_no);
