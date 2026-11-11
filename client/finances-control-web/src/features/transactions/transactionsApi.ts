import { axiosClient } from '../../api/axiosClient'
import type {
  PagedTransactionResponse,
  PaymentMethodCode,
  TransactionDetailsResponse,
  TransactionTypeCode,
} from '../../types/transaction'

export type GetTransactionsParams = {
  pageSize: number
  pageNumber: number
}

export type UpdateTransactionRequest = {
  newAmount: number
  newCategoryId: number
  newDescription: string | null
  hasDescriptionUpdate: boolean
}

export type CreateTransactionRequest = {
  amount: number
  paymentMethod: PaymentMethodCode
  transactionType: TransactionTypeCode
  categoryId: number
  accountId: number
  cardId?: number | null
  bankId?: number | null
  transactionDate: string
  description?: string | null
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

export async function createTransaction(
  accountId: number,
  payload: CreateTransactionRequest,
): Promise<void> {
  await axiosClient.post(`/api/Transaction/${encodeURIComponent(accountId)}`, payload)
}

export async function getTransactionDetails(
  accountId: number,
  transactionId: number,
): Promise<TransactionDetailsResponse> {
  const response = await axiosClient.get<TransactionDetailsResponse>(
    `/api/Transaction/${encodeURIComponent(accountId)}/${encodeURIComponent(transactionId)}`,
  )

  return response.data
}

export async function deleteTransaction(accountId: number, transactionId: number): Promise<void> {
  await axiosClient.delete(
    `/api/Transaction/${encodeURIComponent(accountId)}/${encodeURIComponent(transactionId)}`,
  )
}

export async function updateTransaction(
  accountId: number,
  transactionId: number,
  payload: UpdateTransactionRequest,
): Promise<void> {
  await axiosClient.patch(
    `/api/Transaction/${encodeURIComponent(accountId)}/${encodeURIComponent(transactionId)}`,
    payload,
  )
}
