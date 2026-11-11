import { useCallback, useEffect, useState } from 'react'
import { getCategoryDetails } from './categoriesApi'
import type { CategoryDetails, CategoryDetailsResponse } from '../../types/category'

export function useCategoryDetails(categoryId: number) {
  const [categoryDetails, setCategoryDetails] = useState<CategoryDetails | null>(null)
  const [response, setResponse] = useState<CategoryDetailsResponse | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<unknown>(null)

  const reload = useCallback(async () => {
    setIsLoading(true)
    setError(null)

    try {
      const nextResponse = await getCategoryDetails(categoryId)

      setCategoryDetails(nextResponse.data)
      setResponse(nextResponse)
    } catch (loadError) {
      setCategoryDetails(null)
      setResponse(null)
      setError(loadError)
    } finally {
      setIsLoading(false)
    }
  }, [categoryId])

  useEffect(() => {
    let isActive = true

    async function loadCategoryDetails() {
      setIsLoading(true)
      setError(null)

      try {
        const nextResponse = await getCategoryDetails(categoryId)

        if (!isActive) {
          return
        }

        setCategoryDetails(nextResponse.data)
        setResponse(nextResponse)
      } catch (loadError) {
        if (!isActive) {
          return
        }

        setCategoryDetails(null)
        setResponse(null)
        setError(loadError)
      } finally {
        if (isActive) {
          setIsLoading(false)
        }
      }
    }

    void loadCategoryDetails()

    return () => {
      isActive = false
    }
  }, [categoryId])

  return {
    categoryDetails,
    error,
    errors: response?.errors ?? [],
    isLoading,
    reload,
  }
}
