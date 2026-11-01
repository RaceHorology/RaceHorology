// Minimal client for the JSON API served by DSVAlpin2HTTPServer (see Documents/JSON-API.docx).
// An empty race / run number selects the current race / run.

const API_BASE = '/api/v0.1'

export interface NamedItem {
  Name: string
}

export interface MetaData {
  type: 'metadata'
  data: {
    classes: NamedItem[]
    groups: NamedItem[]
    sex: string[]
    groupings: string[]
    runs: number
  }
}

export interface StartListEntry {
  StartNumber: number
  Name: string
  Firstname: string
  Year: number
  Club: string
  Class: string
}

export interface StartList {
  type: 'startlist'
  groupby: string
  // Group name -> entries; ungrouped lists use the empty group name
  data: Record<string, StartListEntry[]>
}

async function getJson<T>(path: string): Promise<T> {
  const res = await fetch(API_BASE + path)
  if (!res.ok)
    throw new Error(`${res.status} ${res.statusText}`)
  const text = await res.text()
  // The server answers with an empty body if there is no current race
  if (text === '')
    throw new Error('Kein aktuelles Rennen')
  return JSON.parse(text) as T
}

export function fetchMetaData(): Promise<MetaData> {
  return getJson<MetaData>('/races//metadata')
}

export function fetchStartList(): Promise<StartList> {
  return getJson<StartList>('/races//runs//startlist')
}
