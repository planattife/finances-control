import { axiosClient } from '../../api/axiosClient'
import type { AccountKpiResponse, PagedAccountResponse } from '../../types/account'

export type GetAccountsParams = {
  pageSize: number
  pageNumber: number
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

export async function getAccountKpi(accountId: number): Promise<AccountKpiResponse> {
  const response = await axiosClient.get<AccountKpiResponse>(
    `/api/Account/${encodeURIComponent(accountId)}/account-kpi`,
  )

  return response.data
}
