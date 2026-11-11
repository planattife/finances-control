import { useCallback, useState } from 'react'
import { deleteTransaction as deleteTransactionRequest } from './transactionsApi'

type DeleteTransactionParams = {
  accountId: number
  transactionId: number
}

export function useDeleteTransaction() {
  const [isLoading, setIsLoading] = useState(false)

  const deleteTransaction = useCallback(
    async ({ accountId, transactionId }: DeleteTransactionParams) => {
      setIsLoading(true)

      try {
        await deleteTransactionRequest(accountId, transactionId)
      } finally {
        setIsLoading(false)
      }
    },
    [],
  )

  return {
    deleteTransaction,
    isLoading,
  }
}
