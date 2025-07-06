import { useState, useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import type { UpdateProductRequest } from "@features/products/dtos/update-product-request";
import { productApiService } from "../services/product-service";

const initialProduct: UpdateProductRequest = {
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

export function EditProductForm() {
    const navigate = useNavigate();
    const params = useParams();
    const [product, setProduct] = useState<UpdateProductRequest>(initialProduct);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchProduct = async () => {
            try {
                const { product: fetched } = await productApiService.getProductById(Number(params.id));
                setProduct({
                    name: fetched.name,
                    price: fetched.price,
                    stock: fetched.stock,
                    restockThreshold: fetched.restockThreshold,
                    maxStockThreshold: fetched.maxStockThreshold,
                    status: fetched.status,
                    productType: fetched.productType,
                    color: fetched.productColor ?? fetched.productColor ?? 1,
                    height: fetched.height,
                    width: fetched.width,
                    depth: fetched.depth,
                    size: fetched.size,
                    categoryId: fetched.categoryId,
                    supplierId: fetched.supplierId,
                    brandId: fetched.brandId,
                    description: fetched.description ?? "",
                });
            } catch (err: any) {
                setError("Product not found.");
            } finally {
                setLoading(false);
            }
        };
        if (params.id) fetchProduct();
    }, [params.id]);

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
            await productApiService.updateProduct(Number(params.id), product);
            navigate("/products");
        } catch (err: any) {
            setError("Could not update product.");
        } finally {
            setLoading(false);
        }
    };

    if (loading) return <div className="min-h-[20vh] flex items-center justify-center">Loading...</div>;
    if (error) return <div className="text-red-600 text-center">{error}</div>;

    return (
        <form onSubmit={handleSubmit} className="max-w-xl mx-auto py-8">
            <h1 className="text-2xl font-bold mb-4">Edit Product</h1>
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
                className="bg-blue-600 text-white px-4 py-2 rounded"
            >
                {loading ? "Saving..." : "Save Changes"}
            </button>
            {error && <div className="text-red-600 mt-2">{error}</div>}
        </form>
    );
}