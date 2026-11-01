import type { Grouping, MetaData, RaceResultList, RunResultList, StartList } from './types'

const API_BASE = '/api/v0.1'

async function getJson<T>(path: string, groupBy?: Grouping | ''): Promise<T> {
  const url = API_BASE + path + (groupBy ? `?groupby=${groupBy}` : '')
  const res = await fetch(url)
  if (!res.ok)
    throw new Error(`${res.status} ${res.statusText}`)
  const text = await res.text()
  // The server answers with an empty body if there is no current race
  if (text === '')
    throw new Error('Kein aktuelles Rennen')
  return JSON.parse(text) as T
}

// An empty race number selects the current race; runs are counted from 0

export function fetchMetaData(): Promise<MetaData> {
  return getJson<MetaData>('/races//metadata')
}

export function fetchStartList(run: number, groupBy?: Grouping | ''): Promise<StartList> {
  return getJson<StartList>(`/races//runs/${run}/startlist`, groupBy)
}

export function fetchRunResultList(run: number, groupBy?: Grouping | ''): Promise<RunResultList> {
  return getJson<RunResultList>(`/races//runs/${run}/resultlist`, groupBy)
}

export function fetchRaceResultList(groupBy?: Grouping | ''): Promise<RaceResultList> {
  return getJson<RaceResultList>('/races//resultlist', groupBy)
}
