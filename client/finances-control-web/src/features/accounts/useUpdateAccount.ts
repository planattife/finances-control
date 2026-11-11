import { useCallback, useState } from 'react'
import { updateAccount as updateAccountRequest, type UpdateAccountRequest } from './accountsApi'

export type UpdateAccountParams = {
  accountId: number
  payload: UpdateAccountRequest
}

export function useUpdateAccount() {
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<unknown>(null)

  const updateAccount = useCallback(async ({ accountId, payload }: UpdateAccountParams) => {
    setIsLoading(true)
    setError(null)

    try {
      await updateAccountRequest(accountId, payload)
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
    updateAccount,
  }
}
