import { useCallback, useEffect, useState } from 'react'
import { getBanks } from './banksApi'
import type { Bank, PagedBankResponse } from '../../types/bank'

type UsePaginatedBanksOptions = {
  initialPage?: number
  pageNumber?: number
  pageSize?: number
}

export function usePaginatedBanks({
  initialPage = 1,
  pageNumber,
  pageSize = 10,
}: UsePaginatedBanksOptions = {}) {
  const [banks, setBanks] = useState<Bank[]>([])
  const [response, setResponse] = useState<PagedBankResponse | null>(null)
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
        const nextResponse = await getBanks({ pageNumber: requestedPage, pageSize })

        if (!isActive) {
          return
        }

        setBanks(nextResponse.data)
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
      const nextResponse = await getBanks({ pageNumber: requestedPage, pageSize })

      setBanks(nextResponse.data)
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
      const nextResponse = await getBanks({ pageNumber: currentPage + 1, pageSize })

      setBanks((previousBanks) => [...previousBanks, ...nextResponse.data])
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

  return {
    banks,
    currentPage,
    error,
    errors: response?.errors ?? [],
    hasNextPage: response?.hasNextPage ?? false,
    hasPrevPage: response?.hasPrevPage ?? false,
    isLoading,
    isLoadingMore,
    loadNextPage,
    reload,
    totalCount: response?.totalCount ?? 0,
    totalPages: response?.totalPages ?? 0,
  }
}
