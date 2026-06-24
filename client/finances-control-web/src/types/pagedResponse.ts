export type PagedResponse<TData> = {
  errors: string[]
  httpStatusCode: number
  data: TData[]
  totalCount: number
  totalPages: number
  currentPage: number
  hasNextPage: boolean
  hasPrevPage: boolean
}
