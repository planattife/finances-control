import type { AnchorHTMLAttributes, ReactNode } from 'react'

type LinkProps = {
  children: ReactNode
} & AnchorHTMLAttributes<HTMLAnchorElement>

function Link({ children, className = '', href = '#', ...props }: LinkProps) {
  return (
    <a className={`text-link ${className}`.trim()} href={href} {...props}>
      {children}
    </a>
  )
}

export default Link
