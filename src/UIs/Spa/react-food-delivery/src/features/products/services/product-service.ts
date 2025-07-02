import {ApiClient} from "@shared/services/api-client";
import type {Product} from "../models/product";
import type {GetProductsResponse} from "@features/products/dtos/getproducts-response.ts";
import type {GetProductByIdResponse} from "@features/products/dtos/getproductbyid-response.ts";
import type {CreateProductRequest} from "@features/products/dtos/create-product-request.ts";
import type {UpdateProductRequest} from "@features/products/dtos/update-product-request.ts";

export class ProductApiService extends ApiClient {
    constructor() {
        let spaBffAddress = import.meta.env.VITE_GATEWAY_API_BASE_URL;
        super(`${spaBffAddress}/api/v1/catalogs/products`);
    }

    async getProducts(pageNumber: number = 1, pageSize: number = 10): Promise<GetProductsResponse> {
        const params = new URLSearchParams({ PageNumber: String(pageNumber), PageSize: String(pageSize) });
        return this.get<GetProductsResponse>(`?${params.toString()}`);
    }

    async getProductById(id: number): Promise<GetProductByIdResponse> {
        return this.get<GetProductByIdResponse>(`/${id}`);
    }

    async createProduct(product: CreateProductRequest): Promise<Product> {
        return this.post<Product>("", product);
    }

    async updateProduct(id: number, product: UpdateProductRequest): Promise<Product> {
        return this.put<Product>(`/${id}`, product);
    }

    async deleteProduct(id: number): Promise<void> {
        return this.delete<void>(`/${id}`);
    }

    async searchProducts(query: string): Promise<Product[]> {
        return this.get<Product[]>(`/search?q=${encodeURIComponent(query)}`);
    }
}

// Singleton instance
export const productApiService = new ProductApiService();
