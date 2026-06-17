import { axiosClient } from '../../api/axiosClient'
import type { PagedAccountResponse } from '../../types/account'

export type GetAccountsParams = {
  pageSize: number
  pageNumber: number
}

export type CreateAccountRequest = {
  name: string
}

export async function getAccounts({
  pageNumber,
  pageSize,
}: GetAccountsParams): Promise<PagedAccountResponse> {
  const response = await axiosClient.get<PagedAccountResponse>('/api/Account', {
    params: {
      PageSize: pageSize,
      PageNumber: pageNumber,
    },
  })

  return response.data
}

export async function createAccount(payload: CreateAccountRequest): Promise<void> {
  await axiosClient.post('/api/Account', payload)
}
