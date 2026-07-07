import type { PagedResponse } from './pagedResponse'

export type PaymentMethod =
  | 'Credit'
  | 'Debit'
  | 'Pix'
  | 'BankSlip'
  | 'Cash'
  | 'BankTransfer'

export type TransactionType = 'Expense' | 'Income'

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
