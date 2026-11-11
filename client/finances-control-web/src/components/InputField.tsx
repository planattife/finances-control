import type { InputHTMLAttributes } from 'react'

type InputFieldProps = {
  label: string
  type: 'email' | 'password'
  placeholder: string
} & InputHTMLAttributes<HTMLInputElement>

function InputField({ label, id, className = '', ...props }: InputFieldProps) {
  const inputId = id ?? props.name ?? label.toLowerCase().replace(/\s+/g, '-')

  return (
    <div className={`input-field ${className}`.trim()}>
      <label className="input-field__label" htmlFor={inputId}>
        {label}
      </label>
      <input className="input-field__control" id={inputId} {...props} />
    </div>
  )
}

export default InputField
