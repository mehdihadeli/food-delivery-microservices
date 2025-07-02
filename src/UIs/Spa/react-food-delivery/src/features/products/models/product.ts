export interface ProductImage {
  id: number;
  productId: number;
  imageUrl: string;
  isMain: boolean;
}

export interface Product {
  id: number;
  name: string;
  price: number;
  stock: number;
  restockThreshold: number;
  maxStockThreshold: number;
  status: number;
  productType: number;
  productColor: number;
  height: number;
  width: number;
  depth: number;
  size: string;
  categoryId: number;
  categoryName: string;
  supplierId: number;
  supplierName: string;
  brandId: number;
  brandName: string;
  description?: string;
  images?: ProductImage[];
}