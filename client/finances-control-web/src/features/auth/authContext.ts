import { createContext } from 'react'
import type { CurrentUser } from '../../types/auth'

export type AuthContextValue = {
  user: CurrentUser | null
  isCheckingAuth: boolean
  isAuthenticated: boolean
  refreshCurrentUser: () => Promise<CurrentUser>
  clearUser: () => void
}

export const AuthContext = createContext<AuthContextValue | undefined>(undefined)
