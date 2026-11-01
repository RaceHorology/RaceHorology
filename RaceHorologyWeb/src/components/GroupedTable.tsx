import type { ReactNode } from 'react'
import Table from 'react-bootstrap/Table'
import type { GroupedData } from '../api/types'

export interface Column<T> {
  /** Header text, also used as key of the column */
  header: string
  /** Header content if it differs from the plain header text */
  headerContent?: ReactNode
  render: (row: T) => ReactNode
  className?: string
}

export interface GroupedTableProps<T> {
  data: GroupedData<T>
  columns: Column<T>[]
  rowKey: (row: T) => string | number
  rowClassName?: (row: T) => string | undefined
  /** Only show this group; empty shows all */
  filterBy: string
}

/** Renders a list grouped by the server as one table per group */
export default function GroupedTable<T>({ data, columns, rowKey, rowClassName, filterBy }: GroupedTableProps<T>) {
  const groups = Object.entries(data).filter(([group]) => !filterBy || group === filterBy)

  if (groups.every(([, rows]) => rows.length === 0))
    return <p className="text-center text-body-secondary">Keine Einträge</p>

  return (
    <>
      {groups.map(([group, rows]) => (
        <section key={group} className="mb-4">
          {group && <h2 className="h5 rh-group-heading">{group}</h2>}
          <Table striped hover bordered size="sm" responsive className="rh-table">
            <thead>
              <tr>
                {columns.map((c) => (
                  <th key={c.header} className={c.className}>
                    {c.headerContent ?? c.header}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {rows.map((row) => (
                <tr key={rowKey(row)} className={rowClassName?.(row)}>
                  {columns.map((c) => (
                    <td key={c.header} className={c.className}>
                      {c.render(row)}
                    </td>
                  ))}
                </tr>
              ))}
            </tbody>
          </Table>
        </section>
      ))}
    </>
  )
}
