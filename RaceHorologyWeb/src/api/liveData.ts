import { useEffect, useReducer } from 'react'
import type {
  CurrentRaceRunMessage,
  LiveDataMessage,
  OnTrackEntry,
  Participant,
  RaceResultList,
  RunResultList,
  StartListEntry,
} from './types'

export type ConnectionStatus = 'connecting' | 'connected' | 'disconnected'

export interface LiveData {
  status: ConnectionStatus
  /** Next participants to start in the current run */
  onStart: StartListEntry[]
  onTrack: OnTrackEntry[]
  /** Participants who finished most recently, newest first */
  justFinished: Participant[]
  /** Results of the current run */
  runResult: RunResultList | null
  raceResult: RaceResultList | null
  currentRaceRun: CurrentRaceRunMessage['data'] | null
  lastUpdate: Date | null
}

const MAX_JUST_FINISHED = 3
const RECONNECT_DELAY_MS = 3000

const initialState: LiveData = {
  status: 'connecting',
  onStart: [],
  onTrack: [],
  justFinished: [],
  runResult: null,
  raceResult: null,
  currentRaceRun: null,
  lastUpdate: null,
}

type Action = { kind: 'status'; status: ConnectionStatus } | { kind: 'message'; message: LiveDataMessage }

function reduceMessage(state: LiveData, message: LiveDataMessage): LiveData {
  switch (message.type) {
    case 'onstart':
      return { ...state, onStart: message.data }
    case 'ontrack':
      return { ...state, onTrack: message.data }
    case 'racerunresult':
      return { ...state, runResult: message }
    case 'raceresult':
      return { ...state, raceResult: message }
    case 'currentracerun':
      return { ...state, currentRaceRun: message.data, justFinished: [] }
    case 'event_participant': {
      const p = message.data.Participant
      if (message.data.EventType !== 'Finished' || state.justFinished.some((x) => x.StartNumber === p.StartNumber))
        return state
      return { ...state, justFinished: [p, ...state.justFinished].slice(0, MAX_JUST_FINISHED) }
    }
    default:
      return state
  }
}

function reducer(state: LiveData, action: Action): LiveData {
  if (action.kind === 'status')
    return { ...state, status: action.status }
  return { ...reduceMessage(state, action.message), lastUpdate: new Date() }
}

function liveDataUrl(): string {
  const protocol = window.location.protocol === 'https:' ? 'wss:' : 'ws:'
  return `${protocol}//${window.location.host}/api/LiveData`
}

/**
 * Connects to the WebSocket /api/LiveData and keeps the pushed live data.
 * Reconnects automatically if the connection is lost (e.g. RaceHorology restarted).
 */
export function useLiveData(): LiveData {
  const [state, dispatch] = useReducer(reducer, initialState)

  useEffect(() => {
    let socket: WebSocket | null = null
    let reconnectTimer: number | undefined
    let disposed = false

    const connect = () => {
      dispatch({ kind: 'status', status: 'connecting' })
      socket = new WebSocket(liveDataUrl())
      socket.onopen = () => {
        dispatch({ kind: 'status', status: 'connected' })
        // Any message makes the server send the current state of all lists
        socket?.send('init')
      }
      socket.onmessage = (e: MessageEvent<string>) => {
        try {
          dispatch({ kind: 'message', message: JSON.parse(e.data) as LiveDataMessage })
        } catch {
          // Ignore malformed messages
        }
      }
      socket.onclose = () => {
        if (disposed)
          return
        dispatch({ kind: 'status', status: 'disconnected' })
        reconnectTimer = window.setTimeout(connect, RECONNECT_DELAY_MS)
      }
    }

    connect()

    return () => {
      disposed = true
      window.clearTimeout(reconnectTimer)
      socket?.close()
    }
  }, [])

  return state
}
