export interface RunStats {
    totalRuns: number;
    totalWins: number;
    totalLosses: number;
    overallWinRate: number;
    averageFloorReached: number;
    averageWinTime: number | null;
    averageLossTime: number | null;
    longestWinStreak: number;
    byCharacter: CharacterStats[];
    byAscension: AscensionStats[];
    topKillers: KillerStats[];
    lossesByFloor: FloorBucket[];
    byWeek: WeeklyStats[];
}

export interface CharacterStats {
    character: string;
    runCount: number;
    winCount: number;
    winRate: number;
    averageFloorReached: number;
}

export interface AscensionStats {
    ascension: number;
    runCount: number;
    winCount: number;
    winRate: number;
}

export interface KillerStats {
    encounter: string;
    count: number;
    shareOfLosses: number;
}

export interface FloorBucket {
    floorStart: number;
    floorEnd: number;
    losses: number;
}

export interface WeeklyStats {
    weekStart: string;
    runCount: number;
    winCount: number;
    winRate: number;
}

export interface ChoiceStatsResponse {
    runCount: number;
    baselineWinRate: number;
    items: ChoiceStats[];
}

export interface ChoiceStats {
    id: string;
    timesOffered: number;
    timesPicked: number;
    pickRate: number | null;
    runsPicked: number;
    runsWon: number;
    winRate: number | null;
    winRateDelta: number | null;
}
