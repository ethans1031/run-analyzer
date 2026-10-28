export interface RunSummary {
    id: number;
    characterName: string;
    won: boolean;
    ascension: number;
    floorReached: number;
    timeStarted: string;
    timeElapsed: number;
}

export interface RunDetail {
    id: number;
    characterName: string;
    won: boolean;
    ascension: number;
    floorReached: number;
    timeStarted: string;
    timeElapsed: number;
    killedByEncounter: string | null;
    buildId: string;
    seed: string;
    gameMode: string;
    schemaVersion: number;
    cardChoices: CardChoice[];
    relicChoices: RelicChoice[];
}

export interface CardChoice {
    id: number;
    cardId: string;
    picked: boolean;
    act: number;
    floor: number;
    source: string;
}

export interface RelicChoice {
    id: number;
    relicId: string;
    picked: boolean;
    act: number;
    floor: number;
    source: string;
}
