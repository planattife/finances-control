---
name: sonner-toast-notifications
description: Add or update React TypeScript data-changing frontend flows to use the project Sonner toast wrapper. Use when implementing POST, PUT, PATCH, DELETE, or any HTTP operation that changes data, or when working on notification/toast behavior in this app. This project uses standalone Sonner, not shadcn.
---

# Sonner Toast Notifications

Use this skill when adding or changing frontend flows that perform data-changing HTTP operations.

## Core Rule

Show a promise toast for every operation that changes data:

- Use this for `POST`, `PUT`, `PATCH`, `DELETE`, and any other HTTP verb or endpoint that creates, updates, deletes, imports, exports, syncs, submits, approves, rejects, or otherwise mutates server data.
- Use Sonner only through the project wrapper in `src/shared/notifications/toastService.ts`.
- Do not import `toast` from `sonner` outside `src/shared/notifications`.
- Do not use shadcn toast components or shadcn Sonner wrappers.
- Use only two final toast outcomes: success and error.
- Use promise mode so the toast shows a loading state while the operation is pending.

## Existing Project API

Use `showPromise` from the toast service:

```ts
import { showPromise } from '../../shared/notifications/toastService'

await showPromise(operationPromise, {
  loading: 'Saving...',
  success: 'Saved successfully',
  error: 'Could not save',
})
```

The service returns the original promise result and does not swallow errors. Callers can still `await` the operation and handle failures when needed.

## Where To Apply It

Prefer wrapping the mutation call at the UI coordination boundary:

- In a form submit handler that calls a mutation.
- In a button action that performs a data-changing operation.
- In a custom hook only when that hook owns the user-facing mutation behavior for the feature.

Keep the documented frontend flow intact:

```text
Component -> Custom Hook -> API Function -> Axios Client
```

Components and hooks must not call Axios directly. API functions must not show toasts.

## Required Pattern

For every data-changing action:

1. Create the operation promise.
2. Pass that promise to `showPromise`.
3. Await `showPromise`.
4. Let errors continue to reject unless the caller has a clear UI reason to catch them.

```ts
const handleSubmit = async (payload: CreateAccountRequest) => {
  await showPromise(createAccountMutation.mutateAsync(payload), {
    loading: 'Creating account...',
    success: 'Account created successfully',
    error: 'Could not create account',
  })
}
```

When the success message needs response data, use the typed resolved value:

```ts
await showPromise(createAccountMutation.mutateAsync(payload), {
  loading: 'Creating account...',
  success: (account) => `${account.name} created successfully`,
  error: 'Could not create account',
})
```

When the error message needs error details, treat the error as `unknown`:

```ts
await showPromise(updateAccountMutation.mutateAsync(payload), {
  loading: 'Updating account...',
  success: 'Account updated successfully',
  error: (error) => error instanceof Error ? error.message : 'Could not update account',
})
```

## Message Guidelines

- Loading messages should describe the in-progress action: `Saving...`, `Creating account...`, `Updating card...`.
- Success messages should confirm the completed action: `Saved successfully`, `Account created successfully`.
- Error messages should be short and actionable: `Could not save`, `Could not update account`.
- Do not add info or warning toasts for mutation outcomes.
- Do not use `showSuccess` or `showError` manually for data-changing HTTP operations when a promise is available; use `showPromise`.

## Non-Mutating Operations

Do not show promise toasts for normal `GET` or read-only queries by default.

For read-only errors, follow the existing page or component error-state pattern unless the user explicitly asks for toast behavior.

## Setup Expectations

The app should have one root toaster component:

```tsx
<Toaster
  position="top-right"
  richColors
  closeButton
  duration={4000}
/>
```

In this project, keep that setup in `src/shared/notifications/AppToaster.tsx` and mount it once at the app root.

## Validation

After adding or changing toast behavior, run:

```bash
npm.cmd run build
npm.cmd run lint
```

Run the commands from `client/finances-control-web`.
