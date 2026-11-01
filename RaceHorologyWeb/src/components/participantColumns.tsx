import type { Participant } from '../api/types'
import type { Column } from './GroupedTable'

/*
 * The potentially long participant attributes (first name, club, class) have no columns of their own but are shown
 * in the name cell: "Name Firstname" in the first line, club (and class) in a smaller second line.
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
      <span className="fw-normal small"> / Verein{withClass && ' / Klasse'}</span>
    </>
  )
}

export function NameCell({ p, withClass = false }: { p: Participant; withClass?: boolean }) {
  return (
    <>
      {p.Name} {p.Firstname}
      <div className="small text-body-secondary">
        {p.Club}
        {withClass && p.Class && ` · ${p.Class}`}
      </div>
    </>
  )
}

/** Name (with first name, club and optionally class), category and year */
export function participantColumns<T extends Participant>(withClass = false): Column<T>[] {
  return [
    { header: 'Name', headerContent: <NameHeader withClass={withClass} />, render: (e) => <NameCell p={e} withClass={withClass} />, className: 'rh-name' },
    {
      header: 'Kategorie',
      headerContent: <ShortHeader long="Kategorie" short="Kat." />,
      render: (e) => e.Sex,
      className: 'text-center',
    },
    { header: 'Jahrgang', headerContent: <ShortHeader long="Jahrgang" short="Jg." />, render: (e) => e.Year, className: 'text-center' },
  ]
}
