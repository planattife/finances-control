import styles from './KpiCard.module.css'

interface KpiCardProps {
  label: string
  value: number | string
}

function KpiCard({ label, value }: KpiCardProps) {
  return (
    <div className={styles.card}>
      <strong className={styles.value}>{value}</strong>
      <span className={styles.label}>{label}</span>
    </div>
  )
}

export default KpiCard
