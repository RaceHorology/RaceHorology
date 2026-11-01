import type { Participant } from '../api/types'

/** Start number for display; participants without start number get a placeholder */
export function startNumber(p: Participant) {
  return p.StartNumber === 0 ? '---' : p.StartNumber
}

/** Position in brackets as shown after a time, empty without position */
export function position(pos: number | undefined) {
  return pos ? `(${pos})` : ''
}
