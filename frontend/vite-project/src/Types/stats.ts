export interface RunStats {
    totalRuns: number;
    totalWins: number;
    totalLosses: number;
    overallWinRate: number;
    averageFloorReached: number;
    byCharacter: CharacterStats[];
    byAscension: AscensionStats[];
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