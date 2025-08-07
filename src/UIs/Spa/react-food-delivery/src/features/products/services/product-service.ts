import {ApiClient} from "@shared/services/api-client";
import type {Product} from "../models/product";
import type {GetProductsResponse} from "@features/products/dtos/getproducts-response.ts";
import type {GetProductByIdResponse} from "@features/products/dtos/getproductbyid-response.ts";
import type {CreateProductRequest} from "@features/products/dtos/create-product-request.ts";
import type {UpdateProductRequest} from "@features/products/dtos/update-product-request.ts";

export class ProductApiService extends ApiClient {
    private productsV1Base: string;
    constructor() {
        let spaBffAddress = "/gateway/spa-bff/catalogs";
        super(spaBffAddress);
        this.productsV1Base="/api/v1/products";
    }

    async getProducts(pageNumber: number = 1, pageSize: number = 10): Promise<GetProductsResponse> {
        const params = new URLSearchParams({PageNumber: String(pageNumber), PageSize: String(pageSize)});
        return this.get<GetProductsResponse>(`${this.productsV1Base}?${params.toString()}`);
    }

    async getProductById(id: number): Promise<GetProductByIdResponse> {
        return this.get<GetProductByIdResponse>(`${this.productsV1Base}/${id}`);
    }

    async createProduct(product: CreateProductRequest): Promise<Product> {
        return this.post<Product>(this.productsV1Base, product);
    }

    async updateProduct(id: number, product: UpdateProductRequest): Promise<Product> {
        return this.put<Product>(`${this.productsV1Base}/${id}`, product);
    }

    async deleteProduct(id: number): Promise<void> {
        return this.delete<void>(`${this.productsV1Base}/${id}`);
    }

    async searchProducts(query: string): Promise<Product[]> {
        return this.get<Product[]>(`${this.productsV1Base}/search?q=${encodeURIComponent(query)}`);
    }
}

// Singleton instance
export const productApiService = new ProductApiService();
