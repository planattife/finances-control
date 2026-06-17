export type Account = {
  id: number
  isActive: boolean
  name: string
}

export type AccountKpi = {
  accountId: number
  cardsCount: number
  transactionsCount: number
}

export type AccountDetails = {
  id: number
  name: string
  status: string
  createdAt: string
  lastUpdate: string
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

export type AccountKpiResponse = {
  data: AccountKpi
  errors: string[]
  httpStatusCode: number
}

export type AccountDetailsResponse = {
  data: AccountDetails
  errors: string[]
  httpStatusCode: number
}
