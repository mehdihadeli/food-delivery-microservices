export interface CreateProductRequest {
    name: string;
    price: number;
    stock: number;
    restockThreshold: number;
    maxStockThreshold: number;
    status: number; // ProductStatus enum value
    productType: number; // ProductType enum value
    color: number; // ProductColor enum value
    height: number;
    width: number;
    depth: number;
    size: string;
    categoryId: number;
    supplierId: number;
    brandId: number;
    description?: string | null;
    // Optionally: images?: CreateProductImageRequest[]
}
