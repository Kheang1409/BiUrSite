import React from "react";

interface StatProps {
  label: string;
  value: number | string;
  icon?: string;
  className?: string;
}

export function Stat({ label, value, icon, className = "" }: StatProps) {
  return (
    <div className={["text-center", className].filter(Boolean).join(" ")}>
      {icon && <span className="text-lg mb-1 block">{icon}</span>}
      <p className="text-sm font-semibold text-gray-900 dark:text-white">
        {value}
      </p>
      <p className="text-xs text-muted">{label}</p>
    </div>
  );
}

interface StatsGroupProps {
  stats: StatProps[];
  layout?: "horizontal" | "vertical";
}

export function StatsGroup({ stats, layout = "horizontal" }: StatsGroupProps) {
  return (
    <div
      className={`flex ${
        layout === "horizontal" ? "gap-6" : "gap-4"
      } border-t border-gray-200 dark:border-white/10 pt-4`}
    >
      {stats.map((stat, idx) => (
        <Stat key={idx} {...stat} />
      ))}
    </div>
  );
}
