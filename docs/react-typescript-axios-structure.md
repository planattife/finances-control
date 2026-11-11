# React TypeScript API Structure Guidelines

This document defines the standard project structure and coding conventions for API calls in this React + TypeScript project.

The goal is to keep components clean, isolate HTTP logic, and make API-related code predictable and easy to maintain.

---

## Recommended Folder Structure

```txt
src/
  api/
    axiosClient.ts

  features/
    auth/
      authApi.ts
      useLogin.ts

    cards/
      cardsApi.ts
      useCards.ts
      useCreateCard.ts

    transactions/
      transactionsApi.ts
      useTransactions.ts
      useCreateTransaction.ts

  types/
    card.ts
    transaction.ts
    user.ts
```

---

## Architecture Rule

The application should follow this flow:

```txt
Component
  ↓
Custom hook
  ↓
API function
  ↓
Axios client
```

Components should not call Axios directly.

Avoid this:

```tsx
axios.get("/cards");
```

Prefer this:

```tsx
const { data: cards } = useCards();
```

---

## Responsibilities by Layer

### `src/api/axiosClient.ts`

This file contains the shared Axios instance.

It is responsible for:

- Setting the base API URL
- Setting common headers
- Adding authorization tokens
- Handling global request behavior
- Handling global response errors
- Configuring timeouts

Example:

```ts
import axios from "axios";

export const axiosClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  timeout: 10000,
  headers: {
    "Content-Type": "application/json",
  },
});

axiosClient.interceptors.request.use((config) => {
  const token = localStorage.getItem("access_token");

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

axiosClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Handle unauthorized requests here if needed.
      // Example: logout user, clear token, redirect to login, etc.
    }

    return Promise.reject(error);
  }
);
```

Do not create multiple Axios instances unless there is a clear reason, such as communicating with a completely different external API.

---

## Feature API Files

Each feature should have its own API file.

Examples:

```txt
features/cards/cardsApi.ts
features/transactions/transactionsApi.ts
features/auth/authApi.ts
```

These files are responsible for making HTTP requests related to that feature.

Example:

```ts
import { axiosClient } from "../../api/axiosClient";
import type { Card } from "../../types/card";

export type CreateCardRequest = {
  name: string;
  type: "credit" | "debit";
};

export async function getCards(): Promise<Card[]> {
  const response = await axiosClient.get<Card[]>("/cards");
  return response.data;
}

export async function createCard(payload: CreateCardRequest): Promise<Card> {
  const response = await axiosClient.post<Card>("/cards", payload);
  return response.data;
}
```

Rules:

- API files may import `axiosClient`.
- API files should return `response.data`, not the full Axios response, unless the caller specifically needs headers or status codes.
- API files should define request payload types if they are only used by that feature.
- API files should not contain React code.
- API files should not use React hooks.
- API files should not manage component state.

---

## Custom Hook Files

Custom hooks should wrap API functions using TanStack Query or another query/mutation library.

Examples:

```txt
features/cards/useCards.ts
features/cards/useCreateCard.ts
features/transactions/useTransactions.ts
```

### Query Hook Example

Use query hooks for fetching data.

```ts
import { useQuery } from "@tanstack/react-query";
import { getCards } from "./cardsApi";

export function useCards() {
  return useQuery({
    queryKey: ["cards"],
    queryFn: getCards,
  });
}
```

### Mutation Hook Example

Use mutation hooks for creating, updating, or deleting data.

```ts
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createCard } from "./cardsApi";

export function useCreateCard() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: createCard,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["cards"] });
    },
  });
}
```

Rules:

- Hook files may import API functions.
- Hook files may use React Query hooks such as `useQuery` and `useMutation`.
- Hook files should not call Axios directly.
- Hook files should handle cache invalidation when needed.
- Hook files should expose a clean API to components.

---

## Component Files

Components should focus on UI and user interaction.

Example:

```tsx
import { useCards } from "./useCards";
import { useCreateCard } from "./useCreateCard";

export function CardsPage() {
  const { data: cards, isLoading, error } = useCards();
  const createCardMutation = useCreateCard();

  if (isLoading) {
    return <p>Loading cards...</p>;
  }

  if (error) {
    return <p>Error loading cards.</p>;
  }

  return (
    <div>
      <h1>Cards</h1>

      <button
        onClick={() =>
          createCardMutation.mutate({
            name: "Nubank",
            type: "credit",
          })
        }
      >
        Create card
      </button>

      {cards?.map((card) => (
        <p key={card.id}>{card.name}</p>
      ))}
    </div>
  );
}
```

