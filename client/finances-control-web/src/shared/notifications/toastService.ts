import { toast } from 'sonner'

export type PromiseToastMessages<T> = {
  loading: string
  success: string | ((data: T) => string)
  error: string | ((error: unknown) => string)
}

type ToastOptions = {
  description?: string
}

function getToastOptions(description?: string): ToastOptions | undefined {
  return description ? { description } : undefined
}

export function showSuccess(message: string, description?: string): void {
  toast.success(message, getToastOptions(description))
}

export function showError(message: string, description?: string): void {
  toast.error(message, getToastOptions(description))
}

export function showInfo(message: string, description?: string): void {
  toast.info(message, getToastOptions(description))
}

export function showWarning(message: string, description?: string): void {
  toast.warning(message, getToastOptions(description))
}

export function showPromise<T>(
  promise: Promise<T>,
  messages: PromiseToastMessages<T>,
): Promise<T> {
  toast.promise(promise, messages)

  return promise
}
