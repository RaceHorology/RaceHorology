import type { RaceResultEntry, RaceResultRuntime } from '../api/types'
import { position, startNumber } from './format'
import type { Column } from './GroupedTable'
import { participantColumns, ShortHeader } from './participantColumns'

/** Times of the single runs below the total time, if they do not fit in columns of their own (compact level 2) */
export function RunTimes({ runtimes, runs }: { runtimes: RaceResultRuntime[]; runs: number }) {
  return (
    <div className="rh-c2-inline small fw-normal text-body-secondary">
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

/** Remark (e.g. disqualification) below the time, if it does not fit in a column of its own (compact level 2) */
export function Remark({ text }: { text: string | null | undefined }) {
  return text ? <div className="rh-c2-inline fw-normal">{text}</div> : null
}

/** Columns of the race result; the times of the single runs are shown for the given number of runs */
export function raceResultColumns(runs: number): Column<RaceResultEntry>[] {
  return [
    {
      header: 'Platz',
      headerContent: <ShortHeader long="Platz" short="Pl." />,
      render: (e) => e.Position || '',
      className: 'text-center',
    },
    { header: 'StNr', render: startNumber, className: 'text-center' },
    ...participantColumns<RaceResultEntry>(),
    ...Array.from({ length: runs }, (_, i): Column<RaceResultEntry> => ({
      header: `Zeit ${i + 1}`,
      render: (e) => `${e.Runtimes[i]?.Runtime ?? ''} ${position(e.Runtimes[i]?.Position)}`,
      className: 'text-end text-nowrap rh-c2-col',
    })),
    {
      header: 'Zeit',
      render: (e) => (
        <>
          {e.Totaltime}
          <Remark text={e.DisqualText} />
          <RunTimes runtimes={e.Runtimes} runs={runs} />
        </>
      ),
      className: 'text-end text-nowrap fw-bold',
    },
    { header: 'Diff', render: (e) => e.DiffToFirst, className: 'text-end' },
    { header: 'Bemerkung', render: (e) => e.DisqualText, className: 'rh-c2-col' },
  ]
}
