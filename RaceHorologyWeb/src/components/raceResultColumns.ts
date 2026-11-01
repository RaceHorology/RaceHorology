import type { RaceResultEntry } from '../api/types'
import { position, startNumber } from './format'
import type { Column } from './GroupedTable'

/** Columns of the race result; the times of the single runs are shown for the given number of runs */
export function raceResultColumns(runs: number, withClass = false): Column<RaceResultEntry>[] {
  return [
    { header: 'Platz', render: (e) => e.Position || '', className: 'text-center' },
    { header: 'StNr', render: startNumber, className: 'text-center' },
    { header: 'Name', render: (e) => e.Name },
    { header: 'Vorname', render: (e) => e.Firstname },
    { header: 'Kategorie', render: (e) => e.Sex, className: 'text-center' },
    { header: 'Jahrgang', render: (e) => e.Year, className: 'text-center' },
    { header: 'Verein', render: (e) => e.Club },
    ...(withClass ? [{ header: 'Klasse', render: (e: RaceResultEntry) => e.Class }] : []),
    ...Array.from({ length: runs }, (_, i): Column<RaceResultEntry> => ({
      header: `Zeit ${i + 1}`,
      render: (e) => `${e.Runtimes[i]?.Runtime ?? ''} ${position(e.Runtimes[i]?.Position)}`,
      className: 'text-end text-nowrap',
    })),
    { header: 'Zeit', render: (e) => e.Totaltime, className: 'text-end fw-bold' },
    { header: 'Diff', render: (e) => e.DiffToFirst, className: 'text-end' },
    { header: 'Bemerkung', render: (e) => e.DisqualText },
  ]
}
