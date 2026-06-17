import { axiosClient } from '../../api/axiosClient'
import type {
  AccountDetailsResponse,
  AccountKpiResponse,
  PagedAccountResponse,
} from '../../types/account'

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

export async function getAccountKpi(accountId: number): Promise<AccountKpiResponse> {
  const response = await axiosClient.get<AccountKpiResponse>(
    `/api/Account/${encodeURIComponent(accountId)}/account-kpi`,
  )

  return response.data
}

export async function getAccountDetails(accountId: number): Promise<AccountDetailsResponse> {
  const response = await axiosClient.get<AccountDetailsResponse>(
    `/api/Account/${encodeURIComponent(accountId)}`,
  )

  return response.data
}
