import type { RunDetail } from "../Types/run";
import type { RunSummary } from "../Types/run";
import type { RunStats } from "../Types/stats";

const API_BASE_URL = "https://localhost:7041";

interface UploadResult {
    uploaded: { file: string; id: number }[];
    failed: { file: string; error: string }[];
}

export async function uploadRuns(files: File[]): Promise<UploadResult> {
    const formData = new FormData();
    for (const file of files) {
        formData.append("files", file);
    }

    const response = await fetch(`${API_BASE_URL}/api/runs/upload`, 
        {
            method: "POST",
            body: formData
        }
    );

    if (!response.ok) {
        throw new Error(`Failed to upload runs. Status: ${response.status}`);
    }

    return await response.json();
}

export async function getRun(id: number): Promise<RunDetail> {
    const response = await fetch(`${API_BASE_URL}/api/runs/${id}`);

    if (!response.ok) {
        throw new Error(`Failed to fetch run ${id}. Status: ${response.status}`);
    }
    return await response.json();
}

export async function getRuns(): Promise<RunSummary[]> {
    const response = await fetch(`${API_BASE_URL}/api/runs`);

    if (!response.ok) {
        throw new Error(`Failed to fetch runs. Status: ${response.status}`);
    }
    return await response.json();
}

export async function getStats(): Promise<RunStats> {
    const response = await fetch(`${API_BASE_URL}/api/stats`);

    if (!response.ok) {
        throw new Error(`Failed to fetch stats. Status: ${response.status}`);
    }
    return await response.json();
}