// Types of the JSON API served by DSVAlpin2HTTPServer (see RaceHorologyLib/JsonConversion.cs).

/** Grouping criteria the server understands for the "groupby" query parameter */
export type Grouping = 'Class' | 'Group' | 'Sex'

/** Lists are grouped by the server: group name -> entries; ungrouped lists use the empty group name */
export type GroupedData<T> = Record<string, T[]>

export interface NamedItem {
  Name: string
}

export interface Participant {
  Id: string
  StartNumber: number
  Name: string
  Firstname: string
  Sex: string | null
  Year: number
  Club: string
  Nation: string
  Class: string
  Group: string
}

export type StartListEntry = Participant

/** Result of a single run (RunResultWPConverter) */
export interface RunResultEntry extends Participant {
  Position: number
  Runtime: string | null
  DiffToFirst: string | null
  DisqualText: string | null
  JustModified: boolean
}

/** Runtime of one run within the race result; empty if the run has no result */
export interface RaceResultRuntime {
  Runtime?: string | null
  Position?: number
}

/** Overall race result (RaceResultConverter) */
export interface RaceResultEntry extends Participant {
  Position: number
  Totaltime: string | null
  DiffToFirst: string | null
  DisqualText: string | null
  Runtimes: RaceResultRuntime[]
  JustModified: boolean
}

/** Participant currently on track (RunResultConverter) */
export interface OnTrackEntry extends Participant {
  Runtime: string | null
  DisqualText: string | null
}

/** Column key -> German column heading, as sent by the server */
export type FieldNames = Record<string, string | Record<string, string>>

export interface MetaData {
  type: 'metadata'
  data: {
    classes: NamedItem[]
    groups: NamedItem[]
    sex: string[]
    groupings: Grouping[]
    runs: number
  }
}

export interface StartList {
  type: 'startlist'
  groupby: string
  data: GroupedData<StartListEntry>
}

export interface RunResultList {
  type: 'racerunresult'
  fields: FieldNames
  groupby: string
  data: GroupedData<RunResultEntry>
}

export interface RaceResultList {
  type: 'raceresult'
  fields: FieldNames
  groupby: string
  data: GroupedData<RaceResultEntry>
}

// Messages pushed via the WebSocket /api/LiveData

export interface OnStartMessage {
  type: 'onstart'
  data: StartListEntry[]
}

export interface OnTrackMessage {
  type: 'ontrack'
  data: OnTrackEntry[]
}

export interface CurrentRaceRunMessage {
  type: 'currentracerun'
  // RaceType and run number of the current race run, as strings
  data: { type: string | null; run: string | null }
}

export interface ParticipantEventMessage {
  type: 'event_participant'
  data: {
    EventType: 'Started' | 'Finished'
    Participant: Participant
    RunResult: { Runtime: string | null; DisqualText: string | null } | null
  }
}

export type LiveDataMessage =
  | OnStartMessage
  | OnTrackMessage
  | CurrentRaceRunMessage
  | ParticipantEventMessage
  | RaceResultList
  | RunResultList
