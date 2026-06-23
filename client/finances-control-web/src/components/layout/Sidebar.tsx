import type { FC } from 'react'
import { NavLink } from 'react-router-dom'
import styles from './Sidebar.module.css'

type SidebarProps = Record<string, never>

type NavigationItem = {
  label: string
  to: string
}

const navigationItems: NavigationItem[] = [
  { label: 'Accounts', to: '/accounts' },
  { label: 'Banks', to: '/banks' },
  { label: 'Cards', to: '/cards' },
  { label: 'Transactions', to: '/transactions' },
]

const Sidebar: FC<SidebarProps> = () => {
  return (
    <aside className={styles.sidebar}>
      <div className={styles.brand} aria-label="Finances Control">
        <img className={styles.logo} src="/finances_control_logo.png" alt="Finances Control" />
      </div>

      <nav className={styles.navigation} aria-label="Main navigation">
        {navigationItems.map((item) => (
          <NavLink
            className={({ isActive }) =>
              isActive ? `${styles.navigationLink} ${styles.navigationLinkActive}` : styles.navigationLink
            }
            key={item.to}
            to={item.to}
          >
            {item.label}
          </NavLink>
        ))}
      </nav>
    </aside>
  )
}

export default Sidebar
