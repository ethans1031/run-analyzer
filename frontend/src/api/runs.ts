import type { RunDetail, RunSummary } from "../types/run";
import type { ChoiceStatsResponse, RunStats } from "../types/stats";

// Set VITE_API_BASE_URL in frontend/.env.local to point at a different backend.
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7041";

export interface UploadResult {
    uploaded: { file: string; id: number }[];
    failed: { file: string; error: string }[];
}

export interface RunFilters {
    card?: string | null;
    relic?: string | null;
}

// Builds an API URL, skipping query params that are empty.
function apiUrl(path: string, params: Record<string, string | null | undefined> = {}): string {
    const url = new URL(path, API_BASE_URL);
    for (const [key, value] of Object.entries(params)) {
        if (value) {
            url.searchParams.set(key, value);
        }
    }
    return url.toString();
}

async function getJson<T>(url: string, description: string): Promise<T> {
    const response = await fetch(url);

    if (!response.ok) {
        throw new Error(`Failed to fetch ${description}. Status: ${response.status}`);
    }
    return await response.json();
}

export async function uploadRuns(files: File[]): Promise<UploadResult> {
    const formData = new FormData();
    for (const file of files) {
        formData.append("files", file);
    }

    const response = await fetch(apiUrl("/api/runs/upload"), {
        method: "POST",
        body: formData,
    });

    if (!response.ok) {
        throw new Error(`Failed to upload runs. Status: ${response.status}`);
    }

    return await response.json();
}

export function getRun(id: number): Promise<RunDetail> {
    return getJson(apiUrl(`/api/runs/${id}`), `run ${id}`);
}

export function getRuns(filters: RunFilters = {}): Promise<RunSummary[]> {
    return getJson(apiUrl("/api/runs", { card: filters.card, relic: filters.relic }), "runs");
}

export function getStats(): Promise<RunStats> {
    return getJson(apiUrl("/api/stats"), "stats");
}

export function getCardStats(character?: string | null): Promise<ChoiceStatsResponse> {
    return getJson(apiUrl("/api/stats/cards", { character }), "card stats");
}

export function getRelicStats(character?: string | null): Promise<ChoiceStatsResponse> {
    return getJson(apiUrl("/api/stats/relics", { character }), "relic stats");
}
