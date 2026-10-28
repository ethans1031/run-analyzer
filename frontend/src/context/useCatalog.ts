import { useContext } from "react";
import { CatalogContext, type CatalogValue } from "./catalogContext";

export function useCatalog(): CatalogValue {
    const context = useContext(CatalogContext);
    if (!context) {
        throw new Error("useCatalog must be used within a CatalogProvider");
    }
    return context;
}
