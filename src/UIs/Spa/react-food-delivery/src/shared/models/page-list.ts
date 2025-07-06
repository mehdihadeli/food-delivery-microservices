export interface PageList<T> {
    currentPageSize: number;
    currentStartIndex: number;
    currentEndIndex: number;
    totalPages: number;
    hasPrevious: boolean;
    hasNext: boolean;
    items: T[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
}