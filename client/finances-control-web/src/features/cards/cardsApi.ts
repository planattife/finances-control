import { axiosClient } from '../../api/axiosClient'
import type { CardNetwork, PagedCardResponse } from '../../types/card'

export type GetCardsParams = {
  pageSize: number
  pageNumber: number
}

export type CreateDebitCardRequest = {
  cardNumber: string
  expirationDate: string
  cardNetwork: CardNetwork
  bankId: number
  cardName: string
}

export type CreateCreditCardRequest = CreateDebitCardRequest & {
  closingDay: number
  dueDateDay: number
}

export async function getCards(
  accountId: number,
  { pageNumber, pageSize }: GetCardsParams,
): Promise<PagedCardResponse> {
  const response = await axiosClient.get<PagedCardResponse>(
    `/api/Card/${encodeURIComponent(accountId)}`,
    {
      params: {
        PageSize: pageSize,
        PageNumber: pageNumber,
      },
    },
  )

  return response.data
}

export async function createDebitCard(
  accountId: number,
  payload: CreateDebitCardRequest,
): Promise<void> {
  await axiosClient.post(`/api/Card/${encodeURIComponent(accountId)}/debit`, payload)
}

export async function createCreditCard(
  accountId: number,
  payload: CreateCreditCardRequest,
): Promise<void> {
  await axiosClient.post(`/api/Card/${encodeURIComponent(accountId)}/credit`, payload)
}
