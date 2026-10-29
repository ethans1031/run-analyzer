export const CHART_HEIGHT = 300;

export const gridProps = {
    stroke: "var(--color-border)",
    strokeDasharray: "3 3",
    vertical: false,
} as const;

export const axisProps = {
    stroke: "var(--color-border-strong)",
    tick: { fill: "var(--color-text-muted)", fontSize: 13 },
    tickLine: false,
} as const;

export const tooltipProps = {
    contentStyle: {
        backgroundColor: "var(--color-bg-elevated)",
        border: "1px solid var(--color-border-strong)",
        borderRadius: "var(--radius-sm)",
        color: "var(--color-text)",
    },
    labelStyle: { color: "var(--color-text-muted)" },
    itemStyle: { color: "var(--color-text)" },
    cursor: { fill: "var(--color-bg-hover)" },
    separator: ": ",
} as const;

export const BAR_RADIUS: [number, number, number, number] = [4, 4, 0, 0];
