import { useCallback, useEffect, useState } from 'react'
import type { Card, PagedCardResponse } from '../../types/card'
import { getCards } from './cardsApi'

type UsePaginatedCardsOptions = {
  accountId: number | null
  initialPage?: number
  pageNumber?: number
  pageSize?: number
}

export function usePaginatedCards({
  accountId,
  initialPage = 1,
  pageNumber,
  pageSize = 10,
}: UsePaginatedCardsOptions) {
  const [cards, setCards] = useState<Card[]>([])
  const [response, setResponse] = useState<PagedCardResponse | null>(null)
  const [currentPage, setCurrentPage] = useState(pageNumber ?? initialPage)
  const [isLoading, setIsLoading] = useState(Boolean(accountId))
  const [error, setError] = useState<unknown>(null)
  const requestedPage = pageNumber ?? initialPage

  useEffect(() => {
    let isActive = true

    async function loadPage() {
      if (!accountId) {
        setCards([])
        setResponse(null)
        setCurrentPage(requestedPage)
        setIsLoading(false)
        setError(null)
        return
      }

      setIsLoading(true)
      setError(null)

      try {
        const nextResponse = await getCards(accountId, { pageNumber: requestedPage, pageSize })

        if (!isActive) {
          return
        }

        setCards(nextResponse.data)
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
      setCards([])
      setResponse(null)
      setCurrentPage(requestedPage)
      setIsLoading(false)
      setError(null)
      return
    }

    setIsLoading(true)
    setError(null)

    try {
      const nextResponse = await getCards(accountId, { pageNumber: requestedPage, pageSize })

      setCards(nextResponse.data)
      setCurrentPage(nextResponse.currentPage)
      setResponse(nextResponse)
    } catch (loadError) {
      setError(loadError)
    } finally {
      setIsLoading(false)
    }
  }, [accountId, pageSize, requestedPage])

  return {
    cards,
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
