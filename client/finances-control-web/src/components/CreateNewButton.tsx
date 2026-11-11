import type { ButtonHTMLAttributes } from 'react'
import styles from './CreateNewButton.module.css'

type CreateNewButtonProps = ButtonHTMLAttributes<HTMLButtonElement>

function CreateNewButton({ children = 'Create New', className = '', type = 'button', ...props }: CreateNewButtonProps) {
  return (
    <button className={`${styles.button} ${className}`.trim()} type={type} {...props}>
      {children}
    </button>
  )
}

export default CreateNewButton
