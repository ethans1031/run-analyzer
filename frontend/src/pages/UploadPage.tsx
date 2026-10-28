import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { uploadRuns } from "../api/runs";

function UploadPage() {
    const [files, setFiles] = useState<File[]>([]);
    const [status, setStatus] = useState<string>("");
    const [isUploading, setIsUploading] = useState(false);
    const navigate = useNavigate();

    async function handleSubmit(e: React.FormEvent) {
        e.preventDefault();
        if (files.length === 0) return;

        setStatus("");
        setIsUploading(true);

        try {
            const result = await uploadRuns(files);
            const successCount = result.uploaded.length;
            const failCount = result.failed.length;

            if (failCount === 0) {
                setStatus(`Uploaded ${successCount} run${successCount === 1 ? "" : "s"} successfully.`);
                setFiles([]);
                setTimeout(() => navigate("/"), 1000);
            } else {
                setStatus(
                    `Uploaded ${successCount} of ${successCount + failCount} runs. ` +
                    `Failed: ${result.failed.map(f => f.file).join(", ")}`
                );
                setFiles([]);
            }
        } catch (err) {
            setStatus(`Upload failed: ${err instanceof Error ? err.message : "unknown error"}`);
        } finally {
            setIsUploading(false);
        }
    }

    return (
        <div>
            <div className="page-header">
                <h1>Upload Runs</h1>
            </div>

            <form onSubmit={handleSubmit}>
                <div className="form-field">
                    <input
                        type="file"
                        accept=".run"
                        multiple
                        onChange={e => setFiles(Array.from(e.target.files ?? []))}
                        disabled={isUploading}
                    />
                </div>

                {files.length > 0 && (
                    <div>
                        <p className="muted">Selected {files.length} file{files.length === 1 ? "" : "s"}:</p>
                        <ul className="file-list">
                            {files.map((f, i) => (
                                <li key={`${f.name}-${i}`}>
                                    {f.name} ({f.size.toLocaleString()} bytes)
                                </li>
                            ))}
                        </ul>
                    </div>
                )}

                <button type="submit" disabled={files.length === 0 || isUploading}>
                    {isUploading ? "Uploading..." : `Upload ${files.length || ""} run${files.length === 1 ? "" : "s"}`}
                </button>
            </form>

            {status && <p className="upload-status">{status}</p>}

            <p className="upload-back">
                <Link to="/">← Back to runs</Link>
            </p>
        </div>
    );
}

export default UploadPage;
