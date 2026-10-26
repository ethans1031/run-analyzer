import { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import {
    BarChart,
    Bar,
    XAxis,
    YAxis,
    Tooltip,
    ResponsiveContainer,
    CartesianGrid,
} from "recharts";
import type { RunStats } from "../Types/stats";
import { getStats } from "../Api/runs";
import "./StatsPage.css";

function formatCharacter(id: string) {
    return id
        .replace(/^CHARACTER\./, "")
        .toLowerCase()
        .replace(/^./, c => c.toUpperCase());
}

function formatPercent(value: number) {
    return `${(value * 100).toFixed(1)}%`;
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

    // Prepare chart data with formatted character names and percentage-scaled win rates
    const chartData = stats.byCharacter.map(c => ({
        character: formatCharacter(c.character),
        winRatePercent: Number((c.winRate * 100).toFixed(1)),
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
                </div>
            </section>

            <section>
                <h2>Win Rate by Character</h2>
                <div className="chart-wrapper">
                    <ResponsiveContainer width="100%" height={300}>
                        <BarChart data={chartData}>
                            <CartesianGrid strokeDasharray="3 3" />
                            <XAxis dataKey="character" />
                            <YAxis unit="%" domain={[0, 100]} />
                            <Tooltip formatter={(value) => `${value}%`} />
                            <Bar dataKey="winRatePercent" fill="var(--color-accent)" name="Win Rate" />
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