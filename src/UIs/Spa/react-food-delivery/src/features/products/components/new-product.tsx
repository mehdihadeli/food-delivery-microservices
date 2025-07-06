import { useState } from "react";
import { useNavigate } from "react-router-dom";
import type { CreateProductRequest } from "@features/products/dtos/create-product-request";
import { productApiService } from "../services/product-service";

const initialProduct: CreateProductRequest = {
    name: "",
    price: 0,
    stock: 0,
    restockThreshold: 0,
    maxStockThreshold: 0,
    status: 1,
    productType: 1,
    color: 1,
    height: 0,
    width: 0,
    depth: 0,
    size: "",
    categoryId: 0,
    supplierId: 0,
    brandId: 0,
    description: "",
};

export function NewProductForm() {
    const [product, setProduct] = useState<CreateProductRequest>(initialProduct);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const navigate = useNavigate();

    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        const { name, value, type } = e.target;
        setProduct((p) => ({
            ...p,
            [name]: type === "number" ? Number(value) : value,
        }));
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError(null);
        try {
            await productApiService.createProduct(product);
            navigate("/products");
        } catch (err: any) {
            setError(err?.message || "Error creating product");
        } finally {
            setLoading(false);
        }
    };

    return (
        <form onSubmit={handleSubmit} className="max-w-xl mx-auto py-8">
            <h1 className="text-2xl font-bold mb-4">Create New Product</h1>
            <input
                className="border p-2 mb-2 w-full"
                placeholder="Name"
                name="name"
                value={product.name}
                onChange={handleChange}
                required
            />
            <input
                className="border p-2 mb-2 w-full"
                placeholder="Price"
                name="price"
                type="number"
                value={product.price}
                onChange={handleChange}
                required
            />
            <input
                className="border p-2 mb-2 w-full"
                placeholder="Stock"
                name="stock"
                type="number"
                value={product.stock}
                onChange={handleChange}
                required
            />
            <input
                className="border p-2 mb-2 w-full"
                placeholder="Restock Threshold"
                name="restockThreshold"
                type="number"
                value={product.restockThreshold}
                onChange={handleChange}
                required
            />
            <input
                className="border p-2 mb-2 w-full"
                placeholder="Max Stock Threshold"
                name="maxStockThreshold"
                type="number"
                value={product.maxStockThreshold}
                onChange={handleChange}
                required
            />
            <input
                className="border p-2 mb-2 w-full"
                placeholder="Status"
                name="status"
                type="number"
                value={product.status}
                onChange={handleChange}
                required
            />
            <input
                className="border p-2 mb-2 w-full"
                placeholder="Product Type"
                name="productType"
                type="number"
                value={product.productType}
                onChange={handleChange}
                required
            />
            <input
                className="border p-2 mb-2 w-full"
                placeholder="Color"
                name="color"
                type="number"
                value={product.color}
                onChange={handleChange}
                required
            />
            <input
                className="border p-2 mb-2 w-full"
                placeholder="Height"
                name="height"
                type="number"
                value={product.height}
                onChange={handleChange}
                required
            />
            <input
                className="border p-2 mb-2 w-full"
                placeholder="Width"
                name="width"
                type="number"
                value={product.width}
                onChange={handleChange}
                required
            />
            <input
                className="border p-2 mb-2 w-full"
                placeholder="Depth"
                name="depth"
                type="number"
                value={product.depth}
                onChange={handleChange}
                required
            />
            <input
                className="border p-2 mb-2 w-full"
                placeholder="Size"
                name="size"
                value={product.size}
                onChange={handleChange}
            />
            <input
                className="border p-2 mb-2 w-full"
                placeholder="Category Id"
                name="categoryId"
                type="number"
                value={product.categoryId}
                onChange={handleChange}
                required
            />
            <input
                className="border p-2 mb-2 w-full"
                placeholder="Supplier Id"
                name="supplierId"
                type="number"
                value={product.supplierId}
                onChange={handleChange}
                required
            />
            <input
                className="border p-2 mb-2 w-full"
                placeholder="Brand Id"
                name="brandId"
                type="number"
                value={product.brandId}
                onChange={handleChange}
                required
            />
            <textarea
                className="border p-2 mb-2 w-full"
                placeholder="Description"
                name="description"
                value={product.description || ""}
                onChange={handleChange}
            />
            <button
                disabled={loading}
                type="submit"
                className="bg-green-600 text-white px-4 py-2 rounded"
            >
                {loading ? "Creating..." : "Create Product"}
            </button>
            {error && <div className="text-red-600 mt-2">{error}</div>}
        </form>
    );
}