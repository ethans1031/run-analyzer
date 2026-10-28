import { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import type { ChoiceStats, ChoiceStatsResponse } from "../types/stats";
import { getCardStats, getRelicStats, getStats } from "../api/runs";
import {
    formatCard,
    formatRelic,
    formatCharacter,
    formatPercent,
    formatOptionalPercent,
    formatSignedPercent,
} from "../utils/format";
import { useCatalog } from "../context/useCatalog";

type Kind = "card" | "relic";

type SortKey = "name" | "timesOffered" | "timesPicked" | "pickRate" | "runsPicked" | "winRate" | "winRateDelta";

interface SortState {
    key: SortKey;
    direction: "asc" | "desc";
}

// Everything that differs between the card page and the relic page.
const PAGE_CONFIG = {
    card: {
        title: "Card Stats",
        nounPlural: "cards",
        fetchStats: getCardStats,
        formatName: formatCard,
        nameLabel: "Card",
        offeredLabel: "Offered",
        pickedLabel: "Picked",
        rateLabel: "Pick Rate",
        runsLabel: "Runs Picked",
        intro:
            "Pick rate counts card reward screens after combat, elite, and boss fights. " +
            "Win rate counts every run where you took the card, from any source.",
    },
    relic: {
        title: "Relic Stats",
        nounPlural: "relics",
        fetchStats: getRelicStats,
        formatName: formatRelic,
        nameLabel: "Relic",
        offeredLabel: "In Shop",
        pickedLabel: "Bought",
        rateLabel: "Buy Rate",
        runsLabel: "Runs With",
        intro:
            "Buy rate counts shop offers, the only relic choice the run file records. " +
            "Win rate counts every run where you had the relic.",
    },
} as const;

const DEFAULT_MIN_RUNS = 5;

function sortValue(item: ChoiceStats, key: SortKey, formatName: (id: string) => string): string | number | null {
    return key === "name" ? formatName(item.id) : item[key];
}

// Sorts by the chosen column; missing values (e.g. no win rate) always go last.
function sortItems(items: ChoiceStats[], sort: SortState, formatName: (id: string) => string): ChoiceStats[] {
    const sign = sort.direction === "asc" ? 1 : -1;
    return [...items].sort((a, b) => {
        const av = sortValue(a, sort.key, formatName);
        const bv = sortValue(b, sort.key, formatName);
        if (av === null && bv === null) return 0;
        if (av === null) return 1;
        if (bv === null) return -1;
        if (typeof av === "string" && typeof bv === "string") return sign * av.localeCompare(bv);
        return sign * ((av as number) - (bv as number));
    });
}

interface SortableHeaderProps {
    sortKey: SortKey;
    label: string;
    sort: SortState;
    onSort: (key: SortKey) => void;
}

function SortableHeader({ sortKey, label, sort, onSort }: SortableHeaderProps) {
    const isActive = sort.key === sortKey;
    const arrow = isActive ? (sort.direction === "asc" ? " ↑" : " ↓") : "";
    return (
        <th aria-sort={isActive ? (sort.direction === "asc" ? "ascending" : "descending") : undefined}>
            <button type="button" className="sort-button" onClick={() => onSort(sortKey)}>
                {label}{arrow}
            </button>
        </th>
    );
}

function deltaClass(delta: number | null): string {
    if (delta === null || delta === 0) return "muted";
    return delta > 0 ? "text-win" : "text-loss";
}

function ChoiceStatsPage({ kind }: { kind: Kind }) {
    const config = PAGE_CONFIG[kind];
    const { getCardImage, getRelicImage } = useCatalog();
    const getImage = kind === "card" ? getCardImage : getRelicImage;

    const [data, setData] = useState<ChoiceStatsResponse | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [characters, setCharacters] = useState<string[]>([]);
    const [character, setCharacter] = useState("");
    const [search, setSearch] = useState("");
    const [minRuns, setMinRuns] = useState(DEFAULT_MIN_RUNS);
    const [sort, setSort] = useState<SortState>({ key: "runsPicked", direction: "desc" });
    const [enlargedImage, setEnlargedImage] = useState<string | null>(null);

    useEffect(() => {
        let cancelled = false;  // ignore responses for a character the user has already changed

        async function load() {
            try {
                const result = await config.fetchStats(character || null);
                if (!cancelled) {
                    setData(result);
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
    }, [config, character]);

    useEffect(() => {
        async function loadCharacters() {
            try {
                const stats = await getStats();
                setCharacters(stats.byCharacter.map(c => c.character));
            } catch {
                // The character filter is optional; the page works without it.
            }
        }
        loadCharacters();
    }, []);

    function handleSort(key: SortKey) {
        setSort(current =>
            current.key === key
                ? { key, direction: current.direction === "asc" ? "desc" : "asc" }
                : { key, direction: key === "name" ? "asc" : "desc" }
        );
    }

    if (isLoading) return <div>Loading {config.nounPlural}...</div>;
    if (error) return <div>Error loading {config.nounPlural}: {error}</div>;

    if (!data || data.runCount === 0) {
        return (
            <div>
                <h1>{config.title}</h1>
                <div className="empty-state">
                    <p>No runs uploaded yet.</p>
                    <Link to="/upload"><button>Upload Runs</button></Link>
                </div>
            </div>
        );
    }

    const query = search.trim().toLowerCase();
    const visible = sortItems(
        data.items.filter(item =>
            item.runsPicked >= minRuns &&
            (query === "" || config.formatName(item.id).toLowerCase().includes(query))
        ),
        sort,
        config.formatName
    );

    return (
        <div>
            <h1>{config.title}</h1>
            <p className="page-intro">{config.intro}</p>

            <div className="filter-bar">
                <div className="filter-field">
                    <label htmlFor="item-search">Search</label>
                    <input
                        id="item-search"
                        type="text"
                        value={search}
                        placeholder={`Filter ${config.nounPlural}`}
                        onChange={e => setSearch(e.target.value)}
                    />
                </div>
                <div className="filter-field">
                    <label htmlFor="character-filter">Character</label>
                    <select id="character-filter" value={character} onChange={e => setCharacter(e.target.value)}>
                        <option value="">All characters</option>
                        {characters.map(c => (
                            <option key={c} value={c}>{formatCharacter(c)}</option>
                        ))}
                    </select>
                </div>
                <div className="filter-field">
                    <label htmlFor="min-runs">Min. Runs</label>
                    <input
                        id="min-runs"
                        type="number"
                        min={0}
                        value={minRuns}
                        onChange={e => setMinRuns(Math.max(0, Number(e.target.value) || 0))}
                    />
                </div>
            </div>

            <p className="table-meta">
                Showing {visible.length} of {data.items.length} {config.nounPlural}. Overall win rate is{" "}
                {formatPercent(data.baselineWinRate)} across {data.runCount} run{data.runCount === 1 ? "" : "s"}.
                {minRuns > 0 && ` Hiding ${config.nounPlural} picked in fewer than ${minRuns} runs.`}
            </p>

            {visible.length === 0 ? (
                <div className="empty-state">
                    <p>No {config.nounPlural} match these filters. Try lowering the minimum runs.</p>
                </div>
            ) : (
                <div className="table-scroll">
                    <table>
                        <thead>
                            <tr>
                                <th></th>
                                <SortableHeader sortKey="name" label={config.nameLabel} sort={sort} onSort={handleSort} />
                                <SortableHeader sortKey="timesOffered" label={config.offeredLabel} sort={sort} onSort={handleSort} />
                                <SortableHeader sortKey="timesPicked" label={config.pickedLabel} sort={sort} onSort={handleSort} />
                                <SortableHeader sortKey="pickRate" label={config.rateLabel} sort={sort} onSort={handleSort} />
                                <SortableHeader sortKey="runsPicked" label={config.runsLabel} sort={sort} onSort={handleSort} />
                                <SortableHeader sortKey="winRate" label="Win Rate" sort={sort} onSort={handleSort} />
                                <SortableHeader sortKey="winRateDelta" label="Vs. Overall" sort={sort} onSort={handleSort} />
                            </tr>
                        </thead>
                        <tbody>
                            {visible.map(item => {
                                const imgUrl = getImage(item.id);
                                const name = config.formatName(item.id);
                                return (
                                    <tr key={item.id}>
                                        <td>
                                            {imgUrl ? (
                                                <img
                                                    src={imgUrl}
                                                    alt={name}
                                                    className={kind === "card" ? "card-thumb" : "relic-thumb"}
                                                    onClick={() => setEnlargedImage(imgUrl)}
                                                />
                                            ) : null}
                                        </td>
                                        <td>
                                            <Link
                                                to={`/?${kind}=${encodeURIComponent(item.id)}`}
                                                title={`Show runs where you picked ${name}`}
                                            >
                                                {name}
                                            </Link>
                                        </td>
                                        <td>{item.timesOffered}</td>
                                        <td>{item.timesPicked}</td>
                                        <td>{formatOptionalPercent(item.pickRate)}</td>
                                        <td>{item.runsPicked}</td>
                                        <td>{formatOptionalPercent(item.winRate)}</td>
                                        <td>
                                            <span className={deltaClass(item.winRateDelta)}>
                                                {formatSignedPercent(item.winRateDelta)}
                                            </span>
                                        </td>
                                    </tr>
                                );
                            })}
                        </tbody>
                    </table>
                </div>
            )}

            {enlargedImage && (
                <div className="lightbox" onClick={() => setEnlargedImage(null)}>
                    <img src={enlargedImage} alt="Enlarged" onClick={e => e.stopPropagation()} />
                </div>
            )}
        </div>
    );
}

export default ChoiceStatsPage;
