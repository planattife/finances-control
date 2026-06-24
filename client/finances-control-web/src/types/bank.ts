import type { PagedResponse } from './pagedResponse'

export const BankAccountType = {
  Savings: 0,
  Check: 1,
} as const

export type BankAccountType = (typeof BankAccountType)[keyof typeof BankAccountType]

export const bankAccountTypeOptions: Array<{ label: string; value: BankAccountType }> = [
  { label: 'Savings', value: BankAccountType.Savings },
  { label: 'Check', value: BankAccountType.Check },
]

export function getBankAccountTypeLabel(accountType: number) {
  return bankAccountTypeOptions.find((option) => option.value === accountType)?.label ?? 'Unknown'
}

export type Bank = {
  id: number
  name: string
  accountType: BankAccountType
  isActive: boolean
}

export type BankDetails = {
  id: number
  name: string
  accountType: BankAccountType
  status: string
  createdAt: string
  lastUpdate: string | null
}

export type PagedBankResponse = PagedResponse<Bank>

export type BankDetailsResponse = {
  data: BankDetails
  errors: string[]
  httpStatusCode: number
}
