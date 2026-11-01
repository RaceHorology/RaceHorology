import { useEffect, useState } from 'react'
import { fetchMetaData, fetchStartList } from './api/http'
import type { MetaData, StartList } from './api/types'

export default function App() {
  const [meta, setMeta] = useState<MetaData | null>(null)
  const [startList, setStartList] = useState<StartList | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    Promise.all([fetchMetaData(), fetchStartList(0)])
      .then(([m, s]) => {
        setMeta(m)
        setStartList(s)
      })
      .catch((e: unknown) => setError(String(e)))
  }, [])

  return (
    <main>
      <h1>Race Horology</h1>
      <p>Neuer Web-Client (Vorschau)</p>
      {error && <p role="alert">Fehler: {error}</p>}
      {meta && (
        <p>
          {meta.data.runs} Durchgänge, {meta.data.classes.length} Klassen, {meta.data.groups.length} Gruppen
        </p>
      )}
      {startList &&
        Object.entries(startList.data).map(([group, entries]) => (
          <section key={group}>
            {group && <h2>{group}</h2>}
            <table>
              <thead>
                <tr>
                  <th>StNr</th>
                  <th>Name</th>
                  <th>Jahrgang</th>
                  <th>Verein</th>
                  <th>Klasse</th>
                </tr>
              </thead>
              <tbody>
                {entries.map((e) => (
                  <tr key={e.StartNumber}>
                    <td>{e.StartNumber}</td>
                    <td>
                      {e.Name} {e.Firstname}
                    </td>
                    <td>{e.Year}</td>
                    <td>{e.Club}</td>
                    <td>{e.Class}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </section>
        ))}
    </main>
  )
}
