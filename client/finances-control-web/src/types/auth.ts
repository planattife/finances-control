export type LoginRequest = {
  email: string
  password: string
}

export type CurrentUser = {
  email: string
  isEmailConfirmed: boolean
}

export type ApiErrorResponse = {
  type?: string
  title?: string
  status?: number
  detail?: string
}
