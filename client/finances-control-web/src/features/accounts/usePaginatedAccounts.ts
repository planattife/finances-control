import { useCallback, useEffect, useState } from 'react'
import { getAccounts } from './accountsApi'
import type { Account, PagedAccountResponse } from '../../types/account'

type UsePaginatedAccountsOptions = {
  initialPage?: number
  pageNumber?: number
  pageSize?: number
}

export function usePaginatedAccounts({
  initialPage = 1,
  pageNumber,
  pageSize = 5,
}: UsePaginatedAccountsOptions = {}) {
  const [accounts, setAccounts] = useState<Account[]>([])
  const [response, setResponse] = useState<PagedAccountResponse | null>(null)
  const [currentPage, setCurrentPage] = useState(pageNumber ?? initialPage)
  const [isLoading, setIsLoading] = useState(true)
  const [isLoadingMore, setIsLoadingMore] = useState(false)
  const [error, setError] = useState<unknown>(null)
  const requestedPage = pageNumber ?? initialPage

  useEffect(() => {
    let isActive = true

    async function loadPage() {
      setIsLoading(true)
      setError(null)

      try {
        const nextResponse = await getAccounts({ pageNumber: requestedPage, pageSize })

        if (!isActive) {
          return
        }

        setAccounts(nextResponse.data)
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
  }, [pageSize, requestedPage])

  const reload = useCallback(async () => {
    setIsLoading(true)
    setError(null)

    try {
      const nextResponse = await getAccounts({ pageNumber: requestedPage, pageSize })

      setAccounts(nextResponse.data)
      setCurrentPage(nextResponse.currentPage)
      setResponse(nextResponse)
    } catch (loadError) {
      setError(loadError)
    } finally {
      setIsLoading(false)
    }
  }, [pageSize, requestedPage])

  const loadNextPage = useCallback(async () => {
    if (isLoading || isLoadingMore || !response?.hasNextPage) {
      return []
    }

    setIsLoadingMore(true)
    setError(null)

    try {
      const nextResponse = await getAccounts({ pageNumber: currentPage + 1, pageSize })

      setAccounts((previousAccounts) => [...previousAccounts, ...nextResponse.data])
      setCurrentPage(nextResponse.currentPage)
      setResponse(nextResponse)

      return nextResponse.data
    } catch (loadError) {
      setError(loadError)

      return []
    } finally {
      setIsLoadingMore(false)
    }
  }, [currentPage, isLoading, isLoadingMore, pageSize, response?.hasNextPage])

  const loadUntilAccount = useCallback(
    async (accountId: number) => {
      let accumulatedAccounts = accounts
      let nextPage = currentPage + 1
      let canLoadNextPage = response?.hasNextPage ?? false
      let matchedAccount = accumulatedAccounts.find((account) => account.id === accountId)

      setError(null)

      while (!matchedAccount && canLoadNextPage) {
        setIsLoadingMore(true)

        try {
          const nextResponse = await getAccounts({ pageNumber: nextPage, pageSize })

          accumulatedAccounts = [...accumulatedAccounts, ...nextResponse.data]
          matchedAccount = accumulatedAccounts.find((account) => account.id === accountId)
          nextPage = nextResponse.currentPage + 1
          canLoadNextPage = nextResponse.hasNextPage

          setAccounts(accumulatedAccounts)
          setCurrentPage(nextResponse.currentPage)
          setResponse(nextResponse)
        } catch (loadError) {
          setError(loadError)
          break
        } finally {
          setIsLoadingMore(false)
        }
      }

      return {
        accounts: accumulatedAccounts,
        matchedAccount,
      }
    },
    [accounts, currentPage, pageSize, response?.hasNextPage],
  )

  return {
    accounts,
    currentPage,
    error,
    errors: response?.errors ?? [],
    hasNextPage: response?.hasNextPage ?? false,
    hasPrevPage: response?.hasPrevPage ?? false,
    isLoading,
    isLoadingMore,
    loadNextPage,
    loadUntilAccount,
    reload,
    totalCount: response?.totalCount ?? 0,
    totalPages: response?.totalPages ?? 0,
  }
}
