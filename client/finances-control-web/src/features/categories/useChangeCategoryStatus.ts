import { useCallback, useState } from 'react'
import { activateCategory, deactivateCategory } from './categoriesApi'

export type CategoryStatusChange = {
  categoryId: number
  nextStatus: 'active' | 'inactive'
}

export function useChangeCategoryStatus() {
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<unknown>(null)

  const changeCategoryStatus = useCallback(async ({ categoryId, nextStatus }: CategoryStatusChange) => {
    setIsLoading(true)
    setError(null)

    try {
      if (nextStatus === 'active') {
        await activateCategory(categoryId)
      } else {
        await deactivateCategory(categoryId)
      }
    } catch (changeError) {
      setError(changeError)
      throw changeError
    } finally {
      setIsLoading(false)
    }
  }, [])

  return {
    changeCategoryStatus,
    error,
    isLoading,
  }
}
