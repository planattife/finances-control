import { useCallback, useEffect, useState } from 'react'
import type { Card, PagedCardResponse } from '../../types/card'
import { getCards } from './cardsApi'

type UseLoadableCardsOptions = {
  accountId: number | null
  initialPage?: number
  pageSize?: number
}

export function useLoadableCards({
  accountId,
  initialPage = 1,
  pageSize = 5,
}: UseLoadableCardsOptions) {
  const [cards, setCards] = useState<Card[]>([])
  const [response, setResponse] = useState<PagedCardResponse | null>(null)
  const [currentPage, setCurrentPage] = useState(initialPage)
  const [isLoading, setIsLoading] = useState(Boolean(accountId))
  const [isLoadingMore, setIsLoadingMore] = useState(false)
  const [error, setError] = useState<unknown>(null)

  useEffect(() => {
    let isActive = true

    async function loadInitialPage() {
      if (!accountId) {
        setCards([])
        setResponse(null)
        setCurrentPage(initialPage)
        setIsLoading(false)
        setError(null)
        return
      }

      setIsLoading(true)
      setError(null)

      try {
        const nextResponse = await getCards(accountId, { pageNumber: initialPage, pageSize })

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

    void loadInitialPage()

    return () => {
      isActive = false
    }
  }, [accountId, initialPage, pageSize])

  const loadNextPage = useCallback(async () => {
    if (!accountId || isLoading || isLoadingMore || !response?.hasNextPage) {
      return []
    }

    setIsLoadingMore(true)
    setError(null)

    try {
      const nextResponse = await getCards(accountId, {
        pageNumber: currentPage + 1,
        pageSize,
      })

      setCards((previousCards) => [...previousCards, ...nextResponse.data])
      setCurrentPage(nextResponse.currentPage)
      setResponse(nextResponse)

      return nextResponse.data
    } catch (loadError) {
      setError(loadError)

      return []
    } finally {
      setIsLoadingMore(false)
    }
  }, [accountId, currentPage, isLoading, isLoadingMore, pageSize, response?.hasNextPage])

  return {
    cards,
    error,
    errors: response?.errors ?? [],
    hasNextPage: response?.hasNextPage ?? false,
    isLoading,
    isLoadingMore,
    loadNextPage,
  }
}
