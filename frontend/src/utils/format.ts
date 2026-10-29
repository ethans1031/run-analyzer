import type { CardChoice } from "../types/run";

export interface FloorGroup {
    act: number;
    floor: number;
    source: string;
    choices: CardChoice[];
}

export function groupChoicesByFloor(choices: CardChoice[]): FloorGroup[] {
    const groups = new Map<string, FloorGroup>();

    for (const choice of choices) {
        const key = `${choice.act}-${choice.floor}`;
        if (!groups.has(key)) {
            groups.set(key, {
                act: choice.act,
                floor: choice.floor,
                source: choice.source,
                choices: [],
            });
        }
        groups.get(key)!.choices.push(choice);
    }

    // Sort by act, then floor
    return Array.from(groups.values()).sort((a, b) => {
        if (a.act !== b.act) return a.act - b.act;
        return a.floor - b.floor;
    });
}

export function formatCharacter(id: string): string {
    return id
        .replace(/^CHARACTER\./, "")
        .toLowerCase()
        .replace(/^./, c => c.toUpperCase());
}

// "PREFIX.SOME_NAME" -> "Some Name"
function formatGameId(id: string, prefix: string): string {
    return id
        .replace(new RegExp(`^${prefix}\\.`), "")
        .toLowerCase()
        .replace(/_/g, " ")
        .replace(/\b./g, c => c.toUpperCase());
}

export function formatCard(id: string): string {
    return formatGameId(id, "CARD");
}

export function formatRelic(id: string): string {
    return formatGameId(id, "RELIC");
}

export function formatEncounter(id: string): string {
    return formatGameId(id, "ENCOUNTER");
}

export function formatPercent(value: number): string {
    return `${(value * 100).toFixed(1)}%`;
}

export function formatDuration(seconds: number): string {
    const minutes = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${minutes}m ${secs}s`;
}

export function formatOptionalPercent(value: number | null): string {
    return value === null ? "-" : formatPercent(value);
}

export function formatSignedPercent(value: number | null): string {
    if (value === null) return "-";
    const points = (value * 100).toFixed(1);
    return value > 0 ? `+${points}%` : `${points}%`;
}
