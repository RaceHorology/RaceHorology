import { useCallback, useEffect, useState } from 'react'
import Alert from 'react-bootstrap/Alert'
import { fetchStartList } from '../api/http'
import type { Grouping, MetaData, StartList, StartListEntry } from '../api/types'
import GroupedTable, { type Column } from './GroupedTable'
import ListOptions from './ListOptions'

const columns: Column<StartListEntry>[] = [
  { header: 'StNr', render: (e) => e.StartNumber, className: 'text-center' },
  { header: 'Name', render: (e) => e.Name },
  { header: 'Vorname', render: (e) => e.Firstname },
  { header: 'Kategorie', render: (e) => e.Sex, className: 'text-center' },
  { header: 'Jahrgang', render: (e) => e.Year, className: 'text-center' },
  { header: 'Verein', render: (e) => e.Club },
  { header: 'Klasse', render: (e) => e.Class },
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
        effectiveGroupBy={startList?.groupby ?? ''}
        filterBy={filterBy}
        onFilterByChange={setFilterBy}
        onRefresh={load}
      />
      {error && <Alert variant="warning">{error}</Alert>}
      {startList && (
        <GroupedTable data={startList.data} columns={columns} rowKey={(e) => e.Id} filterBy={filterBy} />
      )}
    </>
  )
}
