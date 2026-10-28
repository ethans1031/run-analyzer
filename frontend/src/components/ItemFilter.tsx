import { useId, useState } from "react";

export interface ItemOption {
    id: string;
    name: string;
    detail?: string;
}

interface ItemFilterProps {
    label: string;
    placeholder: string;
    options: ItemOption[];
    selectedId: string | null;
    formatName: (id: string) => string;
    onChange: (id: string | null) => void;
}

/**
 * Text input with autocomplete suggestions (a native datalist) for picking one
 * card or relic. Typing a full name from the list selects it.
 *
 * The parent should pass key={selectedId} so the input text resets whenever the
 * selection changes from outside (e.g. arriving from a link with ?card=...).
 */
function ItemFilter({ label, placeholder, options, selectedId, formatName, onChange }: ItemFilterProps) {
    const [text, setText] = useState(selectedId ? formatName(selectedId) : "");
    const inputId = useId();
    const listId = useId();

    function handleInput(value: string) {
        setText(value);

        const trimmed = value.trim().toLowerCase();
        if (trimmed === "") {
            if (selectedId) onChange(null);
            return;
        }

        const match = options.find(o => o.name.toLowerCase() === trimmed);
        if (match && match.id !== selectedId) {
            onChange(match.id);
        }
    }

    return (
        <div className="filter-field">
            <label htmlFor={inputId}>{label}</label>
            <div className="filter-field__control">
                <input
                    id={inputId}
                    type="text"
                    list={listId}
                    value={text}
                    placeholder={placeholder}
                    autoComplete="off"
                    onChange={e => handleInput(e.target.value)}
                />
                {selectedId && (
                    <button type="button" onClick={() => onChange(null)} aria-label={`Clear ${label.toLowerCase()} filter`}>
                        Clear
                    </button>
                )}
            </div>
            <datalist id={listId}>
                {options.map(o => (
                    <option key={o.id} value={o.name}>{o.detail}</option>
                ))}
            </datalist>
        </div>
    );
}

export default ItemFilter;
