import type { Participant } from '../api/types'
import type { Column } from './GroupedTable'

/*
 * The potentially long participant attributes (first name, club) have columns of their own. If the list does not fit,
 * they are shown in the name cell instead (compact level 1, see useCompactLevel): "Name Firstname" in the first line,
 * the club in a smaller second line.
 */

/** Column header that is abbreviated if the list does not fit otherwise (compact level 3) */
export function ShortHeader({ long, short }: { long: string; short: string }) {
  return (
    <>
      <span className="rh-c3-col">{long}</span>
      <span className="rh-c3-inline">{short}</span>
    </>
  )
}

export function NameHeader() {
  return (
    <>
      Name
      <span className="rh-c1-inline fw-normal small"> / Verein</span>
    </>
  )
}

export function NameCell({ p }: { p: Participant }) {
  return (
    <>
      {p.Name}
      <span className="rh-c1-inline"> {p.Firstname}</span>
      <div className="rh-c1-inline small text-body-secondary">{p.Club}</div>
    </>
  )
}

/** Name, first name, category, year and club */
export function participantColumns<T extends Participant>(): Column<T>[] {
  return [
    { header: 'Name', headerContent: <NameHeader />, render: (e) => <NameCell p={e} />, className: 'rh-name' },
    { header: 'Vorname', render: (e) => e.Firstname, className: 'rh-c1-col' },
    {
      header: 'Kategorie',
      headerContent: <ShortHeader long="Kategorie" short="Kat." />,
      render: (e) => e.Sex,
      className: 'text-center',
    },
    { header: 'Jahrgang', headerContent: <ShortHeader long="Jahrgang" short="Jg." />, render: (e) => e.Year, className: 'text-center' },
    { header: 'Verein', render: (e) => e.Club, className: 'rh-c1-col' },
  ]
}
