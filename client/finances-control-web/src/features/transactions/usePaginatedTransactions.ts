import { useCallback, useEffect, useState } from 'react'
import type { PagedTransactionResponse, Transaction } from '../../types/transaction'
import { getTransactions } from './transactionsApi'

type UsePaginatedTransactionsOptions = {
  accountId: number | null
  initialPage?: number
  pageNumber?: number
  pageSize?: number
}

export function usePaginatedTransactions({
  accountId,
  initialPage = 1,
  pageNumber,
  pageSize = 10,
}: UsePaginatedTransactionsOptions) {
  const [transactions, setTransactions] = useState<Transaction[]>([])
  const [response, setResponse] = useState<PagedTransactionResponse | null>(null)
  const [currentPage, setCurrentPage] = useState(pageNumber ?? initialPage)
  const [isLoading, setIsLoading] = useState(Boolean(accountId))
  const [error, setError] = useState<unknown>(null)
  const requestedPage = pageNumber ?? initialPage

  useEffect(() => {
    let isActive = true

    async function loadPage() {
      if (!accountId) {
        setTransactions([])
        setResponse(null)
        setCurrentPage(requestedPage)
        setIsLoading(false)
        setError(null)
        return
      }

      setIsLoading(true)
      setError(null)

      try {
        const nextResponse = await getTransactions(accountId, {
          pageNumber: requestedPage,
          pageSize,
        })

        if (!isActive) {
          return
        }

        setTransactions(nextResponse.data)
        setCurrentPage(nextResponse.currentPage)
        setResponse(nextResponse)
      } catch (loadError) {
        if (!isActive) {
          return
        }

        setError(loadError)
      } finally {
        if (isActive) {
          setIsLoading(false)
        }
      }
    }

    void loadPage()

    return () => {
      isActive = false
    }
  }, [accountId, pageSize, requestedPage])

  const reload = useCallback(async () => {
    if (!accountId) {
      setTransactions([])
      setResponse(null)
      setCurrentPage(requestedPage)
      setIsLoading(false)
      setError(null)
      return
    }

    setIsLoading(true)
    setError(null)

    try {
      const nextResponse = await getTransactions(accountId, {
        pageNumber: requestedPage,
        pageSize,
      })

      setTransactions(nextResponse.data)
      setCurrentPage(nextResponse.currentPage)
      setResponse(nextResponse)
    } catch (loadError) {
      setError(loadError)
    } finally {
      setIsLoading(false)
    }
  }, [accountId, pageSize, requestedPage])

  return {
    transactions,
    currentPage,
    error,
    errors: response?.errors ?? [],
    hasNextPage: response?.hasNextPage ?? false,
    hasPrevPage: response?.hasPrevPage ?? false,
    isLoading,
    reload,
    totalCount: response?.totalCount ?? 0,
    totalPages: response?.totalPages ?? 0,
  }
}
