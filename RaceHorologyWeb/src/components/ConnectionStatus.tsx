import Badge from 'react-bootstrap/Badge'
import type { LiveData } from '../api/liveData'

const statusDisplay = {
  connecting: { text: 'Verbinde …', bg: 'secondary' },
  connected: { text: 'Verbunden', bg: 'success' },
  disconnected: { text: 'Keine Verbindung', bg: 'danger' },
} as const

export default function ConnectionStatus({ liveData }: { liveData: LiveData }) {
  const s = statusDisplay[liveData.status]
  return (
    <div className="text-end small">
      <Badge bg={s.bg}>{s.text}</Badge>
      {liveData.lastUpdate && (
        <div className="text-body-secondary">Stand: {liveData.lastUpdate.toLocaleTimeString()}</div>
      )}
    </div>
  )
}
