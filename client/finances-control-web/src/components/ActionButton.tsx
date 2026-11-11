import type { ButtonHTMLAttributes, ReactNode } from 'react'
import styles from './ActionButton.module.css'

type ActionButtonVariant = 'primary' | 'secondary'

type ActionButtonProps = {
  children: ReactNode
  dangerHover?: boolean
  icon?: ReactNode
  variant?: ActionButtonVariant
} & ButtonHTMLAttributes<HTMLButtonElement>

function ActionButton({
  children,
  className = '',
  dangerHover = false,
  icon,
  type = 'button',
  variant = 'secondary',
  ...props
}: ActionButtonProps) {
  const buttonClassName = `${styles.button} ${styles[variant]} ${dangerHover ? styles.dangerHover : ''} ${className}`.trim()

  return (
    <button className={buttonClassName} type={type} {...props}>
      {icon && <span className={styles.icon}>{icon}</span>}
      {children}
    </button>
  )
}

export default ActionButton
