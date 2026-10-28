import { createContext } from "react";

export interface CatalogValue {
    getCardImage: (cardId: string) => string | null;
    getRelicImage: (relicId: string) => string | null;
    isLoading: boolean;
    error: string | null;
}

// null default so using the context outside its provider fails loudly in useCatalog().
export const CatalogContext = createContext<CatalogValue | null>(null);
