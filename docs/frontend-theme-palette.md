# Theme and Color Palette

This document defines the project standard for using themes, color palettes, and design tokens.

The goal is to avoid hardcoded colors spread across the codebase and ensure visual consistency across all screens.

---

## Goal

All reusable colors in the project should be centralized in the theme file.

Instead of using values directly inside components, such as:

```tsx
color: "#FFFFFF";
backgroundColor: "#2563EB";
border: "1px solid #E5E7EB";
```

use references from the theme:

```tsx
color: theme.colors.text.inverse;
backgroundColor: theme.colors.primary.main;
border: `1px solid ${theme.colors.border.default}`;
```

This makes the project easier to maintain and allows future visual changes to be made from a single place.

---

## Theme location

The main theme file should be located at:

```txt
src/theme/theme.js
```

If the project already has a more appropriate folder, such as `src/styles`, the theme can be placed at:

```txt
src/styles/theme.js
```

The important rule is that there must be a clear, centralized place for visual tokens.

---

## Recommended structure

The `theme.js` file should use a semantic structure.

Example:

```js
export const theme = {
  colors: {
    primary: {
      main: "#2563EB",
      hover: "#1D4ED8",
      active: "#1E40AF",
      light: "#DBEAFE",
      dark: "#1E3A8A",
      contrast: "#FFFFFF",
    },

    background: {
      app: "#F9FAFB",
      surface: "#FFFFFF",
      surfaceAlt: "#F3F4F6",
    },

    text: {
      primary: "#111827",
      secondary: "#4B5563",
      muted: "#6B7280",
      disabled: "#9CA3AF",
      inverse: "#FFFFFF",
    },

    border: {
      default: "#E5E7EB",
      strong: "#D1D5DB",
      focus: "#2563EB",
    },

    state: {
      success: "#16A34A",
      warning: "#F59E0B",
      error: "#DC2626",
      info: "#0284C7",
    },

    component: {
      disabledBackground: "#E5E7EB",
      disabledText: "#9CA3AF",
      hoverBackground: "#F3F4F6",
      focusRing: "rgba(37, 99, 235, 0.35)",
    },

    overlay: {
      default: "rgba(0, 0, 0, 0.5)",
    },

    shadow: {
      default: "rgba(0, 0, 0, 0.1)",
    },
  },
};
```

The real project palette should be based on the colors already used in the current interface, especially the `LoginPage`.

---

## Usage rules

### 1. Do not use hardcoded colors in components

Avoid:

```tsx
<button style={{ backgroundColor: "#2563EB", color: "#FFFFFF" }}>
  Sign in
</button>
```

Prefer:

```tsx
<button
  style={{
    backgroundColor: theme.colors.primary.main,
    color: theme.colors.primary.contrast,
  }}
>
  Sign in
</button>
```

---

### 2. Use semantic names

Colors should describe their visual purpose, not only their appearance.

Avoid names like:

```js
blue1
blue2
gray1
gray2
darkBlue
lightGray
```

Prefer names like:

```js
primary.main
primary.hover
background.surface
text.secondary
border.default
state.error
```

This makes the code easier to understand.

Example:

```tsx
color: theme.colors.state.error;
```

is clearer than:

```tsx
color: theme.colors.red1;
```

---

### 3. New colors must be added to the theme

If a new screen needs a color that does not exist yet, first check whether an existing theme color already fits the use case.

If the new color is really necessary, add it to `theme.js` using a semantic name.

Avoid adding colors directly inside components.

---

## Palette categories

### `primary`

Used for main actions, primary buttons, important links, focus states, and highlighted elements.

Examples:

```js
theme.colors.primary.main
theme.colors.primary.hover
theme.colors.primary.active
theme.colors.primary.contrast
```

---

### `background`

Used for general application backgrounds.

Examples:

```js
theme.colors.background.app
theme.colors.background.surface
theme.colors.background.surfaceAlt
```

* `app`: main application background
* `surface`: cards, containers, and elevated areas
* `surfaceAlt`: secondary areas or alternative blocks

---

### `text`

Used for text colors.

Examples:

```js
theme.colors.text.primary
theme.colors.text.secondary
theme.colors.text.muted
theme.colors.text.disabled
theme.colors.text.inverse
```

* `primary`: main text
* `secondary`: supporting text
* `muted`: low-emphasis text
* `disabled`: disabled text
* `inverse`: text displayed over dark or colored backgrounds

---

### `border`

Used for borders, dividers, and outlines.

Examples:

```js
theme.colors.border.default
theme.colors.border.strong
theme.colors.border.focus
```

---

### `state`

Used for system feedback.

Examples:

```js
theme.colors.state.success
theme.colors.state.warning
theme.colors.state.error
theme.colors.state.info
```

These colors should be used for messages, alerts, validations, badges, and system states.

---

### `component`

Used for common component states.

Examples:

```js
theme.colors.component.disabledBackground
theme.colors.component.disabledText
theme.colors.component.hoverBackground
theme.colors.component.focusRing
```

---

## Example usage in React

```tsx
import { theme } from "@/theme/theme";

export function LoginButton() {
  return (
    <button
      style={{
        backgroundColor: theme.colors.primary.main,
        color: theme.colors.primary.contrast,
        border: "none",
      }}
    >
      Sign in
    </button>
  );
}
```

---

## Example usage with CSS Modules

If the project uses CSS Modules or plain CSS, it is acceptable to use CSS variables.

Example:

```css
:root {
  --color-primary-main: #2563eb;
  --color-primary-hover: #1d4ed8;

  --color-background-app: #f9fafb;
  --color-background-surface: #ffffff;

  --color-text-primary: #111827;
  --color-text-secondary: #4b5563;
  --color-text-inverse: #ffffff;

  --color-border-default: #e5e7eb;
}
```

Usage:

```css
.button {
  background-color: var(--color-primary-main);
  color: var(--color-text-inverse);
}

.button:hover {
  background-color: var(--color-primary-hover);
}
```

Even when using CSS variables, names should remain semantic.

---

## When to use `theme.js` or CSS variables

Use `theme.js` when colors are applied directly in React code or style objects.

Use CSS variables when colors are applied in `.css`, `.scss`, or `.module.css` files.

The project can use both approaches, as long as they stay synchronized and follow the same semantic naming convention.

---

## Checklist for new screens

Before finishing a new screen or component, check that:

* There are no unnecessary hardcoded hex colors.
* Colors come from the theme or from CSS variables.
* New colors were added to the theme using semantic names.
* Hover, active, focus, and disabled states use theme tokens.
* Error, success, warning, and info messages use `state`.
* The screen remains visually consistent with the rest of the project.

---

## Best practices

* Reuse existing colors before creating new ones.
* Name colors by purpose, not appearance.
* Avoid creating unused variations.
* Do not change a global color without checking its impact on existing screens.
* Use `primary` only for main actions and key highlights.
* Use `state.error`, `state.success`, `state.warning`, and `state.info` for feedback.
* Keep the palette simple and predictable.

---

## Refactoring example

Before:

```tsx
<div
  style={{
    backgroundColor: "#FFFFFF",
    color: "#111827",
    border: "1px solid #E5E7EB",
  }}
>
  Content
</div>
```

After:

```tsx
<div
  style={{
    backgroundColor: theme.colors.background.surface,
    color: theme.colors.text.primary,
    border: `1px solid ${theme.colors.border.default}`,
  }}
>
  Content
</div>
```

---

## Main rule

No screen should define its own isolated color palette.

The single source of truth for reusable colors should be the project theme.
