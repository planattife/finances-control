import { axiosClient } from '../../api/axiosClient'
import type {
  CardDetailsResponse,
  CardNetwork,
  CreditCardDetailsResponse,
  PagedCardResponse,
} from '../../types/card'

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

export async function getDebitCardDetails(
  accountId: number,
  cardId: number,
): Promise<CardDetailsResponse> {
  const response = await axiosClient.get<CardDetailsResponse>(
    `/api/Card/${encodeURIComponent(accountId)}/${encodeURIComponent(cardId)}`,
  )

  return response.data
}

export async function getCreditCardDetails(
  accountId: number,
  cardId: number,
): Promise<CreditCardDetailsResponse> {
  const response = await axiosClient.get<CreditCardDetailsResponse>(
    `/api/Card/${encodeURIComponent(accountId)}/credit/${encodeURIComponent(cardId)}`,
  )

  return response.data
}
