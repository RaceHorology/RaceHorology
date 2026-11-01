import { type DependencyList, useLayoutEffect, useRef, useState } from 'react'

/** Highest compact level, see the .rh-compact-N classes in theme.css */
const MAX_COMPACT_LEVEL = 3

function overflows(container: HTMLElement) {
  return [...container.querySelectorAll<HTMLElement>('.table-responsive')].some((t) => t.scrollWidth > t.clientWidth)
}

/**
 * Makes the tables within the returned container only as compact as needed to fit its width:
 * all columns are shown as long as there is space, columns are merged into other cells step by step otherwise.
 * The levels are tried before the browser paints, so switching between them does not flicker.
 * @param deps the data shown; the levels are tried again whenever it changes
 */
export function useCompactLevel(deps: DependencyList) {
  const ref = useRef<HTMLDivElement>(null)
  const [level, setLevel] = useState(0)
  const [width, setWidth] = useState(0)

  useLayoutEffect(() => {
    const el = ref.current
    if (!el)
      return
    const observer = new ResizeObserver(() => setWidth(el.clientWidth))
    observer.observe(el)
    return () => observer.disconnect()
  }, [])

  // Start with all columns whenever the data or the available width changes ...
  useLayoutEffect(() => setLevel(0), [width, ...deps]) // eslint-disable-line react-hooks/exhaustive-deps

  // ... and merge columns step by step until the tables fit
  useLayoutEffect(() => {
    if (ref.current && level < MAX_COMPACT_LEVEL && overflows(ref.current))
      setLevel(level + 1)
  })

  const className = Array.from({ length: level }, (_, i) => `rh-compact-${i + 1}`).join(' ')
  return { ref, className }
}
