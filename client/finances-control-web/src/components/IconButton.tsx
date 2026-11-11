import type { ButtonHTMLAttributes, ReactNode } from 'react'
import styles from './IconButton.module.css'

type IconButtonProps = {
  label: string
  children: ReactNode
} & Omit<ButtonHTMLAttributes<HTMLButtonElement>, 'aria-label'>

function IconButton({ children, className = '', label, type = 'button', ...props }: IconButtonProps) {
  return (
    <button aria-label={label} className={`${styles.button} ${className}`.trim()} type={type} {...props}>
      <span className={styles.icon}>{children}</span>
    </button>
  )
}

export default IconButton
