import { useCallback, useState } from 'react'
import { createBank as createBankRequest, type CreateBankRequest } from './banksApi'

export function useCreateBank() {
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<unknown>(null)

  const createBank = useCallback(async (payload: CreateBankRequest) => {
    setIsLoading(true)
    setError(null)

    try {
      await createBankRequest(payload)
    } catch (createError) {
      setError(createError)
      throw createError
    } finally {
      setIsLoading(false)
    }
  }, [])

  return {
    createBank,
    error,
    isLoading,
  }
}
