import { useEffect, useState } from 'react'
import Form from 'react-bootstrap/Form'
import Table from 'react-bootstrap/Table'
import { currentRunIndex, type LiveData } from '../api/liveData'
import type { GroupedData, MetaData, Participant, RaceResultRuntime } from '../api/types'
import { position, startNumber } from './format'
import GroupedTable from './GroupedTable'
import { NameCell, NameHeader, ShortHeader } from './participantColumns'
import { raceResultColumns, Remark, RunTimes } from './raceResultColumns'
import { useCompactLevel } from './useCompactLevel'

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

/** Number of columns without the run times: start number, name, first name, category, year, club, time, remark */
const FIXED_COLUMNS = 8

function LiveHeaderRow({ runs }: { runs: number }) {
  return (
    <tr>
      <th className="text-center">StNr</th>
      <th>
        <NameHeader />
      </th>
      <th className="rh-c1-col">Vorname</th>
      <th className="text-center">
        <ShortHeader long="Kategorie" short="Kat." />
      </th>
      <th className="text-center">
        <ShortHeader long="Jahrgang" short="Jg." />
      </th>
      <th className="rh-c1-col">Verein</th>
      {Array.from({ length: runs }, (_, i) => (
        <th key={i} className="text-end rh-c2-col">
          Zeit {i + 1}
        </th>
      ))}
      <th className="text-end">Zeit</th>
      <th className="rh-c2-col"></th>
    </tr>
  )
}

function LiveSection({ title, rows, emptyText, runs }: LiveSectionProps) {
  const columns = FIXED_COLUMNS + runs
  return (
    <tbody>
      <tr className="rh-section-heading">
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
          <td className="rh-name">
            <NameCell p={p} />
          </td>
          <td className="rh-c1-col">{p.Firstname}</td>
          <td className="text-center">{p.Sex}</td>
          <td className="text-center">{p.Year}</td>
          <td className="rh-c1-col">{p.Club}</td>
          {Array.from({ length: runs }, (_, i) => (
            <td key={i} className="text-end text-nowrap rh-c2-col">
              {runtimes[i]?.Runtime} {position(runtimes[i]?.Position)}
            </td>
          ))}
          <td className="text-end text-nowrap fw-bold">
            {time} {position(pos)}
            <Remark text={remark} />
            <RunTimes runtimes={runtimes} runs={runs} />
          </td>
          <td className="rh-c2-col">{remark}</td>
        </tr>
      ))}
    </tbody>
  )
}

export default function LiveTab({ liveData, meta }: { liveData: LiveData; meta: MetaData['data'] | null }) {
  const { onStart, onTrack, justFinished, raceResult } = liveData
  const currentRun = currentRunIndex(liveData)
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
    const run = currentRun !== undefined ? race?.Runtimes[currentRun] : undefined
    return { ...withRuntimes(p), time: run?.Runtime, position: run?.Position }
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

  const compact = useCompactLevel([onStart, onTrack, justFinished, raceResult, runs])

  const groups = Object.keys(raceResult?.data ?? {}).filter((g) => g !== '')
  const activeFilter = groups.includes(filterBy) ? filterBy : ''

  return (
    <>
      <div ref={compact.ref} className={compact.className}>
        <Table hover bordered size="sm" responsive className="rh-table mb-4">
          <thead>
            <LiveHeaderRow runs={runs} />
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
      </div>

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
            rowClassName={(e) => (e.JustModified ? 'just-modified' : undefined)}
            filterBy={activeFilter}
          />
        </section>
      )}
    </>
  )
}
