import { useCallback, useState } from 'react'
import { createCategory as createCategoryRequest, type CreateCategoryRequest } from './categoriesApi'

export function useCreateCategory() {
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<unknown>(null)

  const createCategory = useCallback(async (payload: CreateCategoryRequest) => {
    setIsLoading(true)
    setError(null)

    try {
      await createCategoryRequest(payload)
    } catch (createError) {
      setError(createError)
      throw createError
    } finally {
      setIsLoading(false)
    }
  }, [])

  return {
    createCategory,
    error,
    isLoading,
  }
}
