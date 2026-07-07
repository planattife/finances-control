import { axiosClient } from '../../api/axiosClient'
import type { PagedTransactionResponse } from '../../types/transaction'

export type GetTransactionsParams = {
  pageSize: number
  pageNumber: number
}

export async function getTransactions(
  accountId: number,
  { pageNumber, pageSize }: GetTransactionsParams,
): Promise<PagedTransactionResponse> {
  const response = await axiosClient.get<PagedTransactionResponse>(
    `/api/Transaction/${encodeURIComponent(accountId)}`,
    {
      params: {
        PageSize: pageSize,
        PageNumber: pageNumber,
      },
    },
  )

  return response.data
}
