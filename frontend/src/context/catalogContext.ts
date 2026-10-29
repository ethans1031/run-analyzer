import { createContext } from "react";

export interface CatalogValue {
    getCardImage: (cardId: string) => string | null;
    getRelicImage: (relicId: string) => string | null;
    isLoading: boolean;
    error: string | null;
}

export const CatalogContext = createContext<CatalogValue | null>(null);
