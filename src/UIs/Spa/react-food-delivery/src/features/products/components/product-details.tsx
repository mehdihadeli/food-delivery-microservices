import { useEffect, useState } from "react";
import { useParams, Link, useNavigate } from "react-router-dom";
import { productApiService } from "../services/product-service";
import type { Product } from "@features/products/models/product";
import type { ProblemDetails } from "@shared/models/problem-details";

export const ProductDetails = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const [product, setProduct] = useState<Product | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<ProblemDetails | null>(null);

    useEffect(() => {
        if (!id) return;
        setLoading(true);
        productApiService
            .getProductById(Number(id))
            .then(({ product }) => setProduct(product))
            .catch((e: ProblemDetails) => setError(e))
            .finally(() => setLoading(false));
    }, [id]);

    if (loading) {
        return <div className="min-h-[40vh] flex items-center justify-center">Loading product details...</div>;
    }
    if (error) {
        return <div className="min-h-[40vh] flex items-center justify-center text-red-500">{error.title}</div>;
    }
    if (!product) {
        return <div className="text-center mt-20 text-gray-500">Product not found.</div>;
    }

    return (
        <div className="container mx-auto px-4 py-8 flex flex-col items-center">
            <div className="w-full max-w-2xl bg-white rounded-2xl shadow-xl p-8 flex flex-col sm:flex-row gap-6">
                <div className="flex-shrink-0 w-full sm:w-56 flex justify-center items-center">
                    {product.images?.[0] ? (
                        <img
                            src={product.images[0].imageUrl}
                            alt={product.name}
                            className="rounded-lg border w-full h-48 object-cover"
                        />
                    ) : (
                        <div className="w-full h-48 flex items-center justify-center bg-gray-100 rounded-lg text-gray-300 text-6xl">
                            <span className="material-icons">fastfood</span>
                        </div>
                    )}
                </div>

                <div className="flex-1 flex flex-col">
                    <h1 className="text-3xl font-extrabold mb-2 text-gray-900">
                        {product.name}
                        <span className="ml-2 text-xs align-top font-medium bg-gray-200 text-gray-700 px-2 py-0.5 rounded">{product.categoryName}</span>
                    </h1>
                    <div className="text-lg font-semibold text-green-600 mb-2">${product.price.toFixed(2)}</div>
                    <div className="mb-4 text-gray-600">{product.description || "No description provided."}</div>
                    <div className="grid grid-cols-2 gap-3 text-sm">
                        <span className="font-semibold text-gray-500">Brand:</span>
                        <span>{product.brandName}</span>
                        <span className="font-semibold text-gray-500">Supplier:</span>
                        <span>{product.supplierName}</span>
                        <span className="font-semibold text-gray-500">Stock:</span>
                        <span>
              {product.stock}
                            {product.stock <= product.restockThreshold && (
                                <span className="ml-2 text-xs px-2 py-0.5 rounded-full bg-yellow-400 text-gray-800 font-semibold">Low</span>
                            )}
            </span>
                        <span className="font-semibold text-gray-500">Status:</span>
                        <span>
              {product.status === 1 ? (
                  <span className="px-2 py-0.5 rounded-full bg-green-200 text-green-800">Active</span>
              ) : (
                  <span className="px-2 py-0.5 rounded-full bg-gray-200 text-gray-800">Inactive</span>
              )}
            </span>
                        <span className="font-semibold text-gray-500">Color:</span>
                        <span>{product.productColor ?? "-"}</span>
                        <span className="font-semibold text-gray-500">Size:</span>
                        <span>
              {product.size || `${product.width}x${product.height}x${product.depth}`}
            </span>
                    </div>
                    <div className="flex gap-2 mt-6">
                        <button
                            onClick={() => navigate(-1)}
                            className="flex-1 py-2 bg-gray-100 hover:bg-gray-200 border text-gray-700 rounded-lg font-semibold"
                        >
                            Back
                        </button>
                        <Link
                            to={`/products/edit/${product.id}`}
                            className="flex-1 py-2 bg-blue-500 hover:bg-blue-600 text-white rounded-lg font-semibold text-center"
                        >
                            Edit
                        </Link>
                    </div>
                </div>
            </div>
        </div>
    );
};