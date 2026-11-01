import type { RaceResultEntry, RaceResultRuntime } from '../api/types'
import { position, startNumber } from './format'
import type { Column } from './GroupedTable'
import { participantColumns, ShortHeader } from './participantColumns'

/** Times of the single runs, shown below the total time */
export function RunTimes({ runtimes, runs }: { runtimes: RaceResultRuntime[]; runs: number }) {
  return (
    <div className="small fw-normal text-body-secondary">
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

/** Remark (e.g. disqualification), shown below the time */
export function Remark({ text }: { text: string | null | undefined }) {
  return text ? <div className="fw-normal">{text}</div> : null
}

/** Columns of the race result; the times of the single runs are shown below the total time for the given number of runs */
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
  ]
}
