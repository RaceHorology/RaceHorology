import { useCallback, useEffect, useState } from 'react'
import Alert from 'react-bootstrap/Alert'
import { fetchStartList } from '../api/http'
import type { Grouping, MetaData, StartList, StartListEntry } from '../api/types'
import GroupedTable, { type Column } from './GroupedTable'
import ListOptions from './ListOptions'
import { participantColumns } from './participantColumns'

const columns: Column<StartListEntry>[] = [
  { header: 'StNr', render: (e) => e.StartNumber, className: 'text-center' },
  ...participantColumns<StartListEntry>(true),
]

export default function StartListTab({ meta }: { meta: MetaData['data'] | null }) {
  const [run, setRun] = useState(0)
  const [groupBy, setGroupBy] = useState<Grouping | ''>('')
  const [filterBy, setFilterBy] = useState('')
  const [startList, setStartList] = useState<StartList | null>(null)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(() => {
    fetchStartList(run, groupBy)
      .then((s) => {
        setStartList(s)
        setError(null)
      })
      .catch((e: unknown) => setError(String(e)))
  }, [run, groupBy])

  useEffect(load, [load])

  // A filter for a group that no longer exists (e.g. after changing the run) shows everything
  const activeFilter = startList && filterBy in startList.data ? filterBy : ''

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
        filterItems={startList ? Object.keys(startList.data) : []}
        filterBy={activeFilter}
        onFilterByChange={setFilterBy}
        onRefresh={load}
      />
      {error && <Alert variant="warning">{error}</Alert>}
      {startList && (
        <GroupedTable data={startList.data} columns={columns} rowKey={(e) => e.Id} filterBy={activeFilter} />
      )}
    </>
  )
}
