import type { ButtonHTMLAttributes, ReactNode } from 'react'

type ButtonProps = {
  children: ReactNode
} & ButtonHTMLAttributes<HTMLButtonElement>

function Button({ children, className = '', type = 'button', ...props }: ButtonProps) {
  return (
    <button className={`button button--primary ${className}`.trim()} type={type} {...props}>
      {children}
    </button>
  )
}

export default Button
