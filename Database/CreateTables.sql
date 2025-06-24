CREATE TABLE IF NOT EXISTS trades (
	id INTEGER NOT NULL PRIMARY KEY,
	timestamp TEXT,
	tradeType TEXT,
	coinSymbol TEXT,
	coinName TEXT,
	coinAmount DOUBLE,
	coinPrice DOUBLE,
	tradeValue DOUBLE,
	username TEXT,
	userId INTEGER
);
