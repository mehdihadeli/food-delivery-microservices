export interface UpdateProductRequest {
    name: string;
    price: number;
    stock: number;
    restockThreshold: number;
    maxStockThreshold: number;
    status: number;
    productType: number;
    color: number;
    height: number;
    width: number;
    depth: number;
    size: string;
    categoryId: number;
    supplierId: number;
    brandId: number;
    description?: string | null;
}
