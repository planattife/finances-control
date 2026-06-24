import { useCallback, useState } from 'react'
import { updateBank as updateBankRequest, type UpdateBankRequest } from './banksApi'

export type UpdateBankParams = {
  bankId: number
  payload: UpdateBankRequest
}

export function useUpdateBank() {
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<unknown>(null)

  const updateBank = useCallback(async ({ bankId, payload }: UpdateBankParams) => {
    setIsLoading(true)
    setError(null)

    try {
      await updateBankRequest(bankId, payload)
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
    updateBank,
  }
}
