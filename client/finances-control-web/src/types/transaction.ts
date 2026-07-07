import type { PagedResponse } from './pagedResponse'

export type PaymentMethod =
  | 'Credit'
  | 'Debit'
  | 'Pix'
  | 'BankSlip'
  | 'Cash'
  | 'BankTransfer'

export type TransactionType = 'Expense' | 'Income'

export const PaymentMethodCode = {
  Credit: 0,
  Debit: 1,
  Pix: 2,
  BankSlip: 3,
  Cash: 4,
  BankTransfer: 5,
} as const

export type PaymentMethodCode = (typeof PaymentMethodCode)[keyof typeof PaymentMethodCode]

export const paymentMethodOptions: Array<{ label: string; value: PaymentMethodCode }> = [
  { label: 'Credit', value: PaymentMethodCode.Credit },
  { label: 'Debit', value: PaymentMethodCode.Debit },
  { label: 'Pix', value: PaymentMethodCode.Pix },
  { label: 'Bank Slip', value: PaymentMethodCode.BankSlip },
  { label: 'Cash', value: PaymentMethodCode.Cash },
  { label: 'Bank Transfer', value: PaymentMethodCode.BankTransfer },
]

export const TransactionTypeCode = {
  Expense: 0,
  Income: 1,
} as const

export type TransactionTypeCode = (typeof TransactionTypeCode)[keyof typeof TransactionTypeCode]

export const transactionTypeOptions: Array<{ label: string; value: TransactionTypeCode }> = [
  { label: 'Expense', value: TransactionTypeCode.Expense },
  { label: 'Income', value: TransactionTypeCode.Income },
]

export type TransactionCategory = {
  categoryId: number
  categoryName: string
}

export type TransactionBank = {
  bankId: number
  bankName: string
}

export type TransactionCard = {
  cardId: number
  cardName: string
}

export type Transaction = {
  id: number
  amount: number
  paymentMethod: PaymentMethod
  transactionType: TransactionType
  transactionDate: string
  category: TransactionCategory
  status: string
}

export type PagedTransactionResponse = PagedResponse<Transaction>

export type TransactionDetails = Transaction & {
  bank: TransactionBank | null
  card: TransactionCard | null
  description: string | null
  createdAt: string
  updatedAt: string | null
}

export type TransactionDetailsResponse = {
  data: TransactionDetails
  errors: string[]
  httpStatusCode: number
}

const paymentMethodLabels: Record<PaymentMethod, string> = {
  Credit: 'Credit',
  Debit: 'Debit',
  Pix: 'Pix',
  BankSlip: 'Bank Slip',
  Cash: 'Cash',
  BankTransfer: 'Bank Transfer',
}

const transactionTypeLabels: Record<TransactionType, string> = {
  Expense: 'Expense',
  Income: 'Income',
}

export function getPaymentMethodLabel(paymentMethod: string) {
  return paymentMethodLabels[paymentMethod as PaymentMethod] ?? 'Unknown'
}

export function getTransactionTypeLabel(transactionType: string) {
  return transactionTypeLabels[transactionType as TransactionType] ?? 'Unknown'
}

export function requiresCard(paymentMethod: PaymentMethodCode) {
  return paymentMethod === PaymentMethodCode.Credit || paymentMethod === PaymentMethodCode.Debit
}

export function requiresBank(paymentMethod: PaymentMethodCode) {
  return (
    paymentMethod === PaymentMethodCode.Pix ||
    paymentMethod === PaymentMethodCode.BankSlip ||
    paymentMethod === PaymentMethodCode.BankTransfer
  )
}