Rules:

- Components should not import `axiosClient`.
- Components should not call Axios directly.
- Components should use custom hooks for server data.
- Components should handle UI states such as loading, error, and empty data.
- Components should not know API endpoint URLs.

---

## Type Files

Shared domain types should live in the `types` folder.

Example:

```txt
src/types/card.ts
src/types/transaction.ts
src/types/user.ts
```

Example:

```ts
export type Card = {
  id: number;
  name: string;
  type: "credit" | "debit";
};
```

Rules:

- Use `type` or `interface` consistently.
- Shared response models should be placed in `src/types`.
- Feature-specific request payloads may stay inside the feature API file.
- If a type is used by many features, move it to `src/types`.

---

## Naming Conventions

### API Files

Use the format:

```txt
featureNameApi.ts
```

Examples:

```txt
cardsApi.ts
transactionsApi.ts
authApi.ts
```

### Query Hooks

Use the format:

```txt
useResourceName.ts
```

Examples:

```txt
useCards.ts
useTransactions.ts
useCurrentUser.ts
```

### Mutation Hooks

Use action-based names:

```txt
useCreateCard.ts
useUpdateCard.ts
useDeleteCard.ts
useLogin.ts
```

---

## Query Key Conventions

React Query keys should be stable and descriptive.

Examples:

```ts
["cards"]
["cards", cardId]
["transactions"]
["transactions", { month, year }]
["user", userId]
```

Rules:

- Use arrays for query keys.
- The first item should usually be the resource name.
- Include filters or IDs when needed.
- Use the same query key when invalidating related cached data.

Example:

```ts
queryClient.invalidateQueries({ queryKey: ["cards"] });
```

---

## Error Handling

Global errors can be handled in `axiosClient.ts`.

Feature-specific errors should be handled in hooks or components.

Example:

```tsx
if (error) {
  return <p>Could not load cards.</p>;
}
```

Do not place UI-specific error messages inside API functions.

Avoid this:

```ts
export async function getCards() {
  try {
    const response = await axiosClient.get("/cards");
    return response.data;
  } catch {
    alert("Error loading cards");
  }
}
```

Prefer letting the hook/component decide how to show the error.

---

## Auth Requests

Authentication should follow the same pattern.

Example:

```txt
features/auth/authApi.ts
features/auth/useLogin.ts
```

```ts
import { axiosClient } from "../../api/axiosClient";

export type LoginRequest = {
  email: string;
  password: string;
};

export type LoginResponse = {
  accessToken: string;
};

export async function login(payload: LoginRequest): Promise<LoginResponse> {
  const response = await axiosClient.post<LoginResponse>("/auth/login", payload);
  return response.data;
}
```

```ts
import { useMutation } from "@tanstack/react-query";
import { login } from "./authApi";

export function useLogin() {
  return useMutation({
    mutationFn: login,
  });
}
```

---

## Adding a New Feature

When adding a new feature, follow these steps:

1. Create a folder inside `src/features`.
2. Create a feature API file.
3. Create query and mutation hooks.
4. Create or update shared types in `src/types`.
5. Use the hooks inside components.
6. Do not call Axios directly from TSX files.

Example for a `categories` feature:

```txt
src/
  features/
    categories/
      categoriesApi.ts
      useCategories.ts
      useCreateCategory.ts

  types/
    category.ts
```

---

## Summary of Rules

Always follow these rules:

- Use `axiosClient.ts` as the only shared Axios configuration.
- Keep API calls inside feature API files.
- Keep React Query logic inside custom hook files.
- Keep components focused on UI.
- Do not call Axios directly from components.
- Do not put React hooks inside API files.
- Do not put API URLs inside components.
- Return `response.data` from API functions.
- Store reusable domain models in `src/types`.
- Keep request payload types close to the API function unless they are reused elsewhere.

The preferred pattern is:

```txt
Component -> Custom Hook -> API Function -> Axios Client
```
