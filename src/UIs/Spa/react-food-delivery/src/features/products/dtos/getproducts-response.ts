import type {PageList} from "@shared/models/page-list.ts";
import type {Product} from "@features/products/models/product.ts";

export interface GetProductsResponse {
    productsPageList: PageList<Product>;
}
