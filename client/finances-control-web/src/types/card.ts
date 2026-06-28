import type { PagedResponse } from './pagedResponse'

export const CardType = {
  Debit: 0,
  Credit: 1,
} as const

export type CardType = (typeof CardType)[keyof typeof CardType]

export const cardTypeOptions: Array<{ label: string; value: CardType }> = [
  { label: 'Debit', value: CardType.Debit },
  { label: 'Credit', value: CardType.Credit },
]

export function getCardTypeLabel(cardType: number) {
  return cardTypeOptions.find((option) => option.value === cardType)?.label ?? 'Unknown'
}

export const CardNetwork = {
  Visa: 1,
  Mastercard: 2,
  Elo: 3,
  AmericanExpress: 4,
  Hipercard: 5,
} as const

export type CardNetwork = (typeof CardNetwork)[keyof typeof CardNetwork]

export const cardNetworkOptions: Array<{ label: string; value: CardNetwork }> = [
  { label: 'Visa', value: CardNetwork.Visa },
  { label: 'Mastercard', value: CardNetwork.Mastercard },
  { label: 'Elo', value: CardNetwork.Elo },
  { label: 'American Express', value: CardNetwork.AmericanExpress },
  { label: 'Hipercard', value: CardNetwork.Hipercard },
]

export function getCardNetworkLabel(cardNetwork: number) {
  return cardNetworkOptions.find((option) => option.value === cardNetwork)?.label ?? 'Unknown'
}

export type Card = {
  id: number
  cardNumber: string
  cardType: CardType
  cardNetwork: CardNetwork
  bankId: number
  cardName: string | null
}

export type PagedCardResponse = PagedResponse<Card>
