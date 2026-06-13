import { useCallback, useEffect, useState } from 'react'
import { getAccounts } from './accountsApi'
import type { Account } from '../../types/account'

type UsePaginatedAccountsOptions = {
  pageSize?: number
  initialPage?: number
}

export function usePaginatedAccounts({
  initialPage = 1,
  pageSize = 5,
}: UsePaginatedAccountsOptions = {}) {
  const [accounts, setAccounts] = useState<Account[]>([])
  const [currentPage, setCurrentPage] = useState(initialPage)
  const [hasNextPage, setHasNextPage] = useState(true)
  const [isLoading, setIsLoading] = useState(true)
  const [isLoadingMore, setIsLoadingMore] = useState(false)
  const [error, setError] = useState<unknown>(null)

  useEffect(() => {
    let isActive = true

    async function loadInitialPage() {
      try {
        const response = await getAccounts({ pageNumber: initialPage, pageSize })

        if (!isActive) {
          return
        }

        setAccounts(response.data)
        setCurrentPage(response.currentPage)
        setHasNextPage(response.hasNextPage)
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

    void loadInitialPage()

    return () => {
      isActive = false
    }
  }, [initialPage, pageSize])

  const loadNextPage = useCallback(async () => {
    if (isLoading || isLoadingMore || !hasNextPage) {
      return []
    }

    setIsLoadingMore(true)
    setError(null)

    try {
      const response = await getAccounts({ pageNumber: currentPage + 1, pageSize })

      setAccounts((previousAccounts) => [...previousAccounts, ...response.data])
      setCurrentPage(response.currentPage)
      setHasNextPage(response.hasNextPage)

      return response.data
    } catch (loadError) {
      setError(loadError)

      return []
    } finally {
      setIsLoadingMore(false)
    }
  }, [currentPage, hasNextPage, isLoading, isLoadingMore, pageSize])

  const loadUntilAccount = useCallback(
    async (accountId: number) => {
      let accumulatedAccounts = accounts
      let nextPage = currentPage + 1
      let canLoadNextPage = hasNextPage
      let matchedAccount = accumulatedAccounts.find((account) => account.id === accountId)

      setError(null)

      while (!matchedAccount && canLoadNextPage) {
        setIsLoadingMore(true)

        try {
          const response = await getAccounts({ pageNumber: nextPage, pageSize })

          accumulatedAccounts = [...accumulatedAccounts, ...response.data]
          matchedAccount = accumulatedAccounts.find((account) => account.id === accountId)
          nextPage = response.currentPage + 1
          canLoadNextPage = response.hasNextPage

          setAccounts(accumulatedAccounts)
          setCurrentPage(response.currentPage)
          setHasNextPage(response.hasNextPage)
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
    [accounts, currentPage, hasNextPage, pageSize],
  )

  return {
    accounts,
    error,
    hasNextPage,
    isLoading,
    isLoadingMore,
    loadUntilAccount,
    loadNextPage,
  }
}
