import { useState, useEffect } from "react";
import { Link, useSearchParams } from "react-router-dom";
import type { RunSummary } from "../types/run";
import type { ChoiceStatsResponse } from "../types/stats";
import { getRuns, getCardStats, getRelicStats } from "../api/runs";
import { formatCharacter, formatCard, formatRelic } from "../utils/format";
import ItemFilter, { type ItemOption } from "../components/ItemFilter";

// Filter suggestions: only items picked in at least one run, so every choice returns results.
function toOptions(stats: ChoiceStatsResponse, formatName: (id: string) => string): ItemOption[] {
    return stats.items
        .filter(item => item.runsPicked > 0)
        .map(item => ({
            id: item.id,
            name: formatName(item.id),
            detail: `${item.runsPicked} run${item.runsPicked === 1 ? "" : "s"}`,
        }))
        .sort((a, b) => a.name.localeCompare(b.name));
}

function RunListPage() {
    const [runs, setRuns] = useState<RunSummary[]>([]);
    const [error, setError] = useState<string | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [cardOptions, setCardOptions] = useState<ItemOption[]>([]);
    const [relicOptions, setRelicOptions] = useState<ItemOption[]>([]);

    // Filters live in the URL (?card=...&relic=...) so they survive refresh,
    // work with the back button, and can be linked to from the stats pages.
    const [searchParams, setSearchParams] = useSearchParams();
    const card = searchParams.get("card");
    const relic = searchParams.get("relic");
    const hasFilters = Boolean(card || relic);

    useEffect(() => {
        let cancelled = false;  // ignore responses for filters the user has already changed

        async function load() {
            try {
                const data = await getRuns({ card, relic });
                if (!cancelled) {
                    setRuns(data);
                    setError(null);
                }
            } catch (err) {
                if (!cancelled) setError(err instanceof Error ? err.message : "Unknown error");
            } finally {
                if (!cancelled) setIsLoading(false);
            }
        }
        load();

        return () => {
            cancelled = true;
        };
    }, [card, relic]);

    useEffect(() => {
        async function loadOptions() {
            try {
                const [cardStats, relicStats] = await Promise.all([getCardStats(), getRelicStats()]);
                setCardOptions(toOptions(cardStats, formatCard));
                setRelicOptions(toOptions(relicStats, formatRelic));
            } catch {
                // Suggestions are a convenience; the filters still work without them.
            }
        }
        loadOptions();
    }, []);

    function setFilter(key: "card" | "relic", id: string | null) {
        const next = new URLSearchParams(searchParams);
        if (id) {
            next.set(key, id);
        } else {
            next.delete(key);
        }
        setSearchParams(next);
    }

    if (isLoading) return <div>Loading runs...</div>;
    if (error) return <div>Error loading runs: {error}</div>;

    if (runs.length === 0 && !hasFilters) {
        return (
            <div>
                <div className="page-header">
                    <h1>Runs</h1>
                </div>
                <div className="empty-state">
                    <p>No runs uploaded yet.</p>
                    <Link to="/upload"><button>Upload Your First Run</button></Link>
                </div>
            </div>
        );
    }

    const filterNames = [card && formatCard(card), relic && formatRelic(relic)].filter(Boolean).join(" and ");

    return (
        <div>
            <div className="page-header">
                <h1>Runs</h1>
                <Link to="/upload"><button>Upload New Run</button></Link>
            </div>

            <div className="filter-bar">
                <ItemFilter
                    key={`card-${card ?? ""}`}
                    label="Card"
                    placeholder="Any card"
                    options={cardOptions}
                    selectedId={card}
                    formatName={formatCard}
                    onChange={id => setFilter("card", id)}
                />
                <ItemFilter
                    key={`relic-${relic ?? ""}`}
                    label="Relic"
                    placeholder="Any relic"
                    options={relicOptions}
                    selectedId={relic}
                    formatName={formatRelic}
                    onChange={id => setFilter("relic", id)}
                />
            </div>

            {hasFilters && (
                <p className="table-meta">
                    {runs.length} run{runs.length === 1 ? "" : "s"} where you picked {filterNames}.{" "}
                    <button type="button" className="button-link" onClick={() => setSearchParams({})}>
                        Clear filters
                    </button>
                </p>
            )}

            {runs.length === 0 ? (
                <div className="empty-state">
                    <p>No runs match these filters.</p>
                </div>
            ) : (
                <div className="table-scroll">
                    <table>
                        <thead>
                            <tr>
                                <th>Character</th>
                                <th>Result</th>
                                <th>Floor</th>
                                <th>Ascension</th>
                                <th>Date</th>
                                <th></th>
                            </tr>
                        </thead>
                        <tbody>
                            {runs.map(run => (
                                <tr key={run.id}>
                                    <td>{formatCharacter(run.characterName)}</td>
                                    <td>
                                        <span className={run.won ? "text-win" : "text-loss"}>
                                            {run.won ? "Win" : "Loss"}
                                        </span>
                                    </td>
                                    <td>{run.floorReached}</td>
                                    <td>{run.ascension}</td>
                                    <td>{new Date(run.timeStarted).toLocaleDateString()}</td>
                                    <td><Link to={`/runs/${run.id}`}>Details</Link></td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}
        </div>
    );
}

export default RunListPage;
