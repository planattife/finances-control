import Button from '../../components/Button'
import InputField from '../../components/InputField'
import './LoginPage.css'

function LoginPage() {
  return (
    <main className="login-page">
      <section className="login-page__form-panel" aria-label="Sign in">
        <div className="login-page__form-shell">
          <header className="login-page__header">
            <img
              className="login-page__logo"
              src="/assets/finances_control_logo.png"
              alt="Finances Control"
            />
          </header>

          <form className="login-form" onSubmit={(event) => event.preventDefault()}>
            <div className="login-form__intro">
              <h1>Welcome back</h1>
              <p>Sign in to continue managing your finances with clarity.</p>
            </div>

            <div className="login-form__fields">
              <InputField
                label="Email"
                name="email"
                placeholder="you@example.com"
                type="email"
                autoComplete="email"
              />

              <InputField
                label="Password"
                name="password"
                placeholder="Enter your password"
                type="password"
                autoComplete="current-password"
              />
            </div>

            <Button type="submit">Sign In</Button>
          </form>
        </div>
      </section>

      <aside className="login-page__visual-panel" aria-hidden="true">
        <div className="login-page__visual-overlay" />
        <img
          className="login-page__illustration"
          src="/assets/login_page_illustration.png"
          alt=""
        />
      </aside>
    </main>
  )
}

export default LoginPage
