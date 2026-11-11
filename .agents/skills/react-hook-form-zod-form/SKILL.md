---
name: react-hook-form-zod-form
description: Create or refactor React TypeScript frontend forms using React Hook Form, Zod, zodResolver, inferred schema types, and separated form components.
---

# React Hook Form + Zod Forms

Use this skill when creating a new frontend form or refactoring an existing React + TypeScript form to use React Hook Form, Zod, and `@hookform/resolvers/zod`.

## Core Rules

- Use a real HTML `<form>` element for every form.
- Submit with `<form onSubmit={handleSubmit(handleFormSubmit)}>`; do not rely on `<div>`, button `onClick`, or manual submit handlers as the form boundary.
- Place each form in a separated component folder:

```text
Components/forms/{FormComponentName}/
├── {FormComponentName}.tsx
```

- Create `types.ts` only when extra reusable or local prop types are needed.
- Keep React Hook Form and Zod logic inside the form component.
- Keep parent components free of `register`, `control`, `errors`, `handleSubmit`, schemas, and validation logic.
- Let parent components import and render the form, passing props such as `defaultValues`, `onSubmit`, `isLoading`, initial data, or callbacks.
- Preserve existing behavior: do not remove fields, change business rules, or alter layout unless necessary for the form architecture.

## Refactoring Workflow

Before changing an existing form:

- Identify all fields and their current names, values, types, and UI controls.
- Identify local state used only for input values and replace it with React Hook Form when possible.
- Keep state that is not part of the form, such as modal visibility, external loading, tabs, filters, or display-only UI state.
- Identify current validation and move rules expressible as field or object validation into Zod.
- Keep manual validation only for rules that truly depend on external state or asynchronous checks.
- Identify submit behavior and preserve the data shape expected by the parent or API layer.
- Move form-specific logic into `Components/forms/{FormComponentName}` and update the parent to render the new form component.

## Schema And Types

Create schemas with `z.object` and use clear, user-friendly validation messages.

```tsx
import { z } from "zod";

export const exampleFormSchema = z.object({
  name: z.string().min(1, "Name is required"),
  email: z.string().email("Invalid email"),
});

export type ExampleFormData = z.infer<typeof exampleFormSchema>;
```

Infer TypeScript form data from the schema. Do not duplicate form data types manually when they can be inferred:

```tsx
type FormData = z.infer<typeof formSchema>;
```

## Form Component Pattern

Use React Hook Form with `zodResolver`:

```tsx
const {
  register,
  handleSubmit,
  control,
  formState: { errors, isSubmitting },
} = useForm<FormData>({
  resolver: zodResolver(formSchema),
  defaultValues,
});
```

Use this submit wrapper:

```tsx
const handleFormSubmit = async (data: FormData) => {
  await onSubmit(data);
};
```

Use props shaped like this unless the existing use case requires additional values:

```tsx
type UserFormProps = {
  defaultValues?: Partial<FormData>;
  onSubmit: (data: FormData) => Promise<void> | void;
  isLoading?: boolean;
};
```

Disable submit actions with `isSubmitting` and any external loading prop when appropriate.

## Field Rules

- Use `register` for standard inputs that work directly with native input events.
- Use `Controller` for controlled fields, including custom select components, date pickers, complex checkboxes, input masks, and UI components that do not work directly with `register`.
- Display validation errors close to their respective fields.
- Preserve existing labels, placeholders, disabled states, option lists, masks, formatting behavior, and visual layout unless a small structural change is required to support the form.
- Avoid scattered manual validation such as `if (!name) setError(...)` when the same rule can be represented in Zod.

## Parent Component Pattern

After refactoring, the parent should only coordinate outer behavior:

- Hold modal/page state and external loading state.
- Prepare initial/default values if needed.
- Provide the submit callback.
- Render the form component.

The parent should not import Zod schemas or call React Hook Form APIs unless it is itself the separated form component.
