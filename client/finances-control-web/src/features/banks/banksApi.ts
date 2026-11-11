import { axiosClient } from '../../api/axiosClient'
import type { BankAccountType, BankDetailsResponse, PagedBankResponse } from '../../types/bank'

export type GetBanksParams = {
  pageSize: number
  pageNumber: number
}

export type CreateBankRequest = {
  name: string
  accountType: BankAccountType
}

export type UpdateBankRequest = {
  name: string
  accountType: BankAccountType
}

export async function getBanks({
  pageNumber,
  pageSize,
}: GetBanksParams): Promise<PagedBankResponse> {
  const response = await axiosClient.get<PagedBankResponse>('/api/Bank', {
    params: {
      PageSize: pageSize,
      PageNumber: pageNumber,
    },
  })

  return response.data
}

export async function createBank(payload: CreateBankRequest): Promise<void> {
  await axiosClient.post('/api/Bank', payload)
}

export async function updateBank(bankId: number, payload: UpdateBankRequest): Promise<void> {
  await axiosClient.put(`/api/Bank/${encodeURIComponent(bankId)}`, payload)
}

export async function getBankDetails(bankId: number): Promise<BankDetailsResponse> {
  const response = await axiosClient.get<BankDetailsResponse>(
    `/api/Bank/${encodeURIComponent(bankId)}`,
  )

  return response.data
}

export async function activateBank(bankId: number): Promise<void> {
  await axiosClient.post(`/api/Bank/${encodeURIComponent(bankId)}/activate`)
}

export async function deactivateBank(bankId: number): Promise<void> {
  await axiosClient.post(`/api/Bank/${encodeURIComponent(bankId)}/deactivate`)
}
