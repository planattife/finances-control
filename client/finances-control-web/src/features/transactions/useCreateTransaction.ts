import { useCallback, useState } from 'react'
import {
  createTransaction as createTransactionRequest,
  type CreateTransactionRequest,
} from './transactionsApi'

type CreateTransactionParams = {
  accountId: number
  payload: CreateTransactionRequest
}

export function useCreateTransaction(accountId: number) {
  const [isLoading, setIsLoading] = useState(false)

  const createTransaction = useCallback(
    async (payload: CreateTransactionRequest) => {
      setIsLoading(true)

      try {
        await createTransactionRequest(accountId, payload)
      } finally {
        setIsLoading(false)
      }
    },
    [accountId],
  )

  const createTransactionWithAccount = useCallback(
    async ({ accountId: nextAccountId, payload }: CreateTransactionParams) => {
      setIsLoading(true)

      try {
        await createTransactionRequest(nextAccountId, payload)
      } finally {
        setIsLoading(false)
      }
    },
    [],
  )

  return {
    createTransaction,
    createTransactionWithAccount,
    isLoading,
  }
}
