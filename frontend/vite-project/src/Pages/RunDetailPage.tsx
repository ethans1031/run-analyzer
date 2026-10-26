import { useState, useEffect } from "react";
import { useParams, Link } from "react-router-dom";
import type { RunDetail } from "../Types/run";
import { getRun } from "../Api/runs";
import { formatCharacter, formatCard, formatRelic, formatDuration } from "../Utils/format";
import { useCatalog } from "../Context/context";
import { groupChoicesByFloor } from "../Utils/format";

function RunDetailPage() {
    const { id } = useParams<{ id: string }>();
    const [run, setRun] = useState<RunDetail | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [enlargedImage, setEnlargedImage] = useState<string | null>(null);
    const { getCardImage, getRelicImage } = useCatalog();

    useEffect(() => {
        async function load() {
            try {
                const data = await getRun(Number(id));
                setRun(data);
            } catch (err) {
                setError(err instanceof Error ? err.message : "Unknown error");
            } finally {
                setIsLoading(false);
            }
        }
        load();
    }, [id]);

    if (isLoading) return <div>Loading run...</div>;
    if (error) return <div>Error loading run: {error}</div>;
    if (!run) {
        return (
            <div>
                <Link to="/" className="back-link">← Back to runs</Link>
                <div className="empty-state">
                    <p>Run not found.</p>
                </div>
            </div>
        );
    }

    return (
        <div>
            <Link to="/" className="back-link">← Back to runs</Link>

            <div className="page-header">
                <h1>Run #{run.id}</h1>
                <span className={run.won ? "text-win" : "text-loss"} style={{ fontSize: "1.25rem", fontWeight: 600 }}>
                    {run.won ? "Victory" : "Defeat"}
                </span>
            </div>

            <section>
                <h2>Overview</h2>
                <div className="overview-grid">
                    <OverviewItem label="Character" value={formatCharacter(run.characterName)} />
                    <OverviewItem label="Ascension" value={run.ascension} />
                    <OverviewItem label="Floor Reached" value={run.floorReached} />
                    <OverviewItem label="Time" value={formatDuration(run.timeElapsed)} />
                    <OverviewItem label="Started" value={new Date(run.timeStarted).toLocaleString()} />
                    <OverviewItem label="Killed By" value={run.killedByEncounter ?? "N/A"} />
                    <OverviewItem label="Build" value={run.buildId} />
                    <OverviewItem label="Seed" value={run.seed} />
                </div>
            </section>

            <section>
                <h2>Card Choices ({run.cardChoices.length})</h2>
                {groupChoicesByFloor(run.cardChoices).map(group => (
                    <div key={`${group.act}-${group.floor}`} className="floor-group">
                        <div className="floor-group__header">
                            <span className="floor-group__title">
                                Act {group.act + 1}, Floor {group.floor + 1}
                            </span>
                            <span className="floor-group__source">{group.source}</span>
                        </div>
                        <div className="floor-cards">
                            {group.choices.map(choice => {
                                const imgUrl = getCardImage(choice.cardId);
                                return (
                                    <div
                                        key={choice.id}
                                        className={`floor-card ${choice.picked ? "floor-card--picked" : ""}`}
                                        onClick={() => imgUrl && setEnlargedImage(imgUrl)}
                                    >
                                        {imgUrl ? (
                                            <img src={imgUrl} alt={formatCard(choice.cardId)} />
                                        ) : (
                                            <div style={{ width: 140, height: 190, backgroundColor: "var(--color-bg-hover)", borderRadius: "var(--radius-sm)" }} />
                                        )}
                                        <div className="floor-card__label">{formatCard(choice.cardId)}</div>
                                    </div>
                                );
                            })}
                        </div>
                    </div>
                ))}
            </section>

            <section>
                <h2>Relic Choices ({run.relicChoices.length})</h2>
                <table>
                    <thead>
                        <tr>
                            <th></th>
                            <th>Act</th>
                            <th>Floor</th>
                            <th>Source</th>
                            <th>Relic</th>
                            <th>Picked?</th>
                        </tr>
                    </thead>
                    <tbody>
                        {run.relicChoices.map(choice => {
                            const imgUrl = getRelicImage(choice.relicId);
                            return (
                                <tr key={choice.id}>
                                    <td>
                                        {imgUrl ? (
                                            <img src={imgUrl} alt={formatRelic(choice.relicId)} className="relic-thumb" />
                                        ) : null}
                                    </td>
                                    <td>{choice.act + 1}</td>
                                    <td>{choice.floor}</td>
                                    <td>{choice.source}</td>
                                    <td>{formatRelic(choice.relicId)}</td>
                                    <td>
                                        <span className={choice.picked ? "text-win" : "muted"}>
                                            {choice.picked ? "Yes" : "No"}
                                        </span>
                                    </td>
                                </tr>
                            );
                        })}
                    </tbody>
                </table>
            </section>
            {enlargedImage && (
                <div className="lightbox" onClick={() => setEnlargedImage(null)}>
                    <img src={enlargedImage} alt="Enlarged card" onClick={e => e.stopPropagation()} />
                </div>
            )}
        </div>
    );
}

function OverviewItem({ label, value }: { label: string; value: string | number }) {
    return (
        <div className="overview-item">
            <div className="overview-item__label">{label}</div>
            <div className="overview-item__value">{value}</div>
        </div>
    );
}

export default RunDetailPage;