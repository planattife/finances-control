import { useCallback, useState } from 'react'
import {
  updateTransaction as updateTransactionRequest,
  type UpdateTransactionRequest,
} from './transactionsApi'

type UpdateTransactionParams = {
  accountId: number
  transactionId: number
  payload: UpdateTransactionRequest
}

export function useUpdateTransaction() {
  const [isLoading, setIsLoading] = useState(false)

  const updateTransaction = useCallback(
    async ({ accountId, payload, transactionId }: UpdateTransactionParams) => {
      setIsLoading(true)

      try {
        await updateTransactionRequest(accountId, transactionId, payload)
      } finally {
        setIsLoading(false)
      }
    },
    [],
  )

  return {
    isLoading,
    updateTransaction,
  }
}
