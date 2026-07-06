const CREDIT_CARD_DAY_LIMIT = 31

export function getCreditCardDayLimit() {
  return CREDIT_CARD_DAY_LIMIT
}

export function getCreditCardDayValidationMessage(fieldLabel: string) {
  return `${fieldLabel} must be a number from 1 to ${CREDIT_CARD_DAY_LIMIT}.`
}

export function isValidCreditCardDay(value: number | undefined) {
  return (
    Number.isInteger(value) &&
    value !== undefined &&
    value >= 1 &&
    value <= CREDIT_CARD_DAY_LIMIT
  )
}
