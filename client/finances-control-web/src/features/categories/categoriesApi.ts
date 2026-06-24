import { axiosClient } from '../../api/axiosClient'
import type { CategoryDetailsResponse, PagedCategoryResponse } from '../../types/category'

export type GetCategoriesParams = {
  pageSize: number
  pageNumber: number
}

export type CreateCategoryRequest = {
  name: string
}

export type UpdateCategoryRequest = {
  name: string
}

export async function getCategories({
  pageNumber,
  pageSize,
}: GetCategoriesParams): Promise<PagedCategoryResponse> {
  const response = await axiosClient.get<PagedCategoryResponse>('/api/TransactionCategory', {
    params: {
      PageSize: pageSize,
      PageNumber: pageNumber,
    },
  })

  return response.data
}

export async function getCategoryDetails(categoryId: number): Promise<CategoryDetailsResponse> {
  const response = await axiosClient.get<CategoryDetailsResponse>(
    `/api/TransactionCategory/${encodeURIComponent(categoryId)}`,
  )

  return response.data
}

export async function createCategory(payload: CreateCategoryRequest): Promise<void> {
  await axiosClient.post('/api/TransactionCategory', payload)
}

export async function activateCategory(categoryId: number): Promise<void> {
  await axiosClient.post(`/api/TransactionCategory/${encodeURIComponent(categoryId)}/activate`)
}

export async function deactivateCategory(categoryId: number): Promise<void> {
  await axiosClient.post(`/api/TransactionCategory/${encodeURIComponent(categoryId)}/deactivate`)
}

export async function updateCategory(categoryId: number, payload: UpdateCategoryRequest): Promise<void> {
  await axiosClient.patch(`/api/TransactionCategory/${encodeURIComponent(categoryId)}`, payload)
}
