import { useCallback, useState } from 'react'
import { updateCategory as updateCategoryRequest, type UpdateCategoryRequest } from './categoriesApi'

export type UpdateCategoryParams = {
  categoryId: number
  payload: UpdateCategoryRequest
}

export function useUpdateCategory() {
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<unknown>(null)

  const updateCategory = useCallback(async ({ categoryId, payload }: UpdateCategoryParams) => {
    setIsLoading(true)
    setError(null)

    try {
      await updateCategoryRequest(categoryId, payload)
    } catch (updateError) {
      setError(updateError)
      throw updateError
    } finally {
      setIsLoading(false)
    }
  }, [])

  return {
    error,
    isLoading,
    updateCategory,
  }
}
