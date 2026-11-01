import { useCallback, useEffect, useState } from 'react'
import Alert from 'react-bootstrap/Alert'
import { fetchRunResultList } from '../api/http'
import { currentRunIndex, type LiveData } from '../api/liveData'
import type { Grouping, MetaData, RunResultEntry, RunResultList } from '../api/types'
import { startNumber } from './format'
import GroupedTable, { type Column } from './GroupedTable'
import ListOptions from './ListOptions'
import { Remark } from './raceResultColumns'
import { participantColumns, ShortHeader } from './participantColumns'

const columns: Column<RunResultEntry>[] = [
  {
    header: 'Platz',
    headerContent: <ShortHeader long="Platz" short="Pl." />,
    render: (e) => e.Position || '',
    className: 'text-center',
  },
  { header: 'StNr', render: startNumber, className: 'text-center' },
  ...participantColumns<RunResultEntry>(true),
  {
    header: 'Zeit',
    render: (e) => (
      <>
        {e.Runtime}
        <Remark text={e.DisqualText} />
      </>
    ),
    className: 'text-end fw-bold',
  },
  { header: 'Diff', render: (e) => e.DiffToFirst, className: 'text-end' },
  { header: 'Bemerkung', render: (e) => e.DisqualText, className: 'rh-c2-col' },
]

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

  // The server pushes the race result whenever a time changes, but not the results of the run;
  // reload them if the current run is shown
  const liveUpdate = run === currentRun ? liveData.raceResult : null
  useEffect(load, [load, liveUpdate])

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
          rowClassName={(e) => (e.JustModified ? 'just-modified' : undefined)}
          filterBy={activeFilter}
        />
      )}
    </>
  )
}
