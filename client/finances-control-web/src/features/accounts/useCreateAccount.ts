import { useCallback, useState } from 'react'
import { createAccount as createAccountRequest, type CreateAccountRequest } from './accountsApi'

export function useCreateAccount() {
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<unknown>(null)

  const createAccount = useCallback(async (payload: CreateAccountRequest) => {
    setIsLoading(true)
    setError(null)

    try {
      await createAccountRequest(payload)
    } catch (createError) {
      setError(createError)
      throw createError
    } finally {
      setIsLoading(false)
    }
  }, [])

  return {
    createAccount,
    error,
    isLoading,
  }
}
