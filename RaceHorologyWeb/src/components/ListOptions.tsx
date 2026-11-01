import Button from 'react-bootstrap/Button'
import Form from 'react-bootstrap/Form'
import Stack from 'react-bootstrap/Stack'
import type { Grouping, MetaData } from '../api/types'

const groupingNames: Record<Grouping, string> = {
  Class: 'Klasse',
  Group: 'Gruppe',
  Sex: 'Kategorie',
}

/** Names of the groups the list can be filtered by, for the given grouping */
function filterItems(meta: MetaData['data'], grouping: string): string[] {
  switch (grouping) {
    case 'Class':
      return meta.classes.map((c) => c.Name)
    case 'Group':
      return meta.groups.map((g) => g.Name)
    case 'Sex':
      return meta.sex
    default:
      return []
  }
}

export interface ListOptionsProps {
  meta: MetaData['data'] | null
  /** Selected run (0-based); omit to hide the run selection */
  run?: number
  onRunChange?: (run: number) => void
  groupBy: Grouping | ''
  onGroupByChange: (groupBy: Grouping | '') => void
  /** Grouping the server actually applied (its default if groupBy is empty) */
  effectiveGroupBy: string
  filterBy: string
  onFilterByChange: (filterBy: string) => void
  onRefresh: () => void
}

export default function ListOptions(props: ListOptionsProps) {
  const { meta } = props
  const items = meta ? filterItems(meta, props.effectiveGroupBy) : []

  return (
    <Stack direction="horizontal" gap={2} className="flex-wrap justify-content-center mb-3">
      {props.run !== undefined && props.onRunChange && (
        <Form.Select
          aria-label="Durchgang"
          className="w-auto"
          value={props.run}
          onChange={(e) => props.onRunChange?.(Number(e.target.value))}
        >
          {Array.from({ length: meta?.runs ?? 1 }, (_, i) => (
            <option key={i} value={i}>
              {i + 1}. Durchgang
            </option>
          ))}
        </Form.Select>
      )}
      <Form.Select
        aria-label="Gruppierung"
        className="w-auto"
        value={props.groupBy}
        onChange={(e) => props.onGroupByChange(e.target.value as Grouping | '')}
      >
        <option value="">Standard-Gruppierung</option>
        {meta?.groupings.map((g) => (
          <option key={g} value={g}>
            nach {groupingNames[g] ?? g}
          </option>
        ))}
      </Form.Select>
      <Form.Select
        aria-label="Filter"
        className="w-auto"
        value={props.filterBy}
        disabled={items.length === 0}
        onChange={(e) => props.onFilterByChange(e.target.value)}
      >
        <option value="">Alle</option>
        {items.map((i) => (
          <option key={i} value={i}>
            {i}
          </option>
        ))}
      </Form.Select>
      <Button variant="outline-secondary" onClick={props.onRefresh}>
        Aktualisieren
      </Button>
    </Stack>
  )
}
