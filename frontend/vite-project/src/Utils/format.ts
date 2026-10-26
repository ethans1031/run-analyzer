import type { CardChoice } from "../Types/run";

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

    // Sort by act, then floor.
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

export function formatCard(id: string): string {
    return id
        .replace(/^CARD\./, "")
        .toLowerCase()
        .replace(/_/g, " ")
        .replace(/\b./g, c => c.toUpperCase());
}

export function formatRelic(id: string): string {
    return id
        .replace(/^RELIC\./, "")
        .toLowerCase()
        .replace(/_/g, " ")
        .replace(/\b./g, c => c.toUpperCase());
}

export function formatPercent(value: number): string {
    return `${(value * 100).toFixed(1)}%`;
}

export function formatDuration(seconds: number): string {
    const minutes = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${minutes}m ${secs}s`;
}