import { useCallback, useEffect, useState } from 'react'
import type { Category, PagedCategoryResponse } from '../../types/category'
import { getCategories } from './categoriesApi'

type UseLoadableCategoriesOptions = {
  initialPage?: number
  pageSize?: number
}

export function useLoadableCategories({
  initialPage = 1,
  pageSize = 5,
}: UseLoadableCategoriesOptions = {}) {
  const [categories, setCategories] = useState<Category[]>([])
  const [response, setResponse] = useState<PagedCategoryResponse | null>(null)
  const [currentPage, setCurrentPage] = useState(initialPage)
  const [isLoading, setIsLoading] = useState(true)
  const [isLoadingMore, setIsLoadingMore] = useState(false)
  const [error, setError] = useState<unknown>(null)

  useEffect(() => {
    let isActive = true

    async function loadInitialPage() {
      setIsLoading(true)
      setError(null)

      try {
        const nextResponse = await getCategories({ pageNumber: initialPage, pageSize })

        if (!isActive) {
          return
        }

        setCategories(nextResponse.data)
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
  }, [initialPage, pageSize])

  const loadNextPage = useCallback(async () => {
    if (isLoading || isLoadingMore || !response?.hasNextPage) {
      return []
    }

    setIsLoadingMore(true)
    setError(null)

    try {
      const nextResponse = await getCategories({ pageNumber: currentPage + 1, pageSize })

      setCategories((previousCategories) => [...previousCategories, ...nextResponse.data])
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

  const loadUntilCategory = useCallback(
    async (categoryId: number) => {
      let accumulatedCategories = categories
      let nextPage = currentPage + 1
      let canLoadNextPage = response?.hasNextPage ?? false
      let matchedCategory = accumulatedCategories.find((category) => category.id === categoryId)

      setError(null)

      while (!matchedCategory && canLoadNextPage) {
        setIsLoadingMore(true)

        try {
          const nextResponse = await getCategories({ pageNumber: nextPage, pageSize })

          accumulatedCategories = [...accumulatedCategories, ...nextResponse.data]
          matchedCategory = accumulatedCategories.find((category) => category.id === categoryId)
          nextPage = nextResponse.currentPage + 1
          canLoadNextPage = nextResponse.hasNextPage

          setCategories(accumulatedCategories)
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
        categories: accumulatedCategories,
        matchedCategory,
      }
    },
    [categories, currentPage, pageSize, response?.hasNextPage],
  )

  return {
    categories,
    currentPage,
    error,
    errors: response?.errors ?? [],
    hasNextPage: response?.hasNextPage ?? false,
    isLoading,
    isLoadingMore,
    loadNextPage,
    loadUntilCategory,
  }
}
