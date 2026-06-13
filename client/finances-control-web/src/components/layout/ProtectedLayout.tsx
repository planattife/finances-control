import type { FC } from 'react'
import { Outlet } from 'react-router-dom'
import Sidebar from './Sidebar'
import Topbar from './Topbar'
import styles from './ProtectedLayout.module.css'

type ProtectedLayoutProps = Record<string, never>

const ProtectedLayout: FC<ProtectedLayoutProps> = () => {
  return (
    <div className={styles.layout}>
      <Sidebar />
      <Topbar />
      <main className={styles.content}>
        <Outlet />
      </main>
    </div>
  )
}

export default ProtectedLayout
