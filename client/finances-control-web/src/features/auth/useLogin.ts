import { useState } from 'react'
import { login } from './authApi'
import type { LoginRequest } from '../../types/auth'

export function useLogin() {
  const [isLoading, setIsLoading] = useState(false)

  async function mutateAsync(payload: LoginRequest) {
    setIsLoading(true)

    try {
      await login(payload)
    } finally {
      setIsLoading(false)
    }
  }

  return {
    isLoading,
    mutateAsync,
  }
}
