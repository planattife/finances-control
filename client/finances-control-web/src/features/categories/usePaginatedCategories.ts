import { useCallback, useEffect, useState } from 'react'
import { getCategories } from './categoriesApi'
import type { Category, PagedCategoryResponse } from '../../types/category'

type UsePaginatedCategoriesOptions = {
  initialPage?: number
  pageNumber?: number
  pageSize?: number
}

export function usePaginatedCategories({
  initialPage = 1,
  pageNumber,
  pageSize = 10,
}: UsePaginatedCategoriesOptions = {}) {
  const [categories, setCategories] = useState<Category[]>([])
  const [response, setResponse] = useState<PagedCategoryResponse | null>(null)
  const [currentPage, setCurrentPage] = useState(pageNumber ?? initialPage)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<unknown>(null)
  const requestedPage = pageNumber ?? initialPage

  useEffect(() => {
    let isActive = true

    async function loadPage() {
      setIsLoading(true)
      setError(null)

      try {
        const nextResponse = await getCategories({ pageNumber: requestedPage, pageSize })

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

    void loadPage()

    return () => {
      isActive = false
    }
  }, [pageSize, requestedPage])

  const reload = useCallback(async () => {
    setIsLoading(true)
    setError(null)

    try {
      const nextResponse = await getCategories({ pageNumber: requestedPage, pageSize })

      setCategories(nextResponse.data)
      setCurrentPage(nextResponse.currentPage)
      setResponse(nextResponse)
    } catch (loadError) {
      setError(loadError)
    } finally {
      setIsLoading(false)
    }
  }, [pageSize, requestedPage])

  return {
    categories,
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
