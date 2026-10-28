import { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import {
    BarChart,
    Bar,
    LineChart,
    Line,
    XAxis,
    YAxis,
    Tooltip,
    ResponsiveContainer,
    CartesianGrid,
} from "recharts";
import type { RunStats } from "../types/stats";
import { getStats } from "../api/runs";
import { formatCharacter, formatDuration, formatEncounter, formatPercent } from "../utils/format";
import { BAR_RADIUS, CHART_HEIGHT, axisProps, gridProps, tooltipProps } from "../components/chartTheme";
import "./StatsPage.css";

function formatOptionalDuration(seconds: number | null): string {
    return seconds === null ? "-" : formatDuration(Math.round(seconds));
}

// "2026-10-19" -> "Oct 19". Parsed as a plain date so the time zone can't shift the day.
function formatWeek(isoDate: string): string {
    const [year, month, day] = isoDate.split("-").map(Number);
    return new Date(year, month - 1, day).toLocaleDateString(undefined, { month: "short", day: "numeric" });
}

function StatsPage() {
    const [stats, setStats] = useState<RunStats | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        async function load() {
            try {
                const data = await getStats();
                setStats(data);
            } catch (err) {
                setError(err instanceof Error ? err.message : "Unknown error");
            } finally {
                setIsLoading(false);
            }
        }
        load();
    }, []);

    if (isLoading) return <div>Loading stats...</div>;
    if (error) return <div>Error loading stats: {error}</div>;

    if (!stats || stats.totalRuns === 0) {
        return (
            <div>
                <h1>Stats</h1>
                <p>No runs uploaded yet. <Link to="/upload">Upload runs to view stats</Link>.</p>
            </div>
        );
    }

    // Chart data: readable labels and win rates scaled to 0-100.
    const characterData = stats.byCharacter.map(c => ({
        character: formatCharacter(c.character),
        winRatePercent: Number((c.winRate * 100).toFixed(1)),
    }));

    const weeklyData = stats.byWeek.map(w => ({
        week: formatWeek(w.weekStart),
        winRatePercent: Number((w.winRate * 100).toFixed(1)),
        runCount: w.runCount,
    }));

    const floorData = stats.lossesByFloor.map(b => ({
        floors: `${b.floorStart}-${b.floorEnd}`,
        losses: b.losses,
    }));

    return (
        <div>
            <h1>Stats</h1>

            <section>
                <h2>Overview</h2>
                <div className="stat-cards">
                    <StatCard label="Total Runs" value={stats.totalRuns} />
                    <StatCard label="Wins" value={stats.totalWins} />
                    <StatCard label="Losses" value={stats.totalLosses} />
                    <StatCard label="Win Rate" value={formatPercent(stats.overallWinRate)} />
                    <StatCard label="Avg Floor" value={stats.averageFloorReached.toFixed(1)} />
                    <StatCard label="Best Win Streak" value={stats.longestWinStreak} />
                    <StatCard label="Avg Win Time" value={formatOptionalDuration(stats.averageWinTime)} />
                    <StatCard label="Avg Loss Time" value={formatOptionalDuration(stats.averageLossTime)} />
                </div>
            </section>

            <section>
                <h2>Win Rate by Character</h2>
                <div className="chart-wrapper">
                    <ResponsiveContainer width="100%" height={CHART_HEIGHT}>
                        <BarChart data={characterData}>
                            <CartesianGrid {...gridProps} />
                            <XAxis dataKey="character" {...axisProps} />
                            <YAxis unit="%" domain={[0, 100]} {...axisProps} />
                            <Tooltip {...tooltipProps} formatter={value => `${value}%`} />
                            <Bar dataKey="winRatePercent" fill="var(--color-accent)" name="Win Rate" radius={BAR_RADIUS} />
                        </BarChart>
                    </ResponsiveContainer>
                </div>
            </section>

            <section>
                <h2>Per-Character Breakdown</h2>
                <table>
                    <thead>
                        <tr>
                            <th>Character</th>
                            <th>Runs</th>
                            <th>Wins</th>
                            <th>Win Rate</th>
                            <th>Avg Floor</th>
                        </tr>
                    </thead>
                    <tbody>
                        {stats.byCharacter.map(c => (
                            <tr key={c.character}>
                                <td>{formatCharacter(c.character)}</td>
                                <td>{c.runCount}</td>
                                <td>{c.winCount}</td>
                                <td>{formatPercent(c.winRate)}</td>
                                <td>{c.averageFloorReached.toFixed(1)}</td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </section>

            {weeklyData.length > 1 && (
                <section>
                    <h2>Win Rate Over Time</h2>
                    <div className="chart-wrapper">
                        <ResponsiveContainer width="100%" height={CHART_HEIGHT}>
                            <LineChart data={weeklyData}>
                                <CartesianGrid {...gridProps} />
                                <XAxis dataKey="week" {...axisProps} />
                                <YAxis unit="%" domain={[0, 100]} {...axisProps} />
                                <Tooltip
                                    {...tooltipProps}
                                    cursor={{ stroke: "var(--color-border-strong)" }}
                                    labelFormatter={label => `Week of ${label}`}
                                    formatter={(value, _name, item) => [
                                        `${value}% (${item.payload.runCount} runs)`,
                                        "Win Rate",
                                    ]}
                                />
                                <Line
                                    type="linear"
                                    dataKey="winRatePercent"
                                    stroke="var(--color-accent)"
                                    strokeWidth={2}
                                    dot={{ r: 4, fill: "var(--color-accent)", strokeWidth: 0 }}
                                    activeDot={{ r: 6 }}
                                />
                            </LineChart>
                        </ResponsiveContainer>
                    </div>
                </section>
            )}

            {floorData.length > 0 && (
                <section>
                    <h2>Where Runs End</h2>
                    <p className="page-intro">Losses by the floor you reached, in groups of five floors.</p>
                    <div className="chart-wrapper">
                        <ResponsiveContainer width="100%" height={CHART_HEIGHT}>
                            <BarChart data={floorData}>
                                <CartesianGrid {...gridProps} />
                                <XAxis dataKey="floors" {...axisProps} />
                                <YAxis allowDecimals={false} {...axisProps} />
                                <Tooltip {...tooltipProps} labelFormatter={label => `Floors ${label}`} />
                                <Bar dataKey="losses" fill="var(--color-loss)" name="Losses" radius={BAR_RADIUS} />
                            </BarChart>
                        </ResponsiveContainer>
                    </div>
                </section>
            )}

            {stats.topKillers.length > 0 && (
                <section>
                    <h2>What Kills You</h2>
                    <table>
                        <thead>
                            <tr>
                                <th>Encounter</th>
                                <th>Losses</th>
                                <th>Share of Losses</th>
                            </tr>
                        </thead>
                        <tbody>
                            {stats.topKillers.map(k => (
                                <tr key={k.encounter}>
                                    <td>{formatEncounter(k.encounter)}</td>
                                    <td>{k.count}</td>
                                    <td>{formatPercent(k.shareOfLosses)}</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </section>
            )}

            {stats.byAscension.length > 1 && (
                <section>
                    <h2>Per-Ascension Breakdown</h2>
                    <table>
                        <thead>
                            <tr>
                                <th>Ascension</th>
                                <th>Runs</th>
                                <th>Wins</th>
                                <th>Win Rate</th>
                            </tr>
                        </thead>
                        <tbody>
                            {stats.byAscension.map(a => (
                                <tr key={a.ascension}>
                                    <td>{a.ascension}</td>
                                    <td>{a.runCount}</td>
                                    <td>{a.winCount}</td>
                                    <td>{formatPercent(a.winRate)}</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </section>
            )}
        </div>
    );
}

function StatCard({ label, value }: { label: string; value: string | number }) {
    return (
        <div className="stat-card">
            <div className="stat-card__label">{label}</div>
            <div className="stat-card__value">{value}</div>
        </div>
    );
}

export default StatsPage;
