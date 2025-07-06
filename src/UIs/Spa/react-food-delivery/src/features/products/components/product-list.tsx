import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import type { ProblemDetails } from "@shared/models/problem-details";
import type { Product } from "@features/products/models/product";
import type { PageList } from "@shared/models/page-list";
import { productApiService } from "../services/product-service";

const DEFAULT_PAGE_SIZE = 12;

export const ProductList = () => {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<ProblemDetails | null>(null);
  const [page, setPage] = useState(1);
  const [pageList, setPageList] = useState<PageList<Product> | null>(null);

  const loadProducts = async (pageNumber: number, pageSize: number) => {
    setLoading(true);
    setError(null);
    try {
      const { productsPageList } = await productApiService.getProducts(pageNumber, pageSize);
      setProducts(productsPageList.items);
      setPageList(productsPageList);
    } catch (err) {
      setError(err as ProblemDetails);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadProducts(page, DEFAULT_PAGE_SIZE);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [page]);

  const handleDelete = async (id: number) => {
    try {
      await productApiService.deleteProduct(id);
      loadProducts(page, DEFAULT_PAGE_SIZE);
    } catch (err) {
      setError(err as ProblemDetails);
    }
  };

  const handlePrevious = () => {
    if (page > 1) setPage(page - 1);
  };

  const handleNext = () => {
    if (pageList && page < pageList.totalPages) setPage(page + 1);
  };

  if (loading) return <div className="min-h-[40vh] flex items-center justify-center">Loading products...</div>;
  if (error) return <div className="min-h-[40vh] flex items-center justify-center text-red-500">Error: {error.title}</div>;

  return (
      <div className="container mx-auto px-4 py-8">
        <div className="flex justify-between items-center mb-8">
          <h1 className="text-3xl font-extrabold tracking-tight text-gray-900">🍔 Products</h1>
          <Link
              to="/products/new"
              className="bg-green-500 text-white px-6 py-2 rounded-lg font-semibold shadow-lg hover:bg-green-600 transition"
          >
            + Add Product
          </Link>
        </div>

        {products.length === 0 ? (
            <div className="text-center text-gray-500 mt-24 text-lg">No products found.</div>
        ) : (
            <div className="grid gap-8 grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4">
              {products.map((product) => (
                  <div
                      key={product.id}
                      className="relative bg-white rounded-xl shadow-md hover:shadow-2xl transition p-4 flex flex-col"
                  >
                    <div className="relative">
                      {product.images?.[0] ? (
                          <img
                              src={product.images[0].imageUrl}
                              alt={product.name}
                              className="w-full h-40 object-cover rounded-lg border"
                          />
                      ) : (
                          <div className="w-full h-40 flex items-center justify-center bg-gray-100 rounded-lg text-gray-300 text-4xl">
                            <span className="material-icons">fastfood</span>
                          </div>
                      )}

                      {product.stock <= product.restockThreshold && (
                          <span className="absolute top-2 left-2 bg-yellow-400 text-gray-700 text-xs font-semibold py-1 px-3 rounded-full shadow">
                    Low Stock
                  </span>
                      )}
                      {product.status !== 1 && (
                          <span className="absolute top-2 right-2 bg-gray-400 text-white text-xs font-semibold py-1 px-3 rounded-full shadow">
                    Inactive
                  </span>
                      )}
                    </div>
                    <div className="mt-4 mb-2 flex-1">
                      <h2 className="text-lg font-bold mb-1 truncate">{product.name}</h2>
                      <div className="text-sm text-gray-600 mb-1 truncate">{product.categoryName}</div>
                      <div className="flex items-center justify-between">
                        <span className="text-green-600 font-bold text-xl">${product.price.toFixed(2)}</span>
                        <span className="text-xs text-gray-500">{product.brandName}</span>
                      </div>
                      <div className="mt-2 text-xs text-gray-500 truncate">
                        {product.description
                            ? product.description.slice(0, 60) + (product.description.length > 60 ? "..." : "")
                            : "No description"}
                      </div>
                      <div className="flex gap-1 mt-2">
                  <span className="bg-blue-100 text-blue-700 rounded px-2 py-0.5 text-xs">
                    Size: {product.size || `${product.width}x${product.height}x${product.depth}`}
                  </span>
                        <span className="bg-gray-100 text-gray-700 rounded px-2 py-0.5 text-xs">
                    Stock: {product.stock}
                  </span>
                      </div>
                    </div>
                    <div className="flex gap-2 mt-auto">
                      <Link
                          to={`/products/details/${product.id}`}
                          className="w-1/2 py-2 bg-amber-500 hover:bg-amber-600 text-white rounded-lg font-medium text-sm text-center transition"
                      >
                        Details
                      </Link>

                      <Link
                          to={`/products/edit/${product.id}`}
                          className="w-1/2 py-2 bg-blue-500 hover:bg-blue-600 text-white rounded-lg font-medium text-sm text-center transition"
                      >
                        Edit
                      </Link>
                      <button
                          onClick={() => handleDelete(product.id)}
                          className="w-1/2 py-2 bg-red-500 hover:bg-red-600 text-white rounded-lg font-medium text-sm transition"
                      >
                        Delete
                      </button>
                    </div>
                  </div>
              ))}
            </div>
        )}

        {pageList && pageList.totalPages > 1 && (
            <div className="flex justify-center items-center space-x-4 mt-8">
              <button
                  onClick={handlePrevious}
                  disabled={!pageList.hasPrevious}
                  className="bg-gray-200 disabled:bg-gray-100 px-4 py-2 rounded font-semibold"
              >
                Previous
              </button>
              <span className="font-semibold">
            Page {pageList.pageNumber} of {pageList.totalPages}
          </span>
              <button
                  onClick={handleNext}
                  disabled={!pageList.hasNext}
                  className="bg-gray-200 disabled:bg-gray-100 px-4 py-2 rounded font-semibold"
              >
                Next
              </button>
            </div>
        )}
      </div>
  );
};