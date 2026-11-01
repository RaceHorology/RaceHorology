import { useCallback, useEffect, useState } from 'react'
import Alert from 'react-bootstrap/Alert'
import { fetchRunResultList } from '../api/http'
import type { LiveData } from '../api/liveData'
import type { Grouping, MetaData, RunResultEntry, RunResultList } from '../api/types'
import { startNumber } from './format'
import GroupedTable, { type Column } from './GroupedTable'
import ListOptions from './ListOptions'

const columns: Column<RunResultEntry>[] = [
  { header: 'Platz', render: (e) => e.Position || '', className: 'text-center' },
  { header: 'StNr', render: startNumber, className: 'text-center' },
  { header: 'Name', render: (e) => e.Name },
  { header: 'Vorname', render: (e) => e.Firstname },
  { header: 'Kategorie', render: (e) => e.Sex, className: 'text-center' },
  { header: 'Jahrgang', render: (e) => e.Year, className: 'text-center' },
  { header: 'Verein', render: (e) => e.Club },
  { header: 'Klasse', render: (e) => e.Class },
  { header: 'Zeit', render: (e) => e.Runtime, className: 'text-end fw-bold' },
  { header: 'Diff', render: (e) => e.DiffToFirst, className: 'text-end' },
  { header: 'Bemerkung', render: (e) => e.DisqualText },
]

/** Index (counted from 0) of the run currently timed, the server sends it counted from 1 */
function currentRunIndex(liveData: LiveData): number | undefined {
  const run = Number(liveData.currentRaceRun?.run)
  return run > 0 ? run - 1 : undefined
}

export default function RunResultTab({ liveData, meta }: { liveData: LiveData; meta: MetaData['data'] | null }) {
  const currentRun = currentRunIndex(liveData)
  const [run, setRun] = useState(currentRun ?? 0)
  const [groupBy, setGroupBy] = useState<Grouping | ''>('')
  const [filterBy, setFilterBy] = useState('')
  const [results, setResults] = useState<RunResultList | null>(null)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(() => {
    fetchRunResultList(run, groupBy)
      .then((r) => {
        setResults(r)
        setError(null)
      })
      .catch((e: unknown) => setError(String(e)))
  }, [run, groupBy])

  // The server pushes new results of the current run; reload them with the selected grouping
  const liveRunResult = run === currentRun ? liveData.runResult : null
  useEffect(load, [load, liveRunResult])

  // A filter for a group that no longer exists (e.g. after changing the run) shows everything
  const activeFilter = results && filterBy in results.data ? filterBy : ''

  return (
    <>
      <ListOptions
        meta={meta}
        run={run}
        onRunChange={setRun}
        groupBy={groupBy}
        onGroupByChange={(g) => {
          setGroupBy(g)
          setFilterBy('')
        }}
        filterItems={results ? Object.keys(results.data) : []}
        filterBy={activeFilter}
        onFilterByChange={setFilterBy}
        onRefresh={load}
      />
      {error && <Alert variant="warning">{error}</Alert>}
      {results && (
        <GroupedTable
          data={results.data}
          columns={columns}
          rowKey={(e) => e.Id}
          rowClassName={(e) => (e.JustModified ? 'table-warning' : undefined)}
          filterBy={activeFilter}
        />
      )}
    </>
  )
}
