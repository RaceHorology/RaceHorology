import { useEffect, useState } from 'react'
import { fetchMetaData } from './http'
import type { MetaData } from './types'

/** Loads the metadata of the current race; reloads whenever reloadKey changes (e.g. the current race) */
export function useMetaData(reloadKey: unknown): MetaData['data'] | null {
  const [meta, setMeta] = useState<MetaData['data'] | null>(null)

  useEffect(() => {
    let cancelled = false
    fetchMetaData()
      .then((m) => {
        if (!cancelled)
          setMeta(m.data)
      })
      .catch(() => {
        if (!cancelled)
          setMeta(null)
      })
    return () => {
      cancelled = true
    }
  }, [reloadKey])

  return meta
}
