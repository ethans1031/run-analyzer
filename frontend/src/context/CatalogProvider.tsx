import { useEffect, useState } from "react";
import type { ReactNode } from "react";
import { CatalogContext, type CatalogValue } from "./catalogContext";

const SPIRE_CODEX_BASE = "https://spire-codex.com";

function normalizeId(id: string): string {
    return id.replace(/^(CARD|RELIC)\./, "");
}

export function CatalogProvider({ children }: { children: ReactNode }) {
    const [cardImages, setCardImages] = useState<Map<string, string>>(new Map());
    const [relicImages, setRelicImages] = useState<Map<string, string>>(new Map());
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        async function load() {
            try {
                const [cardsResp, relicsResp] = await Promise.all([
                    fetch(`${SPIRE_CODEX_BASE}/api/cards`),
                    fetch(`${SPIRE_CODEX_BASE}/api/relics`),
                ]);

                if (!cardsResp.ok || !relicsResp.ok) {
                    throw new Error("Failed to load catalog data.");
                }

                const cards = await cardsResp.json();
                const relics = await relicsResp.json();

                const cardMap = new Map<string, string>();
                for (const card of cards) {
                    if (card.id && card.image_url_card) {
                        cardMap.set(card.id, card.image_url_card);
                    }
                }

                const relicMap = new Map<string, string>();
                for (const relic of relics) {
                    if (relic.id && relic.image_url) {
                        relicMap.set(relic.id, `${SPIRE_CODEX_BASE}${relic.image_url}`);
                    }
                }

                setCardImages(cardMap);
                setRelicImages(relicMap);
            } catch (err) {
                setError(err instanceof Error ? err.message : "Unknown error");
            } finally {
                setIsLoading(false);
            }
        }
        load();
    }, []);

    const value: CatalogValue = {
        getCardImage: (id) => cardImages.get(normalizeId(id)) ?? null,
        getRelicImage: (id) => relicImages.get(normalizeId(id)) ?? null,
        isLoading,
        error,
    };

    return (
        <CatalogContext.Provider value={value}>
            {children}
        </CatalogContext.Provider>
    );
}
