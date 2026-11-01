import { useCallback, useEffect, useState } from 'react'
import Alert from 'react-bootstrap/Alert'
import { fetchRaceResultList } from '../api/http'
import type { LiveData } from '../api/liveData'
import type { Grouping, MetaData, RaceResultList } from '../api/types'
import GroupedTable from './GroupedTable'
import ListOptions from './ListOptions'
import { raceResultColumns } from './raceResultColumns'

export default function RaceResultTab({ liveData, meta }: { liveData: LiveData; meta: MetaData['data'] | null }) {
  const [groupBy, setGroupBy] = useState<Grouping | ''>('')
  const [filterBy, setFilterBy] = useState('')
  const [results, setResults] = useState<RaceResultList | null>(null)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(() => {
    fetchRaceResultList(groupBy)
      .then((r) => {
        setResults(r)
        setError(null)
      })
      .catch((e: unknown) => setError(String(e)))
  }, [groupBy])

  // The server pushes the race result whenever a time changes; reload it with the selected grouping
  useEffect(load, [load, liveData.raceResult])

  // Times of the single runs are only of interest if the race has more than one run
  const totalRuns = meta?.runs ?? 1
  const runs = totalRuns > 1 ? totalRuns : 0

  // A filter for a group that no longer exists (e.g. after changing the grouping) shows everything
  const activeFilter = results && filterBy in results.data ? filterBy : ''

  return (
    <>
      <ListOptions
        meta={meta}
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
          columns={raceResultColumns(runs, true)}
          rowKey={(e) => e.Id}
          rowClassName={(e) => (e.JustModified ? 'just-modified' : undefined)}
          filterBy={activeFilter}
        />
      )}
    </>
  )
}
