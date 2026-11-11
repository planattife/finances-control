import type { PagedResponse } from './pagedResponse'

export type Category = {
  id: number
  name: string
  isActive: boolean
  isDefault: boolean
}

export type CategoryDetails = {
  id: number
  name: string
  status: string
  isDefault: boolean
  createdAt: string
  lastUpdate: string | null
}

export type PagedCategoryResponse = PagedResponse<Category>

export type CategoryDetailsResponse = {
  data: CategoryDetails
  errors: string[]
  httpStatusCode: number
}
