# Role & Context
You are an expert UI/UX Designer and a Senior Frontend Engineer specializing in React.js, TypeScript, and clean, component-driven architecture. 

Your task is to build a modern, high-converting, and visually striking Login Page for a **financial control application**. The design should be inspired by the split-screen layout seen in the reference image (a clean form on the left, a vibrant illustrative graphic on the right).

# Asset Declarations
Please use the following specific asset paths in your implementation:
* **Application Logo:** `/assets/finances_control_logo.png`
* **Right-Side Illustration:** `/assets/login_page_illustration.png`

# Design System & Theming (Financial Context)
Since this is a financial control application, the UI must evoke trust, security, clarity, and professionalism. 
* **Primary Palette:** Choose colors that make sense for finance. Use professional deep blues, slates, or charcoals for structure/backgrounds, complemented by trustworthy greens or sharp modern emeralds/teals as your primary brand and action accents (e.g., buttons, active states).
* **Layout:** A crisp 50/50 split-screen layout on desktop views. 
    * **Left Column:** A centered, high-contrast, distraction-free login form with plenty of whitespace.
    * **Right Column:** A full-height, rich visual container displaying the `/assets/login_page_illustration.png` asset, styled gracefully to scale on different screen sizes.
* **Responsiveness:** On mobile/smaller screens, smoothly collapse the right-side illustration and present a clean, centered card or full-screen layout of the login form.

# Engineering & Component Architecture
You must build this with reusability in mind. Do not hardcode the form elements directly into a single page file. Instead, abstract them into isolated, strongly-typed TypeScript components so we can reuse them across the rest of the application.

Please create the following atomic components using React and TypeScript:
1.  **`<InputField />`:** A reusable text input component. It should support props for `label`, `type` (email/password), `placeholder`, and standard HTML input attributes.
2.  **`<Button />`:** A reusable button component implementing the primary financial color scheme, supporting standard button states (default, hover, active).
3.  **`<Link />`:** A reusable inline text link component with clean hover transition states.

# Page Specifications & Constraints
Construct the login page using the reusable components defined above. For this iteration, **focus strictly on the UI and visual presentation**. Do not implement state handling, form validation logic, API fetch requests, or navigation behaviors. All buttons and links should be non-functional (empty or preventing default behavior).

The page must include exactly these elements, matching the hierarchy of the reference layout:
* **Logo Container:** Displays the application logo (`/assets/finances_control_logo.png`).
* **Header / Quick Link:** A "Don't have an account? Create one" call-to-action text using your reusable `<Link />` component (configured with an empty or `#` href).
* **Email Input:** Utilizing `<InputField />`.
* **Password Input:** Utilizing `<InputField />`.
* **Login Button:** Utilizing `<Button />` with clear typography (e.g., "Log In" or "Sign In").

# Deliverables
Please provide clean, production-ready TypeScript (`.tsx`) code, separating the reusable components from the main page layout. Use modern CSS (such as Tailwind CSS, Styled Components, or standard CSS Modules—whichever fits the workspace best) ensuring sleek transitions, proper spacing, and pixel-perfect alignment.