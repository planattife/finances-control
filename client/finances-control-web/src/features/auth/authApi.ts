import { axiosClient } from '../../api/axiosClient'
import type { CurrentUser, LoginRequest } from '../../types/auth'

export async function login(payload: LoginRequest): Promise<void> {
  await axiosClient.post('/login?useCookies=true', payload)
  await getAntiforgeryToken()
}

export async function logout(): Promise<void> {
  await axiosClient.post('/logout')
}

export async function getAntiforgeryToken(): Promise<void> {
  await axiosClient.get('/antiforgery/token')
}

export async function getCurrentUser(): Promise<CurrentUser> {
  const response = await axiosClient.get<CurrentUser>('/manage/info')
  return response.data
}
