import { useEffect, useState } from 'react'
import Form from 'react-bootstrap/Form'
import Table from 'react-bootstrap/Table'
import type { LiveData } from '../api/liveData'
import type { GroupedData, MetaData, Participant, RaceResultEntry, RaceResultRuntime } from '../api/types'
import GroupedTable, { type Column } from './GroupedTable'

/** Number of participants of the same group in a row after which the race result follows that group */
const AUTO_FILTER_FINISHERS = 3

/** Finds the entry of a participant in a grouped list; returns the group name and the entry */
function findEntry<T extends Participant>(data: GroupedData<T> | undefined, p: Participant): [string, T] | undefined {
  for (const [group, entries] of Object.entries(data ?? {})) {
    const entry = entries.find((e) => e.StartNumber === p.StartNumber)
    if (entry)
      return [group, entry]
  }
  return undefined
}

function startNumber(p: Participant) {
  return p.StartNumber === 0 ? '---' : p.StartNumber
}

function position(pos: number | undefined) {
  return pos ? `(${pos})` : ''
}

interface LiveRow {
  participant: Participant
  /** Times of all runs so far, from the race result */
  runtimes: RaceResultRuntime[]
  time?: string | null
  position?: number
  remark?: string | null
}

interface LiveSectionProps {
  title: string
  rows: LiveRow[]
  emptyText: string
  runs: number
}

const participantHeaders = ['Name', 'Vorname', 'Kategorie', 'Jahrgang', 'Verein']

function LiveSection({ title, rows, emptyText, runs }: LiveSectionProps) {
  const columns = 1 + participantHeaders.length + runs + 2
  return (
    <tbody>
      <tr className="table-secondary">
        <th colSpan={columns}>{title}</th>
      </tr>
      {rows.length === 0 && (
        <tr>
          <td colSpan={columns} className="text-center text-body-secondary fst-italic">
            {emptyText}
          </td>
        </tr>
      )}
      {rows.map(({ participant: p, runtimes, time, position: pos, remark }) => (
        <tr key={p.Id}>
          <td className="text-center">{startNumber(p)}</td>
          <td>{p.Name}</td>
          <td>{p.Firstname}</td>
          <td className="text-center">{p.Sex}</td>
          <td className="text-center">{p.Year}</td>
          <td>{p.Club}</td>
          {Array.from({ length: runs }, (_, i) => (
            <td key={i} className="text-end text-nowrap">
              {runtimes[i]?.Runtime} {position(runtimes[i]?.Position)}
            </td>
          ))}
          <td className="text-end text-nowrap fw-bold">
            {time} {position(pos)}
          </td>
          <td>{remark}</td>
        </tr>
      ))}
    </tbody>
  )
}

function raceResultColumns(runs: number): Column<RaceResultEntry>[] {
  return [
    { header: 'Platz', render: (e) => e.Position || '', className: 'text-center' },
    { header: 'StNr', render: startNumber, className: 'text-center' },
    { header: 'Name', render: (e) => e.Name },
    { header: 'Vorname', render: (e) => e.Firstname },
    { header: 'Kategorie', render: (e) => e.Sex, className: 'text-center' },
    { header: 'Jahrgang', render: (e) => e.Year, className: 'text-center' },
    { header: 'Verein', render: (e) => e.Club },
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

export default function LiveTab({ liveData, meta }: { liveData: LiveData; meta: MetaData['data'] | null }) {
  const { onStart, onTrack, justFinished, runResult, raceResult } = liveData
  const [filterBy, setFilterBy] = useState('')

  // Times of the single runs are only of interest if the race has more than one run
  const totalRuns = meta?.runs ?? 1
  const runs = totalRuns > 1 ? totalRuns : 0

  const withRuntimes = (p: Participant): LiveRow => ({
    participant: p,
    runtimes: findEntry(raceResult?.data, p)?.[1].Runtimes ?? [],
  })

  // Finished participants show their total time, or the time of the current run as long as there is no total time
  const finishedRows = justFinished.map((p): LiveRow => {
    const race = findEntry(raceResult?.data, p)?.[1]
    if (race?.Totaltime || race?.DisqualText)
      return { ...withRuntimes(p), time: race.Totaltime, position: race.Position, remark: race.DisqualText }
    const run = findEntry(runResult?.data, p)?.[1]
    return { ...withRuntimes(p), time: run?.Runtime, position: run?.Position, remark: run?.DisqualText }
  })

  // Follow the group (e.g. class) of the participants who finished last
  const finishedGroups = justFinished.map((p) => findEntry(raceResult?.data, p)?.[0])
  const autoFilter =
    finishedGroups.length >= AUTO_FILTER_FINISHERS && finishedGroups.every((g) => g && g === finishedGroups[0])
      ? finishedGroups[0]
      : undefined

  useEffect(() => {
    if (autoFilter)
      setFilterBy(autoFilter)
  }, [autoFilter])

  const groups = Object.keys(raceResult?.data ?? {}).filter((g) => g !== '')
  const activeFilter = groups.includes(filterBy) ? filterBy : ''

  return (
    <>
      <Table size="sm" responsive className="mb-4">
        <thead>
          <tr>
            <th className="text-center">StNr</th>
            {participantHeaders.map((h) => (
              <th key={h} className={h === 'Kategorie' || h === 'Jahrgang' ? 'text-center' : undefined}>
                {h}
              </th>
            ))}
            {Array.from({ length: runs }, (_, i) => (
              <th key={i} className="text-end">
                Zeit {i + 1}
              </th>
            ))}
            <th className="text-end">Zeit</th>
            <th></th>
          </tr>
        </thead>
        <LiveSection
          title="Am Start"
          rows={[...onStart].reverse().map(withRuntimes)}
          emptyText="keine weiteren Starter"
          runs={runs}
        />
        <LiveSection
          title="Im Lauf"
          rows={[...onTrack].reverse().map((p) => ({ ...withRuntimes(p), time: p.Runtime, remark: p.DisqualText }))}
          emptyText="keine Läufer gestartet"
          runs={runs}
        />
        <LiveSection title="Im Ziel" rows={finishedRows} emptyText="noch kein Läufer im Ziel" runs={runs} />
      </Table>

      {raceResult && (
        <section>
          <div className="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
            <h2 className="h4 mb-0">Ergebnis</h2>
            <Form.Select
              aria-label="Filter"
              className="w-auto"
              value={activeFilter}
              disabled={groups.length === 0}
              onChange={(e) => setFilterBy(e.target.value)}
            >
              <option value="">Alle</option>
              {groups.map((g) => (
                <option key={g} value={g}>
                  {g}
                </option>
              ))}
            </Form.Select>
          </div>
          <GroupedTable
            data={raceResult.data}
            columns={raceResultColumns(runs)}
            rowKey={(e) => e.Id}
            rowClassName={(e) => (e.JustModified ? 'table-warning' : undefined)}
            filterBy={activeFilter}
          />
        </section>
      )}
    </>
  )
}
