import type { RaceResultEntry, RaceResultRuntime } from '../api/types'
import { position, startNumber } from './format'
import type { Column } from './GroupedTable'
import { participantColumns, ShortHeader } from './participantColumns'

/**
 * Times of the single runs below the total time, shown on narrow screens instead of a column per run
 * (see .rh-narrow-only in theme.css)
 */
export function NarrowRunTimes({ runtimes, runs }: { runtimes: RaceResultRuntime[]; runs: number }) {
  return (
    <div className="rh-narrow-only small fw-normal text-body-secondary">
      {Array.from({ length: runs }, (_, i) =>
        runtimes[i]?.Runtime ? (
          <div key={i}>
            {i + 1}. {runtimes[i].Runtime} {position(runtimes[i].Position)}
          </div>
        ) : null,
      )}
    </div>
  )
}

/** Remark (e.g. disqualification) below the time, shown on narrow screens instead of an own column */
export function NarrowRemark({ text }: { text: string | null | undefined }) {
  return text ? <div className="rh-narrow-only fw-normal">{text}</div> : null
}

/** Columns of the race result; the times of the single runs are shown for the given number of runs */
export function raceResultColumns(runs: number, withClass = false): Column<RaceResultEntry>[] {
  return [
    {
      header: 'Platz',
      headerContent: <ShortHeader long="Platz" short="Pl." />,
      render: (e) => e.Position || '',
      className: 'text-center',
    },
    { header: 'StNr', render: startNumber, className: 'text-center' },
    ...participantColumns<RaceResultEntry>(withClass),
    ...Array.from({ length: runs }, (_, i): Column<RaceResultEntry> => ({
      header: `Zeit ${i + 1}`,
      render: (e) => `${e.Runtimes[i]?.Runtime ?? ''} ${position(e.Runtimes[i]?.Position)}`,
      className: 'text-end text-nowrap rh-wide-only',
    })),
    {
      header: 'Zeit',
      render: (e) => (
        <>
          {e.Totaltime}
          <NarrowRemark text={e.DisqualText} />
          <NarrowRunTimes runtimes={e.Runtimes} runs={runs} />
        </>
      ),
      className: 'text-end text-nowrap fw-bold',
    },
    { header: 'Diff', render: (e) => e.DiffToFirst, className: 'text-end' },
    { header: 'Bemerkung', render: (e) => e.DisqualText, className: 'rh-wide-only' },
  ]
}
