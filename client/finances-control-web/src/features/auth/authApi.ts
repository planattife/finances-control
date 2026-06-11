import { axiosClient } from '../../api/axiosClient'
import type { CurrentUser, LoginRequest } from '../../types/auth'

export async function login(payload: LoginRequest): Promise<void> {
  await axiosClient.post('/login?useCookies=true', payload)
}

export async function getCurrentUser(): Promise<CurrentUser> {
  const response = await axiosClient.get<CurrentUser>('/manage/info')
  return response.data
}
