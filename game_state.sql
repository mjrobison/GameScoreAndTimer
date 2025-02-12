CREATE TABLE GameState (
    Id INTEGER PRIMARY KEY CHECK (Id = 1), -- Ensures only one row exists
    GameLevel TEXT NOT NULL,
    Quarter INTEGER,
    QuarterTime INTEGER NOT NULL,
    Team1 TEXT NOT NULL,
    Team2 TEXT NOT NULL,
    Team1Score INTEGER,
    Team2Score INTEGER,
    Team1TimeoutsRemaining INTEGER,
    Team2TimeoutsRemaining INTEGER,
    HomeTeamFouls INTEGER,
    AwayTeamFouls INTEGER
);

CREATE TABLE GameRules (
    GameLevel TEXT,
    QuarterTime INT,
    TimeOuts Int
);

INSERT INTO GameRules (GameLevel, QuarterTime, TimeOuts)
VALUES
('18U', 540, 5),
('16U', 420, 5),
('14U', 360, 5),
('12U', 360, 5);