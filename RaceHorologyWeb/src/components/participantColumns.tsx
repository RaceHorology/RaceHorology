import type { Participant } from '../api/types'
import type { Column } from './GroupedTable'

/*
 * Columns of the potentially long participant attributes (first name, club, class).
 * On narrow screens they are hidden and shown in the name cell instead: "Name Firstname" in the first line,
 * club (and class) in a smaller second line. See .rh-wide-only / .rh-narrow-only in theme.css.
 */

/** Column header that is abbreviated on narrow screens */
export function ShortHeader({ long, short }: { long: string; short: string }) {
  return (
    <>
      <span className="rh-wide-only">{long}</span>
      <span className="rh-narrow-only">{short}</span>
    </>
  )
}

export function NameHeader({ withClass = false }: { withClass?: boolean }) {
  return (
    <>
      Name
      <span className="rh-narrow-only fw-normal small"> / Verein{withClass && ' / Klasse'}</span>
    </>
  )
}

export function NameCell({ p, withClass = false }: { p: Participant; withClass?: boolean }) {
  return (
    <>
      {p.Name}
      <span className="rh-narrow-only"> {p.Firstname}</span>
      <div className="rh-narrow-only small text-body-secondary">
        {p.Club}
        {withClass && p.Class && ` · ${p.Class}`}
      </div>
    </>
  )
}

/** Name, first name, category, year and club, optionally followed by the class */
export function participantColumns<T extends Participant>(withClass = false): Column<T>[] {
  return [
    { header: 'Name', headerContent: <NameHeader withClass={withClass} />, render: (e) => <NameCell p={e} withClass={withClass} />, className: 'rh-name' },
    { header: 'Vorname', render: (e) => e.Firstname, className: 'rh-wide-only' },
    {
      header: 'Kategorie',
      headerContent: <ShortHeader long="Kategorie" short="Kat." />,
      render: (e) => e.Sex,
      className: 'text-center',
    },
    { header: 'Jahrgang', headerContent: <ShortHeader long="Jahrgang" short="Jg." />, render: (e) => e.Year, className: 'text-center' },
    { header: 'Verein', render: (e) => e.Club, className: 'rh-wide-only' },
    ...(withClass ? [{ header: 'Klasse', render: (e: T) => e.Class, className: 'rh-wide-only' }] : []),
  ]
}
