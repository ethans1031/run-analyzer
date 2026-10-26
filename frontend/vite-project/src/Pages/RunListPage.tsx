import { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import type { RunSummary } from "../Types/run";
import { getRuns } from "../Api/runs";
import { formatCharacter } from "../Utils/format";

function RunListPage() {
    const [runs, setRuns] = useState<RunSummary[]>([]);
    const [error, setError] = useState<string | null>(null);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        async function load() {
            try {
                const data = await getRuns();
                setRuns(data);
            } catch (err) {
                setError(err instanceof Error ? err.message : "Unknown error");
            } finally {
                setIsLoading(false);
            }
        }
        load();
    }, []);

    if (isLoading) return <div>Loading runs...</div>;
    if (error) return <div>Error loading runs: {error}</div>;

    if (runs.length === 0) {
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

    return (
        <div>
            <div className="page-header">
                <h1>Runs</h1>
                <Link to="/upload"><button>Upload New Run</button></Link>
            </div>
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
    );
}

export default RunListPage;