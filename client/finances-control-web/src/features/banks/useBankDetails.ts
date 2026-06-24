import { useCallback, useEffect, useState } from 'react'
import { getBankDetails } from './banksApi'
import type { BankDetails, BankDetailsResponse } from '../../types/bank'

export function useBankDetails(bankId: number) {
  const [bankDetails, setBankDetails] = useState<BankDetails | null>(null)
  const [response, setResponse] = useState<BankDetailsResponse | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<unknown>(null)

  const reload = useCallback(async () => {
    setIsLoading(true)
    setError(null)

    try {
      const nextResponse = await getBankDetails(bankId)

      setBankDetails(nextResponse.data)
      setResponse(nextResponse)
    } catch (loadError) {
      setBankDetails(null)
      setResponse(null)
      setError(loadError)
    } finally {
      setIsLoading(false)
    }
  }, [bankId])

  useEffect(() => {
    let isActive = true

    async function loadBankDetails() {
      setIsLoading(true)
      setError(null)

      try {
        const nextResponse = await getBankDetails(bankId)

        if (!isActive) {
          return
        }

        setBankDetails(nextResponse.data)
        setResponse(nextResponse)
      } catch (loadError) {
        if (!isActive) {
          return
        }

        setBankDetails(null)
        setResponse(null)
        setError(loadError)
      } finally {
        if (isActive) {
          setIsLoading(false)
        }
      }
    }

    void loadBankDetails()

    return () => {
      isActive = false
    }
  }, [bankId])

  return {
    bankDetails,
    error,
    errors: response?.errors ?? [],
    isLoading,
    reload,
  }
}
