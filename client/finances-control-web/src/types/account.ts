export type Account = {
  id: number
  name: string
}

export type PagedAccountResponse = {
  errors: string[]
  httpStatusCode: number
  data: Account[]
  totalCount: number
  totalPages: number
  currentPage: number
  hasNextPage: boolean
  hasPrevPage: boolean
}
