import { useCallback, useEffect, useState } from 'react'
import type { TransactionDetails, TransactionDetailsResponse } from '../../types/transaction'
import { getTransactionDetails } from './transactionsApi'

export function useTransactionDetails(accountId: number, transactionId: number) {
  const [transactionDetails, setTransactionDetails] = useState<TransactionDetails | null>(null)
  const [response, setResponse] = useState<TransactionDetailsResponse | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<unknown>(null)

  const loadTransactionDetails = useCallback(
    () => getTransactionDetails(accountId, transactionId),
    [accountId, transactionId],
  )

  const reload = useCallback(async () => {
    setIsLoading(true)
    setError(null)

    try {
      const nextResponse = await loadTransactionDetails()

      setTransactionDetails(nextResponse.data)
      setResponse(nextResponse)
    } catch (loadError) {
      setTransactionDetails(null)
      setResponse(null)
      setError(loadError)
    } finally {
      setIsLoading(false)
    }
  }, [loadTransactionDetails])

  useEffect(() => {
    let isActive = true

    async function loadDetails() {
      setIsLoading(true)
      setError(null)

      try {
        const nextResponse = await loadTransactionDetails()

        if (!isActive) {
          return
        }

        setTransactionDetails(nextResponse.data)
        setResponse(nextResponse)
      } catch (loadError) {
        if (!isActive) {
          return
        }

        setTransactionDetails(null)
        setResponse(null)
        setError(loadError)
      } finally {
        if (isActive) {
          setIsLoading(false)
        }
      }
    }

    void loadDetails()

    return () => {
      isActive = false
    }
  }, [loadTransactionDetails])

  return {
    transactionDetails,
    error,
    errors: response?.errors ?? [],
    isLoading,
    reload,
  }
}
